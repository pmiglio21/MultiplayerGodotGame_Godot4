using Enums;
using Globals;
using Godot;
using MobileEntities.PlayerCharacters.Scripts;
using System;
using System.Collections.Generic;

namespace Levels.OverworldLevels.KeyLevelObjects
{
	public partial class Portal : Node2D
	{
		public bool IsPortalActivated = false;
		private bool _isAreaEntered = false;
		private List<string> _playersInArea = new List<string>();

		private AnimationPlayer _animationPlayer;

		public override void _Ready()
		{
			_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		}

		public override void _Process(double delta)
		{
			//CheckForPortalActivation();
		}

		public void PlayPortalActivation()
		{
            _animationPlayer.Play("Idle");
        }

		private void CheckForPortalActivation()
		{
			if (_isAreaEntered && UniversalInputHelper.IsActionJustPressed(InputType.SouthButton))
			{
				var playersWhoPressedButtonThisFrame = UniversalInputHelper.GetPlayersWhoJustPressedButton(InputType.SouthButton);

				bool didOneOfThePlayersInAreaPressTheButton = false;

				foreach (string playerWhoPressedButton in playersWhoPressedButtonThisFrame)
				{
					foreach (string playerInArea in _playersInArea)
					{
						if (playerWhoPressedButton == playerInArea)
						{
							didOneOfThePlayersInAreaPressTheButton = true;
							break;
						}
					}
				}

				if (didOneOfThePlayersInAreaPressTheButton)
				{
					IsPortalActivated = !IsPortalActivated;

					GD.Print("Yuh");
				}
			}
		}

		private void OnCollisionAreaEntered(Area2D area)
		{
			if (area.IsInGroup("PlayerHurtBox"))
			{
				CollisionShape2D collisionShape = area.GetNode<CollisionShape2D>("CollisionShape");

				var character = area.GetParent() as BaseCharacter;

				if (!collisionShape.Disabled)
				{
					_isAreaEntered = true;

					_playersInArea.Add(character.DeviceIdentifier);
				}
			}
		}

		private void OnCollisionAreaExited(Area2D area)
		{
			if (area.IsInGroup("PlayerHurtBox"))
			{
				CollisionShape2D collisionShape = area.GetNode<CollisionShape2D>("CollisionShape");

				var character = area.GetParent() as BaseCharacter;

				if (!collisionShape.Disabled)
				{
					_isAreaEntered = false;

					_playersInArea.Remove(character.DeviceIdentifier);
				}
			}
		}
	}
}

