using Enums;
using Godot;
using Levels.OverworldLevels.Utilities;
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

		#endregion

		#region Input Properties

		Vector2 moveInput = Vector2.Zero;
		float moveInputDeadzone = 0.1f;

		float moveDeadzone = 0.32f;
		protected Vector2 moveDirection = Vector2.Zero;

		float speed = 100.0f;

		private int inputTimer = 0;
		private int inputTimerMax = 20;

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

		private CardinalDirection latestCardinalDirection;

		protected Vector2 attackDirection = Vector2.Zero;

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
		#endregion

		#region References to Outside Nodes

		protected PauseScreenManager pauseScreen;

		protected LevelCamera levelCamera;

		#endregion

		#region Pause Properties

		private bool _pauseChangedRecently = false;
		public bool IsPaused = false;

		private int _pauseTimer = 0;
		private const int _pauseTimerMax = 15;

		#endregion

		public override void _Ready()
		{
			InitializeClassSpecificProperties();

			#region Initializing Components
			animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
			_animationList = animationPlayer.GetAnimationList().ToList();

			playerSprite = GetNode<Sprite2D>("PlayerSprite");
			#endregion

			InitializeDeadZones();

			GetReferencesToOutsideNodes();

			if (PlayerNumber % 2 == 0)
			{
				PlayAppropriateAnimation(CardinalDirection.East, AnimationType.Idle);
			}
			else
			{
				PlayAppropriateAnimation(CardinalDirection.West, AnimationType.Idle);
			}
		}

		public override void _Process(double delta)
		{
			if (inputTimer < inputTimerMax)
			{
				inputTimer++;
			}
			else
			{
				GetPauseInput();

				GetAttackInput();

				GetMovementInput();

				SetAnimationToBePlayed();
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

		private void GetReferencesToOutsideNodes()
		{
			GetPauseScreen();

			GetLevelCamera();
		}

		private void GetPauseScreen()
		{
			var pauseScreens = GetTree().GetNodesInGroup("PauseScreen");

			if (pauseScreens != null)
			{
				pauseScreen = pauseScreens.First() as PauseScreenManager;
			}
		}

		private void GetLevelCamera()
		{
			var levelCameras = GetTree().GetNodesInGroup("LevelCamera");

			if (levelCameras != null)
			{
				levelCamera = levelCameras.First() as LevelCamera;
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
				pauseScreen.Show();
				GetTree().Paused = true;
				IsPaused = true;

				pauseScreen.GetNode<Button>("ResumeGameButton").GrabFocus();
				//GetNode<Button>("ResumeGameButton").GrabFocus();
			}
		}

		protected void GetAttackInput()
		{
			if (!isAttacking)
			{
				isAttacking = Input.IsActionPressed($"SouthButton_{DeviceIdentifier}");

				attackDirection.X = Input.GetActionStrength($"MoveEast_{DeviceIdentifier}") - Input.GetActionStrength($"MoveWest_{DeviceIdentifier}");

				moveDirection = Vector2.Zero;
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
				latestCardinalDirection = FindLatestCardinalDirection(latestCardinalDirection, attackDirection);

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

		#region Trigger Boxes
		private void OnMainHitBoxAreaEntered(Area2D area)
		{
			GD.Print("Player Hit Entered");
		}


		private void OnMainHitBoxAreaExited(Area2D area)
		{
			GD.Print("Player Hit Exited");
		}


		private void OnMainHurtBoxAreaEntered(Area2D area)
		{
			GD.Print("Player Hurt Entered");
		}


		private void OnMainHurtBoxAreaExited(Area2D area)
		{
			GD.Print("Player Hurt Exited");
		}
		#endregion

		#endregion
	}
}



