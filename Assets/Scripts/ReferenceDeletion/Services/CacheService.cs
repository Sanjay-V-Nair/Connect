using ReferenceDeletion.Core;

namespace ReferenceDeletion.Services
{
    /// <summary>
    /// User-facing facade over cache lifecycle operations (build, rebuild, clear),
    /// intended to back manual menu actions such as "Rebuild Reference Index".
    /// Editor menu items call into this rather than touching
    /// <see cref="AssetReferenceDatabase"/> directly, keeping menu wiring thin.
    /// </summary>
    public sealed class CacheService
    {
        private readonly AssetReferenceDatabase _database;

        public CacheService(AssetReferenceDatabase database)
        {
            _database = database;
        }

        /// <summary>Ensures the index exists, building it if this is the first use in the session.</summary>
        public void EnsureReady() => _database.EnsureBuilt();

        /// <summary>Discards the cache and performs a fresh full-project scan.</summary>
        public void Rebuild() => _database.Rebuild();
    }
}
