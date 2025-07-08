using Enums;
using Godot;
using MobileEntities.CharacterStats;

namespace MobileEntities.PlayerCharacters
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

            //TODO: make keyboard and mouse option here too

			this.AddChild(knightSlashInstance);

            if (moveDirection == Vector2.Zero)
            {
                if (latestCardinalDirection == CardinalDirection.East)
                {
                    moveDirection = new Vector2(1, 0);
                }
                else if (latestCardinalDirection == CardinalDirection.West)
                {
                    moveDirection = new Vector2(-1, 0);
                }
            }

            var radius = 16;

            knightSlashInstance.GlobalRotation = moveDirection.Angle() + (Mathf.Pi / 2);
            knightSlashInstance.GlobalPosition = new Vector2(this.GlobalPosition.X + (radius * Mathf.Cos(knightSlashInstance.GlobalRotation - (Mathf.Pi / 2))), this.GlobalPosition.Y + (radius * Mathf.Sin(knightSlashInstance.GlobalRotation - (Mathf.Pi / 2))));

			knightSlashInstance.ZIndex = this.ZIndex + 100;
		}
	}
}
