namespace FastBIRe.ModelGen
{
    internal static class Consts
    {
        public const string Name = "FastBIRe";

        public static readonly string Version = typeof(Consts).Assembly.GetName().Version.ToString();

        public static readonly string CompilerGenerated = "[global::System.Runtime.CompilerServices.CompilerGenerated]";

        public const string DebuggerStepThrough = "[global::System.Diagnostics.DebuggerStepThrough]";

        public static readonly string GenerateCode = $"[global::System.CodeDom.Compiler.GeneratedCode(\"{Name}\",\"{Version}\")]";
        public static class CompilerGeneratedAttribute
        {
            public const string FullName = "System.Runtime.CompilerServices.CompilerGeneratedAttribute";
        }
        public static class RecordToAttribute
        {
            public const string FullName = "FastBIRe.Annotations.RecordToAttribute";

            public const string ToType = "ToType";

            public const string RecordToObjectType = "RecordToObjectType";

            public static string WriteAttribute(string toType,string recordToObjectType)
            {
                return $"[global::{FullName}(typeof({toType}),typeof({recordToObjectType}))]";
            }
        }
        public static class GenerateModelAttribute
        {
            public const string FullName = "FastBIRe.Annotations.GenerateModelAttribute";

            public const string IsPublic = "IsPublic";
        }
        public static class KeyAttribute
        {
            public const string FullName = "System.ComponentModel.DataAnnotations.KeyAttribute";
        }
        public static class MaxLengthAttribute
        {
            public const string FullName = "System.ComponentModel.DataAnnotations.MaxLengthAttribute";

            public const string Length = "Length";
        }
        public static class RequiredAttribute
        {
            public const string FullName = "System.ComponentModel.DataAnnotations.RequiredAttribute";
        }
        public static class DbTypeAttribute
        {
            public const string FullName = "FastBIRe.Annotations.DbTypeAttribute";

            public const string DbType = "DbType";

            public const string DataType = "DataType";
        }
        public static class IndexAttribute
        {
            public const string FullName = "FastBIRe.Annotations.IndexAttribute";

            public const string IndexName = "IndexName";

            public const string Order = "Order";

            public const string IndexGroup = "IndexGroup";
        }
        public static class IgnoreAttribute
        {
            public const string FullName = "FastBIRe.Annotations.IgnoreAttribute";
        }
        public static class AutoNumberAttribute
        {
            public const string FullName = "FastBIRe.Annotations.AutoNumberAttribute";

            public const string IdentityByDefault = "IdentityByDefault";

            public const string IdentitySeed = "IdentitySeed";

            public const string IdentityIncrement = "IdentityIncrement";
        }
        public static class DecimalAttribute
        {
            public const string FullName = "FastBIRe.Annotations.DecimalAttribute";

            public const string Precision = "Precision";

            public const string Scale = "Scale";
        }
        public static class IdAttribute
        {
            public const string FullName = "FastBIRe.Annotations.IdAttribute";
        }
        public static class CreateAfterMethodAttribute
        {
            public const string FullName = "FastBIRe.Annotations.CreateAfterMethodAttribute";
        }
        public static class ColumnNameAttribute
        {
            public const string FullName = "FastBIRe.Annotations.ColumnNameAttribute";
        }
    }
}
