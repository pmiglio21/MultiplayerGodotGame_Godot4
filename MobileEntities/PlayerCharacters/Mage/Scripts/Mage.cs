using Godot;
using MobileEntities.CharacterStats;
using MobileEntities.PlayerCharacters;
using System;

namespace MobileEntities.PlayerCharacters.Scripts
{
	public partial class Mage : BaseCharacter
	{
		protected override void InitializeClassSpecificProperties()
		{
			characterStats = new Stats(10);
		}
	}
}
