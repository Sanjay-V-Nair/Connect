using ReferenceDeletion.Models;

namespace ReferenceDeletion.Interfaces
{
    /// <summary>
    /// Raw persistence of <see cref="CacheData"/> to and from disk (typically under
    /// <c>Library/</c>, never <c>Assets/</c>, so it is never version controlled).
    /// Implementations must be resilient to corrupt or missing files.
    /// </summary>
    public interface ICacheStorage
    {
        /// <summary>Attempts to load previously persisted cache data. Returns false if absent or corrupt.</summary>
        bool TryLoad(out CacheData data);

        /// <summary>Persists cache data, overwriting any existing file.</summary>
        void Save(CacheData data);

        /// <summary>Deletes any persisted cache file, forcing a full rebuild on next load.</summary>
        void Delete();
    }
}
