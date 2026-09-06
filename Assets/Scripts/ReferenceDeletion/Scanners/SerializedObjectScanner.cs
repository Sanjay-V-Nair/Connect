using System;
using System.Collections.Generic;
using ReferenceDeletion.Interfaces;
using ReferenceDeletion.Utils;
using UnityEditor;
using UnityEngine;
using ILogger = ReferenceDeletion.Interfaces.ILogger;

namespace ReferenceDeletion.Scanners
{
    /// <summary>
    /// Scans custom <see cref="ScriptableObject"/> assets by walking their
    /// <see cref="SerializedProperty"/> tree, catching object references that
    /// text-based YAML scanning might miss in unusual serialization edge cases
    /// (e.g. references nested inside custom property drawers or managed references).
    /// </summary>
    public sealed class SerializedObjectScanner : IReferenceScanner
    {
        private readonly ILogger _logger;

        public SerializedObjectScanner(ILogger logger)
        {
            _logger = logger;
        }

        public bool CanScan(string assetPath)
        {
            UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            return asset is ScriptableObject;
        }

        public HashSet<string> Scan(string assetPath)
        {
            HashSet<string> result = new HashSet<string>();

            UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (asset == null)
            {
                return result;
            }

            try
            {
                string selfGuid = AssetUtility.PathToGuid(assetPath);
                using (SerializedObject serializedObject = new SerializedObject(asset))
                {
                    SerializedProperty property = serializedObject.GetIterator();
                    bool enterChildren = true;

                    while (property.NextVisible(enterChildren))
                    {
                        enterChildren = true;

                        if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null)
                        {
                            string refPath = AssetDatabase.GetAssetPath(property.objectReferenceValue);
                            if (!string.IsNullOrEmpty(refPath) && refPath != assetPath)
                            {
                                string guid = AssetUtility.PathToGuid(refPath);
                                if (!string.IsNullOrEmpty(guid) && guid != selfGuid)
                                {
                                    result.Add(guid);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"SerializedObjectScanner: skipped '{assetPath}' due to error: {ex.Message}");
                result.Clear();
            }

            return result;
        }
    }
}
