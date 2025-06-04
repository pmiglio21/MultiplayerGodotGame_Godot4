using Godot;

public partial class InteriorWallBlock : Node2D
{
	public Sprite2D Sprite;
    public LightOccluder2D LightOccluder;

	public bool IsWall = false;

    public override void _Ready()
	{
		Sprite = FindChild("Sprite2D") as Sprite2D;
        LightOccluder = FindChild("LightOccluder2D") as LightOccluder2D;
    }

	public override void _Process(double delta)
	{
	}
}
