using Enums;
using Globals;
using Godot;
using Root;
using System.Linq;

namespace Levels.UtilityLevels
{
	public partial class SettingsScreenManager : Control
	{
        private RootSceneSwapper _rootSceneSwapper;

        public bool IsSettingsScreenBeingShown = false;

		protected PauseScreenManager pauseScreen;

        #region Components

        private Timer _inputTimer;
		private HSlider _masterVolumeSlider;
        private HSlider _musicVolumeSlider;
        private HSlider _soundEffectsVolumeSlider;
        private HSlider _uiVolumeSlider;
		private OptionButton _resolutionOptionButton;
		private CheckButton _fullscreenToggle;
        private Button _returnButton;

        #endregion

        [Signal]
        public delegate void GoToTitleScreenEventHandler();

        public override void _Ready()
		{
            _rootSceneSwapper = GetTree().Root.GetNode<RootSceneSwapper>("RootSceneSwapper");

            _inputTimer = FindChild("InputTimer") as Timer;
            _masterVolumeSlider = FindChild("MasterVolumeSlider") as HSlider;
            _musicVolumeSlider = FindChild("MasterVolumeSlider") as HSlider;
            _soundEffectsVolumeSlider = FindChild("SoundEffectsVolumeSlider") as HSlider;
            _uiVolumeSlider = FindChild("UiVolumeSlider") as HSlider;
            _resolutionOptionButton = FindChild("ResolutionOptionButton") as OptionButton;
            _fullscreenToggle = FindChild("FullscreenToggle") as CheckButton;
            _returnButton = FindChild("ReturnButton") as Button;

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
				if (_fullscreenToggle.HasFocus())
				{
					ToggleFullscreen();
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
                if (_masterVolumeSlider.HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);
                    _musicVolumeSlider.GrabFocus();
                }
                else if (_musicVolumeSlider.HasFocus())
                {
                    _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);
                    _uiVolumeSlider.GrabFocus();
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
		}

		public void GrabFocusOfTopButton()
		{
			_masterVolumeSlider.GrabFocus();
		}

		private void ReturnToPriorScene()
		{
			IsSettingsScreenBeingShown = false;

			if (pauseScreen != null)
			{
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

		private void ToggleFullscreen()
		{
            DisplayServer.WindowMode currentWindowMode = DisplayServer.WindowGetMode();
            DisplayServer.WindowMode previousWindowMode = DisplayServer.WindowGetMode();

            if (currentWindowMode != DisplayServer.WindowMode.Fullscreen)
            {
                previousWindowMode = currentWindowMode;

                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            }
            else
            {
                if (previousWindowMode == DisplayServer.WindowMode.Fullscreen)
                {
                    previousWindowMode = DisplayServer.WindowMode.Windowed;
                }

                DisplayServer.WindowSetMode(previousWindowMode);
            }
        }
	}
}
