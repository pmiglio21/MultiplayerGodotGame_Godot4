using Globals;
using Globals.PlayerManagement;
using Godot;
using Levels.UtilityLevels;
using Levels.UtilityLevels.UserInterfaceComponents;
using MobileEntities.PlayerCharacters.Scripts;
using Models;
using Root;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class LevelHolder : Node
{
	private RootSceneSwapper _rootSceneSwapper;

	public GameRules CurrentGameRules = new GameRules();
    public List<BaseCharacter> ActivePlayers = new List<BaseCharacter>();

    private int _levelCounter;

	public override void _Ready()
	{
        //_rootSceneSwapper = GetTree().Root.GetNode<RootSceneSwapper>("RootSceneSwapper");

        _levelCounter = 0;
	}

	public override void _Process(double delta)
	{
		if (CurrentGameRules.NumberOfLevels != GlobalConstants.Infinity && _levelCounter < int.Parse(CurrentGameRules.NumberOfLevels))
		{
			if (ActivePlayers.All(x => x.IsWaitingForNextLevel))
			{
				_levelCounter++;

				if (_levelCounter != int.Parse(CurrentGameRules.NumberOfLevels))
				{
					ResetSplitScreenManager();
				}
				else
				{
                    ActivePlayers.Clear();
					GetTree().ChangeSceneToFile(LevelScenePaths.TitleScreenPath);
				}
			}
		}
		else if (CurrentGameRules.NumberOfLevels == GlobalConstants.Infinity)
		{
			if (ActivePlayers.All(x => x.IsWaitingForNextLevel))
			{
				ResetSplitScreenManager();
			}
		}
	}

	private void ResetSplitScreenManager()
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

		//Remove pause screen node from original SplitScreenManager
		var pauseScreen = GetTree().GetNodesInGroup("PauseScreen").FirstOrDefault() as PauseScreenManager;
		pauseScreen.Reparent(nextSplitScreenManager);

		this.AddChild(nextSplitScreenManager);

		currentSplitScreenManager.QueueFree();
	}
}
