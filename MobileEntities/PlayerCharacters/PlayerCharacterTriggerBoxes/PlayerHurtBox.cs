using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogGame.MobileEntities.PlayerCharacters.PlayerCharacterTriggerBoxes
{
	public partial class PlayerHurtBox : Area3D
	{
		private CollisionShape3D _hurtBoxCollisionShape;

		public override void _Ready()
		{
			_hurtBoxCollisionShape = GetNode<CollisionShape3D>("HurtBoxCollisionShape");

			_hurtBoxCollisionShape.Disabled = false;
		}
	}
}
