using System.Collections.Generic;
using ReferenceDeletion.Core;

namespace ReferenceDeletion.Services
{
    /// <summary>
    /// Applies incremental changes reported by <see cref="AssetChangeListener"/> to the
    /// shared <see cref="AssetReferenceDatabase"/> and persists the result. Kept as its
    /// own service (rather than folding into the listener) so the update policy —
    /// e.g. batching, save-on-every-change — can evolve independently of the raw
    /// AssetPostprocessor hook.
    /// </summary>
    public sealed class IndexUpdateService
    {
        private readonly AssetReferenceDatabase _database;

        public IndexUpdateService(AssetReferenceDatabase database)
        {
            _database = database;
        }

        public void HandleChanges(IReadOnlyList<string> importedAssets, IReadOnlyList<string> deletedAssets,
            IReadOnlyList<string> movedAssets, IReadOnlyList<string> movedFromAssetPaths)
        {
            if (!_database.IsReady)
            {
                // Nothing built yet (e.g. first domain load); a subsequent EnsureBuilt()
                // will perform a full, up-to-date scan, so there is nothing to patch.
                return;
            }

            bool hasChanges = importedAssets.Count > 0 || deletedAssets.Count > 0 || movedAssets.Count > 0;
            if (!hasChanges)
            {
                return;
            }

            _database.Indexer.ApplyIncrementalChanges(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            _database.SaveCache();
        }
    }
}
