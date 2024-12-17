using System.Collections.Generic;
using Godot;

namespace Levels.UtilityLevels.UserInterfaceComponents
{
    public partial class OptionSelectorMultiSelect : Control
    {
        [Export]
        public bool IsOnlyTwoOptions = false;

        #region Components
        private Label _optionLabel;
        private Button _optionButton;

        private TextureRect _textureRect;

        private TextureRect _leftArrowTexture;
        private bool _isLeftArrowTextureEntered;
        private TextureRect _rightArrowTexture;
        private bool _isRightArrowTextureEntered;

        private AnimationPlayer _leftArrowAnimationPlayer;
        private AnimationPlayer _rightArrowAnimationPlayer;

        #endregion

        [Signal]
        public delegate void LeftArrowClickedEventHandler();

        [Signal]
        public delegate void RightArrowClickedEventHandler();

        [Signal]
        public delegate void EitherArrowClickedEventHandler();

        public override void _Ready()
        {
            _textureRect = FindChild("TextureRect") as TextureRect;
            _leftArrowTexture = FindChild("LeftArrowTexture") as TextureRect;
            _rightArrowTexture = FindChild("RightArrowTexture") as TextureRect;

            _leftArrowTexture.MouseEntered += () => _isLeftArrowTextureEntered = true;
            _rightArrowTexture.MouseEntered += () => _isRightArrowTextureEntered = true;

            _leftArrowTexture.MouseExited += () => _isLeftArrowTextureEntered = false;
            _rightArrowTexture.MouseExited += () => _isRightArrowTextureEntered = false;

            _leftArrowAnimationPlayer = FindChild("LeftArrowAnimationPlayer") as AnimationPlayer;
            _rightArrowAnimationPlayer = FindChild("RightArrowAnimationPlayer") as AnimationPlayer;
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

        public Button GetOptionButton()
        {
            return _optionButton;
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
    }
}
