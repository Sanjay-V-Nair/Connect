using System;
using System.Collections.Generic;
using ReferenceDeletion.Interfaces;
using ReferenceDeletion.Models;
using ReferenceDeletion.Utils;
using UnityEditor;

namespace ReferenceDeletion.Core
{
    /// <summary>
    /// Builds the reference index from scratch (only when no valid cache exists) and
    /// applies cheap incremental updates afterwards. This is the single class in the
    /// system permitted to enumerate every asset in the project; every other consumer
    /// only ever queries the resulting indexes.
    /// </summary>
    public sealed class AssetIndexer : IAssetIndexer
    {
        private readonly Dictionary<string, AssetMetadata> _metadata;
        private readonly ForwardReferenceIndex _forwardIndex;
        private readonly ReverseReferenceIndex _reverseIndex;
        private readonly AssetReferenceScanner _scanner;
        private readonly ILogger _logger;

        public bool IsBuilt { get; private set; }

        public AssetIndexer(
            Dictionary<string, AssetMetadata> metadata,
            ForwardReferenceIndex forwardIndex,
            ReverseReferenceIndex reverseIndex,
            AssetReferenceScanner scanner,
            ILogger logger)
        {
            _metadata = metadata;
            _forwardIndex = forwardIndex;
            _reverseIndex = reverseIndex;
            _scanner = scanner;
            _logger = logger;
        }

        public void BuildFullIndex(Action<int, int> onProgress = null, Func<bool> isCancelled = null)
        {
            DateTime start = DateTime.UtcNow;

            _metadata.Clear();
            _forwardIndex.Clear();
            _reverseIndex.Clear();

            string[] allGuids = AssetDatabase.FindAssets(string.Empty);
            int total = allGuids.Length;
            int skipped = 0;

            for (int i = 0; i < total; i++)
            {
                if (isCancelled != null && isCancelled())
                {
                    _logger.LogWarning($"Index build cancelled after {i}/{total} assets.");
                    break;
                }

                onProgress?.Invoke(i, total);

                string guid = allGuids[i];
                string path = AssetUtility.GuidToPath(guid);

                if (string.IsNullOrEmpty(path) || AssetUtility.IsFolder(path))
                {
                    continue;
                }

                if (!TryIndexAsset(guid, path))
                {
                    skipped++;
                }
            }

            IsBuilt = true;

            TimeSpan elapsed = DateTime.UtcNow - start;
            _logger.Log($"Full index build complete: {_metadata.Count} assets indexed, {skipped} skipped, {elapsed.TotalSeconds:F2}s.");
        }

        public void UpdateAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || AssetUtility.IsFolder(assetPath) || !AssetUtility.AssetExists(assetPath))
            {
                return;
            }

            string guid = AssetUtility.PathToGuid(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                return;
            }

            // Remove old outgoing links before rescanning so stale reverse-index entries don't linger.
            if (_forwardIndex.TryGetRaw(guid, out HashSet<string> previousRefs))
            {
                _reverseIndex.RemoveAllLinksFrom(guid, previousRefs);
            }

            TryIndexAsset(guid, assetPath);
        }

        public void RemoveAsset(string guidOrPath)
        {
            string guid = ResolveGuid(guidOrPath);
            if (string.IsNullOrEmpty(guid))
            {
                return;
            }

            if (_forwardIndex.TryGetRaw(guid, out HashSet<string> previousRefs))
            {
                _reverseIndex.RemoveAllLinksFrom(guid, previousRefs);
            }

            _forwardIndex.Remove(guid);
            _reverseIndex.RemoveTarget(guid);
            _metadata.Remove(guid);
        }

        public void MoveAsset(string oldPath, string newPath)
        {
            // The GUID is stable across moves/renames in Unity, so both indexes remain
            // valid keyed by GUID. Only the cached path/name metadata needs refreshing.
            string guid = AssetUtility.PathToGuid(newPath);
            if (string.IsNullOrEmpty(guid))
            {
                return;
            }

            if (_metadata.TryGetValue(guid, out AssetMetadata meta))
            {
                meta.Path = newPath;
                meta.Name = AssetUtility.GetAssetName(newPath);
            }
            else
            {
                // Wasn't tracked before (e.g. newly created then immediately moved); index it now.
                TryIndexAsset(guid, newPath);
            }
        }

        public void ApplyIncrementalChanges(IReadOnlyList<string> importedAssets, IReadOnlyList<string> deletedAssets,
            IReadOnlyList<string> movedAssets, IReadOnlyList<string> movedFromAssetPaths)
        {
            if (!IsBuilt)
            {
                return;
            }

            foreach (string deletedPath in deletedAssets)
            {
                RemoveAsset(deletedPath);
            }

            for (int i = 0; i < movedAssets.Count; i++)
            {
                string newPath = movedAssets[i];
                string oldPath = i < movedFromAssetPaths.Count ? movedFromAssetPaths[i] : null;
                MoveAsset(oldPath, newPath);
            }

            foreach (string importedPath in importedAssets)
            {
                UpdateAsset(importedPath);
            }

            _logger.LogVerbose($"Incremental update: {importedAssets.Count} imported, {deletedAssets.Count} deleted, {movedAssets.Count} moved.");
        }

        /// <summary>Scans and stores a single asset's metadata and forward references, wiring the reverse index. Returns false if skipped.</summary>
        private bool TryIndexAsset(string guid, string path)
        {
            try
            {
                long fileTs = AssetUtility.GetFileTimestampTicks(path);
                long metaTs = AssetUtility.GetMetaTimestampTicks(path);

                HashSet<string> references = _scanner.Scan(path) ?? new HashSet<string>();

                _metadata[guid] = new AssetMetadata(
                    guid, path, AssetUtility.GetAssetName(path), AssetUtility.GetAssetTypeName(path), fileTs, metaTs);

                _forwardIndex.Set(guid, references);

                foreach (string target in references)
                {
                    _reverseIndex.AddLink(target, guid);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Skipped asset '{path}' during indexing: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Resolves a GUID from either a raw GUID, a still-existing asset path, or a
        /// path whose asset has already been deleted (in which case <c>AssetDatabase</c>
        /// can no longer resolve it, so the cached metadata is searched instead).
        /// </summary>
        private string ResolveGuid(string guidOrPath)
        {
            if (string.IsNullOrEmpty(guidOrPath))
            {
                return null;
            }

            if (LooksLikeGuid(guidOrPath) && _metadata.ContainsKey(guidOrPath))
            {
                return guidOrPath;
            }

            string guid = AssetUtility.PathToGuid(guidOrPath);
            if (!string.IsNullOrEmpty(guid))
            {
                return guid;
            }

            foreach (KeyValuePair<string, AssetMetadata> kvp in _metadata)
            {
                if (kvp.Value.Path == guidOrPath)
                {
                    return kvp.Key;
                }
            }

            return LooksLikeGuid(guidOrPath) ? guidOrPath : null;
        }

        private static bool LooksLikeGuid(string value)
        {
            return value != null && value.Length == 32;
        }
    }
}
