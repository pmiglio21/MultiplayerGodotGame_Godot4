using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogGame.MobileEntities.PlayerCharacters.PlayerCharacterTriggerBoxes
{
	public partial class PlayerHitBox : Area3D
	{
		private CollisionShape3D _hitBoxCollisionShape;

		public override void _Ready()
		{
			_hitBoxCollisionShape = GetNode<CollisionShape3D>("HitBoxCollisionShape");

			_hitBoxCollisionShape.Disabled = true;
		}
	}
}
