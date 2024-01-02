using Enums;
using Godot;
using MobileEntities.CharacterStats;
using MobileEntities.PlayerCharacters;
using System;

namespace MobileEntities.PlayerCharacters.Scripts
{
	public partial class Mage : BaseCharacter
	{
		protected override void InitializeClassSpecificProperties()
		{
			characterStats = new Stats(10);
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
	}
}
