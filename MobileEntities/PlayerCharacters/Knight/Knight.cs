using Enums;
using Godot;
using MobileEntities.CharacterStats;
using MobileEntities.PlayerCharacters.Knight;

namespace MobileEntities.PlayerCharacters.Scripts
{
	public partial class Knight : BaseCharacter
	{
		private PackedScene _knightSlash = GD.Load<PackedScene>("res://MobileEntities/PlayerCharacters/Knight/AttackEntities/KnightSlash.tscn");

		protected override void InitializeClassSpecificProperties()
		{
			CharacterStats = new Stats(3);

			CharacterStats.BaseHealth = 3;
            CharacterStats.BaseAttack = 2;
            CharacterStats.BaseDefense = 4;
            CharacterStats.BaseSpeed = 1;

			CharacterStats.CalculateStatsOnLevelUp();
        }

		protected override void RunAttack()
		{
			var knightSlashInstance = _knightSlash.Instantiate() as KnightSlash;

			this.AddChild(knightSlashInstance);

			if (this.latestCardinalDirection == CardinalDirection.North)
			{
				knightSlashInstance.GlobalPosition = new Vector2(this.GlobalPosition.X, this.GlobalPosition.Y - 16);
				knightSlashInstance.GlobalRotation = 0;
			}
			else if (this.latestCardinalDirection == CardinalDirection.East)
			{
				knightSlashInstance.GlobalPosition = new Vector2(this.GlobalPosition.X + 16, this.GlobalPosition.Y);
				knightSlashInstance.GlobalRotation = Mathf.Pi / 2;
			}
			else if (this.latestCardinalDirection == CardinalDirection.South)
			{
				knightSlashInstance.GlobalPosition = new Vector2(this.GlobalPosition.X, this.GlobalPosition.Y + 16);
				knightSlashInstance.GlobalRotation = Mathf.Pi;
			}
			else if (this.latestCardinalDirection == CardinalDirection.West)
			{
				knightSlashInstance.GlobalPosition = new Vector2(this.GlobalPosition.X - 16, this.GlobalPosition.Y);
				knightSlashInstance.GlobalRotation = -Mathf.Pi / 2;
			}

			knightSlashInstance.ZIndex = this.ZIndex + 100;

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
