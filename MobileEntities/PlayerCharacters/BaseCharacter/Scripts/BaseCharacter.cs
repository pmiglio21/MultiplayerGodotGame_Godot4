using Enums;
using Godot;
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

		private List<string> animationList; 

		protected Sprite2D playerSprite;

		#endregion

		#region Input Properties

		Vector2 moveInput = Vector2.Zero;
		float moveInputDeadzone = 0.1f;

		float moveDeadzone = 0.32f;
		Vector2 moveDirection = Vector2.Zero;

		float speed = 140.0f;

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

		#endregion

		#region Player Identification Properties

		public int PlayerNumber = -1;
		public string DeviceIdentifier = "-1";

		#endregion

		#region Player State Properties
		protected bool isControllable = true;

		protected bool isHurt = false;
		#endregion

		public override void _Ready()
		{
			animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
			animationList = animationPlayer.GetAnimationList().ToList();
			playerSprite = GetNode<Sprite2D>("Sprite");

			InitializeDeadZones();

			PlayAppropriateAnimation(CardinalDirection.South, AnimationType.Idle);
		}

		public override void _Process(double delta)
		{
			GetMovementInput();

			if (moveDirection != Vector2.Zero)
			{
				latestCardinalDirection = FindLatestCardinalDirection(moveDirection);

				PlayAppropriateAnimation(latestCardinalDirection, AnimationType.Move);
			}
			else
			{
				PlayAppropriateAnimation(latestCardinalDirection, AnimationType.Idle);
			}
		}

		public override void _PhysicsProcess(double delta)
		{
			if (isControllable && !isHurt)
			{
				MovePlayer();
			}
		}

		protected void InitializeDeadZones()
		{
			InputMap.ActionSetDeadzone($"MoveRight_{DeviceIdentifier}", moveInputDeadzone);
			InputMap.ActionSetDeadzone($"MoveLeft_{DeviceIdentifier}", moveInputDeadzone);
			InputMap.ActionSetDeadzone($"MoveDown_{DeviceIdentifier}", moveInputDeadzone);
			InputMap.ActionSetDeadzone($"MoveUp_{DeviceIdentifier}", moveInputDeadzone);
		}

		protected void GetMovementInput()
		{
			moveInput.X = Input.GetActionStrength($"MoveRight_{DeviceIdentifier}") - Input.GetActionStrength($"MoveLeft_{DeviceIdentifier}");
			moveInput.Y = Input.GetActionStrength($"MoveDown_{DeviceIdentifier}") - Input.GetActionStrength($"MoveUp_{DeviceIdentifier}");

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

		private void MovePlayer()
		{
			Velocity = moveDirection * speed;

			MoveAndSlide();
		}

		private void PlayAppropriateAnimation(CardinalDirection cardinalDirection, AnimationType animationType)
		{
			if (cardinalDirection != CardinalDirection.Center)
			{
				if (animationList.Contains($"{characterClass}_{animationType}_{cardinalDirection}"))
				{
					animationPlayer.Play($"{characterClass}_{animationType}_{cardinalDirection}");
				}
			}
		}
	}
}
