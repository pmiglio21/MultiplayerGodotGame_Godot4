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
		private Vector2 _newPosition;
		private float _distanceThresholdBeforeCameraMoves = 10;
		private CharacterBody2D _parentPlayer;
		private List<StaticBody2D> _cameraWalls = new List<StaticBody2D>();

		public override void _Ready()
		{
			_parentPlayer = GetParent() as CharacterBody2D;

			foreach (var cameraWall in GetChildren().Where(x => x.IsInGroup("CameraWall")))
			{
				_cameraWalls.Add(cameraWall as StaticBody2D);
			}

			//if (CurrentSaveGameRules.CurrentSplitScreenMergingType != SplitScreenMergingType.SharedScreenLocked)
			//{
				if (_cameraWalls != null)
				{
					foreach (var wall in _cameraWalls)
					{
						var collision = wall.GetNode("CollisionShape2D") as CollisionShape2D;

						collision.Disabled = true;
					}
				}
			//}

			_newPosition = GlobalPosition;
		}

		public override void _Process(double delta)
		{
			if (PlayerManager.ActivePlayers.Count > 1)
			{
				if (CurrentSaveGameRules.CurrentSplitScreenMergingType == SplitScreenMergingType.SharedScreenAdjust)
				{
					RunSharedScreenAdjustedProcess();
				}
				else if (CurrentSaveGameRules.CurrentSplitScreenMergingType == SplitScreenMergingType.SharedScreenLocked)
				{
					Zoom = new Vector2(2, 2);

					SetCameraToPlayerPositionMidpoint();
				}
			}
		}

		private void SetDistance(CharacterBody2D player)
		{
			if (player != null)
			{
				if (GlobalPosition.X - player.GlobalPosition.X >= _distanceThresholdBeforeCameraMoves)
				{
					_newPosition.X = player.GlobalPosition.X + _distanceThresholdBeforeCameraMoves;
				}
				else if (GlobalPosition.X - player.GlobalPosition.X <= -_distanceThresholdBeforeCameraMoves)
				{
					_newPosition.X = player.GlobalPosition.X - _distanceThresholdBeforeCameraMoves;
				}

				if (GlobalPosition.Y - player.GlobalPosition.Y >= _distanceThresholdBeforeCameraMoves)
				{
					_newPosition.Y = player.GlobalPosition.Y + _distanceThresholdBeforeCameraMoves;
				}
				else if (GlobalPosition.Y - player.GlobalPosition.Y <= -_distanceThresholdBeforeCameraMoves)
				{
					_newPosition.Y = player.GlobalPosition.Y - _distanceThresholdBeforeCameraMoves;
				}
			}
		}

		#region Shared Screen Adjust
		private void RunSharedScreenAdjustedProcess()
		{
			SetCameraToPlayerPositionMidpoint();

			float farthestDistanceBetweenPlayers = FindFarthestDistanceBetweenPlayers();

			SetLockedScreenAdjustedProcess(farthestDistanceBetweenPlayers);
		}

		private void SetCameraToPlayerPositionMidpoint()
		{
			//Determine Camera point (middle of all players)
			Vector2 midpointVector = new Vector2();

			foreach (var player in PlayerManager.ActivePlayers)
			{
				midpointVector += player.GlobalPosition;
			}

			midpointVector = midpointVector / PlayerManager.ActivePlayers.Count;

			GlobalPosition = midpointVector;
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

		private void SetLockedScreenAdjustedProcess(float farthestDistanceBetweenPlayers)
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
	}
}
