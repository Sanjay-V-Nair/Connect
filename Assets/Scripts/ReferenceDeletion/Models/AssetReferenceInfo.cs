using System;

namespace ReferenceDeletion.Models
{
    /// <summary>
    /// Presentation-friendly description of a single asset that references
    /// (or is referenced by) another asset. Used to populate UI rows.
    /// </summary>
    [Serializable]
    public sealed class AssetReferenceInfo
    {
        public string Guid;
        public string Path;
        public string Name;
        public string AssetTypeName;

        public AssetReferenceInfo(string guid, string path, string name, string assetTypeName)
        {
            Guid = guid;
            Path = path;
            Name = name;
            AssetTypeName = assetTypeName;
        }
    }
}
