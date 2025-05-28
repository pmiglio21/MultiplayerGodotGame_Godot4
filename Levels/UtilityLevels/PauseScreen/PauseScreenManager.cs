using Enums;
using Globals;
using Godot;
using Root;


namespace Levels.UtilityLevels
{
	public partial class PauseScreenManager : Control
	{
		private RootSceneSwapper _rootSceneSwapper;
		private DungeonLevelSwapper _parentDungeonLevelSwapper;

		#region Signals

		[Signal]
		public delegate void GoToTitleScreenEventHandler();

		#endregion

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
			_rootSceneSwapper = GetTree().Root.GetNode<RootSceneSwapper>("RootSceneSwapper");
			_parentDungeonLevelSwapper = GetParent() as DungeonLevelSwapper;

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

			if (UniversalInputHelper.IsActionJustPressed(InputType.UiActionConfirm))
			{
				if (_resumeGameButton.HasFocus())
				{
					//Get pause input once timer lets up
					if (!_pauseChangedRecently)
					{
						_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);

						IsPauseScreenBeingShown = false;
					}
				}
				else if (_settingsButton.HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);

					_settingsScreen.IsSettingsScreenBeingShown = true;
					_settingsScreen.GrabFocusOfTopButton();

					IsPauseScreenChildBeingShown = true;

					HideAllButThisChildScreen(_settingsScreen);
				}
				else if (_quitGameButton.HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

					GetTree().Paused = false;

					this.Hide();

					EmitSignal(SignalName.GoToTitleScreen);
				}
			}

			if (UniversalInputHelper.IsActionJustPressed(InputType.UiActionCancel))
			{
				_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

				_resumeGameButton.GrabFocus();
			}
		}

		private void GetNavigationInput()
		{
			if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveSouth) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadSouth)))
			{
				if (_resumeGameButton.HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

					_settingsButton.GrabFocus();
				}
				else if (_settingsButton.HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

					_quitGameButton.GrabFocus();
				}

				_inputTimer.Start();
			}
			else if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveNorth) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadNorth)))
			{
				if (_quitGameButton.HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

					_settingsButton.GrabFocus();
				}
				else if (_settingsButton.HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

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
