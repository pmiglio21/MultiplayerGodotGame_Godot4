using Godot;
using System;

namespace Levels.UtilityLevels.UserInterfaceComponents
{
    public partial class SliderButton : Control
    {
        [Export]
        private int MinimumNumber;
        [Export]
        private int MaximumNumber;

        private Button _focusHolder;
        private HSlider _hSlider;
        private TextureRect _textureRect;
        private RichTextLabel _minimumNumber;
        private RichTextLabel _maximumNumber;

        public override void _Ready()
        {
            _focusHolder = FindChild("FocusHolder") as Button;
            _hSlider = FindChild("HSlider") as HSlider;
            _textureRect = FindChild("TextureRect") as TextureRect;

            _minimumNumber = FindChild("MinimumNumber") as RichTextLabel;
            _minimumNumber.Text = MinimumNumber.ToString();

            _maximumNumber = FindChild("MaximumNumber") as RichTextLabel;
            _maximumNumber.Text = MaximumNumber.ToString();

            _focusHolder.FocusEntered += PlayOnFocusAnimation;
            _focusHolder.FocusExited += PlayLoseFocusAnimation;

            //_hSlider.FocusEntered += PlayOnFocusAnimation;
            //_hSlider.FocusExited += PlayLoseFocusAnimation;

            _focusHolder.GrabFocus();
        }

        public override void _Process(double delta)
        {
        }

        private void PlayOnFocusAnimation()
        {
            _textureRect.Texture = ResourceLoader.Load("res://Levels/EarlyLevels/GuiArt/SliderButton/SliderButton1.png") as Texture2D;
        }

        private void PlayLoseFocusAnimation()
        {
            _textureRect.Texture = ResourceLoader.Load("res://Levels/EarlyLevels/GuiArt/SliderButton/SliderButton0.png") as Texture2D;
        }

        public HSlider GetHSlider()
        {
            return _hSlider;
        }

        public Button GetFocusHolder()
        {
            return _focusHolder;
        }
    }
}
