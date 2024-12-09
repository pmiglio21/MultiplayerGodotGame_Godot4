using Enums;
using Globals;
using Godot;
using Levels.UtilityLevels.UserInterfaceComponents;
using Models;
using Root;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Levels.UtilityLevels
{
	public partial class SettingsScreenManager : Control
	{
        private RootSceneSwapper _rootSceneSwapper;

        private Settings _changedSettings = new Settings();

        private List<Vector2I> _resolutionOptions = new List<Vector2I>()
        {
            new Vector2I(1152,648),
            new Vector2I(1280,720),
            new Vector2I(1128,634),
            new Vector2I(1024,768),
            new Vector2I(832,624),
            new Vector2I(800,600),
            new Vector2I(720,576),
            new Vector2I(720,480),
            new Vector2I(640,480),
            new Vector2I(1920,1080),
            new Vector2I(1760,990),
            new Vector2I(1680,1050),
            new Vector2I(1600,900),
            new Vector2I(1440,900),
            new Vector2I(1366,768),
            new Vector2I(1280,1024),
            new Vector2I(1280,960)
        };

        private bool _isNavigatingLeft = true;

        private bool _wasApplyClicked = false;

        #region Pause Screen Stuff

        public bool IsSettingsScreenBeingShown = false;

		protected PauseScreenManager pauseScreen;

        #endregion

        #region Components

        private Timer _inputTimer;
        private SliderButton _musicVolumeSliderButton;
        private SliderButton _soundEffectsVolumeSliderButton;
        private SliderButton _dungeonSoundsVolumeSliderButton;
        private OptionSelector _resolutionOptionSelector;
        private Button _resolutionButton;
        private OptionSelector _fullscreenOptionSelector;
        private Button _fullscreenButton;
        private Button _applyButton;
        private Button _returnButton;

        #endregion

        [Signal]
        public delegate void GoToTitleScreenEventHandler();

        public override void _Ready()
		{
            _rootSceneSwapper = GetTree().Root.GetNode<RootSceneSwapper>("RootSceneSwapper");

            _changedSettings.Resolution = _rootSceneSwapper.CurrentSettings.Resolution;

            _inputTimer = FindChild("InputTimer") as Timer;

            _musicVolumeSliderButton = FindChild("MusicVolumeSliderButton") as SliderButton;

            _soundEffectsVolumeSliderButton = FindChild("SoundEffectsVolumeSliderButton") as SliderButton;
            _soundEffectsVolumeSliderButton.GetHSlider().ValueChanged += ((double newValue) => OnChanged_SoundEffectsVolumeSliderButton(_soundEffectsVolumeSliderButton.GetHSlider().Value));

            _dungeonSoundsVolumeSliderButton = FindChild("DungeonSoundsVolumeSliderButton") as SliderButton;

            _resolutionOptionSelector = FindChild("ResolutionOptionSelector") as OptionSelector;
            _resolutionButton = FindChild("ResolutionButton") as Button;
            _resolutionButton.Text = $"{_rootSceneSwapper.CurrentSettings.Resolution.X} x {_rootSceneSwapper.CurrentSettings.Resolution.Y}";
            _resolutionButton.FocusEntered += _resolutionOptionSelector.PlayOnFocusAnimation;
            _resolutionButton.FocusExited += _resolutionOptionSelector.PlayLoseFocusAnimation;

            _fullscreenOptionSelector = FindChild("FullscreenOptionSelector") as OptionSelector;
            _fullscreenButton = FindChild("FullscreenButton") as Button;
            _fullscreenButton.Text = _rootSceneSwapper.CurrentSettings.FullscreenState;
            _fullscreenButton.FocusEntered += _fullscreenOptionSelector.PlayOnFocusAnimation;
            _fullscreenButton.FocusExited += _fullscreenOptionSelector.PlayLoseFocusAnimation;

            _applyButton = FindChild("ApplyButton") as Button;
            _returnButton = FindChild("ReturnButton") as Button;

            _musicVolumeSliderButton.GetHSlider().Value = _rootSceneSwapper.CurrentSettings.MusicVolume;
            _soundEffectsVolumeSliderButton.GetHSlider().Value = _rootSceneSwapper.CurrentSettings.SoundEffectsVolume;
            _dungeonSoundsVolumeSliderButton.GetHSlider().Value = _rootSceneSwapper.CurrentSettings.DungeonSoundsVolume;

            GetPauseScreen();

            if (pauseScreen == null)
            {
                IsSettingsScreenBeingShown = true;
            }

            GrabFocusOfTopButton();
        }

		public override void _Process(double delta)
		{
			if (IsSettingsScreenBeingShown)
			{
				Show();
			}
			else
			{
				Hide();
			}

			if (IsSettingsScreenBeingShown)
			{
				GetButtonInput();

				GetNavigationInput();
			}
		}

		private void GetButtonInput()
		{
			if (UniversalInputHelper.IsActionJustPressed(InputType.SouthButton))
			{
                if (_resolutionButton.HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

                    _resolutionOptionSelector.PlayClickedOnRightArrow();

                    var indexOfCurrentResolution = _resolutionOptions.IndexOf(new Vector2I(_changedSettings.Resolution.X, _changedSettings.Resolution.Y));
                    var nextIndexOfResolutionOptions = 0;

                    if (indexOfCurrentResolution != _resolutionOptions.Count - 1)
                    {
                        nextIndexOfResolutionOptions = indexOfCurrentResolution + 1;
                    }

                    _changedSettings.Resolution = _resolutionOptions[nextIndexOfResolutionOptions];
                    _resolutionButton.Text = $"{_changedSettings.Resolution.X} x {_changedSettings.Resolution.Y}";
                }
                else if (_fullscreenButton.HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

                    _fullscreenOptionSelector.PlayClickedOnRightArrow();
                    _fullscreenOptionSelector.PlayClickedOnLeftArrow();

                    if (_fullscreenButton.Text == "OFF")
                    {
                        _fullscreenButton.Text = "ON";
                    }
                    else
                    {
                        _fullscreenButton.Text = "OFF";
                    }
                }
                else if (_applyButton.HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

                    ApplyChanges();
                }
                else if (_returnButton.HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

                    ReturnToPriorScene();
                }
            }
			else if (UniversalInputHelper.IsActionJustPressed(InputType.EastButton))
			{
                _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

                ReturnToPriorScene();
            }
            else if (UniversalInputHelper.IsActionJustPressed(InputType.StartButton))
            {
                if (_returnButton.HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

                    ReturnToPriorScene();
                }
            }
        }

		private void GetNavigationInput()
		{
            if (_inputTimer.IsStopped() && UniversalInputHelper.IsActionJustPressed(InputType.MoveSouth))
            {
                if (_musicVolumeSliderButton.GetFocusHolder().HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);
                    _soundEffectsVolumeSliderButton.GetFocusHolder().GrabFocus();
                }
                else if (_soundEffectsVolumeSliderButton.GetFocusHolder().HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);
                    _dungeonSoundsVolumeSliderButton.GetFocusHolder().GrabFocus();
                }
                else if (_dungeonSoundsVolumeSliderButton.GetFocusHolder().HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);

                    if (_fullscreenButton.Text == "ON")
                    {
                        _fullscreenButton.GrabFocus();
                    }
                    else
                    {
                        _resolutionButton.GrabFocus();
                    }
                }
                else if (_resolutionButton.HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);
                    _fullscreenButton.GrabFocus();
                }
                else if (_fullscreenButton.HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);

                    if (_isNavigatingLeft)
                    {
                        _applyButton.GrabFocus();
                    }
                    else
                    {
                        _returnButton.GrabFocus();
                    }
                }

                _inputTimer.Start();
            }
            else if (_inputTimer.IsStopped() && UniversalInputHelper.IsActionJustPressed(InputType.MoveNorth))
            {
                if (_returnButton.HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);
                    _fullscreenButton.GrabFocus();
                }
                else if (_applyButton.HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);
                    _fullscreenButton.GrabFocus();
                }
                else if (_fullscreenButton.HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);
                    
                    if (_fullscreenButton.Text == "ON")
                    {
                        _dungeonSoundsVolumeSliderButton.GetFocusHolder().GrabFocus();
                    }
                    else
                    {
                        _resolutionButton.GrabFocus();
                    }
                }
                else if (_resolutionButton.HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);
                    _dungeonSoundsVolumeSliderButton.GetFocusHolder().GrabFocus();
                }
                else if (_dungeonSoundsVolumeSliderButton.GetFocusHolder().HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);
                    _soundEffectsVolumeSliderButton.GetFocusHolder().GrabFocus();
                }
                else if (_soundEffectsVolumeSliderButton.GetFocusHolder().HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);
                    _musicVolumeSliderButton.GetFocusHolder().GrabFocus();
                }

                _inputTimer.Start();
            }
            else if (_inputTimer.IsStopped() && UniversalInputHelper.IsActionJustPressed(InputType.MoveEast))
            {
                if (_applyButton.HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);
                    _returnButton.GrabFocus();
                    _isNavigatingLeft = false;
                }
            }
            else if (_inputTimer.IsStopped() && UniversalInputHelper.IsActionJustPressed(InputType.MoveWest))
            {
                if (_returnButton.HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);
                    _applyButton.GrabFocus();
                    _isNavigatingLeft = true;
                }
            }
        }

		private void GetPauseScreen()
		{
			var pauseScreens = GetTree().GetNodesInGroup("PauseScreen");

			if (pauseScreens != null && pauseScreens.Count > 0)
			{
				pauseScreen = pauseScreens.First() as PauseScreenManager;
			}

            if (pauseScreen != null)
            {
                var settingsBackground = FindChild("SettingsBackground") as TextureRect;

                settingsBackground.Hide();
            }
		}

		public void GrabFocusOfTopButton()
		{
            _musicVolumeSliderButton.GetFocusHolder().GrabFocus();
		}

        private void ApplyChanges()
        {
            if (_musicVolumeSliderButton.GetHSlider().Value != _rootSceneSwapper.CurrentSettings.MusicVolume)
            {
                _changedSettings.MusicVolume = (float)_musicVolumeSliderButton.GetHSlider().Value;

                //_rootSceneSwapper.
            }

            if (_dungeonSoundsVolumeSliderButton.GetHSlider().Value != _rootSceneSwapper.CurrentSettings.DungeonSoundsVolume)
            {
                _changedSettings.DungeonSoundsVolume = (float)_dungeonSoundsVolumeSliderButton.GetHSlider().Value;

                //_rootSceneSwapper.
            }

            ToggleFullscreen();

            if (_fullscreenButton.Text != "ON")
            {
                ResizeScreenToResolution();
            }

            SaveSettingsToConfigFile();

            _rootSceneSwapper.CurrentSettings = _changedSettings;

            _wasApplyClicked = true;
        }

        private void SaveSettingsToConfigFile()
        {
            // Create new ConfigFile object.
            var config = new ConfigFile();

            // Store some values.
            config.SetValue("Settings", "music_volume", _changedSettings.MusicVolume);
            config.SetValue("Settings", "menu_sounds_volume", _changedSettings.SoundEffectsVolume);
            config.SetValue("Settings", "dungeon_sounds_volume", _changedSettings.DungeonSoundsVolume);
            config.SetValue("Settings", "resolution", _changedSettings.Resolution);
            config.SetValue("Settings", "fullscreen_state", _changedSettings.FullscreenState);

            // Save it to a file (overwrite if already exists).
            var error = config.Save("user://settings.cfg");

            // If the file didn't load, ignore it.
            if (error != Error.Ok)
            {
                return;
            }
        }

        private void OnChanged_SoundEffectsVolumeSliderButton(double newValue)
        {
            _changedSettings.SoundEffectsVolume = (float)newValue;

            _rootSceneSwapper.ChangeMenuSoundsVolume(_changedSettings.SoundEffectsVolume);
        }

        private void ToggleFullscreen()
        {
            if (_fullscreenButton.Text == "ON")
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            }
            else
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                ResizeScreenToResolution();
            }

            _changedSettings.FullscreenState = _fullscreenButton.Text;
        }

        private void ResizeScreenToResolution()
        {
            DisplayServer.WindowSetSize(new Vector2I(_changedSettings.Resolution.X, _changedSettings.Resolution.Y));
        }

        private void ReturnToPriorScene()
        {
            if (!_wasApplyClicked)
            {
                _musicVolumeSliderButton.GetHSlider().Value = _rootSceneSwapper.CurrentSettings.MusicVolume;
                _soundEffectsVolumeSliderButton.GetHSlider().Value = _rootSceneSwapper.CurrentSettings.SoundEffectsVolume;
                _dungeonSoundsVolumeSliderButton.GetHSlider().Value = _rootSceneSwapper.CurrentSettings.DungeonSoundsVolume;

                _resolutionButton.Text = $"{_rootSceneSwapper.CurrentSettings.Resolution.X} x {_rootSceneSwapper.CurrentSettings.Resolution.Y}";

                _fullscreenButton.Text = _rootSceneSwapper.CurrentSettings.FullscreenState;
            }

            _wasApplyClicked = false;

            if (pauseScreen != null)
            {
                IsSettingsScreenBeingShown = false;

                pauseScreen.IsPauseScreenChildBeingShown = false;

                pauseScreen.ShowAllChildren();

                pauseScreen.GrabFocusOfSettingsButton();
            }
            else
            {
                _rootSceneSwapper.PriorSceneName = ScreenNames.Settings;

                EmitSignal(SignalName.GoToTitleScreen);
            }
        }
    }
}
