using Enums;
using Godot;
using MobileEntities.CharacterStats;
using MobileEntities.PlayerCharacters;
using System;

namespace MobileEntities.PlayerCharacters.Scripts
{
	public partial class Mage : BaseCharacter
	{
		private PackedScene _mageSpell = GD.Load<PackedScene>("res://MobileEntities/PlayerCharacters/Mage/Projectiles/MageSpell.tscn");

		protected override void InitializeClassSpecificProperties()
		{
			characterStats = new Stats(3);
		}

		protected override void MoveHurtBoxes(CardinalDirection hurtBoxDirection)
		{
			Area2D mainHurtBox = GetNode<Area2D>("MainHurtBox");

			if (hurtBoxDirection == CardinalDirection.East)
			{
				mainHurtBox.Position = new Vector2(1, 3);
			}
			else if (hurtBoxDirection == CardinalDirection.West)
			{
				mainHurtBox.Position = new Vector2(-1, 3);
			}
		}

		protected override void RunAttack()
		{
			var mageSpellInstance = _mageSpell.Instantiate() as MageSpell;

			//GD.Print($"Mage GlobalPosition {GlobalPosition}");
			//GD.Print($"Mage Position {Position}");

			mageSpellInstance.Position = Position;

			var viewportScene = GetParent().GetParent();
			viewportScene.AddChild(mageSpellInstance);

			var attackDirectionalInput = Vector2.Zero;
			attackDirectionalInput.X = Input.GetActionStrength($"MoveEast_{DeviceIdentifier}") - Input.GetActionStrength($"MoveWest_{DeviceIdentifier}");
			attackDirectionalInput.Y = Input.GetActionStrength($"MoveSouth_{DeviceIdentifier}") - Input.GetActionStrength($"MoveNorth_{DeviceIdentifier}");

			if (attackDirectionalInput == Vector2.Zero)
			{
				if (latestCardinalDirection == CardinalDirection.East)
				{
					attackDirectionalInput = new Vector2(1, 0);
				}
				else if (latestCardinalDirection == CardinalDirection.West)
				{
					attackDirectionalInput = new Vector2(-1, 0);
				}
			}

			mageSpellInstance.MoveDirection = attackDirectionalInput;
		}
	}
}
