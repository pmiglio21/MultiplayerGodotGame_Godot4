using Godot;
using System;

namespace Levels.UtilityLevels.UserInterfaceComponents
{
    public partial class SliderButton : Control
    {
        private HSlider _hSlider;
        private TextureRect _textureRect;

        public override void _Ready()
        {
            _hSlider = FindChild("HSlider") as HSlider;
            _textureRect = FindChild("TextureRect") as TextureRect;

            _hSlider.FocusEntered += PlayOnFocusAnimation;
            _hSlider.FocusExited += PlayLoseFocusAnimation;
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
    }
}
