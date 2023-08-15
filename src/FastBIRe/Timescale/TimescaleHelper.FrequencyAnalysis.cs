namespace FastBIRe.Timescale
{
    public partial class TimescaleHelper
    {
        public string FreqAgg(string min_freq, string value)
        {
            return $"freq_agg({min_freq},{value})";
        }
        public string McvAgg(string n, string value, string? skew = null)
        {
            var skewStr = skew == null ? string.Empty : "," + skew;

            return $"mcv_agg({n},{value}{skewStr})";
        }
        public string MaxFrequency(string agg, string value)
        {
            return $"max_frequency({agg},{value})";
        }
        public string MinFrequency(string agg, string value)
        {
            return $"min_frequency({agg},{value})";
        }
        public string Topn(string agg, string n)
        {
            return $"topn({agg},{n})";
        }
        public string CountMinSketch(string values, string error, string probability)
        {
            return $"count_min_sketch({values},{error},{probability})";
        }
        public string ApproxCount(string item, string agg)
        {
            return $"approx_count({item},{agg})";
        }
    }
}
