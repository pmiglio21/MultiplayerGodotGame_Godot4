using Enums;
using Globals;
using Godot;
using Enums.GameRules;

namespace Levels.EarlyLevels
{
	public partial class TitleLevelManager: Node
	{
		private Button _localButton;
		private Button _onlineButton;
		private Button _gameRulesButton;
		private Button _settingsButton;
		private Button _quitGameButton;

		public override void _Ready()
		{
			_localButton = GetNode<Button>("LocalButton");
			_onlineButton = GetNode<Button>("OnlineButton");
			_gameRulesButton = GetNode<Button>("GameRulesButton");
			_settingsButton = GetNode<Button>("SettingsButton");
			_quitGameButton = GetNode<Button>("QuitGameButton");

			_localButton.GrabFocus();
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
				if (_localButton.HasFocus())
				{
					GetTree().ChangeSceneToFile(LevelScenePaths.PlayerSelectLevelPath);
				}
				//else if (_onlineButton.HasFocus())
				//{
				//	GetTree().ChangeSceneToFile(LevelScenePaths.PlayerSelectLevelPath);
				//}
				else if (_gameRulesButton.HasFocus())
				{
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
				if (_localButton.HasFocus())
				{
					_onlineButton.GrabFocus();
				}
				else if (_onlineButton.HasFocus())
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
					_onlineButton.GrabFocus();
				}
				else if (_onlineButton.HasFocus())
				{
					_localButton.GrabFocus();
				}
			}
		}
	}
}
