using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// ECMAScript identifier code points, independent of the host runtime's Unicode version.
    /// Astral characters are emitted as UTF-16 surrogate-pair regex alternatives because .NET
    /// character classes match UTF-16 code units rather than arbitrary Unicode code points.
    /// </summary>
    internal static class JavaScriptUnicode
    {
        internal static readonly string StartPattern = BuildPattern(JavaScriptUnicodeData.StartRanges);
        internal static readonly string PartPattern = BuildPattern(JavaScriptUnicodeData.PartRanges);

        internal static bool IsStart(int codePoint) => Contains(JavaScriptUnicodeData.StartRanges, codePoint);
        internal static bool IsPart(int codePoint) => Contains(JavaScriptUnicodeData.PartRanges, codePoint);

        private static bool Contains(int[] ranges, int value)
        {
            int low = 0;
            int high = ranges.Length / 2 - 1;
            while (low <= high)
            {
                int middle = low + (high - low) / 2;
                if (value < ranges[middle * 2]) high = middle - 1;
                else if (value > ranges[middle * 2 + 1]) low = middle + 1;
                else return true;
            }
            return false;
        }

        private static string BuildPattern(int[] ranges)
        {
            var bmp = new StringBuilder("[");
            var astral = new SortedDictionary<int, StringBuilder>();
            for (int index = 0; index < ranges.Length; index += 2)
            {
                int start = ranges[index];
                int end = ranges[index + 1];
                if (start <= 0xFFFF) AppendRange(bmp, start, Math.Min(end, 0xFFFF));
                for (int value = Math.Max(start, 0x10000); value <= end;)
                {
                    int high = 0xD800 + ((value - 0x10000) >> 10);
                    int last = Math.Min(end, value | 0x3FF);
                    if (!astral.TryGetValue(high, out var lowRanges))
                        astral.Add(high, lowRanges = new StringBuilder("["));
                    AppendRange(lowRanges, 0xDC00 + ((value - 0x10000) & 0x3FF),
                        0xDC00 + ((last - 0x10000) & 0x3FF));
                    value = last + 1;
                }
            }
            bmp.Append(']');
            var pattern = new StringBuilder("(?:").Append(bmp);
            // Adjacent high surrogates with the same low-surrogate ranges share an alternative.
            // This keeps large CJK planes compact instead of emitting one alternative per page.
            int firstHigh = -1;
            int lastHigh = -1;
            string previous = null;
            foreach (var pair in astral)
            {
                string current = pair.Value.ToString() + "]";
                if (pair.Key == lastHigh + 1 && current == previous) lastHigh = pair.Key;
                else
                {
                    if (previous != null) AppendSurrogates(pattern, firstHigh, lastHigh, previous);
                    firstHigh = lastHigh = pair.Key;
                    previous = current;
                }
            }
            if (previous != null) AppendSurrogates(pattern, firstHigh, lastHigh, previous);
            return pattern.Append(')').ToString();
        }

        private static void AppendSurrogates(StringBuilder target, int first, int last, string lows)
        {
            target.Append('|').Append('[');
            AppendRange(target, first, last);
            target.Append(']').Append(lows);
        }

        private static void AppendRange(StringBuilder target, int first, int last)
        {
            target.Append("\\u").Append(first.ToString("X4", CultureInfo.InvariantCulture));
            if (first != last)
                target.Append("-\\u").Append(last.ToString("X4", CultureInfo.InvariantCulture));
        }
    }
}
