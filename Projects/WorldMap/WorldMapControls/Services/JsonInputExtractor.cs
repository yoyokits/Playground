// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //
namespace WorldMapControls.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;

    /// <summary>
    /// Extracts a JSON object or array substring from a free-form text that may contain additional prose.
    /// Strategy: scan for balanced top-level object/array with bracket counting; return the longest valid JSON segment.
    /// </summary>
    public static class JsonInputExtractor
    {
        public static string? ExtractJson(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var text = raw.Trim();
            // Quick fast-path: direct parse success
            if (IsValidJson(text)) return text;

            // Collect candidate spans
            var candidates = new List<string>();
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '{' || c == '[')
                {
                    int end = FindBalancedEnd(text, i, c);
                    if (end > i)
                    {
                        var slice = text[i..(end + 1)];
                        if (IsValidJson(slice)) candidates.Add(slice);
                    }
                }
            }
            if (candidates.Count == 0) return null;
            // choose longest valid (likely the full intended structure)
            candidates.Sort((a,b)=> b.Length.CompareTo(a.Length));
            return candidates[0];
        }

        private static int FindBalancedEnd(string text, int start, char opener)
        {
            char closer = opener == '{' ? '}' : ']';
            int depth = 0;
            bool inString = false;
            for (int i = start; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '"')
                {
                    // Count preceding escapes
                    int bslashes = 0; int j = i - 1; while (j >= 0 && text[j] == '\\') { bslashes++; j--; }
                    if (bslashes % 2 == 0) inString = !inString;
                }
                if (inString) continue;
                if (c == opener) depth++;
                else if (c == closer)
                {
                    depth--;
                    if (depth == 0) return i;
                }
            }
            return -1;
        }

        private static bool IsValidJson(string input)
        {
            try
            {
                using var doc = JsonDocument.Parse(input);
                return true;
            }
            catch { return false; }
        }
    }
}
