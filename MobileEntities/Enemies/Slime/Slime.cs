using Enums;
using Godot;
using MobileEntities.CharacterStats;

namespace MobileEntities.Enemies.Scripts
{
	public partial class Slime : BaseEnemy
	{
		protected override void InitializeEnemySpecificProperties()
		{
			enemyType = EnemyType.Slime;

			characterStats = new Stats(4);
		}
	}
}
