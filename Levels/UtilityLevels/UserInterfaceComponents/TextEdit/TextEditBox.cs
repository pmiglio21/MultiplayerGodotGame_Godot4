using Godot;

public partial class TextEditBox : Control
{
    private TextureRect _textureRect;
    private Button _focusHolder;
    private TextEdit _rulesetNameTextEdit;

    public override void _Ready()
	{
        _textureRect = FindChild("TextureRect") as TextureRect;
        _focusHolder = GetNode<Button>("FocusHolder");
        _rulesetNameTextEdit = GetNode<TextEdit>("TextEdit");
    }

	public override void _Process(double delta)
	{
	}

    public Button GetFocusHolder()
    {
        return _focusHolder;
    }

    public TextEdit GetTextEditBox()
	{
		return _rulesetNameTextEdit;
	}

    public void PlayOnFocusAnimation()
    {
        _textureRect.Texture = ResourceLoader.Load("res://Levels/EarlyLevels/GuiArt/GuiButton/GuiButton12.png") as Texture2D;
    }

    public void PlayLoseFocusAnimation()
    {
        _textureRect.Texture = ResourceLoader.Load("res://Levels/EarlyLevels/GuiArt/GuiButton/GuiButton11.png") as Texture2D;
    }
}
