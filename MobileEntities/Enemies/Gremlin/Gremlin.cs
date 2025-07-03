using Enums;
using Godot;
using MobileEntities.CharacterStats;
using MobileEntities.PlayerCharacters;

namespace MobileEntities.Enemies.Scripts
{
    public partial class Gremlin : BaseEnemy
    {
        protected override void InitializeEnemySpecificProperties()
        {
            enemyType = EnemyType.Gremlin;

            CharacterStats = new Stats(3);
        }
    }
}
