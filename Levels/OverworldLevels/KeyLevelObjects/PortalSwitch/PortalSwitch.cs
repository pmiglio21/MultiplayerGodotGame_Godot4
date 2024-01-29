using Godot;
using MobileEntities.PlayerCharacters.Scripts;
using System;

public partial class PortalSwitch : CharacterBody2D
{
	public bool SwitchActivated = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		DetectCollisions();
	}

	private void DetectCollisions()
	{
		var collisionCountMax = GetSlideCollisionCount();

		GD.Print(collisionCountMax);

		var index = 0;

		while (index < collisionCountMax)
		{
			var collision = GetSlideCollision(index);

			GD.Print("sorta");

			if (collision.GetCollider() is BaseCharacter)
			{
				GD.Print("GOT IN!");

				//IsRobotTouchingButton = true;

				//var button = collision.Collider as NextLevelButton;

				//if (IsActivatingNextLevelButton)
				//{
				//	button.IsButtonPressed = true;
				//}
			}

			index++;
		}
	}

	//NEED SOMETHING THAT DETECTS WHEN AREAS OVERLAP, NOT WHEN ENTERED EXACTLY
	private void OnCollisionAreaEntered(Area2D area)
	{
		//if (area.IsInGroup("PlayerHurtBox"))
		//{
		//	CollisionShape2D collisionShape = area.GetNode<CollisionShape2D>("CollisionShape");

		//	var character = area.GetParent() as BaseCharacter;

		//	GD.Print($"Is character null? {character == null}");

		//	if (!collisionShape.Disabled)
		//	{
		//		//How to get exact player here?
		//		if (Input.IsActionJustPressed($"SouthButton_{character.DeviceIdentifier}"))
		//		{
		//			SwitchActivated = !SwitchActivated;

		//			GD.Print("Portal Switch Changed");
		//			//Change sprite/animation
		//		}

		//		//GD.Print("Portal Switch Entered");
		//	}
		//}
	}
}



