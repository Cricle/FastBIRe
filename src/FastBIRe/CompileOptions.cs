namespace FastBIRe
{
    public record CompileOptions
    {
        public bool IncludeEffectJoin { get; set; }

        public string? EffectTable { get; set; }

        public bool NoLock { get; set; }

        public bool UseExpandField { get; set; }

        public bool UseView { get; set; }

        public string ViewInsertFormat { get; set; } = MigrationService.DefaultInsertQueryViewFormat;

        public string ViewUpdateFormat { get; set; } = MigrationService.DefaultUpdateQueryViewFormat;

        public string GetInsertViewName(string destTableName, string sourceTableName)
        {
            return string.Format(ViewInsertFormat, MD5Helper.ComputeHash(destTableName + sourceTableName));
        }
        public string GetUpdateViewName(string destTableName, string sourceTableName)
        {
            return string.Format(ViewUpdateFormat, MD5Helper.ComputeHash(destTableName + sourceTableName));
        }
        public CompileOptions WithNoLock(bool noLock = true)
        {
            NoLock = noLock;
            return this;
        }

        public static CompileOptions EffectJoin(string effectTable)
        {
            return new CompileOptions { IncludeEffectJoin = true, EffectTable = effectTable };
        }
        public static CompileOptions View(string viewInsertFormat, string viewUpdateFormat)
        {
            return new CompileOptions { UseView = true, ViewInsertFormat = viewInsertFormat, ViewUpdateFormat = viewUpdateFormat };
        }
    }
}
