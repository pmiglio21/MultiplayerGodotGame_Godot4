using Globals.PlayerManagement;
using Godot;
using Globals;
using Enums.GameRules;
using System.Linq;
using System.Collections.Generic;

namespace Levels.OverworldLevels.Utilities
{
	public partial class PlayerCamera : Camera2D
	{
        private LevelHolder _parentLevelHolder;

        private CharacterBody2D _parentPlayer;
		private List<StaticBody2D> _cameraWalls = new List<StaticBody2D>();
		private List<Area2D> _wallAreas = new List<Area2D>();
		private bool _isCameraSetBetweenPlayers = false;
		private bool _isSomeoneTouchingAWall = false;

		public override void _Ready()
		{
			_parentLevelHolder = GetNode<LevelHolder>("LevelHolder");

            _parentPlayer = GetParent() as CharacterBody2D;

			GetCameraWalls();

			GetWallAreas();

			if (_parentLevelHolder.CurrentGameRules.CurrentSplitScreenMergingType == SplitScreenMergingType.SharedScreenLocked)
			{
				SetCameraToPlayerPositionMidpoint(1, true);
			}
		}

		public override void _Process(double delta)
		{
			//GD.Print($"Is camera set at midpoint yet? {_isCameraSetBetweenPlayers}");

			if (PlayerManager.ActivePlayers.Count > 1)
			{
				if (_parentLevelHolder.CurrentGameRules.CurrentSplitScreenMergingType == SplitScreenMergingType.SharedScreenAdjust)
				{
					RunSharedScreenAdjustedProcess();
				}
				else if (_parentLevelHolder.CurrentGameRules.CurrentSplitScreenMergingType == SplitScreenMergingType.SharedScreenLocked)
				{
					RunSharedScreenLockedProcess(delta);
				}
			}
		}

		private void GetCameraWalls()
		{
			foreach (var cameraWall in GetChildren().Where(x => x.IsInGroup("CameraWall")))
			{
				_cameraWalls.Add(cameraWall as StaticBody2D);
			}

			if (this.Name != "PlayerCamera0" || _parentLevelHolder.CurrentGameRules.CurrentSplitScreenMergingType != SplitScreenMergingType.SharedScreenLocked)
			{
				if (_cameraWalls != null)
				{
					foreach (var wall in _cameraWalls)
					{
						var collision = wall.GetNode("StaticBodyCollision") as CollisionShape2D;

						collision.Disabled = true;
					}
				}
			}
		}

		private void GetWallAreas()
		{
			foreach (var wallArea in GetChildren().Where(x => x.IsInGroup("DetectorArea")))
			{
				_wallAreas.Add(wallArea as Area2D);
			}

			if (this.Name != "PlayerCamera0" || _parentLevelHolder.CurrentGameRules.CurrentSplitScreenMergingType != SplitScreenMergingType.SharedScreenLocked)
			{
				if (_wallAreas != null)
				{
					foreach (var area in _wallAreas)
					{
						var collision = area.GetNode("AreaCollision") as CollisionShape2D;

						collision.Disabled = true;
					}
				}
			}
		}

		#region Shared Screen Locked
		private void RunSharedScreenLockedProcess(double delta)
		{
			Zoom = new Vector2(2, 2);

			var farthestDistanceBetweenPlayers = FindFarthestDistanceBetweenPlayers();

			//Picked a random number		   
			float minPossibleDistanceValue = 256;

			if (!_isSomeoneTouchingAWall || farthestDistanceBetweenPlayers < minPossibleDistanceValue)
			{
				SetCameraToPlayerPositionMidpoint(delta, _isCameraSetBetweenPlayers);
			}
		}
		#endregion

		#region Shared Screen Adjust
		private void RunSharedScreenAdjustedProcess()
		{
			SetCameraToPlayerPositionMidpoint(doLerp: false);

			float farthestDistanceBetweenPlayers = FindFarthestDistanceBetweenPlayers();

			SetSharedScreenAdjustedProcess(farthestDistanceBetweenPlayers);
		}

		private void SetCameraToPlayerPositionMidpoint(double delta = 0, bool doLerp = false)
		{
			//Determine Camera point (middle of all players)
			Vector2 midpointVector = new Vector2();

			foreach (var player in PlayerManager.ActivePlayers)
			{
				midpointVector += player.GlobalPosition;
			}

			midpointVector = midpointVector / PlayerManager.ActivePlayers.Count;

			if (doLerp)
			{
				GlobalPosition = GlobalPosition.Lerp(midpointVector, .8f * (float)delta);

				//_isCameraSetBetweenPlayers = false;
			}
			else
			{
				GlobalPosition = midpointVector;

				_isCameraSetBetweenPlayers = true;
			}
		}

		private float FindFarthestDistanceBetweenPlayers()
		{
			//Determine camera zoom (depends on furthest distance between players)
			float farthestDistanceBetweenPlayers = float.MinValue;


			foreach (var firstPlayer in PlayerManager.ActivePlayers)
			{
				foreach (var secondPlayer in PlayerManager.ActivePlayers.Where(x => x.PlayerNumber != firstPlayer.PlayerNumber))
				{
					if (firstPlayer.GlobalPosition.DistanceTo(secondPlayer.GlobalPosition) > farthestDistanceBetweenPlayers)
					{
						farthestDistanceBetweenPlayers = firstPlayer.GlobalPosition.DistanceTo(secondPlayer.GlobalPosition);
					}
				}
			}

			return farthestDistanceBetweenPlayers;
		}

		private void SetSharedScreenAdjustedProcess(float farthestDistanceBetweenPlayers)
		{
			/////////MAYBE MAKE MAP SQUARE? This fixes the zoom being weird///////////////

			//Picked a random number
			float minPossibleDistanceValue = 256;

			//Needs to be Ceil and not Floor because dividing by 0 is no good.
			var multiplier = farthestDistanceBetweenPlayers > minPossibleDistanceValue ? minPossibleDistanceValue / Mathf.Ceil(farthestDistanceBetweenPlayers) : 1f;

			//Float and Vector2 comparison
			var newZoom = new Vector2(2f, 2f) * multiplier;


			////Up and down lock: .49516442
			////Left and right lock: .60952383
			////Only works for the map size as it is. Don't know how to get the numbers dynamically yet. Maybe check for limits being notified?
			//var newX = newZoom.X <= .6102503f ? .6102503f : newZoom.X;
			//var newY = newZoom.Y <= .49516442f ? .49516442f : newZoom.Y;

			//Zoom = new Vector2(newX, newY);

			Zoom = newZoom;

			//GD.Print(Zoom);
		}
		#endregion

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

