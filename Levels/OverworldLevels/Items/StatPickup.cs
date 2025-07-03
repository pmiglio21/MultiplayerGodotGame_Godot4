using Enums;
using Godot;

using System;

public partial class StatPickup : Node2D
{
	private AnimationPlayer _animationPlayer { get; set; }


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
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}
}
