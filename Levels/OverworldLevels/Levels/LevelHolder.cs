using Globals;
using Globals.PlayerManagement;
using Godot;
using System;
using System.Linq;

public partial class LevelHolder : Node
{
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		if (PlayerManager.ActivePlayers.All(x => x.IsWaitingForNextLevel))
		{
			var currentSplitScreenManager = GetNode("SplitScreenManager");

			var nextSplitScreenManager = GD.Load<PackedScene>(LevelScenePaths.SplitScreenManagerPath).Instantiate();

			foreach (var player in PlayerManager.ActivePlayers)
			{
				player.IsWaitingForNextLevel = false;

				var parent = player.GetParent();

				parent.RemoveChild(player);

				
				//currentSplitScreenManager.RemoveChild(player);
				GD.Print("removed");

				//nextSplitScreenManager.AddChild(player);
			}

			this.AddChild(nextSplitScreenManager);

			currentSplitScreenManager.QueueFree();


			//var nextSplitScreenManager = GD.Load<PackedScene>(LevelScenePaths.SplitScreenManagerPath).Instantiate();

			//this.AddChild(nextSplitScreenManager);

			//PlayerManager.ActivePlayers.ForEach(x => x.IsWaitingForNextLevel = false);
		}
	}
}
