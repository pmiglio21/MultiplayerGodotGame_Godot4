using Enums;
using Godot;
using MobileEntities.PlayerCharacters;
using Root;
using System;

namespace MobileEntities.Enemies.Scripts
{
	public partial class BaseEnemy : BaseMobileEntity
	{
        private DungeonLevelSwapper _parentDungeonLevelSwapper;

        private BaseDungeonLevel _parentLevel;

        private const int _speed = 10;

        private const int _distanceToDetectPlayer = 256;

        private string _statPickupPath = "res://Levels/OverworldLevels/Items/StatPickup.tscn";

        #region Components

        protected Area2D mainHurtBox;
		protected CollisionShape2D mainHurtBoxCollisionShape;
		protected Area2D mainHitBox;
		protected CollisionShape2D mainHitBoxCollisionShape;
		protected AnimationPlayer animationPlayer;
		protected Timer gracePeriodTimer;
        protected Timer playerChaseTimer;
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

            _parentLevel = this.GetParent() as BaseDungeonLevel;

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
            playerChaseTimer = FindChild("PlayerChaseTimer") as Timer;
        }

		#endregion

		public override void _PhysicsProcess(double delta)
		{
			ZIndex = (int)this.GlobalPosition.Y;

            MoveEnemy();
        }

		protected virtual void InitializeEnemySpecificProperties() { }

		protected virtual void MoveEnemy() 
		{
            BaseCharacter closestPlayer = FindClosestPlayer();

            if (closestPlayer != null)
            {
                playerDetectionBox.Scale = Vector2.One;

                var distanceBetweenClosestPlayer = closestPlayer.GlobalPosition.DistanceTo(GlobalPosition);

                var direction = (closestPlayer.GlobalPosition - GlobalPosition).Normalized();

                var angleBetweenClosestPlayerAndEnemy = (closestPlayer.GlobalPosition - GlobalPosition).Angle();

                
                playerDetectionBox.Rotation = angleBetweenClosestPlayerAndEnemy;

                playerDetectionBox.GlobalPosition = (closestPlayer.GlobalPosition + GlobalPosition) / 2;

                var playerDetectionBoxSize = new Vector2(distanceBetweenClosestPlayer, playerDetectionBoxCollisionShape.Shape.GetRect().Size.Y);

                playerDetectionBoxCollisionShape.Shape = new RectangleShape2D() { Size = playerDetectionBoxSize };


                var overlappingBodies = playerDetectionBox.GetOverlappingBodies();

                isWallDetected = false;

                foreach (var body in overlappingBodies)
                {
                    if (body.GetParent() is InteriorWallBlock)
                    {
                        isWallDetected = true;
                        break;
                    }
                }


                if (isPlayerDetected && !isWallDetected)
                {
                    if (playerChaseTimer.IsStopped())
                    {
                        playerChaseTimer.Start();
                    }
                }


                if (!playerChaseTimer.IsStopped() ||
                    (distanceBetweenClosestPlayer <= _distanceToDetectPlayer && isPlayerDetected && !isWallDetected))
                {
                    Velocity = direction * _speed;

                    MoveAndSlide();
                }
            }
            else
            {
                playerDetectionBox.Scale = Vector2.Zero;

                Velocity = Vector2.Zero;

                MoveAndSlide();
            }
        }

		protected BaseCharacter FindClosestPlayer()
		{
            BaseCharacter closestPlayer = null;
            float minDistance = float.MaxValue;

            foreach (var player in _parentDungeonLevelSwapper.ActivePlayers)
			{
				var newDistance = player.GlobalPosition.DistanceTo(this.GlobalPosition);

				if (newDistance <= _distanceToDetectPlayer && newDistance < minDistance)
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

                            RunDeathProcess();
						}
						else
						{
							gracePeriodTimer.Start();
						}
					}
				}
			}
		}

        private void RunDeathProcess()
        {
            PackedScene statPickupScene = GD.Load<PackedScene>(_statPickupPath);

            var statPickupNode = statPickupScene.Instantiate();

            _parentLevel.AddChild(statPickupNode);

            var statPickup = statPickupNode as StatPickup;

            statPickup.StatType = (StatType)GD.RandRange(0, Enum.GetValues(typeof(StatType)).Length - 1);
            statPickup.StatSize = (StatSize)GD.RandRange(0, Enum.GetValues(typeof(StatSize)).Length - 1);

            statPickup.GlobalPosition = GlobalPosition;
            statPickup.ZIndex = ZIndex;
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

                    //GD.Print($"PlayerHurtBox Entered {area.GetParent().Name}");
                }
            }
        }

        private void OnPlayerDetectionBoxAreaExited(Area2D area)
        {
            if (area.IsInGroup("PlayerHurtBox"))
            {
                CollisionShape2D collisionShape = area.GetNode<CollisionShape2D>("CollisionShape");

                if (!collisionShape.Disabled)
                {
                    Node2D character = area.GetParent() as Node2D;

                    isPlayerDetected = false;

                    //GD.Print($"PlayerHurtBox Exited {area.GetParent().Name}");
                }
            }
        }

        private void OnEnemyDetectionAreaEntered(Area2D area)
        {
            if (area.IsInGroup("Enemy"))
            {
                CollisionShape2D collisionShape = area.GetNode<CollisionShape2D>("CollisionShape");

                if (!collisionShape.Disabled)
                {
                    //Node2D enemy = area.GetParent() as Node2D;

                    //var direction = GlobalPosition - area.GlobalPosition;

                    //GlobalPosition += direction.Normalized() * _speed;
                }
            }
        }

        #endregion

        #endregion
    }
}
