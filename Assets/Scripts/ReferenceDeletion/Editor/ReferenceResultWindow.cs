using System;
using System.Collections.Generic;
using System.Linq;
using ReferenceDeletion.Models;
using UnityEditor;
using UnityEngine;

namespace ReferenceDeletion.Editor
{
    /// <summary>
    /// Editor window shown when references exist for an asset the user tried to
    /// delete. Lists every referencing asset with icon/type/path, supports search,
    /// sorting, multi-selection, and Ping/Select/Open actions, plus
    /// "Delete Anyway" / "Cancel".
    /// </summary>
    public sealed class ReferenceResultWindow : EditorWindow
    {
        private enum SortColumn { Name, Type, Path }

        private ReferenceResult _result;
        private Action _onDeleteAnyway;

        private string _searchFilter = string.Empty;
        private Vector2 _scrollPosition;
        private SortColumn _sortColumn = SortColumn.Name;
        private bool _sortAscending = true;
        private readonly HashSet<string> _selectedGuids = new HashSet<string>();

        public static void Show(ReferenceResult result, Action onDeleteAnyway)
        {
            ReferenceResultWindow window = GetWindow<ReferenceResultWindow>(true, "Asset References Found", true);
            window._result = result;
            window._onDeleteAnyway = onDeleteAnyway;
            window._selectedGuids.Clear();
            window.minSize = new Vector2(560, 360);
            window.Show();
        }

        private void OnGUI()
        {
            if (_result == null)
            {
                EditorGUILayout.HelpBox("No data to display.", MessageType.Warning);
                return;
            }

            DrawHeader();
            DrawToolbar();
            DrawColumnHeaders();
            DrawList();
            DrawFooter();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField(System.IO.Path.GetFileName(_result.TargetPath), EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Referenced by {_result.References.Count} asset(s):", EditorStyles.miniLabel);
            EditorGUILayout.Space(4);
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUI.SetNextControlName("SearchField");
            _searchFilter = EditorGUILayout.TextField(_searchFilter, EditorStyles.toolbarSearchField);
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(45)))
            {
                _searchFilter = string.Empty;
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawColumnHeaders()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(20); // icon column
            DrawSortableHeader("Name", SortColumn.Name, 180);
            DrawSortableHeader("Type", SortColumn.Type, 100);
            DrawSortableHeader("Path", SortColumn.Path, 0, expand: true);
            GUILayout.Space(150); // action buttons column
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSortableHeader(string label, SortColumn column, int width, bool expand = false)
        {
            string arrow = _sortColumn == column ? (_sortAscending ? " ▲" : " ▼") : string.Empty;
            GUILayoutOption[] options = expand
                ? new[] { GUILayout.ExpandWidth(true) }
                : new[] { GUILayout.Width(width) };

            if (GUILayout.Button(label + arrow, EditorStyles.toolbarButton, options))
            {
                if (_sortColumn == column)
                {
                    _sortAscending = !_sortAscending;
                }
                else
                {
                    _sortColumn = column;
                    _sortAscending = true;
                }
            }
        }

        private void DrawList()
        {
            IEnumerable<AssetReferenceInfo> filtered = _result.References;

            if (!string.IsNullOrEmpty(_searchFilter))
            {
                filtered = filtered.Where(r =>
                    r.Name.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    r.Path.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    r.AssetTypeName.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            List<AssetReferenceInfo> sorted = SortReferences(filtered).ToList();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            foreach (AssetReferenceInfo info in sorted)
            {
                DrawRow(info);
            }
            EditorGUILayout.EndScrollView();
        }

        private IEnumerable<AssetReferenceInfo> SortReferences(IEnumerable<AssetReferenceInfo> source)
        {
            Func<AssetReferenceInfo, string> keySelector = _sortColumn switch
            {
                SortColumn.Type => r => r.AssetTypeName,
                SortColumn.Path => r => r.Path,
                _ => r => r.Name,
            };

            return _sortAscending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
        }

        private void DrawRow(AssetReferenceInfo info)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(20));

            bool wasSelected = _selectedGuids.Contains(info.Guid);
            bool isSelected = EditorGUILayout.Toggle(wasSelected, GUILayout.Width(18));
            if (isSelected != wasSelected)
            {
                if (isSelected) _selectedGuids.Add(info.Guid);
                else _selectedGuids.Remove(info.Guid);
            }

            UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(info.Path);
            GUIContent icon = new GUIContent(AssetDatabase.GetCachedIcon(info.Path));
            GUILayout.Label(icon, GUILayout.Width(18), GUILayout.Height(18));

            EditorGUILayout.LabelField(info.Name, GUILayout.Width(180));
            EditorGUILayout.LabelField(info.AssetTypeName, GUILayout.Width(100));
            EditorGUILayout.LabelField(info.Path, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("Ping", GUILayout.Width(45)))
            {
                EditorGUIUtility.PingObject(asset);
            }
            if (GUILayout.Button("Select", GUILayout.Width(50)))
            {
                Selection.activeObject = asset;
            }
            if (GUILayout.Button("Open", GUILayout.Width(45)))
            {
                if (asset != null)
                {
                    AssetDatabase.OpenAsset(asset);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Cancel", GUILayout.Width(90)))
            {
                Close();
            }

            GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
            if (GUILayout.Button("Delete Anyway", GUILayout.Width(120)))
            {
                bool confirmed = EditorUtility.DisplayDialog(
                    "Confirm Delete",
                    $"\"{System.IO.Path.GetFileName(_result.TargetPath)}\" is referenced by {_result.References.Count} asset(s).\n\nDelete anyway?",
                    "Delete Anyway",
                    "Cancel");

                if (confirmed)
                {
                    _onDeleteAnyway?.Invoke();
                    Close();
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }
    }
}
