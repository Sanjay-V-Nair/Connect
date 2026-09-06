using System.Collections.Generic;
using UnityEngine;

namespace Connect.Systems.LevelSystem {
    public static class ColorConstants {
        public static readonly Color Red = Color.red;
        public static readonly Color Green = Color.green;
        public static readonly Color Yellow = Color.yellow;
        public static readonly Color Purple = new Color(0.5f, 0f, 0.5f, 1f);
        public static readonly Color Blue = Color.blue;
        public static readonly Color Orange = new Color(1f, 0.5f, 0f, 1f);

        private static readonly List<Color> ValidColors = new List<Color> {
            Red, Green, Yellow, Purple, Blue, Orange
        };

        public static List<Color> GetValidColors() {
            return ValidColors;
        }

        public static Color GetMutatedColor(Color inputColor) {
            if (IsSameColor(inputColor, Red)) return Green;
            if (IsSameColor(inputColor, Green)) return Red;
            if (IsSameColor(inputColor, Yellow)) return Purple;
            if (IsSameColor(inputColor, Purple)) return Yellow;
            if (IsSameColor(inputColor, Blue)) return Orange;
            if (IsSameColor(inputColor, Orange)) return Blue;
            return inputColor; // Fallback
        }

        public static bool IsSameColor(Color a, Color b) {
            // Compare colors with a small tolerance due to floating point precision
            return Vector4.Distance(a, b) < 0.1f;
        }
    }
}
