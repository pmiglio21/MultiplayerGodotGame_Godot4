using Enums;
using Globals;
using Godot;
using Root;
using System;

public partial class PlayModeScreenManager : GridContainer
{
    private RootSceneSwapper _rootSceneSwapper;

    #region Components

    private Button _localButton;
    private Button _onlineButton;

    #endregion

    #region Signals

    [Signal]
    public delegate void GoToTitleScreenEventHandler();

    [Signal]
    public delegate void GoToGameRulesScreenEventHandler();

    #endregion

    public override void _Ready()
    {
        _rootSceneSwapper = GetTree().Root.GetNode<RootSceneSwapper>("RootSceneSwapper");

        _localButton = FindChild("LocalButton") as Button;
        _onlineButton = FindChild("OnlineButton") as Button;

        _localButton.GrabFocus();
    }

    public override void _Process(double delta)
    {
        GetButtonPressInput();

        GetNavigationInput();
    }

    private void GetButtonPressInput()
    {
        if (UniversalInputHelper.IsActionJustPressed(InputType.UiActionConfirm))
        {
            if (_localButton.HasFocus())
            {
                _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);

                _rootSceneSwapper.PriorSceneName = ScreenNames.PlayMode;

                EmitSignal(SignalName.GoToGameRulesScreen);
            }
        }

        if (UniversalInputHelper.IsActionJustPressed(InputType.UiActionCancel))
        {
            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

            _rootSceneSwapper.PriorSceneName = ScreenNames.PlayMode;

            EmitSignal(SignalName.GoToTitleScreen);
        }
    }

    private void GetNavigationInput()
    {
        if (UniversalInputHelper.IsActionJustPressed(InputType.MoveEast) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadEast))
        {
            if (_localButton.HasFocus())
            {
                _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);

                _onlineButton.GrabFocus();
            }
        }

        if (UniversalInputHelper.IsActionJustPressed(InputType.MoveWest) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadWest))
        {
            if (_onlineButton.HasFocus())
            {
                _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);

                _localButton.GrabFocus();
            }
        }
    }

    public void GrabFocusOfFirstButton()
    {
        _localButton.GrabFocus();
    }
}
