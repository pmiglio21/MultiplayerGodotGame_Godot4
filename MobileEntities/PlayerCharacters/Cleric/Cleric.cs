using Godot;
using MobileEntities.CharacterStats;
using MobileEntities.PlayerCharacters;
using System;

namespace MobileEntities.PlayerCharacters.Scripts
{
	public partial class Cleric : BaseCharacter
	{
        protected override void InitializeClassSpecificProperties()
        {
            characterStats = new Stats(3);

            characterStats.BaseHealth = 4;
            characterStats.BaseAttack = 2;
            characterStats.BaseDefense = 2;
            characterStats.BaseSpeed = 2;
        }
    }
}
