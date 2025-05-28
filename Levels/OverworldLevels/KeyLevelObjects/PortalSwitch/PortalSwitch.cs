using Enums;
using Globals;
using Godot;
using Levels.UtilityLevels.UserInterfaceComponents;
using MobileEntities.PlayerCharacters;
using System;
using System.Collections.Generic;

namespace Levels.OverworldLevels.KeyLevelObjects
{
	public partial class PortalSwitch : Node2D
	{
        #region Signals

        [Signal]
        public delegate void SwitchActivatedEventHandler();

        #endregion

        private AnimationPlayer _animationPlayer;

		public bool IsSwitchActivated = false;
		private bool _isAreaEntered = false;
		private List<string> _playersInArea = new List<string>();

		public override void _Ready()
		{
			_animationPlayer = FindChild("AnimationPlayer") as AnimationPlayer;
        }

		public override void _Process(double delta)
		{
			CheckForSwitchActivation();
		}

		private void CheckForSwitchActivation()
		{
			if (_isAreaEntered && UniversalInputHelper.IsActionJustPressed(InputType.GameActionInteract))
			{
				var playersWhoPressedButtonThisFrame = UniversalInputHelper.GetPlayersWhoJustPressedButton(InputType.GameActionInteract);

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
					IsSwitchActivated = !IsSwitchActivated;
					_animationPlayer.Play("Activated");

					EmitSignal(SignalName.SwitchActivated);
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
