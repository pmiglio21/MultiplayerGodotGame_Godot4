using Godot;
using System.Collections.Generic;
using System.Linq;

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

        _rulesetNameTextEdit.TextChanged += FilterUnallowedCharactersFromText;

        _rulesetNameTextEdit.GetHScrollBar().Scale = new Vector2(0, 0);
        _rulesetNameTextEdit.GetVScrollBar().Scale = new Vector2(0, 0);
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

    private void FilterUnallowedCharactersFromText()
    {
        List<string> unallowedCharacters = new List<string>()
        {
             "\n",
             "\t"
        };

        foreach(string character in unallowedCharacters)
        {
            if (_rulesetNameTextEdit.Text.EndsWith(character))
            {
                _rulesetNameTextEdit.Text = _rulesetNameTextEdit.Text.Replace(character, string.Empty);

                break;
            }
        }
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
