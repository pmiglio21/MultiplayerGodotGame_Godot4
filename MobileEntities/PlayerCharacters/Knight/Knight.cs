using Enums;
using Godot;
using MobileEntities.CharacterStats;
using MobileEntities.PlayerCharacters.Knight;
using MobileEntities.PlayerCharacters;
using System;

namespace MobileEntities.PlayerCharacters.Scripts
{
	public partial class Knight : BaseCharacter
	{
		private PackedScene _knightSlash = GD.Load<PackedScene>("res://MobileEntities/PlayerCharacters/Knight/AttackEntities/KnightSlash.tscn");

		protected override void InitializeClassSpecificProperties()
		{
			characterStats = new Stats(10);
		}

		protected override void RunAttack()
		{
			var knightSlashInstance = _knightSlash.Instantiate() as KnightSlash;

			//GD.Print($"Mage GlobalPosition {GlobalPosition}");
			//GD.Print($"Mage Position {Position}");

			knightSlashInstance.Position = Position;
			knightSlashInstance.ZIndex = ZIndex;

			//var viewportScene = GetParent().GetParent();
			this.AddChild(knightSlashInstance);

			//var attackDirectionalInput = Vector2.Zero;
			//attackDirectionalInput.X = Input.GetActionStrength($"MoveEast_{DeviceIdentifier}") - Input.GetActionStrength($"MoveWest_{DeviceIdentifier}");
			//attackDirectionalInput.Y = Input.GetActionStrength($"MoveSouth_{DeviceIdentifier}") - Input.GetActionStrength($"MoveNorth_{DeviceIdentifier}");

			//if (attackDirectionalInput == Vector2.Zero)
			//{
			//    if (latestCardinalDirection == CardinalDirection.East)
			//    {
			//        attackDirectionalInput = new Vector2(1, 0);
			//    }
			//    else if (latestCardinalDirection == CardinalDirection.West)
			//    {
			//        attackDirectionalInput = new Vector2(-1, 0);
			//    }
			//}
		}
	}
}
