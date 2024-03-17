using Enums;
using Globals;
using Globals.PlayerManagement;
using Godot;
using Levels.EarlyLevels;
using System;
using System.Linq;


namespace Levels.OverworldLevels
{
	public partial class PauseScreenManager : Node2D
	{
		public bool IsPauseScreenBeingShown = false;
		public bool IsPauseScreenChildBeingShown = false;

		private bool _pauseChangedRecently = false;
		private bool _isPaused = false;

		private int _pauseTimer = 0;
		private const int _pauseTimerMax = 15;

		private Button _resumeGameButton;
		private Button _settingsButton;
		private Button _quitGameButton;

		protected SettingsScreenManager settingsScreen;

		public override void _Ready()
		{
			_resumeGameButton = GetNode<Button>("ResumeGameButton");
			_settingsButton = GetNode<Button>("SettingsButton");
			_quitGameButton = GetNode<Button>("QuitGameButton");

			GetSettingsScreen();

			_resumeGameButton.GrabFocus();
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			if (IsPauseScreenBeingShown)
			{
				Show();
			}
			else
			{
				Hide();
				GetTree().Paused = false;
				_isPaused = false;
			}

			if (IsPauseScreenBeingShown && !IsPauseScreenChildBeingShown)
			{
				GetButtonInput();

				GetNavigationInput();
			}
		}

		private void GetButtonInput()
		{
			//Pause button hit by player
			if (!_pauseChangedRecently && _isPaused != GetTree().Paused && _pauseTimer < _pauseTimerMax)
			{
				_pauseChangedRecently = true;
				_isPaused = true;
			}

			//Let timer go
			if (_pauseChangedRecently && _pauseTimer < _pauseTimerMax)
			{
				_pauseTimer++;
			}
			else
			{
				_pauseChangedRecently = false;
				_pauseTimer = 0;
			}

			if (UniversalInputHelper.IsActionJustPressed(InputType.StartButton) || UniversalInputHelper.IsActionJustPressed(InputType.SouthButton))
			{
				if (_resumeGameButton.HasFocus())
				{
					//Get pause input once timer lets up
					if (!_pauseChangedRecently)
					{
						IsPauseScreenBeingShown = false;
					}
				}

				if (_settingsButton.HasFocus())
				{
					settingsScreen.IsSettingsScreenBeingShown = true;
					settingsScreen.GrabFocusOfTopButton();

					IsPauseScreenChildBeingShown = true;

					HideAllButThisChildScreen(settingsScreen);
				}

				if (_quitGameButton.HasFocus())
				{
					GetTree().Paused = false;
					PlayerManager.ClearActivePlayers();
					GlobalGameProperties.CurrentGameType = GameType.None;
					GetTree().ChangeSceneToFile(LevelScenePaths.TitleLevelPath);
				}
			}

			if (UniversalInputHelper.IsActionJustPressed(InputType.EastButton))
			{
				_resumeGameButton.GrabFocus();
			}
		}

		private void GetNavigationInput()
		{
			if (UniversalInputHelper.IsActionJustPressed(InputType.MoveSouth))
			{
				if (_resumeGameButton.HasFocus())
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
				else if (_settingsButton.HasFocus())
				{
					_resumeGameButton.GrabFocus();
				}
			}
		}

		private void GetSettingsScreen()
		{
			var settingsScreens = GetTree().GetNodesInGroup("SettingsScreen");

			if (settingsScreens != null)
			{
				settingsScreen = settingsScreens.First() as SettingsScreenManager;
			}
		}

		public void GrabFocusOfTopButton()
		{
			_resumeGameButton.GrabFocus();
		}

		public void GrabFocusOfSettingsButton()
		{
			_settingsButton.GrabFocus();
		}

		private void HideAllButThisChildScreen(Node childScreen)
		{
			foreach (Node child in this.GetChildren())
			{ 
				if (child.Name != childScreen.Name)
				{
					if (child is Node2D)
					{
						(child as Node2D).Hide();
					}
					else if (child is CanvasItem)
					{
						(child as CanvasItem).Hide();
					}
				}
			}
		}

		public void ShowAllChildren()
		{
			foreach (Node child in this.GetChildren())
			{
				if (child is Node2D)
				{
					(child as Node2D).Show();
				}
				else if (child is CanvasItem)
				{
					(child as CanvasItem).Show();
				}
			}
		}
	}
}
