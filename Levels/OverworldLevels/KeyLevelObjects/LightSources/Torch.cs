using Globals;
using Godot;
using MobileEntities.PlayerCharacters;
using System;
using System.Collections.Generic;

public partial class Torch : Node2D
{
	public Sprite2D Sprite;
    private PointLight2D _upperLight;
    private PointLight2D _lowerLight;


    private List<int> _playersInArea = new List<int>();

    private bool _isInsideWall = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        Sprite = GetNode<Sprite2D>("Sprite2D");
        _upperLight = GetNode<PointLight2D>("UpperLight");
        _lowerLight = GetNode<PointLight2D>("LowerLight");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
        if (!_isInsideWall && _lowerLight.Energy == 0)
        {
            _lowerLight.Energy = 2.0f;
        }
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

    private void OnInteriorWallBlockCollisionAreaOnBodyShapeEntered(Node2D node2D)
    {
        if (node2D.IsInGroup("InteriorWallBlock"))
        {
            _isInsideWall = true;

            _upperLight.Scale = new Vector2(1.0f, 1.0f);

            _lowerLight.Hide();
        }
    }
}
