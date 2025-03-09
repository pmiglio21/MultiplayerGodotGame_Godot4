using Godot;
using Levels.UtilityLevels.UserInterfaceComponents;
using System;

public partial class PortalTimer : Node
{
	private Timer _timer;
	private RichTextLabel _timeTextBox;

	public override void _Ready()
	{
        _timer = GetNode<Timer>("Timer");
        _timeTextBox = GetNode<RichTextLabel>("TimeTextBox");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        _timeTextBox.Text = Mathf.Floor(_timer.TimeLeft).ToString();
    }
}
