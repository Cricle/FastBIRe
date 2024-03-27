namespace FastBIRe.Triggering
{
    public record class FieldRaw
    {
        public FieldRaw(string field, string raw, string rawFormat)
        {
            Field = field;
            Raw = raw;
            RawFormat = rawFormat;
        }

        /// <summary>
        /// The field of table column
        /// </summary>
        public string Field { get; }
        /// <summary>
        /// Gets the raw of <see cref="Field"/> what how to do
        /// </summary>
        public string Raw { get; }
        /// <summary>
        /// Gets the raw of <see cref="Raw"/> foramt
        /// </summary>
        public string RawFormat { get; }
    }
}
