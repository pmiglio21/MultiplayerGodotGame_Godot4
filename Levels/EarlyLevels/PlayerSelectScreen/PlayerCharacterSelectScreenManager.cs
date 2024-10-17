using Godot;
using System;

public partial class PlayerCharacterSelectScreenManager : Control
{
	#region Signals

	[Signal]
    public delegate void GoToGameRulesScreenEventHandler();

	#endregion

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	private void OnPlayerCharacterPicker_TellPlayerCharacterSelectScreenToGoToGameRulesScreen()
	{
		EmitSignal(SignalName.GoToGameRulesScreen);
	}
}
