using System;

namespace FastBIRe.Cdc.Checkpoints
{
    public readonly struct CheckpointIdentity : IEquatable<CheckpointIdentity>
    {
        public CheckpointIdentity(string databaseName, string tableName)
        {
            DatabaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
            TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }

        public string DatabaseName { get; }

        public string TableName { get; }

        public bool IsEmpty => string.IsNullOrEmpty(DatabaseName);

        public override int GetHashCode()
        {
            return HashCode.Combine(DatabaseName, TableName);
        }
        public override string ToString()
        {
            return $"{{{DatabaseName}.{TableName}}}";
        }
        public override bool Equals(object obj)
        {
            if (obj is CheckpointIdentity identity)
            {
                return Equals(identity);
            }
            return false;
        }

        public bool Equals(CheckpointIdentity other)
        {
            return other.DatabaseName == DatabaseName &&
                other.TableName == TableName;
        }

        public static bool operator ==(CheckpointIdentity left, CheckpointIdentity right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(CheckpointIdentity left, CheckpointIdentity right)
        {
            return !left.Equals(right);
        }
    }
}
