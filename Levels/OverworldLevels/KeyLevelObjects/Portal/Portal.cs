using Enums;
using Globals;
using Godot;
using MobileEntities.PlayerCharacters;
using System;
using System.Collections.Generic;

namespace Levels.OverworldLevels.KeyLevelObjects
{
	public partial class Portal : Node2D
	{
		public Sprite2D Sprite;
		public bool IsPortalActivated = false;
		private List<string> _playersInArea = new List<string>();

		private AnimationPlayer _animationPlayer;

		public override void _Ready()
		{
            Sprite = this.GetNode<Sprite2D>("Sprite2D");
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		}

		public override void _Process(double delta)
		{

		}

		public void PlayPortalActivation()
		{
            _animationPlayer.Play("Idle");
        }

		private void OnCollisionAreaEntered(Area2D area)
		{
			if (area.IsInGroup("PlayerHurtBox"))
			{
				CollisionShape2D collisionShape = area.GetNode<CollisionShape2D>("CollisionShape");

				var character = area.GetParent() as BaseCharacter;

				if (!collisionShape.Disabled)
				{
					_playersInArea.Add(character.DeviceIdentifier);

                    ShaderMaterial shaderMaterial = GD.Load<ShaderMaterial>(ShaderMaterialPaths.OutlineShaderMaterialPath);
                    Sprite.Material = shaderMaterial;

                    if (!_playersInArea.Contains(character.DeviceIdentifier))
                    {
                        _playersInArea.Add(character.DeviceIdentifier);
                    }
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
                    if (_playersInArea.Contains(character.DeviceIdentifier))
                    {
                        _playersInArea.Remove(character.DeviceIdentifier);
                    }

                    if (_playersInArea.Count == 0)
                    {
                        Sprite.Material = new ShaderMaterial();
                    }
                }
			}
		}
	}
}

