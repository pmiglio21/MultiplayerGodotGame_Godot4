using FrogGame.Enums;
using FrogGame.Globals;
using Godot;

namespace FrogGame.Levels.EarlyLevels.TitleLevel
{
	public partial class TitleLevelManager: Node3D
	{
		private Button _playGameButton;
		private Button _quitGameButton;

		public override void _Ready()
		{
			_playGameButton = GetNode<Button>("TitleToPlayerSelectButton");
			_quitGameButton = GetNode<Button>("QuitGameButton");

			_playGameButton.GrabFocus();
		}

		public override void _Process(double delta)
		{
			GetButtonPressInput();

			GetNavigationInput();
		}

		private void GetButtonPressInput()
		{
			if (UniversalInputHelper.IsButtonJustPressed(InputType.StartButton) || UniversalInputHelper.IsButtonJustPressed(InputType.SouthButton))
			{
				if (_playGameButton.HasFocus())
				{
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
			if (UniversalInputHelper.IsButtonJustPressed(InputType.MoveSouth))
			{
				if (_playGameButton.HasFocus())
				{
					_quitGameButton.GrabFocus();
				}
			}

			if (UniversalInputHelper.IsButtonJustPressed(InputType.MoveNorth))
			{
				if (_quitGameButton.HasFocus())
				{
					_playGameButton.GrabFocus();
				}
			}
		}
	}
}
