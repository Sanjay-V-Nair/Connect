using ReferenceDeletion.Core;
using UnityEditor;

namespace ReferenceDeletion.Services
{
    /// <summary>
    /// Thin Unity hook: listens for asset import/delete/move events and forwards them
    /// to <see cref="IndexUpdateService"/>. Contains no indexing logic itself, keeping
    /// the AssetPostprocessor boundary and the update policy cleanly separated.
    /// </summary>
    public sealed class AssetChangeListener : AssetPostprocessor
    {
        private static IndexUpdateService _updateService;

        /// <summary>Wires this static hook to a service instance. Called once during editor initialization.</summary>
        internal static void Initialize(IndexUpdateService updateService)
        {
            _updateService = updateService;
        }

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            _updateService?.HandleChanges(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
        }
    }

    /// <summary>
    /// Editor bootstrap that wires <see cref="AssetChangeListener"/> to the shared
    /// database on domain load, ensuring incremental updates flow automatically
    /// without requiring any manual setup by the user.
    /// </summary>
    [InitializeOnLoad]
    internal static class AssetChangeListenerBootstrap
    {
        static AssetChangeListenerBootstrap()
        {
            IndexUpdateService updateService = new IndexUpdateService(AssetReferenceDatabase.Instance);
            AssetChangeListener.Initialize(updateService);
        }
    }
}
