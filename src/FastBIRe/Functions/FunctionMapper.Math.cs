using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public partial class FunctionMapper
    {
        public string? ACos(string input)
        {
            return $"ACOS({input})";
        }
        public string? ASin(string input)
        {
            return $"ASIN({input})";
        }
        public string? Abs(string input)
        {
            return $"ABS({input})";
        }
        public string? ATan(string input)
        {
            return $"ATAN({input})";
        }
        public string? ATan2(string n, string m)
        {
            return $"ATAN2({n},{m})";
        }
        public string? Cos(string n)
        {
            return $"COS({n})";
        }
        public string? Sin(string n)
        {
            return $"SIN({n})";
        }
        public string? Cot(string n)
        {
            return $"COT({n})";
        }
        public string? Degress(string n)
        {
            return $"DEGREES({n})";
        }
        public string? Exp(string n)
        {
            return $"EXP({n})";
        }
        public string? Ln(string n)
        {
            return $"Ln({n})";
        }
        public string? PI()
        {
            return $"PI()";
        }
        public string? Pow(string x, string y)
        {
            return $"Pow({x},{y})";
        }
        public string? Sqrt(string x)
        {
            return $"SQRT({x})";
        }
        public string? Log(string n, string? @base = null)
        {
            if (string.IsNullOrEmpty(@base))
            {
                return $"Log({n})";
            }
            return $"Log({@base},{n})";
        }
        public string? Rand()
        {
            switch (SqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    return $"RAND(CHECKSUM(NEWID()))";
                case SqlType.MySql:
                    return $"RAND()";
                case SqlType.SQLite:
                case SqlType.PostgreSql:
                    return $"RANDOM()";
                default:
                    return null;
            }
        }
        public string? RandBetween(string left, string right)
        {
            switch (SqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    return $"FLOOR(({right}-{left}+1)*RAND() + {left})";
                case SqlType.MySql:
                    return $"FLOOR(({right}-{left}+1)*RAND() + {left})";
                case SqlType.SQLite:
                case SqlType.PostgreSql:
                    return $"FLOOR(({right}-{left}+1)*RANDOM() + {left})";
                default:
                    return null;
            }
        }
        public string? Round(string input, string digit)
        {
            return $"ROUND({input}, {digit})";
        }
        public string? RoundUp(string input, string digit)
        {
            return $"ROUND({input} + 0.5 * POWER(10, -{digit}), {digit})";
        }
        public string? RoundDown(string input, string digit)
        {
            return $"ROUND({input} - 0.5 * POWER(10, -{digit}), {digit})";
        }
        public string? Sum(params string[] inputs)
        {
            return string.Join("+", inputs);
        }
        public string? Count(params string[] inputs)
        {
            return string.Join("+", inputs.Select(x => $"(CASE WHEN CAST({x} AS DECIMAL) IS NULL THEN 0 ELSE 1 END)"));
        }
        public string? CountA(params string[] inputs)
        {
            return string.Join("+", inputs.Select(x => $"(CASE {x} WHEN NULL THEN 0 ELSE 1 END)"));
        }
        public string? Average(params string[] inputs)
        {
            return $"({string.Join("+", inputs)})/{inputs.Count()}";
        }
        public string? Average(IEnumerable<string> inputs, string? count)
        {
            if (count == null)
            {
                count = inputs.Count().ToString();
            }
            return $"({string.Join("+", inputs)})/{count}";
        }
        public string? Max(IEnumerable<string> inputs)
        {
            switch (SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return $"SELECT MAX(value) FROM (VALUES {string.Join(",", inputs.Select(x => $"({x})"))}) AS t(value)";
                case SqlType.PostgreSql:
                case SqlType.MySql:
                    return $"GREATEST({string.Join(",", inputs)})";
                case SqlType.SQLite:
                    return $"SELECT MAX(value) FROM ({string.Join(" UNION ", inputs.Select((x, i) => $"SELECT {x} {(i == 0 ? "AS value" : string.Empty)}"))})";
                default:
                    return null;
            }
        }
        public string? MinC(string input)
        {
            return $"MIN({input})";
        }
        public string? MaxC(string input)
        {
            return $"MAX({input})";
        }
        public string? AverageC(string input)
        {
            return $"Avg({input})";
        }
        public string? SumC(string input)
        {
            return $"SUM({input})";
        }
        public string? CountC(string input)
        {
            return $"COUNT({input})";
        }
        public string? DistinctCountC(string input)
        {
            return $"COUNT(DISTINCT {input})";
        }
        public string? StandardDeviation(string input)
        {
            return $"{Sqrt(Var(input)!)}";
        }
        public string? Range(string input)
        {
            return $"{MaxC(input)} - {MinC(input)}";
        }
        public string? Min(params string[] inputs)
        {
            switch (SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return $"SELECT MIN(value) FROM (VALUES {string.Join(",", inputs.Select(x => $"({x})"))}) AS t(value)";
                case SqlType.PostgreSql:
                case SqlType.MySql:
                    return $"LEAST({string.Join(",", inputs)})";
                case SqlType.SQLite:
                    return $"SELECT MIN(value) FROM ({string.Join(" UNION ", inputs.Select((x, i) => $"SELECT {x} {(i == 0 ? "AS value" : string.Empty)}"))})";
                default:
                    return null;
            }
        }
        public string? Median(params string[] inputs)
        {
            switch (SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return $@"
SELECT AVG(val)
FROM 
(
	SELECT val 
	FROM 
	(
		SELECT num AS val, ROW_NUMBER() OVER (ORDER BY num ASC) AS row_num
		FROM (SELECT DISTINCT num FROM ({string.Join("UNION", inputs.Select((x, i) => $"SELECT {x} {(i == 0 ? "AS num" : string.Empty)}"))}) vals) t
	) t1 
	WHERE t1.row_num IN ((SELECT COUNT(*) FROM (SELECT DISTINCT num FROM ({string.Join("UNION", inputs.Select((x, i) => $"SELECT {x} {(i == 0 ? "AS num" : string.Empty)}"))}) vals) t) / 2 + 1, (SELECT COUNT(*) FROM (SELECT DISTINCT num FROM ({string.Join("UNION", inputs.Select((x, i) => $"SELECT {x} {(i == 0 ? "AS num" : string.Empty)}"))}) vals) t) / 2 + ((SELECT COUNT(*) FROM (SELECT DISTINCT num FROM ({string.Join("UNION", inputs.Select((x, i) => $"SELECT {x} {(i == 0 ? "AS num" : string.Empty)}"))}) vals) t) % 2))
) t2
";
                case SqlType.PostgreSql:
                    return $@"SELECT AVG(val) AS median 
FROM (
    SELECT val 
    FROM (
        SELECT num AS val, ROW_NUMBER() OVER (ORDER BY num ASC) AS row_num 
        FROM ( {string.Join("UNION", inputs.Select((x, i) => $"SELECT {x} {(i == 0 ? "AS num" : string.Empty)}"))} ) vals
    ) t1
    WHERE t1.row_num IN ((SELECT COUNT(*) FROM (SELECT DISTINCT num FROM (
        {string.Join("UNION", inputs.Select((x, i) => $"SELECT {x} {(i == 0 ? "AS num" : string.Empty)}"))}
    ) vals) t) / 2 + 1, (SELECT COUNT(*) FROM (SELECT DISTINCT num FROM (
        {string.Join("UNION", inputs.Select((x, i) => $"SELECT {x} {(i == 0 ? "AS num" : string.Empty)}"))}
    ) vals) t) / 2 + ((SELECT COUNT(*) FROM (SELECT DISTINCT num FROM (
        {string.Join("UNION", inputs.Select((x, i) => $"SELECT {x} {(i == 0 ? "AS num" : string.Empty)}"))}
    ) vals) t) % 2))
    ORDER BY val
    LIMIT 2
) t2
";
                case SqlType.MySql:
                    return $@"
SELECT AVG(val)
FROM (
  SELECT num as val
  FROM (
    SELECT num, ROW_NUMBER() OVER (ORDER BY num ASC) AS row_num
    FROM (SELECT DISTINCT num FROM ({string.Join(" UNION ", inputs.Select((x, i) => $"SELECT {x} {(i == 0 ? "AS num" : string.Empty)}"))}) vals) t
  ) numbered_list,
  (SELECT FLOOR((COUNT(*) + 1) / 2) AS floor_median, CEIL((COUNT(*) + 1) / 2) AS ceil_median
   FROM (SELECT DISTINCT num FROM ({string.Join(" UNION ", inputs.Select((x, i) => $"SELECT {x} {(i == 0 ? "AS num" : string.Empty)}"))}) vals) t) stats
  WHERE numbered_list.row_num IN (stats.floor_median, stats.ceil_median)
) subquery
";
                case SqlType.SQLite:
                    return $@"
SELECT AVG(val)
FROM 
(
    SELECT val
    FROM 
    (
        SELECT num AS val, ROW_NUMBER() OVER (ORDER BY num ASC) AS row_num 
        FROM (SELECT DISTINCT num FROM ({string.Join(" UNION ", inputs.Select((x, i) => $"SELECT {x} {(i == 0 ? "AS num" : string.Empty)}"))}) vals) t
    ) t1
    WHERE t1.row_num IN ((SELECT COUNT(*) FROM (SELECT DISTINCT num FROM ({string.Join(" UNION ", inputs.Select((x, i) => $"SELECT {x} {(i == 0 ? "AS num" : string.Empty)}"))}) vals) t) / 2 + 1, (SELECT COUNT(*) FROM (SELECT DISTINCT num FROM ({string.Join(" UNION ", inputs.Select((x, i) => $"SELECT {x} {(i == 0 ? "AS num" : string.Empty)}"))}) vals) t) / 2 + ((SELECT COUNT(*) FROM (SELECT DISTINCT num FROM ({string.Join(" UNION ", inputs.Select((x, i) => $"SELECT {x} {(i == 0 ? "AS num" : string.Empty)}"))}) vals) t) % 2))
) t2
";
                default:
                    return null;
            }
        }
    }
}
