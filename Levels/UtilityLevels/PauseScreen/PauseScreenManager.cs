using Enums;
using Globals;
using Globals.PlayerManagement;
using Godot;
using Levels.UtilityLevels;
using Enums.GameRules;
using System;
using System.Linq;


namespace Levels.UtilityLevels
{
	public partial class PauseScreenManager : Control
	{
		public bool IsPauseScreenBeingShown = false;
		public bool IsPauseScreenChildBeingShown = false;

		//Necessary to keep pause screen from immediately unpausing
		private bool _isPaused = false;
		private int _pauseTimer = 0;
		private const int _pauseTimerMax = 15;
		private bool _pauseChangedRecently = false;

        private Timer _inputTimer;
        private Button _resumeGameButton;
		private Button _settingsButton;
		private Button _quitGameButton;

		private SettingsScreenManager _settingsScreen;

		public override void _Ready()
        {
            _inputTimer = FindChild("InputTimer") as Timer;
            _resumeGameButton = GetNode<Button>("ResumeGameButton");
			_settingsButton = GetNode<Button>("SettingsButton");
			_quitGameButton = GetNode<Button>("QuitGameButton");

			_settingsScreen = GetNode<SettingsScreenManager>("SettingsScreen");

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
				GetButtonPressInput();

				GetNavigationInput();
			}
		}

		private void GetButtonPressInput()
		{
			#region Pause Timer Setup
			//Pause button hit by player
			if (!_pauseChangedRecently && _isPaused != GetTree().Paused && _pauseTimer < _pauseTimerMax)
			{
				_pauseChangedRecently = true;
				_isPaused = true;
			}

			//Let pause timer go
			if (_pauseChangedRecently && _pauseTimer < _pauseTimerMax)
			{
				_pauseTimer++;
			}
			else
			{
				_pauseChangedRecently = false;
				_pauseTimer = 0;
			}
			#endregion

			if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.StartButton) || UniversalInputHelper.IsActionPressed(InputType.SouthButton)))
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
					_settingsScreen.IsSettingsScreenBeingShown = true;
					_settingsScreen.GrabFocusOfTopButton();

					IsPauseScreenChildBeingShown = true;

					HideAllButThisChildScreen(_settingsScreen);
				}

				if (_quitGameButton.HasFocus())
				{
					GetTree().Paused = false;
					PlayerManager.ActivePlayers.Clear();
					GetTree().ChangeSceneToFile(LevelScenePaths.TitleScreenPath);
				}

                _inputTimer.Start();
            }

			if (_inputTimer.IsStopped() && UniversalInputHelper.IsActionPressed(InputType.EastButton))
			{
				_resumeGameButton.GrabFocus();

                _inputTimer.Start();
            }
		}

		private void GetNavigationInput()
		{
			if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveSouth) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadSouth)))
			{
				if (_resumeGameButton.HasFocus())
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
					_resumeGameButton.GrabFocus();
				}

                _inputTimer.Start();
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
