using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Globals
{
    public static class LevelScenePaths
    {
        #region Early Scenes
        public const string TitleScreenPath = "res://Levels/EarlyLevels/TitleScreen/TitleScreen.tscn";
        public const string PlayerSelectScreenPath = "res://Levels/EarlyLevels/PlayerSelectScreen/PlayerSelectScreen.tscn";
        public const string PlayModeScreenPath = "res://Levels/EarlyLevels/PlayModeScreen/PlayModeScreen.tscn";
        #endregion

        #region Overlay UI Scenes
        public const string PauseScreenPath = "res://Levels/OverworldLevels/UserInterface/PauseScreen/PauseScreen.tscn";
        #endregion

        #region Overworld Level Scenes
        public const string BaseOverworldLevelPath = "res://Levels/OverworldLevels/Levels/BaseOverworldLevel.tscn";
        #endregion

        #region Utility Scenes
        public const string LevelHolderPath = "res://Levels/OverworldLevels/Levels/LevelHolder.tscn";
        public const string SplitScreenManagerPath = "res://Levels/UtilityLevels/SplitScreenManager/SplitScreenManager.tscn";
        public const string GameRulesScreenPath = "res://Levels/UtilityLevels/GameRulesScreen/GameRulesScreen.tscn";
        public const string SettingsScreenPath = "res://Levels/UtilityLevels/SettingsScreen/SettingsScreen.tscn";
        public const string GameOverScreenPath = "res://Levels/UtilityLevels/GameOverScreen/GameOverScreen.tscn";
        #endregion
    }
}
