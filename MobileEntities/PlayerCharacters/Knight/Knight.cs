using Enums;
using Globals;
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

            this.AddChild(knightSlashInstance);

            Vector2 attackDirection = Vector2.Zero;

            if (this.DeviceIdentifier == GlobalConstants.KeyboardDeviceIdentifier)
            {
                //TODO: Not working. Not the correct angling
                Vector2 mouseClickPosition = GetViewport().GetMousePosition();

                GD.Print($"Position: {mouseClickPosition}");

                knightSlashInstance.GlobalRotation = GlobalPosition.AngleTo(mouseClickPosition);// + (Mathf.Pi / 2);
            }
            else
            {
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

                attackDirection = moveDirection;

                knightSlashInstance.GlobalRotation = attackDirection.Angle() + (Mathf.Pi / 2);
            }

            var radius = 16;
            
            knightSlashInstance.GlobalPosition = new Vector2(this.GlobalPosition.X + (radius * Mathf.Cos(knightSlashInstance.GlobalRotation - (Mathf.Pi / 2))), this.GlobalPosition.Y + (radius * Mathf.Sin(knightSlashInstance.GlobalRotation - (Mathf.Pi / 2))));

			knightSlashInstance.ZIndex = this.ZIndex + 100;
		}
	}
}
