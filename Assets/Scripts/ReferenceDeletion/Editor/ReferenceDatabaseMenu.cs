using ReferenceDeletion.Core;
using UnityEditor;
using UnityEngine;

namespace ReferenceDeletion.Editor
{
    /// <summary>
    /// Manual maintenance actions for the reference database, useful when a project
    /// is imported fresh, after major source-control operations, or if the cache is
    /// ever suspected to be stale.
    /// </summary>
    internal static class ReferenceDatabaseMenu
    {
        [MenuItem("Tools/Reference Deletion/Rebuild Index")]
        private static void RebuildIndex()
        {
            AssetReferenceDatabase.Instance.Rebuild();
            Debug.Log("[ReferenceDeletion] Reference index rebuilt.");
        }

        [MenuItem("Tools/Reference Deletion/Log Index Stats")]
        private static void LogStats()
        {
            AssetReferenceDatabase database = AssetReferenceDatabase.Instance;
            database.EnsureBuilt();
            Debug.Log($"[ReferenceDeletion] Database ready: {database.IsReady}");
        }
    }
}
