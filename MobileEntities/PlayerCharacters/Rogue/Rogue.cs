using Godot;
using MobileEntities.CharacterStats;
using MobileEntities.PlayerCharacters;
using System;

namespace MobileEntities.PlayerCharacters.Scripts
{
	public partial class Rogue : BaseCharacter
	{
        protected override void InitializeClassSpecificProperties()
        {
            characterStats = new Stats(3);

            characterStats.BaseHealth = 1;
            characterStats.BaseAttack = 3;
            characterStats.BaseDefense = 2;
            characterStats.BaseSpeed = 4;

            characterStats.CalculateStatsOnLevelUp();
        }
    }
}
