using Enums;
using Globals;
using Godot;
using Levels.OverworldLevels;
using System.Linq;

namespace Levels.UtilityLevels
{
	public partial class SettingsScreenManager : Node2D
	{
		public bool IsSettingsScreenBeingShown = false;

		private int _inputTimer = 0;
		private const int _inputTimerMax = 15;
		private bool _inputChangedRecently = false;

		protected PauseScreenManager pauseScreen;

		private Button _returnButton;

		public override void _Ready()
		{
			_returnButton = GetNode<Button>("ReturnButton");

			GetPauseScreen();

			if (pauseScreen == null)
			{
				IsSettingsScreenBeingShown = true;
			}

			_returnButton.GrabFocus();
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
			//Pause button hit by player
			if (!_inputChangedRecently && _inputTimer < _inputTimerMax)
			{
				_inputChangedRecently = true;
			}

			//Let timer go
			if (_inputChangedRecently && _inputTimer < _inputTimerMax)
			{
				_inputTimer++;
			}
			else
			{
				_inputChangedRecently = false;
			}

			if (UniversalInputHelper.IsActionJustPressed(InputType.StartButton) || UniversalInputHelper.IsActionJustPressed(InputType.SouthButton))
			{
				if (_returnButton.HasFocus())
				{
					if (!_inputChangedRecently)
					{
						ReturnToPriorScene();

						_inputTimer = 0;
					}
				}
			}

			if (UniversalInputHelper.IsActionJustPressed(InputType.EastButton))
			{
				if (!_inputChangedRecently)
				{
					ReturnToPriorScene();

					_inputTimer = 0;
				}
			}
		}

		private void GetNavigationInput()
		{
			if (UniversalInputHelper.IsActionJustPressed(InputType.MoveNorth))
			{
				//if (_returnButton.HasFocus())
				//{
				//	_settingsButton.GrabFocus();
				//}
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
			_returnButton.GrabFocus();
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
				GetTree().ChangeSceneToFile(LevelScenePaths.TitleLevelPath);
			}
		}
	}
}
