using Godot;
using MobileEntities.CharacterStats;
using System;

namespace MobileEntities.Enemies.Scripts
{
	public partial class Slime : BaseEnemy
	{
		protected override void InitializeEnemySpecificProperties()
		{
			characterStats = new Stats(4);
		}
	}
}
