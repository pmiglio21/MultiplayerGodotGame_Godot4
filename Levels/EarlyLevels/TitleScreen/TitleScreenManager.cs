using Enums;
using Globals;
using Godot;
using Root;

namespace Levels.EarlyLevels
{
	public partial class TitleScreenManager: Control
	{
		private RootSceneSwapper _rootSceneSwapper;

		private Timer _inputTimer;
		private Button _playButton;
		private Button _gameRulesButton;
		private Button _settingsButton;
		private Button _quitGameButton;

		#region Signals

		//[Signal]
		//public delegate void GoToPlayModeScreenEventHandler();

		[Signal]
        public delegate void GoToPlayerCharacterSelectScreenEventHandler();

        [Signal]
		public delegate void GoToGameRulesScreenEventHandler();

		[Signal]
		public delegate void GoToSettingsScreenEventHandler();

		[Signal]
		public delegate void QuitGameEventHandler();

		#endregion

		public override void _Ready()
		{
			_rootSceneSwapper = GetTree().Root.GetNode<RootSceneSwapper>("RootSceneSwapper");
			
			_inputTimer = FindChild("InputTimer") as Timer;
			_playButton = FindChild("PlayButton") as Button;
			_gameRulesButton = FindChild("GameRulesButton") as Button;
			_settingsButton = FindChild("SettingsButton") as Button;
			_quitGameButton = FindChild("QuitGameButton") as Button;

			_playButton.GrabFocus();
		}

		public override void _Process(double delta)
		{
			GetButtonPressInput();

			GetNavigationInput();
		}

		private void GetButtonPressInput()
		{
			if (UniversalInputHelper.IsActionJustPressed(InputType.StartButton) || UniversalInputHelper.IsActionJustPressed(InputType.SouthButton))
			{
				if (_playButton.HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);

					EmitSignal(SignalName.GoToPlayerCharacterSelectScreen);
				}
				else if (_gameRulesButton.HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);

					_rootSceneSwapper.PriorSceneName = ScreenNames.Title;

					EmitSignal(SignalName.GoToGameRulesScreen);
				}
				else if (_settingsButton.HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);

					EmitSignal(SignalName.GoToSettingsScreen);
				}
				else if (_quitGameButton.HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);

					EmitSignal(SignalName.QuitGame);
				}
			}
		}

		private void GetNavigationInput()
		{
			if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveSouth) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadSouth)))
			{
				if(_playButton.HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

					_gameRulesButton.GrabFocus();
				}
				else if (_gameRulesButton.HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

					_settingsButton.GrabFocus();
				}
				else if (_settingsButton.HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

					_quitGameButton.GrabFocus();
				}

				_inputTimer.Start();
			}
			else if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveNorth) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadNorth)))
			{
				if (_quitGameButton.HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

					_settingsButton.GrabFocus();
				}
				else if (_settingsButton.HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

					_gameRulesButton.GrabFocus();
				}
				else if (_gameRulesButton.HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

					_playButton.GrabFocus();
				}

				_inputTimer.Start();
			}
		}

		public void GrabFocusOfTopButton()
		{
			_playButton.GrabFocus();
		}
	}
}
