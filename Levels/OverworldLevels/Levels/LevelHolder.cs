using Globals;
using Globals.PlayerManagement;
using Godot;
using Levels.UtilityLevels;
using Levels.UtilityLevels.UserInterfaceComponents;
using System;
using System.Linq;
using static Godot.Window;

public partial class LevelHolder : Node
{
	private int _levelCounter;

	public override void _Ready()
	{
        _levelCounter = 0;
	}

	public override void _Process(double delta)
	{
		if (CurrentSaveGameRules.NumberOfLevels != GlobalConstants.Infinity && _levelCounter < int.Parse(CurrentSaveGameRules.NumberOfLevels))
		{
			if (PlayerManager.ActivePlayers.All(x => x.IsWaitingForNextLevel))
			{
				_levelCounter++;

				if (_levelCounter != int.Parse(CurrentSaveGameRules.NumberOfLevels))
				{
					ResetSplitScreenManager();
				}
				else
				{
					PlayerManager.ActivePlayers.Clear();
					GetTree().ChangeSceneToFile(LevelScenePaths.TitleLevelPath);
				}
			}
		}
		else if (CurrentSaveGameRules.NumberOfLevels == GlobalConstants.Infinity)
		{
			if (PlayerManager.ActivePlayers.All(x => x.IsWaitingForNextLevel))
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
		foreach (var player in PlayerManager.ActivePlayers)
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
