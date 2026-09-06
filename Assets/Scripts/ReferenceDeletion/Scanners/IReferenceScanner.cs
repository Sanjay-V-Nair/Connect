using System.Collections.Generic;

namespace ReferenceDeletion.Scanners
{
    /// <summary>
    /// Strategy for extracting the set of GUIDs an asset directly references.
    /// Implementations should be side-effect free and safe to run repeatedly.
    /// </summary>
    public interface IReferenceScanner
    {
        /// <summary>Whether this scanner knows how to handle the given asset path.</summary>
        bool CanScan(string assetPath);

        /// <summary>Returns the set of GUIDs referenced by the asset at <paramref name="assetPath"/>.</summary>
        HashSet<string> Scan(string assetPath);
    }
}
