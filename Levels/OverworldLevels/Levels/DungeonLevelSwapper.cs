using Globals;
using Godot;
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
    private PauseScreenManager _pauseScreenManager;
    private PortalTimer _portalTimer;
    private SplitScreenManager _latestSplitScreenManager;
    private BaseDungeonLevel _latestBaseDungeonLevel;

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

		_pauseScreenManager = FindChild("PauseScreen") as PauseScreenManager;
        _pauseScreenManager.GoToTitleScreen += ChangeScreenToTitleScreen;

        _portalTimer = FindChild("PortalTimer") as PortalTimer;
        _portalTimer.GetTimer().Timeout += RunPortalTimerTimeoutProcess;

        _latestSplitScreenManager = GetNode<SplitScreenManager>("SplitScreenManager");

		_latestBaseDungeonLevel = _latestSplitScreenManager.FindChild("Level") as BaseDungeonLevel;
		_latestBaseDungeonLevel.GoToGameOverScreen += ChangeSceneToGameOverScreen;
        _latestBaseDungeonLevel.ActivatePortal += RunPortalActivationProcess;
	}

	public override void _Process(double delta)
	{
        if (CurrentGameRules.IsEndlessLevelsOn || _levelCounter < CurrentGameRules.NumberOfLevels)
        {
            if (_allSwitchesActivated && ActivePlayers.All(x => x.IsWaitingForNextLevel))
            {
                _levelCounter++;

                if (CurrentGameRules.IsEndlessLevelsOn || _levelCounter != CurrentGameRules.NumberOfLevels)
                {
                    ResetSplitScreenManager();
                }
                else
                {
                    ChangeScreenToTitleScreen();
                }
            }
        }
    }

	private void ResetSplitScreenManager()
	{
		try
		{
            var currentSplitScreenManager = GetTree().GetNodesInGroup("SplitScreenManager").FirstOrDefault() as SplitScreenManager;

            var nextSplitScreenManager = GD.Load<PackedScene>(LevelScenePaths.SplitScreenManagerPath).Instantiate();

            //Remove player node from original SplitScreenManager
            foreach (BaseCharacter player in ActivePlayers)
            {
                player.IsWaitingForNextLevel = false;
                player.IsControllable = true;
                player.Show();

                var parent = player.GetParent();

                parent.RemoveChild(player);
            }

            _allSwitchesActivated = false;

            this.AddChild(nextSplitScreenManager);
            this.MoveChild(nextSplitScreenManager, 0); //Move nextSplitScreenManager to the top of the hierarchy so timer and paus screen are on top

            _latestSplitScreenManager = nextSplitScreenManager as SplitScreenManager;

            _latestBaseDungeonLevel = _latestSplitScreenManager.FindChild("Level") as BaseDungeonLevel;
            _latestBaseDungeonLevel.GoToGameOverScreen += ChangeSceneToGameOverScreen;
            _latestBaseDungeonLevel.ActivatePortal += RunPortalActivationProcess;

            var currentDungeonLevel = currentSplitScreenManager.FindChild("Level") as BaseDungeonLevel;
            currentDungeonLevel.QueueFree();
            currentSplitScreenManager.QueueFree();

            _portalTimer.GetTimer().Start();
            _portalTimer.GetTimer().Stop();
            _portalTimer.Hide();
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
