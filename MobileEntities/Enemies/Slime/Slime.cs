using Enums;
using Godot;
using MobileEntities.CharacterStats;
using MobileEntities.PlayerCharacters;

namespace MobileEntities.Enemies.Scripts
{
	public partial class Slime : BaseEnemy
	{
		protected override void InitializeEnemySpecificProperties()
		{
			enemyType = EnemyType.Slime;

			CharacterStats = new Stats(1);
		}
	}
}
