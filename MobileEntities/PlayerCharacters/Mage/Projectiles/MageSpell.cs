using Godot;
using System;

public partial class MageSpell : CharacterBody2D
{
	public Vector2 MoveDirection = Vector2.Zero;

	private float _speed = 200;

	private Vector2 _originPoint = Vector2.Zero;

	public override void _Ready()
	{
		_originPoint = Position;
	}

	public override void _Process(double delta)
	{
		if (Position.DistanceTo(_originPoint) > 200)
		{
			QueueFree();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Velocity = MoveDirection * _speed;

		MoveAndSlide();
	}
}
