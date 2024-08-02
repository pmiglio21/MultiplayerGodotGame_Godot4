using Enums;
using Globals;
using Godot;

namespace Levels.EarlyLevels
{
	public partial class TitleScreenManager: Node
	{
		private Button _playButton;
		private Button _gameRulesButton;
		private Button _settingsButton;
		private Button _quitGameButton;

		public override void _Ready()
		{
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
			if (UniversalInputHelper.IsActionJustPressed(InputType.MoveSouth))
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
			}

			if (UniversalInputHelper.IsActionJustPressed(InputType.MoveNorth))
			{
				if (_quitGameButton.HasFocus())
				{
					_settingsButton.GrabFocus();
				}
				if (_settingsButton.HasFocus())
				{
					_gameRulesButton.GrabFocus();
				}
				else if (_gameRulesButton.HasFocus())
				{
					_playButton.GrabFocus();
				}
			}
		}
	}
}
