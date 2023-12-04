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
        public const string TitleLevelPath = "res://Levels/EarlyLevels/TitleLevel/TitleLevelUI.tscn";
        public const string PlayerSelectLevelPath = "res://Levels/EarlyLevels/PlayerSelectScene/PlayerSelectLevel.tscn";
        #endregion

        #region Overlay UI Scenes
        public const string PauseScreenPath = "res://Levels/OverworldLevels/UserInterface/PauseScreen/PauseScreen.tscn";
        #endregion

        #region Overworld Level Scenes
        public const string OverworldLevel1Path = "res://Levels/OverworldLevels/Levels/BaseOverworldLevel.tscn";
        #endregion
    }
}
