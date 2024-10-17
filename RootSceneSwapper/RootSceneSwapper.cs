using Enums;
using Globals;
using Godot;
using Levels.UtilityLevels;
using Levels.EarlyLevels;

namespace Root
{
    public partial class RootSceneSwapper : Node
    {
        #region "Global" Properties

        public ScreenNames PriorSceneName;

        #endregion

        private Control _rootGuiControl;

        #region Screen Managers

        private TitleScreenManager _titleScreenManager;
        private PlayModeScreenManager _playModeScreenManager; 
        private GameRulesScreenManager _gameRulesScreenManager; 
        private SettingsScreenManager _settingsScreenManager;
        private PlayerCharacterSelectScreenManager _playerCharacterSelectScreenManager;

        #endregion

        public override void _Ready()
        {
            _rootGuiControl = FindChild("GUI") as Control;

            _titleScreenManager = FindChild("TitleScreenRoot") as TitleScreenManager;

            _titleScreenManager.QuitGame += QuitGame;
        }

        #region Go-To-Screens Methods

        #region From Title Screen

        private void OnTitleScreenRootGoToPlayModeScreen()
        {
            ChangeSceneToPlayModeScreen(_titleScreenManager);
        }

        private void OnTitleScreenRootGoToGameRulesScreen()
        {
            ChangeSceneToGameRulesScreen(_titleScreenManager);
        }

        private void OnTitleScreenRootGoToSettingsScreen()
        {
            ChangeSceneToSettingsScreen();
        }

        #endregion

        #region From Play Mode Screen

        public void OnPlayModeScreenGoToTitleScreen()
        {
            ChangeSceneToTitleScreen(_playModeScreenManager);
        }

        public void OnPlayModeScreenGoToGameRulesScreen()
        {
            ChangeSceneToGameRulesScreen(_playModeScreenManager);
        }

        #endregion

        #region Game Rules Screen

        public void OnGameRulesScreenGoToTitleScreen()
        {
            ChangeSceneToTitleScreen(_gameRulesScreenManager);
        }

        public void OnGameRulesScreenGoToPlayModeScreen()
        {
            ChangeSceneToPlayModeScreen(_gameRulesScreenManager);
        }

        public void OnGameRulesScreenGoToPlayerCharacterSelectScreen()
        {
            ChangeSceneToPlayerCharacterSelectScreen(_gameRulesScreenManager);
        }

        #endregion

        #region Settings Screen

        public void OnSettingsScreenGoToTitleScreen()
        {
            ChangeSceneToTitleScreen(_settingsScreenManager);
        }

        #endregion

        #region Player Character Select Screen

        public void OnPlayerCharacterSelectScreenGoToGameRulesScreen()
        {
            ChangeSceneToGameRulesScreen(_playerCharacterSelectScreenManager);
        }

        #endregion

        #region General Use

        private void ChangeSceneToTitleScreen(Control currentUiScene)
        {
            _rootGuiControl.AddChild(_titleScreenManager);

            _rootGuiControl.RemoveChild(currentUiScene);

            _titleScreenManager.GrabFocusOfTopButton();
        }

        private void ChangeSceneToPlayModeScreen(Control currentUIScene)
        {
            if (_playModeScreenManager == null)
            {
                _playModeScreenManager = GD.Load<PackedScene>(LevelScenePaths.PlayModeScreenPath).Instantiate() as PlayModeScreenManager;

                _playModeScreenManager.GoToTitleScreen += OnPlayModeScreenGoToTitleScreen;
                _playModeScreenManager.GoToGameRulesScreen += OnPlayModeScreenGoToGameRulesScreen;
            }

            _rootGuiControl.AddChild(_playModeScreenManager);

            _rootGuiControl.RemoveChild(currentUIScene);

            _playModeScreenManager.GrabFocusOfFirstButton();
        }

        private void ChangeSceneToGameRulesScreen(Control currentUiScene)
        {
            if (_gameRulesScreenManager == null)
            {
                _gameRulesScreenManager = GD.Load<PackedScene>(LevelScenePaths.GameRulesScreenPath).Instantiate() as GameRulesScreenManager;

                _gameRulesScreenManager.GoToTitleScreen += OnGameRulesScreenGoToTitleScreen;
                _gameRulesScreenManager.GoToPlayModeScreen += OnGameRulesScreenGoToPlayModeScreen;
                _gameRulesScreenManager.GoToPlayerCharacterSelectScreen += OnGameRulesScreenGoToPlayerCharacterSelectScreen;
            }

            _rootGuiControl.AddChild(_gameRulesScreenManager);

            _rootGuiControl.RemoveChild(currentUiScene);

            _gameRulesScreenManager.GrabFocusOfTopButton();
        }

        private void ChangeSceneToSettingsScreen()
        {
            if (_settingsScreenManager == null)
            {
                _settingsScreenManager = GD.Load<PackedScene>(LevelScenePaths.SettingsScreenPath).Instantiate() as SettingsScreenManager;

                _settingsScreenManager.GoToTitleScreen += OnSettingsScreenGoToTitleScreen;
            }

            _rootGuiControl.AddChild(_settingsScreenManager);

            _rootGuiControl.RemoveChild(_titleScreenManager);

            _settingsScreenManager.GrabFocusOfTopButton();
        }

        private void ChangeSceneToPlayerCharacterSelectScreen(Control currentUiScene)
        {
            if (_playerCharacterSelectScreenManager == null)
            {
                _playerCharacterSelectScreenManager = GD.Load<PackedScene>(LevelScenePaths.PlayerCharacterSelectScreenPath).Instantiate() as PlayerCharacterSelectScreenManager;

                _playerCharacterSelectScreenManager.GoToGameRulesScreen += OnPlayerCharacterSelectScreenGoToGameRulesScreen;
            }

            _rootGuiControl.AddChild(_playerCharacterSelectScreenManager);

            _rootGuiControl.RemoveChild(currentUiScene);
        }

        private void ChangeSceneToDungeonLevelHolderScreen(Control currentUiScene)
        {
            //if (_playerCharacterSelectScreenManager == null)
            //{
            //    _playerCharacterSelectScreenManager = GD.Load<PackedScene>(LevelScenePaths.PlayerCharacterSelectScreenPath).Instantiate() as PlayerCharacterSelectScreenManager;

            //    _playerCharacterSelectScreenManager.GoToGameRulesScreen += OnPlayerCharacterSelectScreenGoToGameRulesScreen;
            //}

            //_rootGuiControl.AddChild(_playerCharacterSelectScreenManager);

            _rootGuiControl.RemoveChild(currentUiScene);
        }

        private void QuitGame()
        {
            //Need this or else game confuses memory while quitting
            _titleScreenManager?.QueueFree();
            _playModeScreenManager?.QueueFree();
            _gameRulesScreenManager?.QueueFree();
            _settingsScreenManager?.QueueFree();
            _playerCharacterSelectScreenManager?.QueueFree();

            GetTree().Quit();
        }

        #endregion

        #endregion
    }
}
