using System;
using System.Collections.Generic;

namespace ReferenceDeletion.Interfaces
{
    /// <summary>
    /// Builds the reference index from scratch and applies incremental updates
    /// for individual assets. This is the only component allowed to perform a
    /// full project scan, and only when no valid cache exists.
    /// </summary>
    public interface IAssetIndexer
    {
        /// <summary>True once an initial index is present in memory (loaded from cache or freshly built).</summary>
        bool IsBuilt { get; }

        /// <summary>
        /// Performs a full project scan and builds the forward/reverse indexes from scratch.
        /// Only called when no valid persisted cache exists.
        /// </summary>
        /// <param name="onProgress">Optional progress callback: (current, total).</param>
        /// <param name="isCancelled">Optional cancellation check.</param>
        void BuildFullIndex(Action<int, int> onProgress = null, Func<bool> isCancelled = null);

        /// <summary>Re-scans a single asset, removing its stale references and inserting fresh ones.</summary>
        void UpdateAsset(string assetPath);

        /// <summary>Removes an asset entirely from both indexes and the metadata cache.</summary>
        void RemoveAsset(string guidOrPath);

        /// <summary>Handles a path rename/move without needing to rescan referenced content.</summary>
        void MoveAsset(string oldPath, string newPath);

        /// <summary>Applies a batch of imported/deleted/moved paths, as delivered by <c>OnPostprocessAllAssets</c>.</summary>
        void ApplyIncrementalChanges(IReadOnlyList<string> importedAssets, IReadOnlyList<string> deletedAssets,
            IReadOnlyList<string> movedAssets, IReadOnlyList<string> movedFromAssetPaths);
    }
}
