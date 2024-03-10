using Enums;
using Globals;
using Godot;

namespace Levels.EarlyLevels
{
	public partial class SettingsLevelManager : Node
	{
		private Button _settingsButton;
		private Button _returnButton;

		public override void _Ready()
		{
			_settingsButton = GetNode<Button>("SettingsToScreenMergingButton");
			_returnButton = GetNode<Button>("ReturnButton");

			_settingsButton.GrabFocus();
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
				if (_settingsButton.HasFocus())
				{
					//GlobalGameProperties.CurrentGameType = GameType.LocalCompetitive;
					//GetTree().ChangeSceneToFile(LevelScenePaths.PlayerSelectLevelPath);
				}
				else if (_returnButton.HasFocus())
				{
					//GetTree().Quit();
				}
			}
		}

		private void GetNavigationInput()
		{
			if (UniversalInputHelper.IsActionJustPressed(InputType.MoveSouth))
			{
				if (_settingsButton.HasFocus())
				{
					_returnButton.GrabFocus();
				}
			}

			if (UniversalInputHelper.IsActionJustPressed(InputType.MoveNorth))
			{
				if (_returnButton.HasFocus())
				{
					_settingsButton.GrabFocus();
				}
			}
		}
	}
}
