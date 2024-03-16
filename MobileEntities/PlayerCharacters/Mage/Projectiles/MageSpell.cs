using Godot;
using System;

public partial class MageSpell : CharacterBody2D
{
	public Vector2 MoveDirection = Vector2.Zero;

	private float _speed = 200;

	private Vector2 _originPoint = Vector2.Zero;

	public override void _Ready()
	{
		_originPoint = GlobalPosition;
	}

	public override void _Process(double delta)
	{
		if (GlobalPosition.DistanceTo(_originPoint) > 200)
		{
			QueueFree();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Velocity = MoveDirection * _speed;

		MoveAndSlide();
	}

	private void OnAreaEntered(Area2D area)
	{
		if (area.IsInGroup("EnemyHurtBox"))
		{
			CollisionShape2D collisionShape = area.GetNode<CollisionShape2D>("CollisionShape");

			if (!collisionShape.Disabled)
			{
				QueueFree();
			}
		}
	}
}

