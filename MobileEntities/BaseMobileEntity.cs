using Enums;
using Godot;
using System;

namespace MobileEntities
{
	public partial class BaseMobileEntity : CharacterBody2D
	{
		#region MobileEntity Movement Helpers
		protected CardinalDirection FindLatestCardinalDirection(Vector2 moveDirection)
		{
			CardinalDirection latestCardinalDirection = CardinalDirection.East;

			if (moveDirection.X == 0 && moveDirection.X > 0)
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

		protected void FlipCharacter(Vector3 moveDirection, Sprite3D playerSprite)
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
