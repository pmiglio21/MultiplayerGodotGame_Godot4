using Enums;
using Globals;
using Godot;
using Levels.UtilityLevels;
using Levels.EarlyLevels;
using Models;
using MobileEntities.PlayerCharacters.Scripts;
using Scenes.UI.PlayerSelectScene;
using System.Collections.Generic;

namespace Root
{
    public partial class RootSceneSwapper : Node
    {
        #region "Global" Properties

        public ScreenNames PriorSceneName;
        public GameRules CurrentGameRules = new GameRules();

        public List<BaseCharacter> ActivePlayers = new List<BaseCharacter>();
        public List<PlayerCharacterPicker> ActivePlayerCharacterPickers = new List<PlayerCharacterPicker>();

        #endregion

        private Control _rootGuiControl;

        #region Screen Managers

        private TitleScreenManager _titleScreenManager;
        private PlayModeScreenManager _playModeScreenManager; 
        private GameRulesScreenManager _gameRulesScreenManager; 
        private SettingsScreenManager _settingsScreenManager;
        private PlayerCharacterSelectScreenManager _playerCharacterSelectScreenManager; 
        private DungeonLevelSwapper _dungeonLevelSwapper;
        private GameOverScreenManager _gameOverScreenManager;

        #endregion

        public override void _Ready()
        {
            GetTree().Root.SizeChanged += CentralizeGui;

            _rootGuiControl = FindChild("GUI") as Control;

            CentralizeGui();

            _titleScreenManager = FindChild("TitleScreenRoot") as TitleScreenManager;

            _titleScreenManager.GoToPlayModeScreen += OnTitleScreenRootGoToPlayModeScreen;
            _titleScreenManager.GoToGameRulesScreen += OnTitleScreenRootGoToGameRulesScreen; 
            _titleScreenManager.GoToSettingsScreen += OnTitleScreenRootGoToSettingsScreen;
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
            this.CurrentGameRules = new GameRules();

            ChangeSceneToTitleScreen(_gameRulesScreenManager);
        }

        public void OnGameRulesScreenGoToPlayModeScreen()
        {
            this.CurrentGameRules = _gameRulesScreenManager.CurrentGameRules;

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
            this.ActivePlayers.Clear();
            this.ActivePlayerCharacterPickers.Clear();

            ChangeSceneToGameRulesScreen(_playerCharacterSelectScreenManager);
        }

        public void OnPlayerCharacterSelectScreenGoToDungeonLevelSwapper()
        {
            ChangeSceneToDungeonLevelSwapperScreen(_playerCharacterSelectScreenManager);
        }

        #endregion

        #region Dungeon Level Swapper Screen

        public void OnDungeonLevelSwapperScreenGoToTitleScreen()
        {
            _playerCharacterSelectScreenManager = null;

            ChangeSceneToTitleScreen(_dungeonLevelSwapper);

            //if (_dungeonLevelSwapper != null)
            //{
            //    _dungeonLevelSwapper.QueueFree();
            //}
        }

        public void OnDungeonLevelSwapperScreenGoToGameOverScreen()
        {
            _playerCharacterSelectScreenManager = null;

            ChangeSceneToGameOverScreen(_dungeonLevelSwapper);
        }

        #endregion

        #region Game Over Screen

        public void OnGameOverScreenGoToTitleScreen()
        {
            _playerCharacterSelectScreenManager = null;

            ChangeSceneToTitleScreen(_gameOverScreenManager);
        }

        #endregion

        #region General Use

        private void ChangeSceneToTitleScreen(Control currentUiScene)
        {
            _rootGuiControl.AddChild(_titleScreenManager);

            _rootGuiControl.RemoveChild(currentUiScene);

            _titleScreenManager.GrabFocusOfTopButton();
        }

        private void ChangeSceneToTitleScreen(Node currentScene)
        {
            _rootGuiControl.AddChild(_titleScreenManager);

            _rootGuiControl.RemoveChild(currentScene);

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
                _playerCharacterSelectScreenManager.GoToDungeonLevelSwapper += OnPlayerCharacterSelectScreenGoToDungeonLevelSwapper;
            }

            _rootGuiControl.AddChild(_playerCharacterSelectScreenManager);

            _rootGuiControl.RemoveChild(currentUiScene);
        }

        private void ChangeSceneToDungeonLevelSwapperScreen(Control currentUiScene)
        {
            //Doesn't need to be checked for null, just reset every time
            _dungeonLevelSwapper = GD.Load<PackedScene>(LevelScenePaths.DungeonLevelSwapperPath).Instantiate() as DungeonLevelSwapper;

            _dungeonLevelSwapper.GoToTitleScreen += OnDungeonLevelSwapperScreenGoToTitleScreen;
            _dungeonLevelSwapper.GoToGameOverScreen += OnDungeonLevelSwapperScreenGoToGameOverScreen;

            _dungeonLevelSwapper.ActivePlayers = _playerCharacterSelectScreenManager.ActivePlayers;
            _dungeonLevelSwapper.CurrentGameRules = this.CurrentGameRules;
            this.ActivePlayerCharacterPickers = _playerCharacterSelectScreenManager.ActivePlayerCharacterPickers;

            _rootGuiControl.AddChild(_dungeonLevelSwapper);

            _rootGuiControl.RemoveChild(currentUiScene);
        }

        private void ChangeSceneToGameOverScreen(Node currentUiScene)
        {
            _gameOverScreenManager = GD.Load<PackedScene>(LevelScenePaths.GameOverScreenPath).Instantiate() as GameOverScreenManager;

            _gameOverScreenManager.GoToTitleScreen += OnGameOverScreenGoToTitleScreen;

            _rootGuiControl.AddChild(_gameOverScreenManager);

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
            _dungeonLevelSwapper?.QueueFree();

            GetTree().Quit();
        }

        private void CentralizeGui()
        {
            //var tree = GetTree();

            //Vector2I mainViewportSize = tree.Root.Size;

            //_rootGuiControl.GlobalPosition = new Vector2(mainViewportSize.X / 2, mainViewportSize.Y / 2);
        }

        #endregion

        #endregion

        public DungeonLevelSwapper GetDungeonLevelSwapper()
        {
            return _dungeonLevelSwapper;
        }
    }
}
