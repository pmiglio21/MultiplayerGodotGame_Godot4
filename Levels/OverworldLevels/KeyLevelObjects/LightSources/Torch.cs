using Globals;
using Godot;
using System;

public partial class Torch : Node2D
{
	public ColorRect ColorRect;
	public Sprite2D Sprite;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        ColorRect = this.GetNode<ColorRect>("ColorRect");
        Sprite = this.GetNode<Sprite2D>("Sprite2D");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	private void OnInteractionAreaEntered(Area2D area)
	{
        if (area.IsInGroup("PlayerHurtBox"))
        {
            ShaderMaterial shaderMaterial = GD.Load<ShaderMaterial>(ShaderMaterialPaths.OutlineShaderMaterialPath);
            Sprite.Material = shaderMaterial;
        }
	}

    private void OnInteractionAreaExited(Area2D area)
    {
        if (area.IsInGroup("PlayerHurtBox"))
        {
            Sprite.Material = new ShaderMaterial();
        }
    }
}
