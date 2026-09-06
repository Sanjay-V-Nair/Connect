using ReferenceDeletion.Models;

namespace ReferenceDeletion.Interfaces
{
    /// <summary>
    /// Public entry point for all consumers (delete tool, future "Find References",
    /// "Safe Move", dependency graph, etc.). Consumers must never scan the project
    /// themselves; they only query this database.
    /// </summary>
    public interface IReferenceDatabase
    {
        /// <summary>True once the database has a valid index ready to serve queries.</summary>
        bool IsReady { get; }

        /// <summary>
        /// Ensures the database is ready to serve queries: loads the persisted cache,
        /// or performs a one-time full build if no cache exists.
        /// </summary>
        void EnsureBuilt();

        /// <summary>Returns all assets that reference the given asset. O(1) index lookup, no scanning.</summary>
        ReferenceResult FindReferences(string guid);

        /// <summary>Returns the GUIDs of all assets that the given asset directly references.</summary>
        System.Collections.Generic.IReadOnlyCollection<string> GetForwardReferences(string guid);

        /// <summary>Forces a full rebuild, discarding any existing cache. Intended for a manual "Rebuild Index" menu action.</summary>
        void Rebuild();
    }
}
