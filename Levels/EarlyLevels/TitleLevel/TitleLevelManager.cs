using Enums;
using Globals;
using Godot;

namespace Levels.EarlyLevels.TitleLevel
{
	public partial class TitleLevelManager: Node
	{
		private Button _competitiveGameButton;
		private Button _coopGameButton;
		private Button _quitGameButton;

		public override void _Ready()
		{
			_competitiveGameButton = GetNode<Button>("TitleToCompetitivePlayerSelectButton");
			_coopGameButton = GetNode<Button>("TitleToCoopPlayerSelectButton");
			_quitGameButton = GetNode<Button>("QuitGameButton");

			_competitiveGameButton.GrabFocus();
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
				if (_competitiveGameButton.HasFocus())
				{
					GlobalGameProperties.CurrentGameType = GameType.Competitive;
					GetTree().ChangeSceneToFile(LevelScenePaths.PlayerSelectLevelPath);
				}

				if (_coopGameButton.HasFocus())
				{
					GlobalGameProperties.CurrentGameType = GameType.Coop;
					GetTree().ChangeSceneToFile(LevelScenePaths.PlayerSelectLevelPath);
				}

				if (_quitGameButton.HasFocus())
				{
					GetTree().Quit();
				}
			}
		}

		private void GetNavigationInput()
		{
			if (UniversalInputHelper.IsActionJustPressed(InputType.MoveSouth))
			{
				if (_competitiveGameButton.HasFocus())
				{
					_coopGameButton.GrabFocus();
				}
				else if (_coopGameButton.HasFocus())
				{
					_quitGameButton.GrabFocus();
				}
			}

			if (UniversalInputHelper.IsActionJustPressed(InputType.MoveNorth))
			{
				if (_quitGameButton.HasFocus())
				{
					_coopGameButton.GrabFocus();
				}
				else if (_coopGameButton.HasFocus())
				{
					_competitiveGameButton.GrabFocus();
				}
			}
		}
	}
}
