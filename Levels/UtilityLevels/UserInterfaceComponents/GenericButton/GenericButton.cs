using Godot;

public partial class GenericButton : Button
{
    [Export]
    public TextureRect GenericButtonTexture;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        FocusEntered += PlayOnFocusAnimation;
        FocusExited += PlayLoseFocusAnimation;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void PlayOnFocusAnimation()
    {
        GenericButtonTexture.Texture = ResourceLoader.Load("res://Levels/EarlyLevels/GuiArt/GuiButton/GuiButton14.png") as Texture2D;
    }

    public void PlayLoseFocusAnimation()
    {
        GenericButtonTexture.Texture = ResourceLoader.Load("res://Levels/EarlyLevels/GuiArt/GuiButton/GuiButton13.png") as Texture2D;
    }
}
