using Godot;

public partial class TextEditBox : Control
{
    private Button _focusHolder;
    private TextEdit _rulesetNameTextEdit;

    public override void _Ready()
	{
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
}
