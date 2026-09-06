using System.Collections.Generic;
using ReferenceDeletion.Models;

namespace ReferenceDeletion.Core
{
    /// <summary>
    /// Translates between the flat, disk-friendly <see cref="CacheData"/> DTO and the
    /// live <see cref="ForwardReferenceIndex"/> / <see cref="ReverseReferenceIndex"/> /
    /// metadata dictionary used for queries. Kept separate from <c>CacheStorage</c> so
    /// data-shape concerns don't mix with raw file I/O concerns.
    /// </summary>
    public sealed class CacheSerializer
    {
        /// <summary>Builds a <see cref="CacheData"/> snapshot from the live in-memory state.</summary>
        public CacheData ToCacheData(IReadOnlyDictionary<string, AssetMetadata> metadata, ForwardReferenceIndex forwardIndex)
        {
            CacheData data = new CacheData();

            foreach (KeyValuePair<string, AssetMetadata> kvp in metadata)
            {
                data.Assets.Add(kvp.Value);
            }

            foreach (KeyValuePair<string, HashSet<string>> kvp in forwardIndex.Snapshot())
            {
                data.ForwardIndex.Add(new CacheData.ForwardEntry(kvp.Key, kvp.Value));
            }

            return data;
        }

        /// <summary>
        /// Populates the live metadata dictionary and both indexes from a loaded
        /// <see cref="CacheData"/>. The reverse index is derived from the forward index.
        /// </summary>
        public void ApplyToLiveState(CacheData data, Dictionary<string, AssetMetadata> metadata,
            ForwardReferenceIndex forwardIndex, ReverseReferenceIndex reverseIndex)
        {
            metadata.Clear();
            forwardIndex.Clear();
            reverseIndex.Clear();

            foreach (AssetMetadata meta in data.Assets)
            {
                metadata[meta.Guid] = meta;
            }

            foreach (CacheData.ForwardEntry entry in data.ForwardIndex)
            {
                HashSet<string> refs = new HashSet<string>(entry.References);
                forwardIndex.Set(entry.Guid, refs);

                foreach (string target in refs)
                {
                    reverseIndex.AddLink(target, entry.Guid);
                }
            }
        }
    }
}
