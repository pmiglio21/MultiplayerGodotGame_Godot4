using Globals.PlayerManagement;
using MobileEntities.PlayerCharacters.Scripts;
using Godot;
using System;
using Globals;
using Enums.GameRules;
using System.Linq;

namespace Levels.OverworldLevels.Utilities
{
	public partial class PlayerCamera : Camera2D
	{
		private Vector2 _newPosition;
		private float _distanceThresholdBeforeCameraMoves = 10;
		private CharacterBody2D _parentPlayer;

		public override void _Ready()
		{
			_parentPlayer = GetParent() as CharacterBody2D;

			_newPosition = GlobalPosition;
		}

		public override void _Process(double delta)
		{
			if ((CurrentSaveGameRules.CurrentSplitScreenMergingType == SplitScreenMergingType.SharedScreenAdjust) && PlayerManager.ActivePlayers.Count > 1)
			{
				RunSharedScreenAdjustedProcess();
			}

			//if (PlayerManager.ActivePlayers.Count > 0)
			//{
			//	//Find nearest player and set camera distance based on that player
			//	if (PlayerManager.ActivePlayers.Count == 1)
			//	{
			//		SetDistance(PlayerManager.ActivePlayers[0]);
			//	}
			//	else if (PlayerManager.ActivePlayers.Count > 1)
			//	{
			//		BaseCharacter minDistancePlayer = null;
			//		float minDistance = float.MaxValue;

			//		foreach (var player in PlayerManager.ActivePlayers)
			//		{
			//			if (Mathf.Abs(GlobalPosition.X - player.GlobalPosition.X) < minDistance)
			//			{
			//				minDistancePlayer = player;
			//				minDistance = GlobalPosition.X - player.GlobalPosition.X;
			//			}
			//		}

			//		SetDistance(minDistancePlayer);
			//	}

			//	//Maybe try MoveAndSlide()?

			//	GlobalPosition = GlobalPosition.Lerp(_newPosition, .01f);

			//	//GlobalPosition = _newPosition;
			//}
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
			float farthestDistanceBetweenPlayers = FindFarthestDistanceBetweenPlayers();

			SetLockedScreenAdjustedProcess(farthestDistanceBetweenPlayers);
		}

		private float FindFarthestDistanceBetweenPlayers()
		{
			//Determine Camera point (middle of all players)
			Vector2 midpointVector = new Vector2();

			foreach (var player in PlayerManager.ActivePlayers)
			{
				midpointVector += player.GlobalPosition;
			}

			midpointVector = midpointVector / PlayerManager.ActivePlayers.Count;

			GlobalPosition = midpointVector;


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
			float minPossibleDistanceValue = 256;

			//Needs to be Ceil and not Floor because dividing by 0 is no good.
			var multiplier = farthestDistanceBetweenPlayers > minPossibleDistanceValue ? minPossibleDistanceValue / Mathf.Ceil(farthestDistanceBetweenPlayers) : 1f;

			//Float and Vector2 comparison
			Zoom = new Vector2(2f, 2f) * multiplier;
		}
		#endregion
	}
}
