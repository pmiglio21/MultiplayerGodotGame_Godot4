using Enums;
using Globals;
using Globals.PlayerManagement;
using Godot;
using Levels.UtilityLevels;
using Enums.GameRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileEntities.PlayerCharacters.Scripts
{
	public partial class BaseCharacter : BaseMobileEntity
	{
		#region Components
		[Export]
		protected PlayableCharacterClass characterClass;

		protected AnimationPlayer animationPlayer;

		private List<string> _animationList; 

		protected Sprite2D playerSprite;

		public Camera2D playerCamera;

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
		protected bool isControllable = true;

		protected bool isHurt = false;

		protected bool isAttacking = false;

		protected bool finishedAttack = false;

		protected bool isDead = false;

		protected bool isInPortal = false;
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
			InitializeClassSpecificProperties();

			#region Initializing Components
			animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
			_animationList = animationPlayer.GetAnimationList().ToList();

			playerSprite = GetNode<Sprite2D>("PlayerSprite");

			playerCamera = GetNode<Camera2D>("Camera2D");

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
			if (characterStats.Health.HealthAmount <= 0)
			{
				isDead = true;

				//REMOVE THIS LATER
				//isControllable = false;

				PlayAppropriateAnimation(latestCardinalDirection, AnimationType.Dead);
			}

			if (isControllable)
			{
				if (_portalWaitTimer < _portalWaitTimerMax && isInPortal)
				{
					_portalWaitTimer++;
				}
				else if (_portalWaitTimer >= _portalWaitTimerMax && isInPortal)
				{
					//Portal jump

					//GD.Print("PORTAL JUMP");

					_portalWaitTimer = 0;
				}

				if (initialInputTimer < initialInputTimerMax)
				{
					initialInputTimer++;
				}
				else
				{
					#region Get Input

					GetPauseInput();

					GetAttackInput();

					GetMovementInput();
					 
					#endregion

					if (isAttacking && attackInputTimer == attackInputTimerMax && !finishedAttack)
					{
						moveDirection = Vector2.Zero;

						attackInputTimer = 0;

						RunAttack();

						finishedAttack = true;

						//GD.Print("Attacking");
					}

					MoveHurtBoxes(latestCardinalDirection);

					SetAnimationToBePlayed();
				}
			}

			if (CurrentSaveGameRules.CurrentSplitScreenMergingType == SplitScreenMergingType.ScreenPerPlayer ||
				(PlayerManager.ActivePlayers.Count == 1))
			{
				playerCamera.GlobalPosition = this.GlobalPosition;
			}
		}

		public override void _PhysicsProcess(double delta)
		{
			if (isControllable && !isHurt)
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

		protected virtual void InitializeClassSpecificProperties() { }

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

			if (!_pauseChangedRecently && Input.IsActionJustPressed($"StartButton_{DeviceIdentifier}"))
			{
				pauseScreen.IsPauseScreenBeingShown = true;
				GetTree().Paused = true;
				IsPaused = true;

				pauseScreen.GrabFocusOfTopButton();
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
				isAttacking = Input.IsActionJustPressed($"WestButton_{DeviceIdentifier}");

				attackTurnDirection.X = Input.GetActionStrength($"MoveEast_{DeviceIdentifier}") - Input.GetActionStrength($"MoveWest_{DeviceIdentifier}");

				if (!isAttacking)
				{
					finishedAttack = false;
				}
			}
		}

		protected void GetMovementInput()
		{
			if (!isAttacking)
			{
				moveInput.X = Input.GetActionStrength($"MoveEast_{DeviceIdentifier}") - Input.GetActionStrength($"MoveWest_{DeviceIdentifier}");
				moveInput.Y = Input.GetActionStrength($"MoveSouth_{DeviceIdentifier}") - Input.GetActionStrength($"MoveNorth_{DeviceIdentifier}");

				if (Vector2.Zero.DistanceTo(moveInput) > moveDeadzone * Math.Sqrt(2.0))
				{
					//if you want to check input for walking and running speeds, do it here
					moveDirection = moveInput.Normalized();
				}
				else
				{
					moveDirection = Vector2.Zero;
				}

				//GD.Print($"Movement Vector: {moveDirection}");
			}
		}

		private void MovePlayer()
		{
			Velocity = moveDirection * speed;

			MoveAndSlide();

			GD.Print(Position);

			if (CurrentSaveGameRules.CurrentSplitScreenMergingType == SplitScreenMergingType.SharedScreenLocked)
			{
				//var canvasPos = GetGlobalTransform() * Position;
				//Position = GetGlobalTransform().AffineInverse() * canvasPos;

				//var screenCord = GetViewport().GetScreenTransform() * GetGlobalTransformWithCanvas() * Position;

				//GD.Print($"Screen Coord: {screenCord}");



				//var a = GlobalGameComponents.AvailableSubViewports[0].GetScreenTransform().;



				//var posX = Mathf.Clamp(Position.X, 0, );
				//var posY = Mathf.Clamp(Position.Y, 0, GlobalGameComponents.AvailableSubViewports[0].Position.Y);

				//GD.Print($"GetWindow().Size: {GlobalGameComponents.AvailableSubViewports[0].GetCamera2D().GetViewportRect().Size.X}, {GlobalGameComponents.AvailableSubViewports[0].GetCamera2D().GetViewportRect().Size.Y}");
				//GD.Print($"Chosen Position: {posX}, {posY}");

				

				//Position = new Vector2(posX, posY);
			}
		}

		private void SetAnimationToBePlayed()
		{
			if (!isAttacking)
			{
				if (moveDirection == Vector2.Zero)
				{
					//GD.Print($"idling {latestCardinalDirection}");
					PlayAppropriateAnimation(latestCardinalDirection, AnimationType.Idle);
				}
				if (moveDirection != Vector2.Zero)
				{
					latestCardinalDirection = FindLatestCardinalDirection(latestCardinalDirection, moveDirection);

					//GD.Print($"moving {latestCardinalDirection}");

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
				//GD.Print($"Playing: {animationType}_{cardinalDirection}");
				animationPlayer.Play($"{animationType}_{cardinalDirection}");
			}
		}

		#endregion

		#region Signal Receptions

		private void OnAnimationPlayerAnimationFinished(StringName animationName)
		{
			string animationNameString = animationName.ToString();

			//GD.Print($"{animationName} finished");
			if (animationNameString.Contains(AnimationType.Attack.ToString()))
			{
				isAttacking = false;
			}
		}

		private void OnVisibleOnScreenNotifierScreenExited()
		{
			Vector2I mainViewportSize = GetWindow().Size;

			GD.Print($"{PlayerNumber} leaving");

			//GlobalGameComponents.AvailableSubViewports[0].GetCamera2D().Zoom = GlobalGameComponents.AvailableSubViewports[0].GetCamera2D().Zoom * .8f;

			// //Testing Camera manipulation
			//GlobalGameComponents.AvailableSubViewports[0].Size = new Vector2I((mainViewportSize.X / 2), mainViewportSize.Y);
			//GlobalGameComponents.AvailableSubViewports[1].Size = new Vector2I((mainViewportSize.X / 2), mainViewportSize.Y);
		}

		private void OnVisibleOnScreenNotifierScreenEntered()
		{
			Vector2I mainViewportSize = GetWindow().Size;

			GD.Print($"{PlayerNumber} entering");

			//GlobalGameComponents.AvailableSubViewports[0].GetCamera2D().Zoom = GlobalGameComponents.AvailableSubViewports[0].GetCamera2D().Zoom * 1.25f;

			// //Testing Camera manipulation
			//GlobalGameComponents.AvailableSubViewports[0].Size = mainViewportSize;
			//GlobalGameComponents.AvailableSubViewports[1].Size = Vector2I.Zero;
		}

		#region Trigger Boxes
		private void OnMainHitBoxAreaEntered(Area2D area)
		{
			//GD.Print("Player Hit Entered");
		}


		private void OnMainHitBoxAreaExited(Area2D area)
		{
			//GD.Print("Player Hit Exited");
		}


		private void OnMainHurtBoxAreaEntered(Area2D area)
		{
			if (area.IsInGroup("EnemyHitBox"))
			{
				CollisionShape2D collisionShape = area.GetNode<CollisionShape2D>("CollisionShape");

				//if (!collisionShape.Disabled)
				//{
				//	GD.Print("Player Hurt Entered");

				//	characterStats.Health.HealthAmount -= 1;
				//}
			}
			else if (area.IsInGroup("PortalArea"))
			{
				CollisionShape2D collisionShape = area.GetNode<CollisionShape2D>("CollisionShape2D");

				if (!collisionShape.Disabled)
				{
					//GD.Print("IsInPortal");
					isInPortal = true;
				}
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
					//GD.Print("!IsInPortal");
					isInPortal = false;
					_portalWaitTimer = 0;
				}
			}
		}
		#endregion

		#endregion
	}
}





