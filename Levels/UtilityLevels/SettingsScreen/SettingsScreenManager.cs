using Enums;
using Globals;
using Godot;
using Levels.UtilityLevels.UserInterfaceComponents;
using Models;
using Newtonsoft.Json;
using Root;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Levels.UtilityLevels
{
	public partial class SettingsScreenManager : Control
	{
        private RootSceneSwapper _rootSceneSwapper;

        #region Pause Screen Stuff

        public bool IsSettingsScreenBeingShown = false;

		protected PauseScreenManager pauseScreen;

        private Settings _changedSettings = new Settings();

        #endregion

        #region Components

        private Timer _inputTimer;
        private SliderButton _musicVolumeSliderButton;
        private SliderButton _soundEffectsVolumeSliderButton;
        private SliderButton _dungeonSoundsVolumeSliderButton;
        private OptionSelector _fullscreenOptionSelector;
        private Button _fullscreenButton;
        private Button _returnButton;

        #endregion

        [Signal]
        public delegate void GoToTitleScreenEventHandler();

        public override void _Ready()
		{
            _rootSceneSwapper = GetTree().Root.GetNode<RootSceneSwapper>("RootSceneSwapper");

            _inputTimer = FindChild("InputTimer") as Timer;

            _musicVolumeSliderButton = FindChild("MusicVolumeSliderButton") as SliderButton;

            _soundEffectsVolumeSliderButton = FindChild("SoundEffectsVolumeSliderButton") as SliderButton;
            _soundEffectsVolumeSliderButton.GetHSlider().ValueChanged += ((double newValue) => OnChanged_SoundEffectsVolumeSliderButton(_soundEffectsVolumeSliderButton.GetHSlider().Value));

            _dungeonSoundsVolumeSliderButton = FindChild("DungeonSoundsVolumeSliderButton") as SliderButton;

            _fullscreenOptionSelector = FindChild("FullscreenOptionSelector") as OptionSelector;
            _fullscreenButton = FindChild("FullscreenButton") as Button;
            _fullscreenButton.Text = _rootSceneSwapper.CurrentSettings.FullscreenState;
            _fullscreenButton.FocusEntered += _fullscreenOptionSelector.PlayOnFocusAnimation;
            _fullscreenButton.FocusExited += _fullscreenOptionSelector.PlayLoseFocusAnimation;

            _returnButton = FindChild("ReturnButton") as Button;

            _musicVolumeSliderButton.GetHSlider().Value = _rootSceneSwapper.CurrentSettings.MusicVolume;
            _soundEffectsVolumeSliderButton.GetHSlider().Value = _rootSceneSwapper.CurrentSettings.SoundEffectsVolume;
            _dungeonSoundsVolumeSliderButton.GetHSlider().Value = _rootSceneSwapper.CurrentSettings.DungeonSoundsVolume;

            GetPauseScreen();

            if (pauseScreen == null)
            {
                IsSettingsScreenBeingShown = true;
            }

            _changedSettings = new Settings()
            {
                MusicVolume = _rootSceneSwapper.CurrentSettings.MusicVolume,
                SoundEffectsVolume = _rootSceneSwapper.CurrentSettings.SoundEffectsVolume,
                DungeonSoundsVolume = _rootSceneSwapper.CurrentSettings.DungeonSoundsVolume,
                FullscreenState = _rootSceneSwapper.CurrentSettings.FullscreenState
            };

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
            if (UniversalInputHelper.IsActionJustPressed(InputType.UiActionConfirm))
            {
                if (_musicVolumeSliderButton.GetHSlider().HasFocus())
                {
                    _musicVolumeSliderButton.GetFocusHolder().GrabFocus();
                }
                else if (_soundEffectsVolumeSliderButton.GetHSlider().HasFocus())
                {
                    _soundEffectsVolumeSliderButton.GetFocusHolder().GrabFocus();
                }
                else if (_dungeonSoundsVolumeSliderButton.GetHSlider().HasFocus())
                {
                    _dungeonSoundsVolumeSliderButton.GetFocusHolder().GrabFocus();
                }
                else if (_musicVolumeSliderButton.GetFocusHolder().HasFocus())
                {
                    _musicVolumeSliderButton.GetHSlider().GrabFocus();
                }
                else if (_soundEffectsVolumeSliderButton.GetFocusHolder().HasFocus())
                {
                    _soundEffectsVolumeSliderButton.GetHSlider().GrabFocus();
                }
                else if (_dungeonSoundsVolumeSliderButton.GetFocusHolder().HasFocus())
                {
                    _dungeonSoundsVolumeSliderButton.GetHSlider().GrabFocus();
                }
                else if (_fullscreenButton.HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

                    _fullscreenOptionSelector.PlayClickedOnRightArrow();
                    _fullscreenOptionSelector.PlayClickedOnLeftArrow();

                    if (_fullscreenButton.Text == GlobalConstants.OffOnOptionOff)
                    {
                        _fullscreenButton.Text = GlobalConstants.OffOnOptionOn;
                    }
                    else
                    {
                        _fullscreenButton.Text = GlobalConstants.OffOnOptionOff;
                    }

                    _changedSettings.FullscreenState = _fullscreenButton.Text;
                }
                else if (_returnButton.HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

                    ReturnToPriorScene();
                }
            }
            else if (UniversalInputHelper.IsActionJustPressed(InputType.UiActionCancel))
            {
                if (_musicVolumeSliderButton.GetHSlider().HasFocus())
                {
                    _musicVolumeSliderButton.GetFocusHolder().GrabFocus();
                }
                else if (_soundEffectsVolumeSliderButton.GetHSlider().HasFocus())
                {
                    _soundEffectsVolumeSliderButton.GetFocusHolder().GrabFocus();
                }
                else if (_dungeonSoundsVolumeSliderButton.GetHSlider().HasFocus())
                {
                    _dungeonSoundsVolumeSliderButton.GetFocusHolder().GrabFocus();
                }
                else
                {
                    ReturnToPriorScene();
                }
            }
            else if (UniversalInputHelper.IsActionJustPressed(InputType.GameActionPause))
            {
                if (_returnButton.HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

                    ReturnToPriorScene();
                }
            }
            else if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionJustPressed(InputType.MoveEast) || UniversalInputHelper.IsActionJustPressed_GamePadOnly(InputType.DPadEast)))
            {
               if (_fullscreenButton.HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

                    _fullscreenOptionSelector.PlayClickedOnRightArrow();
                    _fullscreenOptionSelector.PlayClickedOnLeftArrow();

                    if (_fullscreenButton.Text == GlobalConstants.OffOnOptionOff)
                    {
                        _fullscreenButton.Text = GlobalConstants.OffOnOptionOn;
                    }
                    else
                    {
                        _fullscreenButton.Text = GlobalConstants.OffOnOptionOff;
                    }

                    _changedSettings.FullscreenState = _fullscreenButton.Text;
                }
            }
            else if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionJustPressed(InputType.MoveWest) || UniversalInputHelper.IsActionJustPressed_GamePadOnly(InputType.DPadWest)))
            {
                if (_fullscreenButton.HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

                    _fullscreenOptionSelector.PlayClickedOnRightArrow();
                    _fullscreenOptionSelector.PlayClickedOnLeftArrow();

                    if (_fullscreenButton.Text == GlobalConstants.OffOnOptionOff)
                    {
                        _fullscreenButton.Text = GlobalConstants.OffOnOptionOn;
                    }
                    else
                    {
                        _fullscreenButton.Text = GlobalConstants.OffOnOptionOff;
                    }

                    _changedSettings.FullscreenState = _fullscreenButton.Text;
                }
            }
        }

		private void GetNavigationInput()
		{
            if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionJustPressed(InputType.MoveSouth) || UniversalInputHelper.IsActionJustPressed_GamePadOnly(InputType.DPadSouth)))
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

                    _fullscreenButton.GrabFocus();
                }
                else if (_fullscreenButton.HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);

                    _returnButton.GrabFocus();
                }

                _inputTimer.Start();
            }
            else if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionJustPressed(InputType.MoveNorth) || UniversalInputHelper.IsActionJustPressed_GamePadOnly(InputType.DPadNorth)))
            {
                if (_returnButton.HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);
                    _fullscreenButton.GrabFocus();
                }
                else if (_fullscreenButton.HasFocus())
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

        private void SaveSettingsToConfigFile()
        {
            // Create new ConfigFile object.
            var config = new ConfigFile();

            // Store some values.
            config.SetValue("Settings", "music_volume", _rootSceneSwapper.CurrentSettings.MusicVolume);
            config.SetValue("Settings", "menu_sounds_volume", _rootSceneSwapper.CurrentSettings.SoundEffectsVolume);
            config.SetValue("Settings", "dungeon_sounds_volume", _rootSceneSwapper.CurrentSettings.DungeonSoundsVolume);
            config.SetValue("Settings", "fullscreen_state", _rootSceneSwapper.CurrentSettings.FullscreenState);

            // Save it to a file (overwrite if already exists).
            var error = config.Save(PersistentFilePaths.GameSettingsFilePath);

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

            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiVolumeSliderTickSoundPath);
        }

        private void ToggleFullscreen()
        {
            if (_fullscreenButton.Text == GlobalConstants.OffOnOptionOn)
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            }
            else
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                ResizeScreenToResolution();
            }

            _rootSceneSwapper.CurrentSettings.FullscreenState = _fullscreenButton.Text;
        }

        private void ResizeScreenToResolution()
        {
            DisplayServer.WindowSetSize(GlobalConstants.DefaultResolution);
        }

        private void ReturnToPriorSceneTimeout()
        {
            ReturnToPriorScene();
        }

        private void ReturnToPriorScene()
        {
            if (JsonConvert.SerializeObject(_rootSceneSwapper.CurrentSettings) != JsonConvert.SerializeObject(_changedSettings))
            {
                _rootSceneSwapper.CurrentSettings = new Settings()
                { 
                    MusicVolume = _changedSettings.MusicVolume,
                    SoundEffectsVolume = _changedSettings.SoundEffectsVolume,
                    DungeonSoundsVolume = _changedSettings.DungeonSoundsVolume,
                    FullscreenState = _changedSettings.FullscreenState
                };

                SaveSettingsToConfigFile();

                ResizeScreenToResolution();

                ToggleFullscreen();
            }

            if (pauseScreen != null)
            {
                IsSettingsScreenBeingShown = false;

                pauseScreen.IsPauseScreenChildBeingShown = false;

                _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

                pauseScreen.ShowAllChildren();

                pauseScreen.GrabFocusOfSettingsButton();
            }
            else
            {
                _rootSceneSwapper.PriorSceneName = ScreenNames.Settings;

                _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

                EmitSignal(SignalName.GoToTitleScreen);
            }
        }

        #region Signal Receptions

        public void OnFullscreenOptionSelector_EitherArrowClicked()
        {
            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

            if (_fullscreenButton.Text == GlobalConstants.OffOnOptionOff)
            {
                _fullscreenButton.Text = GlobalConstants.OffOnOptionOn;
            }
            else
            {
                _fullscreenButton.Text = GlobalConstants.OffOnOptionOff;
            }
        }

        public void OnReturnButtonPressed()
        {
            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

            ReturnToPriorScene();
        }

        #endregion
    }
}
