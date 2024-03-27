using DatabaseSchemaReader.DataSchema;
using System.Data;

namespace FastBIRe.Mig
{
    public class VColumn : VObject
    {
        public string Name { get; set; }

        public DbType Type { get; set; }

        public int Length { get; set; } = 255;

        public int Scale { get; set; } = 2;

        public int Precision { get; set; } = 22;

        public bool Nullable { get; set; } = true;

        public bool PK { get; set; }

        public bool IX { get; set; }

        public bool AI { get; set; }

        public void ToDatabaseColumn(DatabaseColumn column, SqlType sqlType)
        {
            column.Name = Name;
            column.Length = Length;
            column.Scale = Scale;
            column.Precision = Precision;
            column.SetTypeDefault(sqlType, Type);
        }
    }
}