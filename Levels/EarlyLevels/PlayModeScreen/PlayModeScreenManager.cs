using Enums;
using Globals;
using Godot;
using Root;
using System;

public partial class PlayModeScreenManager : GridContainer
{
    private RootSceneSwapper _rootSceneSwapper;

    private Button _localButton;
    private Button _onlineButton;

    [Signal]
    public delegate void GoToTitleScreenEventHandler();

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
        if (UniversalInputHelper.IsActionJustPressed(InputType.StartButton) || UniversalInputHelper.IsActionJustPressed(InputType.SouthButton))
        {
            if (_localButton.HasFocus())
            {
                GlobalGameComponents.PriorSceneName = LevelScenePaths.PlayModeScreenPath;
                GetTree().ChangeSceneToFile(LevelScenePaths.GameRulesScreenPath);
            }
        }

        if (UniversalInputHelper.IsActionJustPressed(InputType.StartButton) || UniversalInputHelper.IsActionJustPressed(InputType.EastButton))
        {
            if (_localButton.HasFocus())
            {
                EmitSignal(SignalName.GoToTitleScreen);
                //GetTree().ChangeSceneToFile(LevelScenePaths.TitleScreenPath);
            }
        }
    }

    private void GetNavigationInput()
    {
        if (UniversalInputHelper.IsActionJustPressed(InputType.MoveEast) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadEast))
        {
            if (_localButton.HasFocus())
            {
                _onlineButton.GrabFocus();
                GD.Print("Reached online");
            }
        }

        if (UniversalInputHelper.IsActionJustPressed(InputType.MoveWest) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadWest))
        {
            if (_onlineButton.HasFocus())
            {
                _localButton.GrabFocus();
                GD.Print("Reached local");
            }
        }
    }
}
