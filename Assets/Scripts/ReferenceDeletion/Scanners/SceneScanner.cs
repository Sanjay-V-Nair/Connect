using System;
using System.Collections.Generic;
using ReferenceDeletion.Interfaces;
using ReferenceDeletion.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ILogger = ReferenceDeletion.Interfaces.ILogger;

namespace ReferenceDeletion.Scanners
{
    /// <summary>
    /// Optional "deep mode" scanner for scenes. Opens the scene additively, traverses
    /// the full hierarchy via <see cref="SerializedObject"/>, then unloads it — catching
    /// references that a pure YAML text scan could miss for exotic serialized formats.
    /// This is slow relative to <see cref="YamlScanner"/>, so it is opt-in and not part
    /// of the default scanning pipeline.
    /// </summary>
    public sealed class SceneScanner : IReferenceScanner
    {
        private readonly ILogger _logger;

        /// <summary>Deep scene scanning is expensive; disabled by default and enabled explicitly by the caller.</summary>
        public bool Enabled { get; set; }

        public SceneScanner(ILogger logger, bool enabled = false)
        {
            _logger = logger;
            Enabled = enabled;
        }

        public bool CanScan(string assetPath)
        {
            return Enabled && assetPath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase);
        }

        public HashSet<string> Scan(string assetPath)
        {
            HashSet<string> result = new HashSet<string>();
            string selfGuid = AssetUtility.PathToGuid(assetPath);

            Scene scene = default;
            bool opened = false;

            try
            {
                scene = EditorSceneManager.OpenScene(assetPath, OpenSceneMode.Additive);
                opened = scene.IsValid();

                if (!opened)
                {
                    return result;
                }

                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    ScanGameObjectRecursive(root, selfGuid, result);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"SceneScanner: skipped '{assetPath}' due to error: {ex.Message}");
                result.Clear();
            }
            finally
            {
                if (opened)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            return result;
        }

        private static void ScanGameObjectRecursive(GameObject go, string selfGuid, HashSet<string> result)
        {
            foreach (Component component in go.GetComponents<Component>())
            {
                if (component == null)
                {
                    continue; // missing script
                }

                using (SerializedObject serializedObject = new SerializedObject(component))
                {
                    SerializedProperty property = serializedObject.GetIterator();
                    bool enterChildren = true;

                    while (property.NextVisible(enterChildren))
                    {
                        enterChildren = true;

                        if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null)
                        {
                            string refPath = AssetDatabase.GetAssetPath(property.objectReferenceValue);
                            if (!string.IsNullOrEmpty(refPath))
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

            foreach (Transform child in go.transform)
            {
                ScanGameObjectRecursive(child.gameObject, selfGuid, result);
            }
        }
    }
}
