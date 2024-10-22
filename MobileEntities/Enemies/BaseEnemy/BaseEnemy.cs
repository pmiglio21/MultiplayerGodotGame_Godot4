using Enums;
using Globals.PlayerManagement;
using Godot;
using MobileEntities.PlayerCharacters.Scripts;
using Root;
using System.Collections.Generic;

namespace MobileEntities.Enemies.Scripts
{
	public partial class BaseEnemy : BaseMobileEntity
	{
        private LevelHolder _parentDungeonLevelSwapper;

        #region Components

        protected Area2D mainHurtBox;
		protected CollisionShape2D mainHurtBoxCollisionShape;
		protected Area2D mainHitBox;
		protected CollisionShape2D mainHitBoxCollisionShape;
		protected AnimationPlayer animationPlayer;
		protected Timer gracePeriodTimer;

		#endregion

		protected EnemyType enemyType;

		#region On Ready

		public override void _Ready()
		{
            RootSceneSwapper rootSceneSwapper = GetTree().Root.GetNode<RootSceneSwapper>("RootSceneSwapper");

            _parentDungeonLevelSwapper = rootSceneSwapper.GetDungeonLevelSwapper();


            InitializeComponents();

			InitializeEnemySpecificProperties();
		}

		private void InitializeComponents()
		{
			mainHurtBox = GetNode<Area2D>("MainHurtBox");
			mainHurtBoxCollisionShape = mainHurtBox.GetNode<CollisionShape2D>("CollisionShape");
			mainHurtBoxCollisionShape.Scale = new Vector2(1, 1);

			mainHitBox = GetNode<Area2D>("MainHitBox");
			mainHitBoxCollisionShape = mainHitBox.GetNode<CollisionShape2D>("CollisionShape");
			mainHitBoxCollisionShape.Scale = new Vector2(1, 1);
			//mainHitBoxCollisionShape.Disabled = true;

			animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
			animationPlayer.Play("Idle_East");

			gracePeriodTimer = FindChild("GracePeriodTimer") as Timer;
		}

		#endregion

		public override void _Process(double delta)
		{
			ZIndex = (int)this.GlobalPosition.Y;

			MoveEnemy();
		}

		protected virtual void InitializeEnemySpecificProperties() { }

		protected virtual void MoveEnemy() { }

		protected BaseCharacter FindClosestPlayer()
		{
			BaseCharacter closestPlayer = null;
			float minDistance = float.MaxValue; 

			foreach (var player in _parentDungeonLevelSwapper.ActivePlayers)
			{
				var newDistance = player.GlobalPosition.DistanceTo(this.GlobalPosition);

				if (newDistance <= 200 && newDistance < minDistance)
				{
					closestPlayer = player;
					minDistance = newDistance;
				}
			}

			return closestPlayer;
		}

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
			if (area.IsInGroup("PlayerAttack") || area.IsInGroup("PlayerHitBox"))
			{
				CollisionShape2D collisionShape = area.GetNode<CollisionShape2D>("CollisionShape");

				if (!collisionShape.Disabled)
				{
					GD.Print("Enemy Hurt Entered");

					if (gracePeriodTimer.IsStopped())
					{
						characterStats.Health.HealthAmount -= 1;

						GD.Print($"Health: {characterStats.Health.HealthAmount}");

						if (characterStats.Health.HealthAmount <= 0)
						{
							this.QueueFree();
						}
						else
						{
							gracePeriodTimer.Start();
						}
					}
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
