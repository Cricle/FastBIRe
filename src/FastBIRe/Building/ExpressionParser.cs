using FastBIRe.Functions;
using System.Linq.Expressions;
using System.Reflection;

namespace FastBIRe.Building
{
    public class ExpressionParser
    {
        public static readonly ExpressionParser Default = new ExpressionParser();

        protected virtual string GetMemberName(MemberExpression member)
        {
            return member.Member.Name;
        }
        protected virtual string GetMethodName(MethodCallExpression methodCall)
        {
            return methodCall.Method.Name;
        }
        protected virtual object? GetRawValue(Expression expression)
        {
            object? value;
            if (expression is ConstantExpression constant)
            {
                value = constant.Value;
            }
            else
            {
                value = Expression.Lambda(expression).Compile().DynamicInvoke();
            }
            return value;
        }
        protected virtual IQueryMetadata GetValue(Expression expression)
        {
            var value = GetRawValue(expression);
            if (value is IQueryMetadata cqm)
            {
                return cqm;
            }
            return new ValueMetadata(value);

        }
        public virtual IQueryMetadata Parse(Expression expression)
        {
            if (expression is ConstantExpression constant)
            {
                return new ValueMetadata(constant.Value);
            }
            else if (expression is LambdaExpression lambda)
            {
                return Parse(lambda.Body);
            }
            else if (expression is MemberExpression member)
            {
                if (member.Member is PropertyInfo || member.Member is FieldInfo)
                {
                    return new ValueMetadata(GetMemberName(member), true);
                }
                return Parse(member.Expression);
            }
            else if (expression is MethodCallExpression methodCall)
            {
                if (methodCall.Method == FBR.InvokeMethod)
                {
                    var fun = (SQLFunctions)GetRawValue(methodCall.Arguments[0])!;
                    var fbrPars = new IQueryMetadata[methodCall.Arguments.Count - 1];
                    for (int i = 1; i < methodCall.Arguments.Count; i++)
                    {
                        fbrPars[i] = GetValue(methodCall.Arguments[i]);
                    }
                    return new MethodMetadata(fun, fbrPars);
                }
                var pars = new IQueryMetadata[methodCall.Arguments.Count];
                for (int i = 0; i < methodCall.Arguments.Count; i++)
                {
                    pars[i] = Parse(methodCall.Arguments[i]);
                }
                return new MethodMetadata(GetMethodName(methodCall), pars);
            }
            else if (expression is BinaryExpression binary)
            {
                var left = Parse(binary.Left);
                var right = Parse(binary.Right);
                return WrapperMetadata.Brackets(new BinaryMetadata(left, binary.NodeType, right));
            }
            else if (expression is UnaryExpression unary)
            {
                var left = Parse(unary.Operand);
                if (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked)
                {
                    return new UnaryMetadata(left, ExpressionType.Default);
                }
                return new UnaryMetadata(left, unary.NodeType);
            }
            else if (expression is BlockExpression block)
            {
                var metadatas = new MultipleQueryMetadata(block.Expressions.Count);
                for (int i = 0; i < block.Expressions.Count; i++)
                {
                    metadatas.Add(Parse(block.Expressions[i]));
                }
                return metadatas;
            }
            else if (expression is ParameterExpression parameter)
            {
                return new ValueMetadata(parameter.Name, true);
            }
            else if (expression is InvocationExpression invocation)
            {
                var expLambda = Expression.Lambda(invocation.Expression).Compile();
                var res = (Delegate)expLambda.DynamicInvoke();
                var args = new object[invocation.Arguments.Count];
                for (int i = 0; i < args.Length; i++)
                {
                    args[i] = Expression.Lambda(invocation.Arguments[i]).Compile().DynamicInvoke();
                }
                var data = res.DynamicInvoke(args);
                return new ValueMetadata(res, true);
            }
            else
            {
                throw new NotSupportedException(expression.ToString());
            }
        }
    }
}
