using Enums;
using Godot;
using MobileEntities.CharacterStats;
using MobileEntities.PlayerCharacters.Scripts;

namespace MobileEntities.Enemies.Scripts
{
	public partial class Slime : BaseEnemy
	{
		protected override void InitializeEnemySpecificProperties()
		{
			enemyType = EnemyType.Slime;

			characterStats = new Stats(3);
		}

		protected override void MoveEnemy()
		{
			BaseCharacter closestPlayer = FindClosestPlayer();

			if (closestPlayer != null)
			{
				var direction = (closestPlayer.GlobalPosition - GlobalPosition).Normalized();

				Velocity = direction * 5;
			}
			else
			{
				Velocity = Vector2.Zero;
			}

			MoveAndSlide();
		}
	}
}
