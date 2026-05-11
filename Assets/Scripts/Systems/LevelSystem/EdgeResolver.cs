using UnityEngine;

namespace Connect.Systems.LevelSystem {
    public static class EdgeResolver {
        public static bool HasLeftEdge(Vector2Int position) {
            return position.x > 0;
        }

        public static bool HasRightEdge(Vector2Int position, int gridXSize) {
            return position.x < gridXSize - 1;
        }

        public static bool HasBottomEdge(Vector2Int position) {
            return position.y > 0;
        }

        public static bool HasTopEdge(Vector2Int position, int gridYSize) {
            return position.y < gridYSize - 1;
        }
    }
}
