using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileEntities.Enemies.EnemyCharacterTriggerBoxes
{
	public partial class EnemyHitBox : Area3D
	{
		private CollisionShape3D _hitBoxCollisionShape;

		public override void _Ready()
		{
			_hitBoxCollisionShape = GetNode<CollisionShape3D>("HitBoxCollisionShape");

			_hitBoxCollisionShape.Disabled = true;
		}
	}
}
