using System.ComponentModel;
using System.Data;
#if !NETSTANDARD2_0
using System.Diagnostics.CodeAnalysis;
#endif
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
#if NETSTANDARD2_0
    public static class ObjectMapper<T>
#else
    public static class ObjectMapper<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] T>
#endif
    {
        private static readonly Func<IDataReader, T> creator;
        private static readonly MethodInfo convertMethod;
        private static readonly PropertyInfo dataReadMethod;

        static ObjectMapper()
        {
            convertMethod = typeof(Convert).GetMethod(nameof(Convert.ChangeType),
                BindingFlags.Public | BindingFlags.Static,
                null,
                new Type[] { typeof(object), typeof(TypeCode) },
                null) ??
                throw new NotSupportedException("Convert.ChangeType not found");
            dataReadMethod = typeof(IDataRecord).GetProperties().First(x => x.GetIndexParameters().Length != 0 && x.GetIndexParameters()[0].ParameterType == typeof(string));
            creator = BuildCreator();
        }
        private static Func<IDataRecord, T> BuildCreator()
        {
            var par1 = Expression.Parameter(typeof(IDataRecord));
            var mappers = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x =>
                {
                    var browser = x.GetCustomAttribute<BrowsableAttribute>();
                    return browser == null || browser.Browsable;
                })
                .ToArray();
            var instance = Expression.New(typeof(T));
            var var = Expression.Variable(typeof(T));
            var entry = Expression.Assign(var, instance);
            var assigns = new List<Expression>(mappers.Length + 2) { entry };
            foreach (var item in mappers)
            {
                if (item.CanWrite)
                {
                    var name = item.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? item.Name;
                    var assign = GetGetValueExpression(par1, name, item.PropertyType, item, var, false);
                    assigns.Add(assign);
                }
            }
            assigns.Add(var);
            var blocks = Expression.Block(new[] { var }, assigns);
            var tree = Expression.Lambda<Func<IDataRecord, T>>(blocks, par1);
            return tree.Compile();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Fill(IDataReader reader)
        {
            return creator(reader);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<T> FillList(IDataReader reader)
        {
            var lst = new List<T>();
            while (reader.Read())
            {
                lst.Add(creator(reader));
            }
            return lst;
        }
        private static readonly MethodInfo OrMethod = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetOrdinal), new Type[] { typeof(string) }) ??
            throw new NotSupportedException("IDataRecord.GetOrdinal not found");
        private static readonly MethodInfo isDbNullMethod = typeof(IDataRecord).GetMethod(nameof(IDataRecord.IsDBNull), new Type[] { typeof(int) }) ??
            throw new NotSupportedException("IDataRecord.IsDBNull not found");
        private static Expression ReaderMethod(Expression instance, string methodName, string name)
        {
            var getMethod = typeof(IDataRecord).GetMethod(methodName);
            return Expression.Call(instance, getMethod!, Expression.Call(instance, OrMethod, Expression.Constant(name)));
        }
        private static Expression GetGetValueExpression(Expression input, string name, Type type, PropertyInfo property, Expression instance, bool ignoreSet)
        {
            var typeCode = Type.GetTypeCode(type);
            Expression? methodValue = null;
            switch (typeCode)
            {
                case TypeCode.Empty:
                    return Expression.Call(instance, property.SetMethod!, Expression.Constant(null));
                case TypeCode.DBNull:
                    return Expression.Call(instance, property.SetMethod!, Expression.Constant(DBNull.Value));
                case TypeCode.Boolean:
                    methodValue = ReaderMethod(input, nameof(IDataRecord.GetBoolean), name);
                    break;
                case TypeCode.Char:
                    methodValue = ReaderMethod(input, nameof(IDataRecord.GetChar), name);
                    break;
                case TypeCode.Byte:
                    methodValue = ReaderMethod(input, nameof(IDataRecord.GetByte), name);
                    break;
                case TypeCode.Int16:
                    methodValue = ReaderMethod(input, nameof(IDataRecord.GetInt16), name);
                    break;
                case TypeCode.Int32:
                    methodValue = ReaderMethod(input, nameof(IDataRecord.GetInt32), name);
                    break;
                case TypeCode.Int64:
                    methodValue = ReaderMethod(input, nameof(IDataRecord.GetInt64), name);
                    break;
                case TypeCode.Single:
                    methodValue = ReaderMethod(input, nameof(IDataRecord.GetFloat), name);
                    break;
                case TypeCode.Double:
                    methodValue = ReaderMethod(input, nameof(IDataRecord.GetDouble), name);
                    break;
                case TypeCode.Decimal:
                    methodValue = ReaderMethod(input, nameof(IDataRecord.GetDecimal), name);
                    break;
                case TypeCode.DateTime:
                    methodValue = ReaderMethod(input, nameof(IDataRecord.GetDateTime), name);
                    break;
                case TypeCode.String:
                    methodValue = ReaderMethod(input, nameof(IDataRecord.GetString), name);
                    break;
                default:
                    break;
            }
            if (property.PropertyType == typeof(Guid))
            {
                methodValue = ReaderMethod(input, nameof(IDataRecord.GetGuid), name);
            }
            if (property.PropertyType == typeof(object))
            {
                methodValue = ReaderMethod(input, nameof(IDataRecord.GetValue), name);
            }
            if (methodValue != null)
            {
                if (ignoreSet)
                {
                    return methodValue;
                }
                return Expression.Call(instance, property.SetMethod!, methodValue);
            }
            if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var actualType = property.PropertyType.GenericTypeArguments[0];
                var exp = GetGetValueExpression(input, name, actualType, property, instance, true);
                return Expression.IfThenElse(Expression.Call(input, isDbNullMethod, Expression.Call(input, OrMethod, Expression.Constant(name))),
                     Expression.Call(instance, property.SetMethod!, Expression.Convert(Expression.Constant(null), property.PropertyType)),
                     Expression.Call(instance, property.SetMethod!, Expression.Convert(exp, property.PropertyType)));
            }
            var result = Expression.Call(input, dataReadMethod.GetMethod!, Expression.Constant(name));
            if (ignoreSet)
            {
                return Expression.IfThenElse(Expression.TypeIs(result, property.PropertyType),
                       Expression.Convert(result, property.PropertyType),
                       Expression.Convert(Expression.Call(null, convertMethod, result, Expression.Constant(typeCode)),
                        property.PropertyType));
            }
            return Expression.IfThenElse(Expression.TypeIs(result, property.PropertyType),
                    Expression.Call(instance, property.SetMethod!, Expression.Convert(result, property.PropertyType)),
                   Expression.Call(instance, property.SetMethod!, Expression.Convert(Expression.Call(null, convertMethod, result, Expression.Constant(typeCode)),
                    property.PropertyType)));

        }
    }
}
