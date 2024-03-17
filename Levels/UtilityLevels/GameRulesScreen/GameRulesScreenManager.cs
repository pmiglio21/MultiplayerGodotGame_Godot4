using Enums;
using Globals;
using Godot;
using Levels.OverworldLevels;
using System.Linq;

namespace Levels.EarlyLevels
{
	public partial class GameRulesScreenManager : Node2D
	{
		protected PauseScreenManager pauseScreen;

		private Button _screenMergingButton;
		private Button _returnButton;

		public override void _Ready()
		{
			_screenMergingButton = GetNode<Button>("ToScreenMergingButton");
			_returnButton = GetNode<Button>("ReturnButton");

			_screenMergingButton.GrabFocus();
		}

		public override void _Process(double delta)
		{
			GetButtonInput();

			GetNavigationInput();
		}

		private void GetButtonInput()
		{
			if (UniversalInputHelper.IsActionJustPressed(InputType.StartButton) || UniversalInputHelper.IsActionJustPressed(InputType.SouthButton))
			{
				if (_screenMergingButton.HasFocus())
				{
					//GlobalGameProperties.CurrentGameType = GameType.LocalCompetitive;
					//GetTree().ChangeSceneToFile(LevelScenePaths.PlayerSelectLevelPath);
				}
				else if (_returnButton.HasFocus())
				{
					ReturnToPriorScene();
				}
			}

			if (UniversalInputHelper.IsActionJustPressed(InputType.EastButton))
			{
				ReturnToPriorScene();
			}
		}

		private void GetNavigationInput()
		{
			if (UniversalInputHelper.IsActionJustPressed(InputType.MoveSouth))
			{
				if (_screenMergingButton.HasFocus())
				{
					_returnButton.GrabFocus();
				}
			}

			if (UniversalInputHelper.IsActionJustPressed(InputType.MoveNorth))
			{
				if (_returnButton.HasFocus())
				{
					_screenMergingButton.GrabFocus();
				}
			}
		}

		public void GrabFocusOfTopButton()
		{
			_screenMergingButton.GrabFocus();
		}

		private void ReturnToPriorScene()
		{
			GetTree().ChangeSceneToFile(LevelScenePaths.TitleLevelPath);
		}
	}
}
