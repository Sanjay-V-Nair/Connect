namespace ReferenceDeletion.Interfaces
{
    /// <summary>
    /// Abstraction over logging so core logic never depends directly on
    /// <c>UnityEngine.Debug</c>. Makes unit testing possible outside Unity.
    /// </summary>
    public interface ILogger
    {
        /// <summary>Verbose/diagnostic messages, only emitted when verbose logging is enabled.</summary>
        void LogVerbose(string message);

        /// <summary>Normal informational messages (e.g. summary stats).</summary>
        void Log(string message);

        void LogWarning(string message);

        void LogError(string message);
    }
}
