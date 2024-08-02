using Enums;
using Globals;
using Godot;
using System;

public partial class PlayModeScreenManager : GridContainer
{
    private Button _localButton;
    private Button _onlineButton;

    public override void _Ready()
    {
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
                GetTree().ChangeSceneToFile(LevelScenePaths.PlayerSelectScreenPath);
            }
        }

        if (UniversalInputHelper.IsActionJustPressed(InputType.StartButton) || UniversalInputHelper.IsActionJustPressed(InputType.EastButton))
        {
            if (_localButton.HasFocus())
            {
                GetTree().ChangeSceneToFile(LevelScenePaths.TitleScreenPath);
            }
        }
    }

    private void GetNavigationInput()
    {
        if (UniversalInputHelper.IsActionJustPressed(InputType.MoveEast))
        {
            if (_localButton.HasFocus())
            {
                _onlineButton.GrabFocus();
            }
        }

        if (UniversalInputHelper.IsActionJustPressed(InputType.MoveWest))
        {
            if (_onlineButton.HasFocus())
            {
                _localButton.GrabFocus();
            }
        }
    }
}
