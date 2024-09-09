using Enums;
using Enums.GameRules;
using Globals;
using Globals.PlayerManagement;
using Godot;
using Levels.OverworldLevels;
using Levels.UtilityLevels.UserInterfaceComponents;
using Scenes.UI.PlayerSelectScene;
using System;
using System.ComponentModel;
using System.Linq;

namespace Levels.UtilityLevels
{
	public partial class GameRulesScreenManager : Control
	{
		protected PauseScreenManager pauseScreen;

        private Timer _inputTimer;
        private OptionSelector _splitScreenOptionSelector;
		private OptionSelector _relativePlayerSpawnDistanceOptionSelector;
		private NumberSpinner _numberSpinner;
		private OptionSelector _levelSizeSelector;
		private Button _returnButton;
		private Button _continueButton;

		public override void _Ready()
		{
            _inputTimer = FindChild("InputTimer") as Timer;
            _splitScreenOptionSelector = GetNode<OptionSelector>("SplitScreenOptionSelector");
			_relativePlayerSpawnDistanceOptionSelector = GetNode<OptionSelector>("RelativePlayerSpawnDistanceOptionSelector");
			_numberSpinner = GetNode<NumberSpinner>("NumberSpinner");
			_levelSizeSelector = GetNode<OptionSelector>("LevelSizeOptionSelector");
			_returnButton = GetNode<Button>("ReturnButton");
			_continueButton = GetNode<Button>("ContinueButton");

			if (GlobalGameComponents.PriorSceneName == LevelScenePaths.PlayModeScreenPath)
			{
				_continueButton.Show();
			}
			else
			{
				_continueButton.Hide();

			}

			_splitScreenOptionSelector.GetOptionButton().GrabFocus();
		}

		public override void _Process(double delta)
		{
			GetButtonPressInput();

			GetNavigationInput();
		}

		private void GetButtonPressInput()
		{
			if (_inputTimer.IsStopped())
			{
                if (UniversalInputHelper.IsActionJustPressed(InputType.StartButton) || UniversalInputHelper.IsActionJustPressed(InputType.SouthButton))
                {
                    if (_returnButton.HasFocus())
                    {
                        ReturnToPriorScene();
                    }
                    else if (_continueButton.HasFocus())
                    {
                        SaveOutGameRules();

                        //Maybe change this
                        GetTree().ChangeSceneToFile(LevelScenePaths.PlayerSelectScreenPath);
                    }
                    else
                    {
                        if (_continueButton.Visible)
                        {
                            _continueButton.GrabFocus();
                        }
                        else if (_returnButton.Visible)
                        {
                            _returnButton.GrabFocus();
                        }
                    }

                    _inputTimer.Start();
                }

                if (UniversalInputHelper.IsActionJustPressed(InputType.EastButton))
                {
                    ReturnToPriorScene();

                    _inputTimer.Start();
                }
            }
		}

		private void GetNavigationInput()
		{
			if (_inputTimer.IsStopped())
			{
                if (UniversalInputHelper.IsActionPressed(InputType.MoveSouth))
                {
                    if (_splitScreenOptionSelector.GetOptionButton().HasFocus())
                    {
                        _relativePlayerSpawnDistanceOptionSelector.GetOptionButton().GrabFocus();
                    }
                    else if (_relativePlayerSpawnDistanceOptionSelector.GetOptionButton().HasFocus())
                    {
                        _levelSizeSelector.GetOptionButton().GrabFocus();
                    }
                    else if (_levelSizeSelector.GetOptionButton().HasFocus())
                    {
                        _numberSpinner.GetNumberSpinnerButton().GrabFocus();
                    }
                    else if (_numberSpinner.GetNumberSpinnerButton().HasFocus())
                    {
                        _returnButton.GrabFocus();
                    }
                    else if (_returnButton.HasFocus() && _continueButton.Visible)
                    {
                        _continueButton.GrabFocus();
                    }

                    _inputTimer.Start();
                }

                if (UniversalInputHelper.IsActionPressed(InputType.MoveNorth))
                {
                    if (_continueButton.HasFocus() && _continueButton.Visible)
                    {
                        _returnButton.GrabFocus();
                    }
                    else if (_returnButton.HasFocus())
                    {
                        _numberSpinner.GetNumberSpinnerButton().GrabFocus();
                    }
                    else if (_numberSpinner.GetNumberSpinnerButton().HasFocus())
                    {
                        _levelSizeSelector.GetOptionButton().GrabFocus();
                    }
                    else if (_levelSizeSelector.GetOptionButton().HasFocus())
                    {
                        _relativePlayerSpawnDistanceOptionSelector.GetOptionButton().GrabFocus();
                    }
                    else if (_relativePlayerSpawnDistanceOptionSelector.GetOptionButton().HasFocus())
                    {
                        _splitScreenOptionSelector.GetOptionButton().GrabFocus();
                    }

                    _inputTimer.Start();
                }
            }
		}

		public void GrabFocusOfTopButton()
		{
			_splitScreenOptionSelector.GetOptionButton().GrabFocus();
		}

		private void ReturnToPriorScene()
		{
			SaveOutGameRules();

			PlayerManager.ActivePlayers.Clear();
			PlayerCharacterPickerManager.ActivePickers.Clear();

			GetTree().ChangeSceneToFile(GlobalGameComponents.PriorSceneName);
		}

		private void SaveOutGameRules()
		{
			foreach (var enumValue in Enum.GetValues(typeof(SplitScreenMergingType)))
			{
				var enumDescription = UniversalEnumHelper.GetEnumDescription(enumValue);

				if (enumDescription != "None" && enumDescription == _splitScreenOptionSelector.GetOptionButton().Text)
				{
					CurrentSaveGameRules.CurrentSplitScreenMergingType = (SplitScreenMergingType)enumValue;
				}
			}

			foreach (var enumValue in Enum.GetValues(typeof(RelativePlayerSpawnDistanceType)))
			{
				var enumDescription = UniversalEnumHelper.GetEnumDescription(enumValue);

				if (enumDescription != "None" && enumDescription == _relativePlayerSpawnDistanceOptionSelector.GetOptionButton().Text)
				{
					CurrentSaveGameRules.CurrentRelativePlayerSpawnDistanceType = (RelativePlayerSpawnDistanceType)enumValue;
				}
			}

			CurrentSaveGameRules.NumberOfLevels = _numberSpinner.GetNumberSpinnerButton().Text;

			foreach (var enumValue in Enum.GetValues(typeof(LevelSize)))
			{
				var enumDescription = UniversalEnumHelper.GetEnumDescription(enumValue);

				if (enumDescription != "None" && enumDescription == _levelSizeSelector.GetOptionButton().Text)
				{
					CurrentSaveGameRules.CurrentLevelSize = (LevelSize)enumValue;
				}
			}
		}
	}
}
