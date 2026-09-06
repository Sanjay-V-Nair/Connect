using System;

namespace ReferenceDeletion.Models
{
    /// <summary>
    /// Lightweight, serializable snapshot of an asset's identity and file state.
    /// Used to avoid repeated <c>AssetDatabase</c> calls and to detect staleness
    /// via file/meta timestamps.
    /// </summary>
    [Serializable]
    public sealed class AssetMetadata
    {
        /// <summary>Stable Unity GUID for the asset.</summary>
        public string Guid;

        /// <summary>Project-relative asset path (e.g. "Assets/Prefabs/Player.prefab").</summary>
        public string Path;

        /// <summary>File name without extension.</summary>
        public string Name;

        /// <summary>Assembly-qualified-free type name (e.g. "GameObject", "Material").</summary>
        public string AssetTypeName;

        /// <summary>Last known write time (UTC ticks) of the asset file itself.</summary>
        public long FileTimestamp;

        /// <summary>Last known write time (UTC ticks) of the associated .meta file.</summary>
        public long MetaTimestamp;

        public AssetMetadata()
        {
        }

        public AssetMetadata(string guid, string path, string name, string assetTypeName, long fileTimestamp, long metaTimestamp)
        {
            Guid = guid;
            Path = path;
            Name = name;
            AssetTypeName = assetTypeName;
            FileTimestamp = fileTimestamp;
            MetaTimestamp = metaTimestamp;
        }

        /// <summary>
        /// Returns true if either the file or meta timestamp differs from the supplied values,
        /// meaning the asset should be rescanned.
        /// </summary>
        public bool IsStale(long currentFileTimestamp, long currentMetaTimestamp)
        {
            return FileTimestamp != currentFileTimestamp || MetaTimestamp != currentMetaTimestamp;
        }
    }
}
