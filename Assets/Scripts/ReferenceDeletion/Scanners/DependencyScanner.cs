using System;
using System.Collections.Generic;
using ReferenceDeletion.Interfaces;
using ReferenceDeletion.Utils;
using UnityEditor;

namespace ReferenceDeletion.Scanners
{
    /// <summary>
    /// Fallback scanner for binary/imported assets that Unity already understands
    /// (textures, models, audio clips, etc.) where direct YAML parsing isn't applicable.
    /// Uses <see cref="AssetDatabase.GetDependencies(string, bool)"/> with
    /// <c>recursive: false</c> so it only returns direct references, matching the
    /// forward-index semantics used everywhere else.
    /// </summary>
    public sealed class DependencyScanner : IReferenceScanner
    {
        private readonly ILogger _logger;

        public DependencyScanner(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Acts as the catch-all: returns true for any non-folder asset, so it should
        /// be registered last in the scanner pipeline (after more specific scanners).
        /// </summary>
        public bool CanScan(string assetPath)
        {
            return !AssetUtility.IsFolder(assetPath);
        }

        public HashSet<string> Scan(string assetPath)
        {
            HashSet<string> result = new HashSet<string>();

            try
            {
                string selfGuid = AssetUtility.PathToGuid(assetPath);
                string[] dependencies = AssetDatabase.GetDependencies(assetPath, false);

                foreach (string depPath in dependencies)
                {
                    if (depPath == assetPath)
                    {
                        continue;
                    }

                    string guid = AssetUtility.PathToGuid(depPath);
                    if (!string.IsNullOrEmpty(guid) && guid != selfGuid)
                    {
                        result.Add(guid);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"DependencyScanner: skipped '{assetPath}' due to error: {ex.Message}");
                result.Clear();
            }

            return result;
        }
    }
}
