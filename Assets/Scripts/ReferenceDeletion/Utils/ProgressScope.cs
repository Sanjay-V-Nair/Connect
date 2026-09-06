using System;
using UnityEditor;

namespace ReferenceDeletion.Utils
{
    /// <summary>
    /// RAII-style wrapper around <see cref="EditorUtility.DisplayProgressBar"/> /
    /// <see cref="EditorUtility.ClearProgressBar"/> that also exposes a cancellation check.
    /// </summary>
    public sealed class ProgressScope : IDisposable
    {
        private readonly string _title;
        private bool _cancelled;

        public bool WasCancelled => _cancelled;

        public ProgressScope(string title)
        {
            _title = title;
        }

        /// <summary>
        /// Updates the progress bar and returns true if the user requested cancellation
        /// (e.g. pressed Esc or clicked Cancel).
        /// </summary>
        public bool Update(string info, float progress01)
        {
            if (EditorUtility.DisplayCancelableProgressBar(_title, info, progress01))
            {
                _cancelled = true;
            }
            return _cancelled;
        }

        public void Dispose()
        {
            EditorUtility.ClearProgressBar();
        }
    }
}
