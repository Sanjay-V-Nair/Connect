using ReferenceDeletion.Core;
using ReferenceDeletion.Models;
using ReferenceDeletion.Utils;
using UnityEditor;
using UnityEngine;

namespace ReferenceDeletion.Editor
{
    /// <summary>
    /// Entry point for "Delete With Reference Check". Queries the shared
    /// <see cref="AssetReferenceDatabase"/> (never scans the project itself) and
    /// routes to the appropriate UI: a simple confirmation when there are no
    /// references, or <see cref="ReferenceResultWindow"/> when references exist.
    /// </summary>
    public static class DeleteWithReferenceCommand
    {
        private const string MenuPath = "Assets/Delete With Reference Check";

        [MenuItem(MenuPath, false, 20)]
        private static void Execute()
        {
            Object[] selected = Selection.objects;
            if (selected == null || selected.Length == 0)
            {
                return;
            }

            // Process one asset at a time so each gets its own clear confirmation/result UI.
            foreach (Object obj in selected)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                ExecuteForAsset(path);
            }
        }

        [MenuItem(MenuPath, true)]
        private static bool Validate()
        {
            return Selection.objects != null && Selection.objects.Length > 0;
        }

        private static void ExecuteForAsset(string assetPath)
        {
            string guid = AssetUtility.PathToGuid(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogWarning($"[ReferenceDeletion] Could not resolve GUID for '{assetPath}'.");
                return;
            }

            ReferenceResult result = AssetReferenceDatabase.Instance.FindReferences(guid);
            string assetName = System.IO.Path.GetFileName(assetPath);

            if (!result.HasReferences)
            {
                if (DeleteConfirmationWindow.ShowNoReferencesDialog(assetName))
                {
                    DeleteAsset(assetPath, guid);
                }
                return;
            }

            ReferenceResultWindow.Show(result, () => DeleteAsset(assetPath, guid));
        }

        private static void DeleteAsset(string assetPath, string guid)
        {
            bool deleted = AssetDatabase.DeleteAsset(assetPath);
            if (deleted)
            {
                // AssetChangeListener also fires for this via OnPostprocessAllAssets,
                // but removing eagerly keeps the in-memory index correct immediately
                // even if the postprocessor callback is delayed.
                AssetReferenceDatabase.Instance.Indexer.RemoveAsset(guid);
                AssetReferenceDatabase.Instance.SaveCache();
            }
            else
            {
                Debug.LogError($"[ReferenceDeletion] Failed to delete '{assetPath}'.");
            }
        }
    }
}
