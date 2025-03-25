using Globals;
using Godot;
using Levels.OverworldLevels;
using Levels.UtilityLevels;
using Levels.UtilityLevels.UserInterfaceComponents;
using MobileEntities.PlayerCharacters.Scripts;
using Models;
using Root;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DungeonLevelSwapper : Node
{
	private RootSceneSwapper _rootSceneSwapper;

    private CanvasLayer _canvasLayer;
    private List<Button> _hudButtons = new List<Button>();

    private PauseScreenManager _pauseScreenManager;
    private PortalTimer _portalTimer;
    private SplitScreenManager _latestSplitScreenManager;
    private BaseDungeonLevel _latestBaseDungeonLevel;
    private PlayerUpgradesScreenManager _latestPlayerUpgradesScreenManager;

    public Vector2 MaxScreenSize = new Vector2();

    #region Signals

    [Signal]
    public delegate void GoToTitleScreenEventHandler();

    [Signal]
    public delegate void GoToGameOverScreenEventHandler();

    #endregion

    public GameRules CurrentGameRules = new GameRules();
    public List<BaseCharacter> ActivePlayers = new List<BaseCharacter>();

    private int _levelCounter = 0;
    private bool _allSwitchesActivated = false;


    public override void _Ready()
	{
		_rootSceneSwapper = GetTree().Root.GetNode<RootSceneSwapper>("RootSceneSwapper");
		CurrentGameRules = _rootSceneSwapper.CurrentGameRules;
        MaxScreenSize = _rootSceneSwapper.MaxScreenSize;

        _pauseScreenManager = FindChild("PauseScreen") as PauseScreenManager;
        _pauseScreenManager.GoToTitleScreen += ChangeScreenToTitleScreen;

        _portalTimer = FindChild("PortalTimer") as PortalTimer;
        _portalTimer.GetTimer().Timeout += RunPortalTimerTimeoutProcess;

        _latestSplitScreenManager = GetNode<SplitScreenManager>("SplitScreenManager");

		_latestBaseDungeonLevel = _latestSplitScreenManager.FindChild("Level") as BaseDungeonLevel;
		_latestBaseDungeonLevel.GoToGameOverScreen += ChangeSceneToGameOverScreen;
        _latestBaseDungeonLevel.ActivatePortal += RunPortalActivationProcess;

        _canvasLayer = FindChild("CanvasLayer") as CanvasLayer;
        _hudButtons = GetCanvasLayerButtons();
        MoveButtonsAround();

        GetTree().Root.SizeChanged += ResizeUI;
    }

    private List<Button> GetCanvasLayerButtons()
    {
        List<Button> hudButtons = new List<Button>();

        int buttonCount = 1;

        while (buttonCount < 5)
        {
            Button button = _canvasLayer.FindChild($"Button{buttonCount}") as Button;
            
            if (this.ActivePlayers.Count < buttonCount)
            {
                _canvasLayer.RemoveChild(button);
            }
            else
            {
                hudButtons.Add(button);
            }

            buttonCount++;
        }

        return hudButtons;
    }

    private void ResizeUI()
    {
        MoveButtonsAround();
    }

    private void MoveButtonsAround()
    {
        Vector2I mainViewportSize = (Vector2I)GetViewport().GetVisibleRect().Size;

        int buttonCount = 1;

        int offset = 20;

        foreach(Button button in _hudButtons)
        {
            if (buttonCount == 1)
            {
                button.GlobalPosition = new Vector2(offset, offset);
            }
            else if (buttonCount == 2)
            {
                button.GlobalPosition = new Vector2((mainViewportSize.X / 2) + offset, offset);
            }
            else if (buttonCount == 3)
            {
                button.GlobalPosition = new Vector2(offset, (mainViewportSize.Y / 2) + offset);
            }
            else if (buttonCount == 4)
            {
                button.GlobalPosition = new Vector2((mainViewportSize.X / 2) + offset, (mainViewportSize.Y / 2) + offset);
            }

            buttonCount++;
        }
    }


    public override void _Process(double delta)
	{
        if (CurrentGameRules.IsEndlessLevelsOn || _levelCounter < CurrentGameRules.NumberOfLevels)
        {
            if (_allSwitchesActivated && ActivePlayers.All(x => x.IsWaitingForNextLevel || x.IsDead))
            {
                _levelCounter++;

                if (CurrentGameRules.IsEndlessLevelsOn || _levelCounter != CurrentGameRules.NumberOfLevels)
                {
                    GoToUpgradesScreen();

                    //ResetSplitScreenManager();
                }
                else
                {
                    ChangeScreenToTitleScreen();
                }
            }
        }
    }

    private void GoToUpgradesScreen()
    {
        var nextPlayerUpgradesScreenManager = GD.Load<PackedScene>(LevelScenePaths.UpgradesScreenManagerPath).Instantiate();

        //Remove player node from original SplitScreenManager
        foreach (BaseCharacter player in ActivePlayers)
        {
            player.IsWaitingForNextLevel = false;
            player.IsControllable = true;
            player.Show();

            var parent = player.GetParent();

            parent.RemoveChild(player);
        }

        this.AddChild(nextPlayerUpgradesScreenManager);
        this.MoveChild(nextPlayerUpgradesScreenManager, 0); //Move nextSplitScreenManager to the top of the hierarchy so timer and pause screen are on top

        _latestPlayerUpgradesScreenManager = nextPlayerUpgradesScreenManager as PlayerUpgradesScreenManager;
        _latestPlayerUpgradesScreenManager.GoToSplitScreenManager += ResetSplitScreenManager;

        var currentSplitScreenManager = GetTree().GetNodesInGroup("SplitScreenManager").FirstOrDefault() as SplitScreenManager;

        var currentDungeonLevel = currentSplitScreenManager.FindChild("Level") as BaseDungeonLevel;
        currentDungeonLevel.QueueFree();
        currentSplitScreenManager.QueueFree();

        _portalTimer.GetTimer().Start();
        _portalTimer.GetTimer().Stop();
        _portalTimer.Hide();
    }

	private void ResetSplitScreenManager()
	{
		try
		{
            var nextSplitScreenManager = GD.Load<PackedScene>(LevelScenePaths.SplitScreenManagerPath).Instantiate();

            _allSwitchesActivated = false;

            this.AddChild(nextSplitScreenManager);
            this.MoveChild(nextSplitScreenManager, 0); //Move nextSplitScreenManager to the top of the hierarchy so timer and pause screen are on top

            _latestSplitScreenManager = nextSplitScreenManager as SplitScreenManager;

            _latestBaseDungeonLevel = _latestSplitScreenManager.FindChild("Level") as BaseDungeonLevel;
            _latestBaseDungeonLevel.GoToGameOverScreen += ChangeSceneToGameOverScreen;
            _latestBaseDungeonLevel.ActivatePortal += RunPortalActivationProcess;

            _latestPlayerUpgradesScreenManager.QueueFree();
        }
		catch (Exception ex)
		{
			GD.PrintErr(ex);
		}
	}

    private void RunPortalActivationProcess()
    {
        _allSwitchesActivated = true;
        _portalTimer.GetTimer().Start();
        _portalTimer.Show();
    }

	private void ChangeScreenToTitleScreen()
	{
        EmitSignal(SignalName.GoToTitleScreen);
    }

    private void RunPortalTimerTimeoutProcess()
    {
        KillAllExposedPlayers();

        if (ActivePlayers.All(x => x.IsDead))
        {
            ChangeSceneToGameOverScreen();
        }
    }

    private void KillAllExposedPlayers()
    {
        foreach (BaseCharacter player in ActivePlayers.Where(x => !x.IsWaitingForNextLevel))
        {
            player.IsDead = true;
        }
    }

	private void ChangeSceneToGameOverScreen()
	{
        EmitSignal(SignalName.GoToGameOverScreen);
    }
}
