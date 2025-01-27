using Godot;
using Globals;
using System.Linq;
using System.Collections.Generic;
using Root;

namespace Levels.OverworldLevels.Utilities
{
	public partial class PlayerCamera : Camera2D
	{
        private DungeonLevelSwapper _parentDungeonLevelSwapper;

        private CharacterBody2D _parentPlayer;
		private List<StaticBody2D> _cameraWalls = new List<StaticBody2D>();
		private List<Area2D> _wallAreas = new List<Area2D>();
		private bool _isCameraSetBetweenPlayers = false;
		private bool _isSomeoneTouchingAWall = false;

		public override void _Ready()
		{
            RootSceneSwapper rootSceneSwapper = GetTree().Root.GetNode<RootSceneSwapper>("RootSceneSwapper");

            _parentDungeonLevelSwapper = rootSceneSwapper.GetDungeonLevelSwapper();

            _parentPlayer = GetParent() as CharacterBody2D;
		}

		public override void _Process(double delta)
		{
			
		}

		#region Signal Receptions

		private void OnDetectorAreaBodyEntered(Node2D body)
		{
			_isSomeoneTouchingAWall = true;
			//GD.Print("Collision!");
		}

		private void OnDetectorAreaBodyExited(Node2D body)
		{
			_isSomeoneTouchingAWall = false;
			//GD.Print("No collision!");
		}

		#endregion
	}
}

