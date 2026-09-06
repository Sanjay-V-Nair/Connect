using System.Collections.Generic;
using ReferenceDeletion.Scanners;

namespace ReferenceDeletion.Core
{
    /// <summary>
    /// Composite scanner: holds an ordered list of <see cref="IReferenceScanner"/>
    /// strategies and delegates to the first one that reports it can handle a given
    /// asset. Order matters — more specific scanners (YAML, SerializedObject) should
    /// be registered before the generic <see cref="DependencyScanner"/> fallback.
    /// </summary>
    public sealed class AssetReferenceScanner
    {
        private readonly List<IReferenceScanner> _scanners;

        public AssetReferenceScanner(IEnumerable<IReferenceScanner> scanners)
        {
            _scanners = new List<IReferenceScanner>(scanners);
        }

        /// <summary>Scans an asset using the first applicable strategy. Returns an empty set if none apply.</summary>
        public HashSet<string> Scan(string assetPath)
        {
            foreach (IReferenceScanner scanner in _scanners)
            {
                if (scanner.CanScan(assetPath))
                {
                    return scanner.Scan(assetPath);
                }
            }
            return new HashSet<string>();
        }
    }
}
