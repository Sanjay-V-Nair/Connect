using System;
using System.Collections.Generic;
using System.IO;
using ReferenceDeletion.Interfaces;
using ReferenceDeletion.Utils;

namespace ReferenceDeletion.Scanners
{
    /// <summary>
    /// Scans Unity's text/YAML-serialized assets (prefabs, scenes, materials,
    /// ScriptableObjects, controllers, timelines, etc.) for <c>guid:</c> tokens.
    /// Reads the file directly and parses it manually via <see cref="GuidParser"/>
    /// instead of regex, for speed and near-zero allocation.
    /// </summary>
    public sealed class YamlScanner : IReferenceScanner
    {
        private readonly ILogger _logger;

        public YamlScanner(ILogger logger)
        {
            _logger = logger;
        }

        public bool CanScan(string assetPath)
        {
            return AssetUtility.IsLikelyYamlAsset(assetPath);
        }

        public HashSet<string> Scan(string assetPath)
        {
            HashSet<string> result = new HashSet<string>();
            string absolutePath = AssetUtility.ToAbsolutePath(assetPath);

            if (!File.Exists(absolutePath))
            {
                return result;
            }

            try
            {
                string selfGuid = AssetUtility.PathToGuid(assetPath);
                string text = File.ReadAllText(absolutePath);
                GuidParser.ExtractGuids(text.AsSpan(), result, selfGuid);
            }
            catch (Exception ex)
            {
                // Corrupt or unreadable asset: skip it and keep indexing everything else.
                _logger?.LogWarning($"YamlScanner: skipped '{assetPath}' due to read error: {ex.Message}");
                result.Clear();
            }

            return result;
        }
    }
}
