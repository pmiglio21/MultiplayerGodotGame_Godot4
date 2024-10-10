using Enums;
using Globals;
using Godot;

namespace Levels.EarlyLevels
{
	public partial class TitleScreenManager: Node
	{
		private Timer _inputTimer;
		private Button _playButton;
		private Button _gameRulesButton;
		private Button _settingsButton;
		private Button _quitGameButton;

		public override void _Ready()
		{
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
                    GetTree().ChangeSceneToFile(LevelScenePaths.PlayModeScreenPath);
                }
                else if (_gameRulesButton.HasFocus())
                {
                    GlobalGameComponents.PriorSceneName = LevelScenePaths.TitleScreenPath;
                    GetTree().ChangeSceneToFile(LevelScenePaths.GameRulesScreenPath);
                }
                else if (_settingsButton.HasFocus())
                {
                    GetTree().ChangeSceneToFile(LevelScenePaths.SettingsScreenPath);
                }
                else if (_quitGameButton.HasFocus())
                {
                    GetTree().Quit();
                }
            }
        }

		private void GetNavigationInput()
		{
			if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveSouth) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadSouth)))
			{
				if (_playButton.HasFocus())
				{
					_gameRulesButton.GrabFocus();
				}
				else if (_gameRulesButton.HasFocus())
				{
					_settingsButton.GrabFocus();
				}
				else if (_settingsButton.HasFocus())
				{
					_quitGameButton.GrabFocus();
				}

				_inputTimer.Start();
			}
			else if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveNorth) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadNorth)))
			{
				if (_quitGameButton.HasFocus())
				{
					_settingsButton.GrabFocus();
				}
				else if (_settingsButton.HasFocus())
				{
					_gameRulesButton.GrabFocus();
				}
				else if (_gameRulesButton.HasFocus())
				{
					_playButton.GrabFocus();
				}

				_inputTimer.Start();
			}
		}
	}
}
