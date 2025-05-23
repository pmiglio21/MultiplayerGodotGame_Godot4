using Godot;
using MobileEntities.CharacterStats;
using System;

namespace MobileEntities.PlayerCharacters
{
	public partial class Cleric : BaseCharacter
	{
        protected override void InitializeClassSpecificProperties()
        {
            CharacterStats = new Stats(3);

            CharacterStats.BaseHealth = 4;
            CharacterStats.BaseAttack = 2;
            CharacterStats.BaseDefense = 2;
            CharacterStats.BaseSpeed = 2;

            CharacterStats.CalculateStatsOnLevelUp();
        }
    }
}
