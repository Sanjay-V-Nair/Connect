using System.Collections.Generic;

namespace ReferenceDeletion.Models
{
    /// <summary>
    /// The outcome of a reverse-reference query for a single target asset:
    /// which assets (if any) reference it.
    /// </summary>
    public sealed class ReferenceResult
    {
        public readonly string TargetGuid;
        public readonly string TargetPath;
        public readonly IReadOnlyList<AssetReferenceInfo> References;

        public bool HasReferences => References != null && References.Count > 0;

        public ReferenceResult(string targetGuid, string targetPath, IReadOnlyList<AssetReferenceInfo> references)
        {
            TargetGuid = targetGuid;
            TargetPath = targetPath;
            References = references ?? System.Array.Empty<AssetReferenceInfo>();
        }
    }
}
