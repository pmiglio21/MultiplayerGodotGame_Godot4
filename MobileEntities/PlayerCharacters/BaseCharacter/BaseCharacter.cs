using Enums;
using Globals;
using Godot;
using Levels.OverworldLevels.KeyLevelObjects;
using Levels.UtilityLevels;
using System;
using System.Collections.Generic;
using System.Linq;
using Root;
using MobileEntities.CharacterStats;
using Levels.UtilityLevels.UserInterfaceComponents;

namespace MobileEntities.PlayerCharacters
{
	public partial class BaseCharacter : BaseMobileEntity
	{
        private DungeonLevelSwapper _parentDungeonLevelSwapper;
        private SplitScreenManager _splitScreenManager;
		private BaseDungeonLevel _baseDungeonLevel;

        private List<Node2D> _inventoryPocketableObjectsInArea = new List<Node2D>();

        #region Components
        [Export]
		protected PlayableCharacterClass characterClass;

		protected AnimationPlayer animationPlayer;

		private List<string> _animationList; 

		protected Sprite2D playerSprite;

		public Camera2D playerCamera;

		protected Timer rollingTimer;

		#endregion

		#region Input Properties

		Vector2 moveInput = Vector2.Zero;
		float moveInputDeadzone = 0.1f;

		float moveDeadzone = 0.32f;
		protected Vector2 moveDirection = Vector2.Zero;

		float speed = 100.0f;

		private int initialInputTimer = 0;
		private int initialInputTimerMax = 25;

		private int attackInputTimer = 0;
		private int attackInputTimerMax = 10;

		public bool IsFacingRight
		{
			get { return _isFacingRight; }

			set
			{
				if (value != _isFacingRight)
				{
					_isFacingRight = value;
				}
			}
		}
		private bool _isFacingRight = false;

		protected CardinalDirection latestCardinalDirection = CardinalDirection.East;

		protected Vector2 attackTurnDirection = Vector2.Zero;

		#endregion

		#region Player Identification Properties

		public PlayableCharacterClass CharacterClassName;

		public int PlayerNumber = -1;
		public string DeviceIdentifier = "-1";

		#endregion

		#region Player State Properties
		public bool IsControllable = true;

		protected bool isHurt = false;

		protected bool isAttacking = false;

		protected bool finishedAttack = false;

		protected bool isRolling = false;

		public bool IsDead
		{
			get { return _isDead; }
			set
			{
				if (_isDead != value)
				{
					_isDead = value;

                    //IsControllable = false;

                    GD.Print("DEAD");

                    this.playerSprite.Modulate = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                }
            }
		}
		private bool _isDead = false;

		public bool IsInPortalArea = false;	 //Portal not necessarily activated

        public bool IsWaitingForPortal = false;	//Portal is activated and character is awaiting jump

        public bool IsWaitingForNextLevel = false;
		#endregion

		#region Player System Properties

		public Inventory Inventory = new Inventory();
		
		private float _staminaAmount = 100;
        private float _staminaDepletionAmount = 20;
        private float _staminaAdditionAmount = .2f;

        #endregion

        #region References to Outside Nodes

        protected PauseScreenManager pauseScreen;

		#endregion

		#region Pause Properties

		private bool _pauseChangedRecently = false;
		public bool IsPaused = false;

		private int _pauseTimer = 0;
		private const int _pauseTimerMax = 15;

		#endregion

		#region Portal Properties

		private int _portalWaitTimer = 0;

		private const int _portalWaitTimerMax = 150;

		#endregion

		public override void _Ready()
		{
            RootSceneSwapper rootSceneSwapper = GetTree().Root.GetNode<RootSceneSwapper>("RootSceneSwapper");

            _parentDungeonLevelSwapper = rootSceneSwapper.GetDungeonLevelSwapper();

            InitializeClassSpecificProperties();

			#region Initializing Components
			animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
			_animationList = animationPlayer.GetAnimationList().ToList();

			playerSprite = GetNode<Sprite2D>("PlayerSprite");

			playerCamera = GetNode<Camera2D>("Camera2D");

            rollingTimer = GetNode<Timer>("RollingTimer");

            #endregion

            InitializeDeadZones();

			GetReferencesToOutsideNodes();

			if (PlayerNumber % 2 == 0)
			{
				PlayAppropriateAnimation(CardinalDirection.East, AnimationType.Idle);
				MoveHurtBoxes(CardinalDirection.East);
			}
			else
			{
				PlayAppropriateAnimation(CardinalDirection.West, AnimationType.Idle);
				MoveHurtBoxes(CardinalDirection.West);
			}
		}

