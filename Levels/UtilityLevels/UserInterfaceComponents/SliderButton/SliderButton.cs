using Godot;
using System;

namespace Levels.UtilityLevels.UserInterfaceComponents
{
    public partial class SliderButton : Control
    {
        private HSlider _hSlider;

        public override void _Ready()
        {
            _hSlider = FindChild("HSlider") as HSlider;
        }

        public override void _Process(double delta)
        {
        }

        public HSlider GetHSlider()
        {
            return _hSlider;
        }
    }
}
