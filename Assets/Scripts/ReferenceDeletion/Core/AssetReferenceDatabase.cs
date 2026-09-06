using System.Collections.Generic;
using ReferenceDeletion.Interfaces;
using ReferenceDeletion.Models;
using ReferenceDeletion.Persistence;
using ReferenceDeletion.Scanners;
using ReferenceDeletion.Utils;

namespace ReferenceDeletion.Core
{
    /// <summary>
    /// The single, shared reference database for the project. This is the only class
    /// in the system intended to exist as shared/global state; every other component
    /// receives its dependencies through its constructor.
    /// <para>
    /// Consumers (delete tool, "Find References", future dependency-graph tools, etc.)
    /// depend only on <see cref="IReferenceDatabase"/> and never touch the indexer or
    /// scanners directly.
    /// </para>
    /// </summary>
    public sealed class AssetReferenceDatabase : IReferenceDatabase
    {
        private static AssetReferenceDatabase _instance;

        /// <summary>Lazily-created, process-wide instance wired with the default production dependencies.</summary>
        public static AssetReferenceDatabase Instance => _instance ??= AssetReferenceDatabaseFactory.CreateDefault();

        private readonly Dictionary<string, AssetMetadata> _metadata;
        private readonly ForwardReferenceIndex _forwardIndex;
        private readonly ReverseReferenceIndex _reverseIndex;

        private readonly IAssetIndexer _indexer;
        private readonly ICacheStorage _cacheStorage;
        private readonly CacheSerializer _cacheSerializer;
        private readonly ILogger _logger;

        public bool IsReady { get; private set; }

        /// <summary>Exposed for <see cref="Services.IndexUpdateService"/>, which needs to trigger incremental updates and re-saves.</summary>
        internal IAssetIndexer Indexer => _indexer;

        public AssetReferenceDatabase(
            Dictionary<string, AssetMetadata> metadata,
            ForwardReferenceIndex forwardIndex,
            ReverseReferenceIndex reverseIndex,
            IAssetIndexer indexer,
            ICacheStorage cacheStorage,
            CacheSerializer cacheSerializer,
            ILogger logger)
        {
            _metadata = metadata;
            _forwardIndex = forwardIndex;
            _reverseIndex = reverseIndex;
            _indexer = indexer;
            _cacheStorage = cacheStorage;
            _cacheSerializer = cacheSerializer;
            _logger = logger;
        }

        public void EnsureBuilt()
        {
            if (IsReady)
            {
                return;
            }

            if (_cacheStorage.TryLoad(out CacheData cached))
            {
                _cacheSerializer.ApplyToLiveState(cached, _metadata, _forwardIndex, _reverseIndex);
                _logger.Log($"Loaded reference cache: {_metadata.Count} assets.");
                IsReady = true;
                return;
            }

            using (ProgressScope progress = new ProgressScope("Building Asset Reference Index"))
            {
                _indexer.BuildFullIndex(
                    onProgress: (current, total) => progress.Update($"{current}/{total} assets", total > 0 ? (float)current / total : 0f),
                    isCancelled: () => progress.WasCancelled);
            }

            SaveCache();
            IsReady = true;
        }

        public ReferenceResult FindReferences(string guid)
        {
            EnsureBuilt();

            string targetPath = _metadata.TryGetValue(guid, out AssetMetadata targetMeta) ? targetMeta.Path : AssetUtility.GuidToPath(guid);

            IReadOnlyCollection<string> referencingGuids = _reverseIndex.GetReferencingGuids(guid);
            List<AssetReferenceInfo> infos = new List<AssetReferenceInfo>(referencingGuids.Count);

            foreach (string refGuid in referencingGuids)
            {
                if (_metadata.TryGetValue(refGuid, out AssetMetadata meta))
                {
                    infos.Add(new AssetReferenceInfo(meta.Guid, meta.Path, meta.Name, meta.AssetTypeName));
                }
            }

            return new ReferenceResult(guid, targetPath, infos);
        }

        public IReadOnlyCollection<string> GetForwardReferences(string guid)
        {
            EnsureBuilt();
            return _forwardIndex.Get(guid);
        }

        public void Rebuild()
        {
            IsReady = false;
            _cacheStorage.Delete();
            EnsureBuilt();
        }

        /// <summary>Persists the current in-memory state. Called after initial build and after incremental updates.</summary>
        public void SaveCache()
        {
            CacheData data = _cacheSerializer.ToCacheData(_metadata, _forwardIndex);
            _cacheStorage.Save(data);
        }
    }

    /// <summary>
    /// Composition root for the default, production-wired <see cref="AssetReferenceDatabase"/>.
    /// Kept separate from the database class itself so the database stays free of
    /// construction/wiring concerns (single responsibility).
    /// </summary>
    internal static class AssetReferenceDatabaseFactory
    {
        public static AssetReferenceDatabase CreateDefault()
        {
            ILogger logger = new Logger();

            AssetReferenceScanner scanner = new AssetReferenceScanner(new IReferenceScanner[]
            {
                new YamlScanner(logger),
                new SerializedObjectScanner(logger),
                new DependencyScanner(logger),
            });

            Dictionary<string, AssetMetadata> metadata = new Dictionary<string, AssetMetadata>();
            ForwardReferenceIndex forwardIndex = new ForwardReferenceIndex();
            ReverseReferenceIndex reverseIndex = new ReverseReferenceIndex();

            IAssetIndexer indexer = new AssetIndexer(metadata, forwardIndex, reverseIndex, scanner, logger);
            ICacheStorage cacheStorage = new CacheStorage(logger);
            CacheSerializer cacheSerializer = new CacheSerializer();

            return new AssetReferenceDatabase(metadata, forwardIndex, reverseIndex, indexer, cacheStorage, cacheSerializer, logger);
        }
    }
}
