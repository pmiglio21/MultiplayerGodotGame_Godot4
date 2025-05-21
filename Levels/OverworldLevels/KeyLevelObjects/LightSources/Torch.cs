using Godot;
using System;

public partial class Torch : Node2D
{
	public ColorRect ColorRect;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        ColorRect = this.GetNode<ColorRect>("ColorRect");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
