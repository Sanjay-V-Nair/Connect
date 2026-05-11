using UnityEngine;

namespace Connect.Systems.Datastore {
    public static class LocalDatastore {
        
        public static void SaveData(string key, string value) {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }
        
        public static string GetData(string key) {
            return PlayerPrefs.GetString(key, null);
        }
        
    }
}