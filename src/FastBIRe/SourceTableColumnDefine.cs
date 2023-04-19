﻿namespace FastBIRe
{
    public record SourceTableColumnDefine(string Field, string Raw, bool IsGroup, TableColumnDefine DestColumn, ToRawMethod Method, string RawFormat, bool OnlySet = false)
        : TableColumnDefine(Field, Raw, RawFormat, OnlySet)
    {
        public SourceTableColumnDefine Copy()
        {
            return new SourceTableColumnDefine(this);
        }

        public SourceTableColumnDefine AllNotNull()
        {
            Nullable = false;
            if (DestColumn != null)
            {
                DestColumn.Nullable = false;
            }
            return this;
        }
    }
}
