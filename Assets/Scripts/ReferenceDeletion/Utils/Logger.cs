using ReferenceDeletion.Interfaces;
using UnityEngine;
using ILogger = ReferenceDeletion.Interfaces.ILogger;

namespace ReferenceDeletion.Utils
{
    /// <summary>
    /// Default <see cref="ILogger"/> implementation. Prefixes all messages and
    /// gates verbose output behind a toggle so large projects don't spam the console.
    /// </summary>
    public sealed class Logger : ILogger
    {
        private const string Prefix = "[ReferenceDeletion] ";

        /// <summary>When false, <see cref="LogVerbose"/> calls are no-ops.</summary>
        public bool VerboseEnabled { get; set; }

        public Logger(bool verboseEnabled = false)
        {
            VerboseEnabled = verboseEnabled;
        }

        public void LogVerbose(string message)
        {
            if (VerboseEnabled)
            {
                Debug.Log(Prefix + message);
            }
        }

        public void Log(string message)
        {
            Debug.Log(Prefix + message);
        }

        public void LogWarning(string message)
        {
            Debug.LogWarning(Prefix + message);
        }

        public void LogError(string message)
        {
            Debug.LogError(Prefix + message);
        }
    }
}
