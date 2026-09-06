using System;
using System.Collections.Generic;

namespace ReferenceDeletion.Utils
{
    /// <summary>
    /// Extracts Unity GUIDs from raw YAML asset text using manual, span-based
    /// scanning instead of regex. Unity YAML references look like:
    /// <c>fileID: 12345, guid: 3f2a9c1b7e6d4a4a8b0e1c2d3e4f5061, type: 3</c>.
    /// </summary>
    public static class GuidParser
    {
        private const string GuidToken = "guid: ";
        private const int GuidLength = 32;

        /// <summary>
        /// Scans <paramref name="text"/> for all "guid: &lt;32 hex chars&gt;" occurrences
        /// and adds the unique GUIDs (excluding <paramref name="selfGuid"/>) into <paramref name="result"/>.
        /// </summary>
        public static void ExtractGuids(ReadOnlySpan<char> text, HashSet<string> result, string selfGuid = null)
        {
            ReadOnlySpan<char> token = GuidToken.AsSpan();
            int offset = 0;

            while (true)
            {
                ReadOnlySpan<char> remaining = text.Slice(offset);
                int relativeIndex = remaining.IndexOf(token);
                if (relativeIndex < 0)
                {
                    break;
                }

                int guidStart = offset + relativeIndex + token.Length;

                if (guidStart + GuidLength > text.Length)
                {
                    break;
                }

                ReadOnlySpan<char> candidate = text.Slice(guidStart, GuidLength);
                if (IsValidGuid(candidate))
                {
                    // Only add if not immediately followed by another hex char
                    // (guards against malformed/truncated tokens).
                    int after = guidStart + GuidLength;
                    if (after >= text.Length || !IsHexChar(text[after]))
                    {
                        if (selfGuid == null || !candidate.SequenceEqual(selfGuid.AsSpan()))
                        {
                            result.Add(candidate.ToString());
                        }
                    }
                }

                offset = guidStart;
            }
        }

        private static bool IsValidGuid(ReadOnlySpan<char> candidate)
        {
            for (int i = 0; i < candidate.Length; i++)
            {
                if (!IsHexChar(candidate[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsHexChar(char c)
        {
            return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
        }
    }
}
