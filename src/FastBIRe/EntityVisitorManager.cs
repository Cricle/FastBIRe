namespace FastBIRe
{
    public static class EntityVisitorManager
    {
        private static readonly Dictionary<Type, IEntityVisitor> visitorMap = new Dictionary<Type, IEntityVisitor>();
        private static readonly object locker = new object();

        public static IReadOnlyDictionary<Type, IEntityVisitor> VisitorMap => visitorMap;

        public static void Set(Type type,IEntityVisitor entityVisitor)
        {
            lock (locker)
            {
                visitorMap[type] = entityVisitor;
            }
        }
        public static void Remove(Type type)
        {
            lock (locker)
            {
                visitorMap.Remove(type);
            }
        }
    }
}