		public override void _Process(double delta)
		{
			ZIndex = (int)this.GlobalPosition.Y;

			if (CharacterStats.Health <= 0)
			{
				IsDead = true;

                //PlayAppropriateAnimation(latestCardinalDirection, AnimationType.Dead);
            }

            if (IsControllable)
			{
				if (_portalWaitTimer < _portalWaitTimerMax && IsWaitingForPortal)
				{
					_portalWaitTimer++;
				}
				else if (_portalWaitTimer >= _portalWaitTimerMax && IsWaitingForPortal)
				{
					GD.Print("PORTAL JUMP");

					_portalWaitTimer = 0;

					IsWaitingForNextLevel = true;
                    IsControllable = false;
					Hide();
				}

				if (initialInputTimer < initialInputTimerMax)
				{
					initialInputTimer++;
				}
				else
				{
					#region Get Input

					if (rollingTimer.IsStopped())
                    {
                        GetPauseInput();

                        GetInteractWithEnvironmentInput();

						GetInteractWithItemInput();

                        GetAttackInput();

                        GetMovementInput();

                        #endregion

                        if (isAttacking && attackInputTimer == attackInputTimerMax && !finishedAttack)
                        {
                            moveDirection = Vector2.Zero;

                            attackInputTimer = 0;

                            RunAttack();

                            finishedAttack = true;

                            GD.Print("Attacking");
                        }

                        MoveHurtBoxes(latestCardinalDirection);

                        SetAnimationToBePlayed();
                    }
				}
			}

            playerCamera.GlobalPosition = this.GlobalPosition;
        }

		public override void _PhysicsProcess(double delta)
		{
			if (IsControllable && !isHurt)
			{
				MovePlayer();
			}
		}

		#region On Ready Methods

		protected void InitializeDeadZones()
		{
			InputMap.ActionSetDeadzone($"MoveEast_{DeviceIdentifier}", moveInputDeadzone);
			InputMap.ActionSetDeadzone($"MoveWest_{DeviceIdentifier}", moveInputDeadzone);
			InputMap.ActionSetDeadzone($"MoveSouth_{DeviceIdentifier}", moveInputDeadzone);
			InputMap.ActionSetDeadzone($"MoveNorth_{DeviceIdentifier}", moveInputDeadzone);
		}

		protected virtual void InitializeClassSpecificProperties() 
		{
            CharacterStats = new Stats(3);
        }

		protected virtual void MoveHurtBoxes(CardinalDirection hurtBoxDirection) { }

		protected virtual void RunAttack() { }

		private void GetReferencesToOutsideNodes()
		{
			GetPauseScreen();
		}

		private void GetPauseScreen()
		{
			var pauseScreens = GetTree().GetNodesInGroup("PauseScreen");

			if (pauseScreens != null)
			{
				pauseScreen = pauseScreens.First() as PauseScreenManager;
			}
		}

		#endregion

		#region On Update Methods

		private void GetPauseInput()
		{
			//Pause button hit in PauseScreen
			if (!_pauseChangedRecently && IsPaused != GetTree().Paused && _pauseTimer < _pauseTimerMax)
			{
				_pauseChangedRecently = true;
				IsPaused = false;
			}

			//Let timer go
			if (_pauseChangedRecently && _pauseTimer < _pauseTimerMax)
			{
				_pauseTimer++;
			}
			else
			{
				_pauseChangedRecently = false;
				_pauseTimer = 0;
			}

			if (!_pauseChangedRecently && Input.IsActionJustPressed($"{InputType.GameActionPause}_{DeviceIdentifier}"))
			{
				pauseScreen.IsPauseScreenBeingShown = true;
				GetTree().Paused = true;
				IsPaused = true;

				pauseScreen.GrabFocusOfTopButton();
			}
		}

