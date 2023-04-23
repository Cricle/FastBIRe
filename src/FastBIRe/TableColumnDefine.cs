﻿namespace FastBIRe
{
    public record TableColumnDefine(string Field, string Raw, string RawFormat, bool OnlySet = false)
    {
        public string? Type { get; set; }

        public string? Id { get; set; }

        public bool Nullable { get; set; } = true;

        public int Length { get; set; }

        public bool ExpandDateTime { get; set; }

        public TableColumnDefine SetExpandDateTime(bool expand = true)
        {
            ExpandDateTime = expand;
            return this;
        }
    }
}
