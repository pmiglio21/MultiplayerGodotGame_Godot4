using Enums;
using Globals;
using Godot;
using Root;
using System;
using System.Linq;

namespace Levels.UtilityLevels
{
    public partial class GameOverScreenManager : Control
    {
        private RootSceneSwapper _rootSceneSwapper;

        #region Signals

        [Signal]
        public delegate void GoToTitleScreenEventHandler();

        #endregion

        public bool IsSettingsScreenBeingShown = false;

        private int _inputTimer = 0;
        private const int _inputTimerMax = 15;
        private bool _inputChangedRecently = false;

        private Button _toTitleScreenButton;

        public override void _Ready()
        {
            _rootSceneSwapper = GetTree().Root.GetNode<RootSceneSwapper>("RootSceneSwapper");

            _toTitleScreenButton = FindChild("ToTitleScreenButton") as Button;

            _toTitleScreenButton.GrabFocus();
        }

        public override void _Process(double delta)
        {
            GetButtonInput();

            GetNavigationInput();
        }

        private void GetButtonInput()
        {
            //Pause button hit by player
            if (!_inputChangedRecently && _inputTimer < _inputTimerMax)
            {
                _inputChangedRecently = true;
            }

            //Let timer go
            if (_inputChangedRecently && _inputTimer < _inputTimerMax)
            {
                _inputTimer++;
            }
            else
            {
                _inputChangedRecently = false;
            }

            if (UniversalInputHelper.IsActionJustPressed(InputType.StartButton) || UniversalInputHelper.IsActionJustPressed(InputType.SouthButton))
            {
                if (_toTitleScreenButton.HasFocus())
                {
                    if (!_inputChangedRecently)
                    {
                        _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

                        EmitSignal(SignalName.GoToTitleScreen);

                        _inputTimer = 0;
                    }
                }
            }
        }

        private void GetNavigationInput()
        {
           
        }

        public void GrabFocusOfTopButton()
        {
            _toTitleScreenButton.GrabFocus();
        }
    }
}

