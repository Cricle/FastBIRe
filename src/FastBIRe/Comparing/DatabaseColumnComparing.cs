using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.Comparing
{
    public class DefaultDatabaseColumnComparer : IEqualityComparer<DatabaseColumn>
    {
        public static readonly DefaultDatabaseColumnComparer Instance = new DefaultDatabaseColumnComparer();

        private DefaultDatabaseColumnComparer() { }

        public bool Equals(DatabaseColumn? x, DatabaseColumn? y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            if (x.Name != y.Name)
            {
                return false;
            }
            if (x.DataType.NetDataType != y.DataType.NetDataType)
            {
                return false;
            }
            if (x.DataType.IsString)
            {
                return x.Length == y.Length;
            }
            if (x.DataType.NetDataType == typeof(decimal).FullName)
            {
                return x.Scale == y.Scale &&
                    x.Precision == y.Precision;
            }
            return true;
        }

        public int GetHashCode(DatabaseColumn obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.Name);
            if (obj.DataType.IsString)
            {
                hashCode.Add(obj.Length);
            }
            if (obj.DataType.NetDataType == typeof(decimal).FullName)
            {
                hashCode.Add(obj.Scale);
                hashCode.Add(obj.Precision);
            }
            return hashCode.GetHashCode();
        }
    }
}
