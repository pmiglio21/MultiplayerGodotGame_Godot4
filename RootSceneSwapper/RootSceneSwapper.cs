using Globals;
using Godot;
using Levels.UtilityLevels;
using Levels.EarlyLevels;

namespace Root
{
    public partial class RootSceneSwapper : Node
    {
        #region "Global" Properties

        public string PriorSceneName;

        #endregion

        private Control _rootGuiControl;

        #region Screen Managers

        private TitleScreenManager _titleScreenManager;
        private PlayModeScreenManager _playModeScreenManager; 
        private GameRulesScreenManager _gameRulesScreenManager; 
        private SettingsScreenManager _settingsScreenManager;
        //private PlayerSelectScreenManager _playerSelectScreenManager;

        #endregion

        public override void _Ready()
        {
            _rootGuiControl = FindChild("GUI") as Control;

            _titleScreenManager = FindChild("TitleScreenRoot") as TitleScreenManager;
        }

        #region Go-To-Screens Methods

        #region Go To Play Mode Screen

        private void OnTitleScreenRootGoToPlayModeScreen()
        {
            ChangeSceneToPlayModeScreen();
        }

        private void ChangeSceneToPlayModeScreen()
        {
            _playModeScreenManager = GD.Load<PackedScene>(LevelScenePaths.PlayModeScreenPath).Instantiate() as PlayModeScreenManager;

            _rootGuiControl.AddChild(_playModeScreenManager);

            _rootGuiControl.RemoveChild(_titleScreenManager);
        }

        #endregion

        #region Go To Game Rules Screen

        private void OnTitleScreenRootGoToGameRulesScreen()
        {
            ChangeSceneToGameRulesScreen();
        }

        private void ChangeSceneToGameRulesScreen()
        {
            _gameRulesScreenManager = GD.Load<PackedScene>(LevelScenePaths.GameRulesScreenPath).Instantiate() as GameRulesScreenManager;

            _rootGuiControl.AddChild(_gameRulesScreenManager);

            _rootGuiControl.RemoveChild(_titleScreenManager);
        }

        #endregion

        #region Go To Settings Screen

        private void OnTitleScreenRootGoToSettingsScreen()
        {
            ChangeSceneToSettingsScreen();
        }

        private void ChangeSceneToSettingsScreen()
        {
            _settingsScreenManager = GD.Load<PackedScene>(LevelScenePaths.SettingsScreenPath).Instantiate() as SettingsScreenManager;

            _rootGuiControl.AddChild(_settingsScreenManager);

            _rootGuiControl.RemoveChild(_titleScreenManager);
        }

        #endregion

        #endregion
    }
}
