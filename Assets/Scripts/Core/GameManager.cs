using System;
using Connect.Models;
using Connect.Systems.Datastore;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Connect.Core {
    public class GameManager : MonoBehaviour{
        
        public static GameManager Instance { get; private set; }

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable() {
            SetGameData();
        }

        private void SetGameData() {
            var gameLevel = LocalDatastore.GetData("Level");
            if (string.IsNullOrEmpty(gameLevel)) gameLevel = "1";
            LocalDatastore.SaveData("Level", gameLevel);
            var level = int.TryParse(gameLevel, out var result) ? result : 1;
            GameStateData.SetGameLevel(level);
        }
        
        public void UnlockNextLevel() {
            GameStateData.UpdateGameLevel();
            LocalDatastore.SaveData("Level", GameStateData.GetGameLevel().ToString());
        }
        
        public void LoadSceneAsync(string sceneName) {
            SceneManager.LoadScene(sceneName);
            SetAudioForScene(sceneName);
        }

        private void SetAudioForScene(string sceneName) {
            switch(sceneName) {
                case "DashboardScene":
                    AudioManager.Instance.PlayGameAudio(true);
                    break;
                case "GameScene":
                    AudioManager.Instance.PlayLevelAudio(true);
                    break;
            }
        }
    }
}