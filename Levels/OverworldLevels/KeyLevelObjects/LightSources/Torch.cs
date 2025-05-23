using Globals;
using Godot;
using MobileEntities.PlayerCharacters.Scripts;
using System;
using System.Collections.Generic;

public partial class Torch : Node2D
{
	public ColorRect ColorRect;
	public Sprite2D Sprite;

    private List<int> _playersInArea = new List<int>();

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
            var character = area.GetParent() as BaseCharacter;

            ShaderMaterial shaderMaterial = GD.Load<ShaderMaterial>(ShaderMaterialPaths.OutlineShaderMaterialPath);
            Sprite.Material = shaderMaterial;

            if (!_playersInArea.Contains(character.PlayerNumber))
            {
                _playersInArea.Add(character.PlayerNumber);
            }
        }
	}

    private void OnInteractionAreaExited(Area2D area)
    {
        if (area.IsInGroup("PlayerHurtBox"))
        {
            var character = area.GetParent() as BaseCharacter;

            if (_playersInArea.Contains(character.PlayerNumber))
            {
                _playersInArea.Remove(character.PlayerNumber);
            }

            if (_playersInArea.Count == 0)
            {
                Sprite.Material = new ShaderMaterial();
            }
        }
    }
}
