namespace Connect.Models {
    public static class GameStateData {

        private static int currentLevel  = 1;

        public static void UpdateGameLevel(){
            currentLevel++;
        }
        
        public static void ResetGameLevel(){
            currentLevel = 1;
        }
        
        public static void SetGameLevel(int level){
            currentLevel = level;
        }

        public static int GetGameLevel() {
            return currentLevel;
        }
        
    }
}