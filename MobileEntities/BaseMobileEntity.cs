using Enums;
using Godot;
using MobileEntities.CharacterStats;
using System;

namespace MobileEntities
{
	public partial class BaseMobileEntity : CharacterBody2D
	{
		#region Entity Stats
		public Stats CharacterStats;
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

            //CardinalDirection latestCardinalDirection = lastUsedCardinalDirection;

            //         if (moveDirection.X == 0 && moveDirection.Y < 0)
            //         {
            //             latestCardinalDirection = CardinalDirection.North;
            //         }
            //         else if (moveDirection.X > 0 && moveDirection.Y < 0)
            //         {
            //             latestCardinalDirection = CardinalDirection.NorthEast;
            //         }
            //         else if (moveDirection.X > 0 && moveDirection.Y == 0)
            //{
            //	latestCardinalDirection = CardinalDirection.East;
            //}
            //         else if (moveDirection.X > 0 && moveDirection.Y > 0)
            //         {
            //             latestCardinalDirection = CardinalDirection.SouthEast;
            //         }
            //         else if (moveDirection.X == 0 && moveDirection.Y > 0)
            //         {
            //             latestCardinalDirection = CardinalDirection.South;
            //         }
            //         else if (moveDirection.X < 0 && moveDirection.Y > 0)
            //         {
            //             latestCardinalDirection = CardinalDirection.SouthWest;
            //         }
            //         else if (moveDirection.X < 0 && moveDirection.Y == 0)
            //{
            //	latestCardinalDirection = CardinalDirection.West;
            //}
            //         else if (moveDirection.X < 0 && moveDirection.Y < 0)
            //         {
            //             latestCardinalDirection = CardinalDirection.NorthWest;
            //         }

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
