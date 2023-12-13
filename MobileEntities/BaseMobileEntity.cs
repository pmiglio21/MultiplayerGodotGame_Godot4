using Enums;
using Godot;
using System;

namespace MobileEntities
{
	public partial class BaseMobileEntity : CharacterBody2D
	{
		// Called when the node enters the scene tree for the first time.
		//public override void _Ready()
		//{
		//}

		//// Called every frame. 'delta' is the elapsed time since the previous frame.
		//public override void _Process(double delta)
		//{
		//}

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
