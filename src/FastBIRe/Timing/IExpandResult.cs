namespace FastBIRe.Timing
{
    /// <summary>
    /// The result for any expand result
    /// </summary>
    public interface IExpandResult
    {
        /// <summary>
        /// Gets the value for unfolded name
        /// </summary>
        string OriginName { get; }

        /// <summary>
        /// Gets the name for expand column
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the field for <see cref="Name"/> field of type <see cref="Type"/> trigger formatter
        /// </summary>
        /// <remarks>
        /// Example in sqlserver the field value like <code>CONVERT(VARCHAR(22),{0},16)</code>
        /// </remarks>
        string? ExparessionFormatter { get; }

        /// <summary>
        /// Format the expression use <see cref="ExparessionFormatter"/> and paramter 0 use <paramref name="input"/>
        /// </summary>
        /// <param name="input">The format paramter</param>
        /// <returns>The result</returns>
        string FormatExpression(string input);
    }
}
