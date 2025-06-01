using Enums;
using Godot;
using MobileEntities.PlayerCharacters;
using Root;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace MobileEntities.Enemies.Scripts
{
	public partial class BaseEnemy : BaseMobileEntity
	{
        private DungeonLevelSwapper _parentDungeonLevelSwapper;

        private const int _speed = 10;
        private const int _playerDetectionFrameCountMax = 7;
		private int _playerDetectionFrameCount = 0;

        private const int _distanceToPlayerDetectionCheck = 256;
        private const int _distanceToPlayerDetectionMoving = 256;

        #region Components

        protected Area2D mainHurtBox;
		protected CollisionShape2D mainHurtBoxCollisionShape;
		protected Area2D mainHitBox;
		protected CollisionShape2D mainHitBoxCollisionShape;
		protected AnimationPlayer animationPlayer;
		protected Timer gracePeriodTimer;
        protected Area2D playerDetectionBox;
        protected CollisionShape2D playerDetectionBoxCollisionShape;

        #endregion

        protected bool isPlayerDetected = false;
        protected bool isWallDetected = false;

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

            playerDetectionBox = GetNode<Area2D>("PlayerDetectionBox");
            playerDetectionBoxCollisionShape = playerDetectionBox.GetNode<CollisionShape2D>("CollisionShape");

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

		protected virtual void MoveEnemy() 
		{
            if (_playerDetectionFrameCount == _playerDetectionFrameCountMax)
            {
                _playerDetectionFrameCount = 0;
            }
            else
            {
                _playerDetectionFrameCount++;
            }

            BaseCharacter closestPlayer = FindClosestPlayer();

            if (closestPlayer != null)
            {
                playerDetectionBox.Scale = Vector2.One;

                var distanceBetweenClosestPlayer = closestPlayer.GlobalPosition.DistanceTo(GlobalPosition);

                var direction = (closestPlayer.GlobalPosition - GlobalPosition).Normalized();

                var angleBetweenClosestPlayerAndEnemy = (closestPlayer.GlobalPosition - GlobalPosition).Angle();

                playerDetectionBox.Rotation = angleBetweenClosestPlayerAndEnemy;

                playerDetectionBox.GlobalPosition = (closestPlayer.GlobalPosition + GlobalPosition) / 2;

                playerDetectionBoxCollisionShape.Shape = new RectangleShape2D() { Size = new Vector2(distanceBetweenClosestPlayer, playerDetectionBoxCollisionShape.Shape.GetRect().Size.Y) };

                Velocity = direction * _speed;

                if (distanceBetweenClosestPlayer <= _distanceToPlayerDetectionMoving && isPlayerDetected && !isWallDetected)
                {
                    MoveAndSlide();
                }
            }
            else
            {
                playerDetectionBox.Scale = Vector2.Zero;

                Velocity = Vector2.Zero;

                MoveAndSlide();
            }

            isPlayerDetected = false;
            isWallDetected = false;
        }

		protected BaseCharacter FindClosestPlayer()
		{
            BaseCharacter closestPlayer = null;
            float minDistance = float.MaxValue;

            foreach (var player in _parentDungeonLevelSwapper.ActivePlayers)
			{
				var newDistance = player.GlobalPosition.DistanceTo(this.GlobalPosition);

				if (newDistance <= _distanceToPlayerDetectionCheck && newDistance < minDistance)
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
						CharacterStats.Health -= 1;

						GD.Print($"Health: {CharacterStats.Health}");

						if (CharacterStats.Health <= 0)
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

		private void OnPlayerDetectionBoxAreaEntered(Area2D area)
		{
            if (area.IsInGroup("PlayerHurtBox"))
            {
                CollisionShape2D collisionShape = area.GetNode<CollisionShape2D>("CollisionShape");

                if (!collisionShape.Disabled)
                {
                    isPlayerDetected = true;

                    //GD.Print("I detect player");
                }
            }
        }

        private void OnPlayerDetectionBoxBodyEntered(Node2D node2D)
        {
            if (node2D.IsInGroup("InteriorWallBlock"))
            {
                CollisionShape2D collisionShape = node2D.GetNode<CollisionShape2D>("CollisionShape2D");

                if (!collisionShape.Disabled)
                {
                    isWallDetected = true;

                    //GD.Print("I detect wall");
                }
            }
        }

        #endregion

        #endregion
    }
}
