using Enums;
using Globals;
using Godot;
using Levels.OverworldLevels;
using System.Linq;

namespace Levels.EarlyLevels
{
	public partial class SettingsScreenManager : Node2D
	{
		public bool IsSettingsScreenEnabled = false;

		protected PauseScreenManager pauseScreen;

		private Button _settingsButton;
		private Button _returnButton;

		public override void _Ready()
		{
			_settingsButton = GetNode<Button>("SettingsToScreenMergingButton");
			_returnButton = GetNode<Button>("ReturnButton");

			GetPauseScreen();

			if (pauseScreen == null)
            {
				IsSettingsScreenEnabled = true;
            }

			_settingsButton.GrabFocus();
		}

		public override void _Process(double delta)
		{
			if (IsSettingsScreenEnabled)
			{
				Show();
			}
			else
			{
				Hide();
			}

			if (IsSettingsScreenEnabled)
            {
				GetButtonPressInput();

				GetNavigationInput();
			}
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
			_settingsButton.GrabFocus();
        }

		private void ReturnToPriorScene()
        {
			IsSettingsScreenEnabled = false;

			if (pauseScreen != null)
			{
				pauseScreen.Show();

				pauseScreen.GrabFocusOfTopButton();
			}
			else
			{
				GetTree().ChangeSceneToFile(GlobalGameProperties.PriorScene);
			}
		}
	}
}