		protected void GetInteractWithEnvironmentInput()
		{
			if (_inventoryPocketableObjectsInArea.Any(x => x is Torch) && Input.IsActionJustPressed($"{InputType.GameActionInteract}_{DeviceIdentifier}"))
			{
				InventoryItem torchItem = new InventoryItem()
				{ 
					Type = InventoryItemType.Torch
				};

				if (Inventory.ItemsByType.ContainsKey(InventoryItemType.Torch))
                {
                    Inventory.ItemsByType[InventoryItemType.Torch].Add(torchItem);
                }
				else
				{
					Inventory.ItemsByType.Add(InventoryItemType.Torch, new List<InventoryItem>());
                    Inventory.ItemsByType[InventoryItemType.Torch].Add(torchItem);
					Inventory.ItemTypeOrder.Add(InventoryItemType.Torch);
                }

				Node2D torchNode = _inventoryPocketableObjectsInArea.FirstOrDefault(x => x is Torch);

				if (torchNode != null)
				{
					_inventoryPocketableObjectsInArea.Remove(torchNode);

					_parentDungeonLevelSwapper.GetLatestBaseDungeonLevel().RemoveChild(torchNode);

					if (!torchNode.IsQueuedForDeletion())
					{
                        torchNode.QueueFree();
                    }
					else
					{
						var a = 0;
					}
				}
            }
		}

		protected void GetInteractWithItemInput()
		{
			if (Inventory.ItemTypeOrder.Count > 0)
			{
                if (Input.IsActionJustPressed($"{InputType.GameActionLoopItemLeft}_{DeviceIdentifier}"))
                {
                    if (Inventory.CurrentItemTypeIndex == 0)
                    {
                        Inventory.CurrentItemTypeIndex = Inventory.ItemTypeOrder.Count - 1;
                    }
                    else
                    {
                        Inventory.CurrentItemTypeIndex = Inventory.CurrentItemTypeIndex - 1;
                    }

                    GD.Print($"Current Item Type Index: {Inventory.ItemTypeOrder[Inventory.CurrentItemTypeIndex]}");
                }
                else if (Input.IsActionJustPressed($"{InputType.GameActionLoopItemRight}_{DeviceIdentifier}"))
                {
                    if (Inventory.CurrentItemTypeIndex == Inventory.ItemTypeOrder.Count - 1)
                    {
                        Inventory.CurrentItemTypeIndex = 0;
                    }
                    else
                    {
                        Inventory.CurrentItemTypeIndex = Inventory.CurrentItemTypeIndex + 1;
                    }

                    GD.Print($"Current Item Type Index: {Inventory.ItemTypeOrder[Inventory.CurrentItemTypeIndex]}");
                }

				if (Input.IsActionJustPressed($"{InputType.GameActionActivateItem}_{DeviceIdentifier}"))
				{
					UseItem();
				}
            }
        }

        protected void GetAttackInput()
		{
			if (attackInputTimer < attackInputTimerMax)
			{
				attackInputTimer++;
			}
			else if (!isAttacking && attackInputTimer == attackInputTimerMax)
			{
				isAttacking = Input.IsActionJustPressed($"{InputType.GameActionAttack1}_{DeviceIdentifier}");

				attackTurnDirection.X = Input.GetActionStrength($"MoveEast_{DeviceIdentifier}") - Input.GetActionStrength($"MoveWest_{DeviceIdentifier}");

				if (!isAttacking)
				{
					finishedAttack = false;
				}
			}
		}

		protected void GetMovementInput()
		{
			moveInput.X = Input.GetActionStrength($"MoveEast_{DeviceIdentifier}") - Input.GetActionStrength($"MoveWest_{DeviceIdentifier}");
			moveInput.Y = Input.GetActionStrength($"MoveSouth_{DeviceIdentifier}") - Input.GetActionStrength($"MoveNorth_{DeviceIdentifier}");
			
            if (Vector2.Zero.DistanceTo(moveInput) > moveDeadzone * Math.Sqrt(2.0))
			{
                float speed = (float)((float)(100 + CharacterStats.Speed) / 100);
				var normalizedMoveInput = moveInput.Normalized();

                isRolling = Input.IsActionJustPressed($"{InputType.GameActionInteract}_{DeviceIdentifier}");

				if (_staminaAmount >= _staminaDepletionAmount && isRolling)
				{
					rollingTimer.Start();

                    //if you want to check input for walking and running speeds, do it here
                    moveDirection = normalizedMoveInput * speed * 2;

					_staminaAmount = Mathf.Clamp(_staminaAmount - _staminaDepletionAmount, 0, 100);

                    playerSprite.Modulate = ColorPaths.RollingColor;

                    //GD.Print($"I'm rolling {_staminaAmount}");
                }
				else
				{
                    //if you want to check input for walking and running speeds, do it here
                    moveDirection = normalizedMoveInput * speed;

                    _staminaAmount = Mathf.Clamp(_staminaAmount + _staminaAdditionAmount, 0, 100);

                    playerSprite.Modulate = ColorPaths.DefaultColor;

                    //GD.Print($"I never was {_staminaAmount}");
                }
            }
			else
			{
				moveDirection = Vector2.Zero;

				isRolling = false;

                _staminaAmount = Mathf.Clamp(_staminaAmount + _staminaAdditionAmount, 0, 100);

				playerSprite.Modulate = ColorPaths.DefaultColor;

                //GD.Print($"Now I'm not {_staminaAmount}");
            }
		}

