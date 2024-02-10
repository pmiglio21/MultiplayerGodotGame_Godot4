using Enums;
using Godot;
using MobileEntities.CharacterStats;
using System;

namespace MobileEntities
{
	public partial class BaseMobileEntity : CharacterBody2D
	{
		#region Entity Stats
		protected Stats characterStats = new Stats(1);
		#endregion

		#region MobileEntity Movement Helpers
		protected CardinalDirection FindLatestCardinalDirection(CardinalDirection lastUsedCardinalDirection, Vector2 moveDirection)
		{
			CardinalDirection latestCardinalDirection = lastUsedCardinalDirection;

			if (moveDirection.X > 0)
			{
				latestCardinalDirection = CardinalDirection.East;
			}
			else if (moveDirection.X < 0)
			{
				latestCardinalDirection = CardinalDirection.West;
			}

			//GD.Print(latestCardinalDirection);

			return latestCardinalDirection;
		}

		protected void FlipCharacter(Vector3 moveDirection, Sprite2D playerSprite)
		{
			if (moveDirection.X > 0 && playerSprite.FlipH)
			{
				playerSprite.FlipH = false;
			}
			else if (moveDirection.X < 0 && !playerSprite.FlipH)
			{
				playerSprite.FlipH = true;
			}
		}
		#endregion
	}
}
