using UnityEditor;
using UnityEngine;

public class PlayerPrefsEditor : EditorWindow {

    private string keyToSet = "";
    private string valueToSet = "";
    private bool isExistingKey = false;
    
    [MenuItem("Tools/Editor Player Prefs")]
    private static void OpenWindow() {
        var window = GetWindow<PlayerPrefsEditor>("Player Prefs Editor Tool");
        window.minSize = new Vector2(420f, 460f);
    }
    
    private void OnGUI() {
        EditorGUILayout.LabelField("CLEAR PLAYER PREFS", EditorStyles.boldLabel);
        EditorGUILayout.Space(10f);

        if (GUILayout.Button("Clear All Player Prefs")) {
            if (EditorUtility.DisplayDialog("Confirm Clear", "Are you sure you want to clear all Player Prefs?", "Yes", "No")) {
                PlayerPrefs.DeleteAll();
                Debug.Log("All Player Prefs have been cleared.");
            }
        }

        EditorGUILayout.Space(20f);
        
        EditorGUILayout.LabelField("SET PLAYER PREF", EditorStyles.boldLabel);
        EditorGUILayout.Space(10f);
        keyToSet = EditorGUILayout.TextField("Player Pref Key", keyToSet);
        valueToSet = EditorGUILayout.TextField("Player Pref Value", valueToSet);
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Set Player Pref")) {
            if (string.IsNullOrEmpty(keyToSet)) {
                EditorUtility.DisplayDialog("Invalid Key", "Please enter a valid key for the Player Pref.", "OK");
                return;
            }
            
            PlayerPrefs.SetString(keyToSet, valueToSet);
            PlayerPrefs.Save();
            Debug.Log($"Player Pref '{keyToSet}' set to '{valueToSet}'.");
        }
        
        EditorGUILayout.Space();
    }
    
}