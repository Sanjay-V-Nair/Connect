using System.Collections.Generic;
using System.Threading;

namespace ReferenceDeletion.Core
{
    /// <summary>
    /// Maps an asset GUID to the set of GUIDs that reference it. This is the ONLY
    /// index the delete tool queries — lookups are O(1) average case and never
    /// trigger a scan.
    /// </summary>
    public sealed class ReverseReferenceIndex
    {
        private readonly Dictionary<string, HashSet<string>> _map = new Dictionary<string, HashSet<string>>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public void AddLink(string referencedGuid, string referencingGuid)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!_map.TryGetValue(referencedGuid, out HashSet<string> set))
                {
                    set = new HashSet<string>();
                    _map[referencedGuid] = set;
                }
                set.Add(referencingGuid);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void RemoveLink(string referencedGuid, string referencingGuid)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_map.TryGetValue(referencedGuid, out HashSet<string> set))
                {
                    set.Remove(referencingGuid);
                    if (set.Count == 0)
                    {
                        _map.Remove(referencedGuid);
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>Removes every link where <paramref name="referencingGuid"/> is the source (used before rescanning it).</summary>
        public void RemoveAllLinksFrom(string referencingGuid, IEnumerable<string> previousTargets)
        {
            _lock.EnterWriteLock();
            try
            {
                foreach (string target in previousTargets)
                {
                    if (_map.TryGetValue(target, out HashSet<string> set))
                    {
                        set.Remove(referencingGuid);
                        if (set.Count == 0)
                        {
                            _map.Remove(target);
                        }
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>Removes an asset entirely as a possible target (used when the asset itself is deleted).</summary>
        public void RemoveTarget(string guid)
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

        /// <summary>O(1) average-case lookup: who references this GUID.</summary>
        public IReadOnlyCollection<string> GetReferencingGuids(string guid)
        {
            _lock.EnterReadLock();
            try
            {
                return _map.TryGetValue(guid, out HashSet<string> set) ? new HashSet<string>(set) : System.Array.Empty<string>();
            }
            finally
            {
                _lock.ExitReadLock();
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
