using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ReferenceDeletion.Utils
{
    /// <summary>
    /// Shared, stateless helpers for working with Unity assets. Kept separate from
    /// scanners/indexer so those classes stay focused on their single responsibility.
    /// </summary>
    public static class AssetUtility
    {
        /// <summary>
        /// Extensions the system actively understands for scanning purposes. Assets outside
        /// this set are still tracked (for reverse-lookup completeness) but scanned via the
        /// generic <see cref="ReferenceDeletion.Scanners.DependencyScanner"/> fallback only.
        /// </summary>
        private static readonly HashSet<string> TextYamlExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".prefab", ".unity", ".mat", ".asset", ".controller", ".playable",
            ".anim", ".shadergraph", ".shadersubgraph", ".vfx", ".vfxoperator", ".vfxblock",
            ".mask", ".physicMaterial", ".physicsMaterial2D", ".guiskin", ".fontsettings",
            ".preset", ".signal", ".signalasset", ".mixer", ".spriteatlas", ".terrainlayer"
        };

        public static bool IsLikelyYamlAsset(string assetPath)
        {
            string ext = Path.GetExtension(assetPath);
            return TextYamlExtensions.Contains(ext);
        }

        public static string PathToGuid(string assetPath)
        {
            return AssetDatabase.AssetPathToGUID(assetPath);
        }

        public static string GuidToPath(string guid)
        {
            return AssetDatabase.GUIDToAssetPath(guid);
        }

        public static bool AssetExists(string assetPath)
        {
            return !string.IsNullOrEmpty(assetPath) && File.Exists(ToAbsolutePath(assetPath));
        }

        public static string ToAbsolutePath(string assetPath)
        {
            // Assets/... -> <ProjectRoot>/Assets/...
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, assetPath);
        }

        public static long GetFileTimestampTicks(string assetPath)
        {
            string absolute = ToAbsolutePath(assetPath);
            return File.Exists(absolute) ? File.GetLastWriteTimeUtc(absolute).Ticks : 0L;
        }

        public static long GetMetaTimestampTicks(string assetPath)
        {
            string metaPath = ToAbsolutePath(assetPath) + ".meta";
            return File.Exists(metaPath) ? File.GetLastWriteTimeUtc(metaPath).Ticks : 0L;
        }

        public static string GetAssetName(string assetPath)
        {
            return Path.GetFileNameWithoutExtension(assetPath);
        }

        public static string GetAssetTypeName(string assetPath)
        {
            Type type = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            return type != null ? type.Name : "Unknown";
        }

        public static Texture GetAssetIcon(string assetPath)
        {
            return AssetDatabase.GetCachedIcon(assetPath);
        }

        /// <summary>True for folders, which are tracked structurally but never scanned for references.</summary>
        public static bool IsFolder(string assetPath)
        {
            return AssetDatabase.IsValidFolder(assetPath);
        }
    }
}
