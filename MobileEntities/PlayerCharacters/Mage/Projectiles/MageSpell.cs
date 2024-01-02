using Godot;
using System;

public partial class MageSpell : CharacterBody2D
{
	public Vector2 MoveDirection = Vector2.Zero;

	private float _speed = 3;

	public override void _Ready()
	{
	}

	public override void _PhysicsProcess(double delta)
	{
		Velocity = MoveDirection * _speed;

		MoveAndSlide();
	}
}
