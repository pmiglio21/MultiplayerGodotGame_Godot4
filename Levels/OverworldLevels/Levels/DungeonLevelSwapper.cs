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

        _latestSplitScreenManager = GetNode<SplitScreenManager>("SplitScreenManager");

		_latestBaseDungeonLevel = _latestSplitScreenManager.FindChild("Level") as BaseDungeonLevel;
		_latestBaseDungeonLevel.GoToGameOverScreen += ChangeSceneToGameOverScreen;
        _latestBaseDungeonLevel.ResetBaseDungeonLevel += () => _allSwitchesActivated = true;
	}

	public override void _Process(double delta)
	{
        if (_levelCounter < CurrentGameRules.NumberOfLevels)
        {
            if (_allSwitchesActivated && ActivePlayers.All(x => x.IsWaitingForNextLevel))
            {
                _levelCounter++;

                if (_levelCounter != CurrentGameRules.NumberOfLevels)
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
            foreach (var player in ActivePlayers)
            {
                player.IsWaitingForNextLevel = false;

                var parent = player.GetParent();

                parent.RemoveChild(player);
            }

            _allSwitchesActivated = false;

            //Vector2 pauseScreenManagerOriginalGlobalPosition = _pauseScreenManager.GlobalPosition;

            this.AddChild(nextSplitScreenManager);

            //Remove pause screen node from original SplitScreenManager
            _pauseScreenManager.Reparent(nextSplitScreenManager);

            _latestSplitScreenManager = nextSplitScreenManager as SplitScreenManager;

            _latestBaseDungeonLevel = _latestSplitScreenManager.FindChild("Level") as BaseDungeonLevel;
            _latestBaseDungeonLevel.GoToGameOverScreen += ChangeSceneToGameOverScreen;
            _latestBaseDungeonLevel.ResetBaseDungeonLevel += () => _allSwitchesActivated = true;

            var currentDungeonLevel = currentSplitScreenManager.FindChild("Level") as BaseDungeonLevel;
            currentDungeonLevel.QueueFree();
            currentSplitScreenManager.QueueFree();

			//_pauseScreenManager.GlobalPosition = pauseScreenManagerOriginalGlobalPosition;
		}
		catch (Exception ex)
		{
			GD.PrintErr(ex);
		}
	}

	private void ChangeScreenToTitleScreen()
	{
        EmitSignal(SignalName.GoToTitleScreen);
    }

	private void ChangeSceneToGameOverScreen()
	{
        EmitSignal(SignalName.GoToGameOverScreen);
    }
}
