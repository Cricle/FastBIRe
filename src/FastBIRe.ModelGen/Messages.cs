using Microsoft.CodeAnalysis;

namespace FastBIRe.ModelGen
{
    internal static class Messages
    {
        static class Categorys
        {
            public const string Mapping = "FastBIRe.Mapping";
        }

        public static DiagnosticDescriptor PropertyMustReadAndWrite = new DiagnosticDescriptor(
            "FBR0001",
            "Mapped property must can read and write",
            "The property {0} for ORM/ModelMap need to full access",
            Categorys.Mapping,
            DiagnosticSeverity.Error, true);

        public static DiagnosticDescriptor AfterCallMethodMustNoArgumentAndMustPublic = new DiagnosticDescriptor(
            "FBR0002",
            "After call method must no argument and must public",
            "The \"After Call Method\" will be call when after the orm active instance and write values, so I has no argument to provide, and I need to visilibity to call",
            Categorys.Mapping,
            DiagnosticSeverity.Error, true);

        public static DiagnosticDescriptor DataTypeMustBeKnowTypesOrCustomer = new DiagnosticDescriptor(
            "FBR0003",
            "Data type must be know types or custom type",
            $"The property {{0}} if automatic judgment type, I only support {string.Join(",", Types.SupportDbTypes)}",
            Categorys.Mapping,
            DiagnosticSeverity.Error, true);
    }
}
