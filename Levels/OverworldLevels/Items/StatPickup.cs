using Enums;
using Godot;
using MobileEntities.PlayerCharacters;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class StatPickup : Node2D
{
    #region Components 

    private AnimationPlayer _animationPlayer { get; set; }
    protected Area2D _attractionArea;
    protected CollisionShape2D _collisionShape;

    #endregion

    private List<BaseCharacter> _playersInArea = new List<BaseCharacter>();

    public StatType StatType 
	{ 
		get { return _statType;  }

		set
		{
			if (_statType != value)
			{
				_statType = value;
            }

            if (_animationPlayer != null)
            {
                _animationPlayer.Play($"{StatType}{StatSize}");
            }
        } 
	}
	private StatType _statType;

    public StatSize StatSize
    {
        get { return _statSize; }

        set
        {
            if (_statSize != value)
            {
                _statSize = value;
            }

            if (_animationPlayer != null)
            {
                _animationPlayer.Play($"{StatType}{StatSize}");
            }
        }
    }
    private StatSize _statSize;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	    _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _attractionArea = GetNode<Area2D>("AttractionArea");
        _collisionShape = _attractionArea.GetNode<CollisionShape2D>("CollisionShape2D");
    }

    private BaseCharacter _nearestCharacter = null;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (_playersInArea.Count > 0)
        {
            float minDistance = float.MaxValue;

            foreach (BaseCharacter character in _playersInArea)
            {
                float newDistance = character.GlobalPosition.DistanceTo(GlobalPosition);

                if (newDistance < minDistance)
                {
                    _nearestCharacter = character;

                    minDistance = newDistance;
                }
            }
        }
        else
        {
            _nearestCharacter = null;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_nearestCharacter != null)
        {
            Vector2 newDistance = -.5f * _nearestCharacter.GlobalPosition.DirectionTo(GlobalPosition);

            Translate(newDistance);
        }
    }

    private void OnAttractionAreaEntered(Area2D area)
    {
        if (area.IsInGroup("PlayerHurtBox"))
        {
            CollisionShape2D collisionShape = area.GetNode<CollisionShape2D>("CollisionShape");

            if (!collisionShape.Disabled)
            {
                BaseCharacter character = area.GetParent() as BaseCharacter;

                if (!_playersInArea.Any(x => x == character))
                {
                    _playersInArea.Add(character);

                    GD.Print($"PlayerHurtBox Entered {area.GetParent().Name}");
                }
            }
        }
    }

    private void OnAttractionAreaExited(Area2D area)
    {
        if (area.IsInGroup("PlayerHurtBox"))
        {
            CollisionShape2D collisionShape = area.GetNode<CollisionShape2D>("CollisionShape");

            if (!collisionShape.Disabled)
            {
                BaseCharacter character = area.GetParent() as BaseCharacter;

                if (_playersInArea.Any(x => x == character))
                {
                    _playersInArea.Remove(character);

                    GD.Print($"PlayerHurtBox Exited {area.GetParent().Name}");
                }
            }
        }
    }

    private void OnPickupAreaEntered(Area2D area)
    {
        if (area.IsInGroup("PlayerHurtBox"))
        {
            //CallDeferred(nameof(QueueFree));

            this.QueueFree();
        }
    }
}
