using System.Runtime.CompilerServices;

namespace FastBIRe.Wrapping
{
    public static class DateTimeToStringHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToFullString(DateTime dt, ref Span<char> buffer)
        {
            ToFullString(dt, ref buffer, '-', ':');
        }
        public unsafe static void ToYearTimeString(DateTime dt, ref Span<char> buffer)
        {
            var yearStr = dt.Year.ToString();
#if NETSTANDARD2_0
            dt.Year.ToString().AsSpan().CopyTo(buffer);
#else
            dt.Year.ToString().CopyTo(buffer);
#endif
        }
        public unsafe static void ToTimeString(DateTime dt, ref Span<char> buffer, char mergeTime)
        {
            var hour = dt.Hour;
            var minute = dt.Minute;
            var second = dt.Second;
            if (hour <= 9)
            {
                buffer[0] = '0';
                buffer[1] = (char)('0' + hour);
            }
            else
            {
                var q = hour / 10;
                buffer[0] = (char)('0' + q);
                buffer[1] = (char)('0' + hour - (q * 10));
            }
            buffer[2] = mergeTime;
            if (minute <= 9)
            {
                buffer[3] = '0';
                buffer[4] = (char)('0' + minute);
            }
            else
            {
                var q = minute / 10;
                buffer[3] = (char)('0' + q);
                buffer[4] = (char)('0' + minute - (q * 10));
            }
            buffer[5] = mergeTime;
            if (second <= 9)
            {
                buffer[6] = '0';
                buffer[7] = (char)('0' + second);
            }
            else
            {
                var q = second / 10;
                buffer[6] = (char)('0' + q);
                buffer[7] = (char)('0' + second - (q * 10));
            }
        }
        public unsafe static void ToDateString(DateTime dt, ref Span<char> buffer, char mergeDate)
        {
            var month = dt.Month;
            var day = dt.Day;
            var year = dt.Year;
            for (int i = 0; i < 4; i++)
            {
#if NETSTANDARD2_0
                year = Math.DivRem(year, 10, out var Remainder);
#else
                (year, int Remainder) = Math.DivRem(year, 10);
#endif
                buffer[3 - i] = (char)('0' + Remainder);
            }
            buffer[4] = mergeDate;
            if (month <= 9)
            {
                buffer[5] = '0';
                buffer[6] = (char)('0' + month);
            }
            else
            {
                var q = month / 10;
                buffer[5] = (char)('0' + q);
                buffer[6] = (char)('0' + month - (q * 10));
            }
            buffer[7] = mergeDate;
            if (day <= 9)
            {
                buffer[8] = '0';
                buffer[9] = (char)('0' + day);
            }
            else
            {
                var q = day / 10;
                buffer[8] = (char)('0' + q);
                buffer[9] = (char)('0' + day - (q * 10));
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void ToFullString(DateTime dt, ref Span<char> buffer, char mergeDate, char mergeTime)
        {
            ToDateString(dt, ref buffer, mergeDate);
            buffer[10] = ' ';
            var tsp = buffer.Slice(11);
            ToTimeString(dt, ref tsp, mergeTime);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static string ToFullString(DateTime dt, char mergeDate, char mergeTime)
        {
            Span<char> buffer = stackalloc char[19];
            ToFullString(dt, ref buffer, mergeDate, mergeTime);
            return buffer.ToString();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static string ToTimeString(DateTime dt, char mergeTime)
        {
            Span<char> buffer = stackalloc char[8];
            ToTimeString(dt, ref buffer, mergeTime);
            return buffer.ToString();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static string ToDateString(DateTime dt, char mergeDate)
        {
            Span<char> buffer = stackalloc char[10];
            ToDateString(dt, ref buffer, mergeDate);
            return buffer.ToString();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static string ToFullString(DateTime dt)
        {
            return ToFullString(dt, '-', ':');
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static string ToTimeString(DateTime dt)
        {
            Span<char> buffer = stackalloc char[8];
            ToTimeString(dt, ref buffer, ':');
            return buffer.ToString();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static string ToDateString(DateTime dt)
        {
            Span<char> buffer = stackalloc char[10];
            ToDateString(dt, ref buffer, '-');
            return buffer.ToString();
        }
    }

}
