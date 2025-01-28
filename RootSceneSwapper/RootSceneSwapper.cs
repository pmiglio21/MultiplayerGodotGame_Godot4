using Enums;
using Globals;
using Godot;
using Levels.UtilityLevels;
using Levels.EarlyLevels;
using Models;
using MobileEntities.PlayerCharacters.Scripts;
using Scenes.UI.PlayerSelectScene;
using System.Collections.Generic;
using System;

namespace Root
{
	public partial class RootSceneSwapper : Node
	{
		#region "Global" Properties

		public ScreenNames PriorSceneName;
		public GameRules CurrentGameRules = new GameRules();
        public Settings CurrentSettings = new Settings();

        public List<BaseCharacter> ActivePlayers = new List<BaseCharacter>();
		public List<PlayerCharacterPicker> ActivePlayerCharacterPickers = new List<PlayerCharacterPicker>();

		#endregion

		#region Components

		private Control _rootGuiControl;

		private AudioStreamPlayer _uiAudioStreamPlayer;

		#endregion

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
			_rootGuiControl = FindChild("GUI") as Control;
			_uiAudioStreamPlayer = FindChild("UiSoundEffectsAudioStreamPlayer") as AudioStreamPlayer;

			//GetTree().Root.SizeChanged += AdjustUiOnWindowSizeChanged;

            _titleScreenManager = FindChild("TitleScreenRoot") as TitleScreenManager;

            //_titleScreenManager.GoToPlayModeScreen += OnTitleScreenRootGoToPlayModeScreen;
            _titleScreenManager.GoToPlayerCharacterSelectScreen += OnTitleScreenRootGoToPlayerCharacterSelectScreen;
            _titleScreenManager.GoToGameRulesScreen += OnTitleScreenRootGoToGameRulesScreen;
            _titleScreenManager.GoToSettingsScreen += OnTitleScreenRootGoToSettingsScreen;
			_titleScreenManager.QuitGame += QuitGame;

			//GetLastSavedGameRules();
            LoadOriginalSettings();
        }

