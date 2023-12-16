using Godot;
using System;

namespace MobileEntities.Enemies.Scripts
{
	public partial class BaseEnemy : Node2D
	{
		public override void _Ready()
		{

		}

		public override void _Process(double delta)
		{

		}

		#region	Signal Receptions

		#region Trigger Boxes

		private void OnMainHitBoxAreaEntered(Area2D area)
		{
			GD.Print("Enemy Hit Entered");
		}


		private void OnMainHitBoxAreaExited(Area2D area)
		{
			GD.Print("Enemy Hit Exited");
		}


		private void OnMainHurtBoxAreaEntered(Area2D area)
		{
			GD.Print("Enemy Hurt Entered");
		}


		private void OnMainHurtBoxAreaExited(Area2D area)
		{
			GD.Print("Enemy Hurt Exited");
		}

		#endregion

		#endregion
	}


}


