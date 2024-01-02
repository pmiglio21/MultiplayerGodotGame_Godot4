using Godot;
using System;

namespace MobileEntities.Enemies.Scripts
{
	public partial class BaseEnemy : BaseMobileEntity
	{
		protected Area2D mainHurtBox;
		protected CollisionShape2D mainHurtBoxCollisionShape;
		protected Area2D mainHitBox;
		protected CollisionShape2D mainHitBoxCollisionShape;

		public override void _Ready()
		{
			mainHurtBox = GetNode<Area2D>("MainHurtBox");
			mainHurtBoxCollisionShape = mainHurtBox.GetNode<CollisionShape2D>("CollisionShape");
			mainHurtBoxCollisionShape.Scale = new Vector2(1, 1);

			mainHitBox = GetNode<Area2D>("MainHitBox");
			mainHitBoxCollisionShape = mainHitBox.GetNode<CollisionShape2D>("CollisionShape");
			mainHitBoxCollisionShape.Scale = new Vector2(1, 1);
			//mainHitBoxCollisionShape.Disabled = true;
		}

		public override void _Process(double delta)
		{
			InitializeEnemySpecificProperties();
		}

		protected virtual void InitializeEnemySpecificProperties() { }

		#region	Signal Receptions

		#region Trigger Boxes

		private void OnMainHitBoxAreaEntered(Area2D area)
		{
			//GD.Print("Enemy Hit Entered");
		}


		private void OnMainHitBoxAreaExited(Area2D area)
		{
			//GD.Print("Enemy Hit Exited");
		}


		private void OnMainHurtBoxAreaEntered(Area2D area)
		{
			if (area.IsInGroup("PlayerProjectileTriggerBox"))
			{
				CollisionShape2D collisionShape = area.GetNode<CollisionShape2D>("CollisionShape");

				if (!collisionShape.Disabled)
				{
					GD.Print("Enemy Hurt Entered");

					characterStats.Health.HealthAmount -= 1;
				}
			}
		}


		private void OnMainHurtBoxAreaExited(Area2D area)
		{
			//GD.Print("Enemy Hurt Exited");
		}

		#endregion

		#endregion
	}


}