		public DungeonLevelSwapper GetDungeonLevelSwapper()
		{
			return _dungeonLevelSwapper;
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

        private void OnTitleScreenRootGoToPlayerCharacterSelectScreen()
        {
            ChangeSceneToPlayerCharacterSelectScreen(_titleScreenManager);
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
			PriorSceneName = ScreenNames.PlayMode;

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

        public void OnPlayerCharacterSelectScreenGoToTitleScreen()
        {
            this.ActivePlayers.Clear();
            this.ActivePlayerCharacterPickers.Clear();

            ChangeSceneToTitleScreen(_playerCharacterSelectScreenManager);
        }

        public void OnPlayerCharacterSelectScreenGoToGameRulesScreen()
		{
            //this.ActivePlayers.Clear();
            //this.ActivePlayerCharacterPickers.Clear();

            PriorSceneName = ScreenNames.PlayerCharacterSelect;

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

            if (_dungeonLevelSwapper != null)
            {
                _dungeonLevelSwapper.QueueFree();
            }

            //GetTree().Root.SizeChanged += AdjustUiOnWindowSizeChanged;
        }

		public void OnDungeonLevelSwapperScreenGoToGameOverScreen()
		{
			_playerCharacterSelectScreenManager = null;

			ChangeSceneToGameOverScreen(_dungeonLevelSwapper);

            //GetTree().Root.SizeChanged += AdjustUiOnWindowSizeChanged;
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

				_playerCharacterSelectScreenManager.GoToTitleScreen += OnPlayerCharacterSelectScreenGoToTitleScreen;
                _playerCharacterSelectScreenManager.GoToGameRulesScreen += OnPlayerCharacterSelectScreenGoToGameRulesScreen; 
				_playerCharacterSelectScreenManager.GoToDungeonLevelSwapper += OnPlayerCharacterSelectScreenGoToDungeonLevelSwapper;
			}

			_rootGuiControl.AddChild(_playerCharacterSelectScreenManager);

			_rootGuiControl.RemoveChild(currentUiScene);
		}

		private void ChangeSceneToDungeonLevelSwapperScreen(Control currentUiScene)
		{
            //GetTree().Root.SizeChanged -= AdjustUiOnWindowSizeChanged;

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

        public override void _Notification(int notificationCode)
        {
            if (notificationCode == NotificationWMCloseRequest)
			{
				QuitGame();
            }
        }

		//Move this to each screen manager. Can't have it here because we don't want split screen manager to shrink even more
		//private void AdjustUiOnWindowSizeChanged()
		//{
		//	if (GetTree().Root.GetWindow().Size.X * GetTree().Root.GetWindow().Size.Y > 1000000)
		//	{
		//		_rootGuiControl.Scale = new Vector2(2, 2);
  //          }
  //          else if (GetTree().Root.GetWindow().Size.X * GetTree().Root.GetWindow().Size.Y < 500000)
  //          {
  //              _rootGuiControl.Scale = new Vector2(.5f, .5f);
  //          }
  //          else
  //          {
  //              _rootGuiControl.Scale = new Vector2(1, 1);
  //          }
  //      }

        private void QuitGame()
		{
			try
			{
                //Need this or else game confuses memory while quitting
                if (IsInstanceValid(_dungeonLevelSwapper) && _dungeonLevelSwapper != null)
                {
                    _dungeonLevelSwapper.QueueFree();
                }

                _playerCharacterSelectScreenManager?.QueueFree();
                _settingsScreenManager?.QueueFree();
                _gameRulesScreenManager?.QueueFree();
                _playModeScreenManager?.QueueFree();
                _titleScreenManager?.QueueFree();

                GetTree().Quit();
            }
			catch (Exception ex)
			{
				GD.PushError(ex.Message);
            }
		}

		#endregion

		#endregion

		#region Audio Players

		private bool _isUiAudioStreamPlayerMuted = false;

		public void PlayUiSoundEffect(string soundPath)
		{
			AudioStream audioStream = ResourceLoader.Load(soundPath) as AudioStream;

			if (!_isUiAudioStreamPlayerMuted)
			{
                _uiAudioStreamPlayer.Stream = audioStream;

                _uiAudioStreamPlayer.Play();
            }
		}

        public void ChangeMenuSoundsVolume(float volume)
        {
            if (volume == 0)
            {
				_isUiAudioStreamPlayerMuted = true;
            }
			else
			{
				_isUiAudioStreamPlayerMuted = false;

                _uiAudioStreamPlayer.VolumeDb = 20 * volume;
            }
        }

        #endregion

        #region Game Rules

        private void GetLastSavedGameRules()
		{

        }

        #endregion

        #region Settings

        private void LoadOriginalSettings()
        {
            var settingsData = new Godot.Collections.Dictionary();
            var config = new ConfigFile();

            // Load data from a file.
            // Found in C:\Users\pmigl\AppData\Roaming\Godot\app_userdata\Multiplayer Godot Game Godot 4
            Error error = config.Load("user://settings.cfg");

            // If the file didn't load, ignore it.
            if (error != Error.Ok)
            {
                return;
            }
            else
            {
                // Iterate over all sections.
                foreach (string section in config.GetSections())
                {
                    // Fetch the data for each section.
                    CurrentSettings.MusicVolume = (float)config.GetValue(section, "music_volume");
                    CurrentSettings.SoundEffectsVolume = (float)config.GetValue(section, "menu_sounds_volume");
                    CurrentSettings.DungeonSoundsVolume = (float)config.GetValue(section, "dungeon_sounds_volume");
                    CurrentSettings.Resolution = (Vector2I)config.GetValue(section, "resolution");
                    CurrentSettings.FullscreenState = (string)config.GetValue(section, "fullscreen_state");
                }
            }

			if (CurrentSettings.FullscreenState != "ON")
			{
                DisplayServer.WindowSetSize(new Vector2I(CurrentSettings.Resolution.X, CurrentSettings.Resolution.Y));
            }
			else
			{
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            }
        }

        #endregion
    }
}