		private void MovePlayer()
		{
			Velocity = moveDirection * speed;

			MoveAndSlide();
		}

		private void SetAnimationToBePlayed()
		{
			if (!isAttacking)
			{
				if (moveDirection == Vector2.Zero)
				{
					PlayAppropriateAnimation(latestCardinalDirection, AnimationType.Idle);
				}
				if (moveDirection != Vector2.Zero)
				{
					latestCardinalDirection = FindLatestCardinalDirection(latestCardinalDirection, moveDirection);

					PlayAppropriateAnimation(latestCardinalDirection, AnimationType.Move);
				}
			}
			else if (isAttacking)
			{
				latestCardinalDirection = FindLatestCardinalDirection(latestCardinalDirection, attackTurnDirection);

				PlayAppropriateAnimation(latestCardinalDirection, AnimationType.Attack);
			}
		}

		private void PlayAppropriateAnimation(CardinalDirection cardinalDirection, AnimationType animationType)
		{
			if (_animationList.Contains($"{animationType}_{cardinalDirection}"))
			{
				animationPlayer.Play($"{animationType}_{cardinalDirection}");
			}
		}

		#endregion

		#region Signal Receptions

		private void OnAnimationPlayerAnimationFinished(StringName animationName)
		{
			string animationNameString = animationName.ToString();

			GD.Print($"{animationName} finished");
			if (animationNameString.Contains(AnimationType.Attack.ToString()))
			{
				isAttacking = false;
			}
		}

		#region Trigger Boxes

		private void OnMainHurtBoxAreaEntered(Area2D area)
		{
			if (area.IsInGroup("EnemyHitBox"))
			{
				CollisionShape2D collisionShape = area.GetNode<CollisionShape2D>("CollisionShape");

				if (!collisionShape.Disabled)
				{
					GD.Print($"Player Hurt Entered - Health: {CharacterStats.Health}");

					//CharacterStats.Health -= 1;
				}
			}
			else if (area.IsInGroup("PortalArea"))
			{
				CollisionShape2D collisionShape = area.GetNode<CollisionShape2D>("CollisionShape2D");

				if (!collisionShape.Disabled)
				{
                    IsInPortalArea = true;
				}

                if (!collisionShape.Disabled && (collisionShape.GetParent().GetParent() as Portal).IsPortalActivated)
                {
                    IsWaitingForPortal = true;
                }
            }
			else if (area.IsInGroup("InventoryPocketable"))
			{
                _inventoryPocketableObjectsInArea.Add(area.GetParent() as Node2D);
            }
			else if (area.IsInGroup("PlayerDetectionBox"))
			{
				//GD.Print($"FROM CHARACTER {DeviceIdentifier}, entered PlayerDetectionBox");
			}
		}


		private void OnMainHurtBoxAreaExited(Area2D area)
		{
			//GD.Print("Player Hurt Exited");

			if (area.IsInGroup("PortalArea"))
			{
				CollisionShape2D collisionShape = area.GetNode<CollisionShape2D>("CollisionShape2D");

				if (!collisionShape.Disabled)
				{
                    IsInPortalArea = false;
                    IsWaitingForPortal = false;
                    _portalWaitTimer = 0;
				}
			}
            else if (area.IsInGroup("PlayerDetectionBox"))
            {
                //GD.Print($"FROM CHARACTER {DeviceIdentifier}, exited PlayerDetectionBox");
            }
            else if (area.IsInGroup("InventoryPocketable"))
            {
                _inventoryPocketableObjectsInArea.Remove(area.GetParent() as Node2D);
            }
        }
		#endregion

		#endregion
	}
}





