using Godot;
using System;

namespace Levels.UtilityLevels.UserInterfaceComponents
{
    public partial class SliderButton : Control
    {
        [Export]
        private bool _showSliderNumbers = false;
        [Export]
        private int _minimumNumber;
        [Export]
        private int _maximumNumber;
        [Export]
        private double SliderStep;

        private Button _focusHolder;
        private HSlider _hSlider;
        private TextureRect _textureRect;
        private RichTextLabel _minimumNumberText;
        private RichTextLabel _currentValueText;
        private RichTextLabel _maximumNumberText;

        public override void _Ready()
        {
            _focusHolder = FindChild("FocusHolder") as Button;
            _hSlider = FindChild("HSlider") as HSlider;
            _textureRect = FindChild("TextureRect") as TextureRect;

            _minimumNumberText = FindChild("MinimumNumber") as RichTextLabel;
            _minimumNumberText.Text = _minimumNumber.ToString();
            _hSlider.MaxValue = _minimumNumber;

            _maximumNumberText = FindChild("MaximumNumber") as RichTextLabel;
            _maximumNumberText.Text = _maximumNumber.ToString();
            _hSlider.MaxValue = _maximumNumber;

            _currentValueText = FindChild("CurrentValue") as RichTextLabel;

            _hSlider.Step = SliderStep;

            if (_showSliderNumbers)
            {
                _minimumNumberText.Show();
                _maximumNumberText.Show();
                _currentValueText.Show();
            }
            else
            {
                _minimumNumberText.Hide();
                _maximumNumberText.Hide();
                _currentValueText.Hide();
            }

            _focusHolder.FocusEntered += PlayOnFocusAnimation;
            _focusHolder.FocusExited += PlayLoseFocusAnimation;

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

        public RichTextLabel GetCurrentValue()
        {
            return _currentValueText;
        }

        public Button GetFocusHolder()
        {
            return _focusHolder;
        }
    }
}
