using Enums;
using Globals;
using Globals.PlayerManagement;
using Godot;
using System;

public partial class PauseScreenManager : Node2D
{
	private bool _pauseChangedRecently = false;
	private bool _isPaused = false;

	private int _pauseTimer = 0;
	private const int _pauseTimerMax = 15;

	private Button _resumeGameButton;
	private Button _quitGameButton;

	public override void _Ready()
	{
		_resumeGameButton = GetNode<Button>("ResumeGameButton");
		_quitGameButton = GetNode<Button>("QuitGameButton");

		_resumeGameButton.GrabFocus();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GetButtonInput();

		GetNavigationInput();
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

		if (UniversalInputHelper.IsButtonJustPressed(InputType.StartButton) || UniversalInputHelper.IsButtonJustPressed(InputType.SouthButton))
		{
			if (_resumeGameButton.HasFocus())
			{
				//Get pause input once timer lets up
				if (!_pauseChangedRecently)
				{
					this.Hide();
					GetTree().Paused = false;
					_isPaused = false;
				}
			}

			if (_quitGameButton.HasFocus())
			{
				GetTree().Paused = false;
				PlayerManager.ClearActivePlayers();
				GetTree().ChangeSceneToFile(LevelScenePaths.TitleLevelPath);
			}
		}
	}

	private void GetNavigationInput()
	{
		if (UniversalInputHelper.IsButtonJustPressed(InputType.MoveSouth))
		{
			if (_resumeGameButton.HasFocus())
			{
				_quitGameButton.GrabFocus();
			}
		}

		if (UniversalInputHelper.IsButtonJustPressed(InputType.MoveNorth))
		{
			if (_quitGameButton.HasFocus())
			{
				_resumeGameButton.GrabFocus();
			}
		}
	}
}
