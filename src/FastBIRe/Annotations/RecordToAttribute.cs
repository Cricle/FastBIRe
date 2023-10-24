using System.Reflection;

namespace FastBIRe.Annotations
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class RecordToAttribute : Attribute
    {
        public static readonly Type RecordToObjectInterfaceType = typeof(IRecordToObject<>);

        public RecordToAttribute(Type toType, Type recordToObjectType)
        {
            ToType = toType ?? throw new ArgumentNullException(nameof(toType));
            RecordToObjectType = recordToObjectType ?? throw new ArgumentNullException(nameof(recordToObjectType));

            if (!recordToObjectType.IsClass || recordToObjectType.IsAbstract)
            {
                ThrowRecordToObjectTypeNotMatch(recordToObjectType);
            }
            
            var constructor = recordToObjectType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Type.DefaultBinder, Type.EmptyTypes, Array.Empty<ParameterModifier>());
            if (constructor==null)
            {
                ThrowRecordToObjectTypeNotMatch(recordToObjectType);
            }

            var interfaces=recordToObjectType.GetInterfaces();
            if (!interfaces.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == RecordToObjectInterfaceType && x.GenericTypeArguments[0] == toType))
            {
                throw new ArgumentException($"Type {recordToObjectType} is not implement interface {RecordToObjectInterfaceType.MakeGenericType(toType)}");
            }
        }

        public Type ToType { get; }

        public Type RecordToObjectType { get; }

        private static void ThrowRecordToObjectTypeNotMatch(Type type)
        {
            throw new ArgumentException($"Type {type} must no abstract class and has no paramters public constructor.");
        }
    }
}
