using System.Collections.Generic;
using System.Threading;

namespace ReferenceDeletion.Core
{
    /// <summary>
    /// Maps an asset GUID to the set of asset GUIDs it directly references.
    /// Scanning may occur on worker threads, so all mutation is guarded by a lock;
    /// reads use a reader-writer lock for concurrency.
    /// </summary>
    public sealed class ForwardReferenceIndex
    {
        private readonly Dictionary<string, HashSet<string>> _map = new Dictionary<string, HashSet<string>>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try { return _map.Count; }
                finally { _lock.ExitReadLock(); }
            }
        }

        /// <summary>Replaces the full reference set for an asset (used after a rescan).</summary>
        public void Set(string guid, HashSet<string> references)
        {
            _lock.EnterWriteLock();
            try
            {
                _map[guid] = references;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public HashSet<string> Get(string guid)
        {
            _lock.EnterReadLock();
            try
            {
                return _map.TryGetValue(guid, out HashSet<string> refs) ? new HashSet<string>(refs) : new HashSet<string>();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool TryGetRaw(string guid, out HashSet<string> references)
        {
            _lock.EnterReadLock();
            try
            {
                return _map.TryGetValue(guid, out references);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Remove(string guid)
        {
            _lock.EnterWriteLock();
            try
            {
                _map.Remove(guid);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _map.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>Snapshot enumeration for serialization. Safe to call from the main thread only.</summary>
        public IEnumerable<KeyValuePair<string, HashSet<string>>> Snapshot()
        {
            _lock.EnterReadLock();
            try
            {
                List<KeyValuePair<string, HashSet<string>>> copy = new List<KeyValuePair<string, HashSet<string>>>(_map.Count);
                foreach (KeyValuePair<string, HashSet<string>> kvp in _map)
                {
                    copy.Add(new KeyValuePair<string, HashSet<string>>(kvp.Key, new HashSet<string>(kvp.Value)));
                }
                return copy;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}
