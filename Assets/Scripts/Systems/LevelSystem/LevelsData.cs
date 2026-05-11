using System.Collections.Generic;
using UnityEngine;

namespace Connect.Systems.LevelSystem {
    [CreateAssetMenu(fileName = "Levels",menuName = "SO/AllLevels")]
    public class LevelsData : ScriptableObject {
        public List<LevelData> levels;
    }
}