using Godot;

public partial class InteriorWallBlock : Node2D
{
	public Sprite2D Sprite; 

	public override void _Ready()
	{
		Sprite = FindChild("Sprite2D") as Sprite2D;
	}

	public override void _Process(double delta)
	{
        ZIndex = (int)this.GlobalPosition.Y;
    }
}
