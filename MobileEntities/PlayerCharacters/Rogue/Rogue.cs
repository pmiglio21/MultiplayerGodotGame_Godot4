using Godot;
using MobileEntities.CharacterStats;
using System;

namespace MobileEntities.PlayerCharacters
{
	public partial class Rogue : BaseCharacter
	{
        protected override void InitializeClassSpecificProperties()
        {
            CharacterStats = new Stats(3);

            CharacterStats.BaseHealth = 1;
            CharacterStats.BaseAttack = 3;
            CharacterStats.BaseDefense = 2;
            CharacterStats.BaseSpeed = 4;

            CharacterStats.CalculateStatsOnLevelUp();
        }
    }
}
