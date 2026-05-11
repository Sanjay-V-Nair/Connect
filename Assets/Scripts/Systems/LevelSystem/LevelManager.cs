using System;
using UnityEngine;

namespace Connect.Systems.LevelSystem {
    public class LevelManager : MonoBehaviour {
        
        public static LevelManager Instance { get; private set; }

        private void Awake() {
            if(Instance == null) {
                Instance = this;
            }
        }

        [SerializeField] private LevelsData levels;
        
        public LevelData GetLevel(int levelNumber) {
            return levels.levels.Find(level => level.levelNumber == levelNumber);
        }
        
    }
}