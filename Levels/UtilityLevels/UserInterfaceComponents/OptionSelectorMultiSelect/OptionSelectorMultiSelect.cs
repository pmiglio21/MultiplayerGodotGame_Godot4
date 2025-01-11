using System.Collections.Generic;
using Godot;

namespace Levels.UtilityLevels.UserInterfaceComponents
{
    public partial class OptionSelectorMultiSelect : Control
    {
        [Export]
        public bool IsOnlyTwoOptions = false;

        #region Components
        private Button _focusHolder;

        private TextureRect _textureRect;

        private TextureRect _leftArrowTexture;
        private bool _isLeftArrowTextureEntered;
        private TextureRect _rightArrowTexture;
        private bool _isRightArrowTextureEntered;
        private Button _optionSelectButton;

        private AnimationPlayer _leftArrowAnimationPlayer;
        private AnimationPlayer _rightArrowAnimationPlayer;
        private AnimationPlayer _optionSelectAnimationPlayer;

        #endregion

        [Signal]
        public delegate void LeftArrowClickedEventHandler();

        [Signal]
        public delegate void RightArrowClickedEventHandler();

        [Signal]
        public delegate void EitherArrowClickedEventHandler();

        [Signal]
        public delegate void OptionButtonPressedEventHandler();

        public override void _Ready()
        {
            _focusHolder = FindChild("FocusHolder") as Button;

            _textureRect = FindChild("TextureRect") as TextureRect;
            _leftArrowTexture = FindChild("LeftArrowTexture") as TextureRect;
            _rightArrowTexture = FindChild("RightArrowTexture") as TextureRect;

            _leftArrowTexture.MouseEntered += () => _isLeftArrowTextureEntered = true;
            _rightArrowTexture.MouseEntered += () => _isRightArrowTextureEntered = true;

            _leftArrowTexture.MouseExited += () => _isLeftArrowTextureEntered = false;
            _rightArrowTexture.MouseExited += () => _isRightArrowTextureEntered = false;

            _optionSelectButton = FindChild("OptionSelectButton") as Button;
            _optionSelectButton.Pressed += ToggleOptionButton;

            _leftArrowAnimationPlayer = FindChild("LeftArrowAnimationPlayer") as AnimationPlayer;
            _rightArrowAnimationPlayer = FindChild("RightArrowAnimationPlayer") as AnimationPlayer;
            _optionSelectAnimationPlayer = FindChild("OptionSelectAnimationPlayer") as AnimationPlayer;
        }

        public override void _Process(double delta)
        {
            if (_isLeftArrowTextureEntered && Input.IsActionJustPressed("LeftMouseClick"))
            {
                if (IsOnlyTwoOptions)
                {
                    EmitSignal(SignalName.EitherArrowClicked);

                    PlayClickedOnLeftArrow();
                    PlayClickedOnRightArrow();
                }
                else
                {
                    EmitSignal(SignalName.LeftArrowClicked);

                    PlayClickedOnLeftArrow();
                }
            }
            else if (_isRightArrowTextureEntered && Input.IsActionJustPressed("LeftMouseClick"))
            {
                if (IsOnlyTwoOptions)
                {
                    //Send signal back to SettingsScreenManager to go right and play sound
                    EmitSignal(SignalName.EitherArrowClicked);

                    PlayClickedOnLeftArrow();
                    PlayClickedOnRightArrow();
                }
                else
                {
                    //Send signal back to SettingsScreenManager to go right and play sound
                    EmitSignal(SignalName.RightArrowClicked);

                    PlayClickedOnRightArrow();
                }
            }
        }

        public Button GetFocusHolder()
        {
            return _focusHolder;
        }

        public Button GetOptionSelectButton()
        {
            return _optionSelectButton;
        }

        public void PlayOnFocusAnimation()
        {
            _textureRect.Texture = ResourceLoader.Load("res://Levels/EarlyLevels/GuiArt/OptionSelectorButton/OptionSelectorButton1.png") as Texture2D;
        }

        public void PlayLoseFocusAnimation()
        {
            _textureRect.Texture = ResourceLoader.Load("res://Levels/EarlyLevels/GuiArt/OptionSelectorButton/OptionSelectorButton0.png") as Texture2D;
        }

        public void PlayClickedOnLeftArrow()
        {
            _leftArrowAnimationPlayer.Play("Clicked");
        }

        public void PlayClickedOnRightArrow()
        {
            _rightArrowAnimationPlayer.Play("Clicked");
        }

        public void PlayActivatedOnOptionSelect()
        {
            _optionSelectButton.Icon = ResourceLoader.Load("res://Levels/EarlyLevels/GuiArt/OptionSelectorToggle/OptionSelectorToggle0.png") as Texture2D;
            //_optionSelectAnimationPlayer.Play("Activated");
        }

        public void PlayDeactivatedOnOptionSelect()
        {
            _optionSelectButton.Icon = ResourceLoader.Load("res://Levels/EarlyLevels/GuiArt/OptionSelectorToggle/OptionSelectorToggle1.png") as Texture2D;
            //_optionSelectAnimationPlayer.Play("Deactivated");
        }

        private void ToggleOptionButton()
        {
            EmitSignal(SignalName.OptionButtonPressed);
        }
    }
}
