using UnityEditor;

namespace ReferenceDeletion.Editor
{
    /// <summary>
    /// Displays the simple "no references found, delete?" confirmation dialog.
    /// A dedicated static class rather than inline dialog calls, so the UI text and
    /// behavior can be unit-referenced/tested and reused by future tools.
    /// </summary>
    public static class DeleteConfirmationWindow
    {
        /// <summary>Shows the confirmation dialog and returns true if the user chose to delete.</summary>
        public static bool ShowNoReferencesDialog(string assetName)
        {
            return EditorUtility.DisplayDialog(
                "No References Found",
                $"No references found.\n\nDelete \"{assetName}\"?",
                "Delete",
                "Cancel");
        }
    }
}
