using Globals.PlayerManagement;
using Godot;
using Globals;
using Enums.GameRules;
using System.Linq;
using System.Collections.Generic;
using MobileEntities.PlayerCharacters.Scripts;

namespace Levels.OverworldLevels.Utilities
{
	public partial class PlayerCamera : Camera2D
	{
		private float _distanceThresholdBeforeCameraMoves = 10;
		private CharacterBody2D _parentPlayer;
		private List<StaticBody2D> _cameraWalls = new List<StaticBody2D>();

		//private List<int> _playersInLockedAreas = new List<int>();

		public override void _Ready()
		{
			_parentPlayer = GetParent() as CharacterBody2D;

			foreach (var cameraWall in GetChildren().Where(x => x.IsInGroup("CameraWall")))
			{
				_cameraWalls.Add(cameraWall as StaticBody2D);
			}

			if (this.Name != "PlayerCamera0" || CurrentSaveGameRules.CurrentSplitScreenMergingType != SplitScreenMergingType.SharedScreenLocked)
			{
				if (_cameraWalls != null)
				{
					foreach (var wall in _cameraWalls)
					{
						var collision = wall.GetNode("CollisionShape2D") as CollisionShape2D;

						collision.Disabled = true;
					}
				}
			}

			//if (CurrentSaveGameRules.CurrentSplitScreenMergingType == SplitScreenMergingType.SharedScreenLocked)
			//{
			//	SetCameraToPlayerPositionMidpoint(false);
			//}
		}

		public override void _Process(double delta)
		{
			//GD.Print($"Viewport 0 Rect: {GlobalGameComponents.AvailableSubViewports[0].GetVisibleRect()}");
			//GD.Print($"Viewport 1 Rect: {GlobalGameComponents.AvailableSubViewports[1].GetVisibleRect()}");

			if (PlayerManager.ActivePlayers.Count > 1)
			{
				if (CurrentSaveGameRules.CurrentSplitScreenMergingType == SplitScreenMergingType.SharedScreenAdjust)
				{
					RunSharedScreenAdjustedProcess();
				}
				else if (CurrentSaveGameRules.CurrentSplitScreenMergingType == SplitScreenMergingType.SharedScreenLocked)
				{
					Zoom = new Vector2(2, 2);

					bool tooCloseToWalls = false;

					foreach (var cameraWall in _cameraWalls)
					{
						var collision = cameraWall.GetNode("CollisionShape2D") as CollisionShape2D;

						foreach (var player in PlayerManager.ActivePlayers)
						{
							//GD.Print($"Player {player.PlayerNumber}");

							//if (player.PlayerNumber == 0)
							//{
							//	GD.Print($"X: Player: {player.GlobalPosition.X}, {cameraWall.Name}: {collision.GlobalPosition.X}");
							//	GD.Print($"Y: Player: {player.GlobalPosition.Y}, {cameraWall.Name}: {collision.GlobalPosition.Y}");
							//}

							if (cameraWall.Name == "TopWall" || cameraWall.Name == "BottomWall")
							{
								if (Mathf.Abs(player.GlobalPosition.Y - collision.GlobalPosition.Y) <= 50)
								{
									GD.Print($"Culprit player: {player.PlayerNumber}, wall: {cameraWall.Name}");
									GD.Print($"Y diff: {Mathf.Abs(player.GlobalPosition.Y - collision.GlobalPosition.Y)}");

									tooCloseToWalls = true;
									break;
								}
							}

							if (cameraWall.Name == "RightWall" || cameraWall.Name == "LeftWall")
							{
								if (Mathf.Abs(player.GlobalPosition.X - collision.GlobalPosition.X) <= 50)
								{
									GD.Print($"Culprit player: {player.PlayerNumber}, wall: {cameraWall.Name}");
									GD.Print($"X diff: {Mathf.Abs(player.GlobalPosition.X - collision.GlobalPosition.X)}");

									tooCloseToWalls = true;
									break;
								}
							}
						}
					}

					if (!tooCloseToWalls)
					{
						SetCameraToPlayerPositionMidpoint(false);
					}
				}
			}
		}

		#region Shared Screen Adjust
		private void RunSharedScreenAdjustedProcess()
		{
			SetCameraToPlayerPositionMidpoint(false);

			float farthestDistanceBetweenPlayers = FindFarthestDistanceBetweenPlayers();

			SetLockedScreenAdjustedProcess(farthestDistanceBetweenPlayers);
		}

		private void SetCameraToPlayerPositionMidpoint(bool doLerp = false)
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
				GlobalPosition = GlobalPosition.Lerp(midpointVector, .01f);
			}
			else
			{
				GlobalPosition = midpointVector;
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

		#region Signal Receptions
		private void OnBodyEntered(Node2D body)
		{
			GD.Print("entering top area");
			if (body is BaseCharacter)
			{
				//GD.Print("entering top area");

				//_playersInLockedAreas.Add((body as BaseCharacter).PlayerNumber);
			}
		}

		private void OnBodyExited(Node2D body)
		{
			GD.Print("exiting top area");
			if (body is BaseCharacter)
			{
				//GD.Print("exiting top area");

				//_playersInLockedAreas.Remove((body as BaseCharacter).PlayerNumber);
			}
		}

		#endregion
	}
}
