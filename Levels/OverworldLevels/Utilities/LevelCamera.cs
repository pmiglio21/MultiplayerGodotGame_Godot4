using Globals.PlayerManagement;
using MobileEntities.PlayerCharacters.Scripts;
using Godot;
using System;

namespace Levels.OverworldLevels.Utilities
{
	public partial class LevelCamera : Camera2D
	{
		private Vector2 _newPosition;
		private float _distanceThresholdBeforeCameraMoves = 10;

		public override void _Ready()
		{
			_newPosition = GlobalPosition;
		}

		public override void _Process(double delta)
		{
			if (PlayerManager.ActivePlayers.Count > 0)
			{
				//Find nearest player and set camera distance based on that player
				if (PlayerManager.ActivePlayers.Count == 1)
				{
					SetDistance(PlayerManager.ActivePlayers[0]);
				}
				else if (PlayerManager.ActivePlayers.Count > 1)
				{
					BaseCharacter minDistancePlayer = null;
					float minDistance = float.MaxValue;

					foreach (var player in PlayerManager.ActivePlayers)
					{
						if (Mathf.Abs(GlobalPosition.X - player.GlobalPosition.X) < minDistance)
						{
							minDistancePlayer = player;
							minDistance = GlobalPosition.X - player.GlobalPosition.X;
						}
					}

					SetDistance(minDistancePlayer);
				}

				//Maybe try MoveAndSlide()?

				GlobalPosition = GlobalPosition.Lerp(_newPosition, .01f);

				//GlobalPosition = _newPosition;
			}
		}

		private void SetDistance(BaseCharacter player)
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
	}
}
