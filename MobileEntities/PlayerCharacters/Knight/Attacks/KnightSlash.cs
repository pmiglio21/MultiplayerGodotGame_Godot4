using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileEntities.PlayerCharacters.Knight
{
	public partial class KnightSlash : Node2D
	{
		private AnimationPlayer _animationPlayer;

		public override void _Ready()
		{
			_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
			_animationPlayer.Play("Active");
		}

		private void OnAnimationPlayerAnimationFinished(StringName anim_name)
		{
			QueueFree();
		}
	}
}
