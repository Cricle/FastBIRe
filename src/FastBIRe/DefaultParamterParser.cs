using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public sealed class DefaultParamterParser : IParamterParser
    {
        public static readonly DefaultParamterParser Instance = new DefaultParamterParser();

        class ObjectPropertyMap
        {
            public ObjectPropertyMap(string propertyName, Func<object, object?> propertyReader)
            {
                PropertyName = propertyName;
                PropertyReader = propertyReader;
            }

            public string PropertyName { get; }

            public Func<object, object?> PropertyReader { get; }
        }
        class ObjectVisitor
        {
            private static readonly Type ObjectType = typeof(object);

            public readonly Type Type;

            public readonly IList<ObjectPropertyMap> propertyReaders;

            public IEnumerable<KeyValuePair<string, object?>> EnumerableProperties(object instance)
            {
                for (int i = 0; i < propertyReaders.Count; i++)
                {
                    var map = propertyReaders[i];
                    yield return new KeyValuePair<string, object?>(map.PropertyName, map.PropertyReader(instance));
                }
            }

            public ObjectVisitor(Type type)
            {
                Type = type;
                propertyReaders = new ObjectPropertyMap[type.GetProperties().Length];
                Analysis();
            }

            public void Analysis()
            {
                var props = Type.GetProperties();
                for (int i = 0; i < props.Length; i++)
                {
                    var prop = props[i];
                    var par0 = Expression.Parameter(ObjectType);
                    var body = Expression.Convert(Expression.Call(Expression.Convert(par0, Type), prop.GetMethod!), ObjectType);
                    propertyReaders[i] = new ObjectPropertyMap(prop.Name, Expression.Lambda<Func<object, object?>>(body, par0).Compile());
                }
            }
        }

        private static readonly ConcurrentDictionary<Type, ObjectVisitor> objectVisitor = new ConcurrentDictionary<Type, ObjectVisitor>();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable<KeyValuePair<string, object?>> EnumerableList(IList list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                yield return new KeyValuePair<string, object?>(i.ToString(), list[i]);
            }
        }

        private DefaultParamterParser() { }

        public IEnumerable<KeyValuePair<string, object?>> Parse(object? obj)
        {
            if (obj == null)
            {
                return Enumerable.Empty<KeyValuePair<string, object?>>();
            }
            if (obj is IEnumerable<KeyValuePair<string, object?>> args)
            {
                return args;
            }
            if (obj is IList list)
            {
                return EnumerableList(list);
            }
            var type = obj.GetType();
            if (type.IsClass)
            {
                return objectVisitor.GetOrAdd(type, static t => new ObjectVisitor(t)).EnumerableProperties(obj);
            }
            return new OneEnumerable<KeyValuePair<string, object?>>(new KeyValuePair<string, object?>(string.Empty, obj));
        }
    }
}
