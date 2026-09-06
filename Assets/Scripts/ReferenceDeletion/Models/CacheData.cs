using System;
using System.Collections.Generic;

namespace ReferenceDeletion.Models
{
    /// <summary>
    /// Flat, serialization-friendly representation of the reference database.
    /// This is the only type that touches disk; live lookups use
    /// <see cref="ReferenceDeletion.Core.ForwardReferenceIndex"/> and
    /// <see cref="ReferenceDeletion.Core.ReverseReferenceIndex"/> instead.
    /// </summary>
    [Serializable]
    public sealed class CacheData
    {
        /// <summary>Format version. Bump when the on-disk layout changes so old caches are discarded safely.</summary>
        public int Version = CurrentVersion;

        public const int CurrentVersion = 1;

        public List<AssetMetadata> Assets = new List<AssetMetadata>();
        public List<ForwardEntry> ForwardIndex = new List<ForwardEntry>();

        [Serializable]
        public sealed class ForwardEntry
        {
            public string Guid;
            public List<string> References = new List<string>();

            public ForwardEntry()
            {
            }

            public ForwardEntry(string guid, IEnumerable<string> references)
            {
                Guid = guid;
                References = new List<string>(references);
            }
        }
    }
}
