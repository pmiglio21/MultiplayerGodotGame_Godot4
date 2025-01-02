using Enums;
using Enums.GameRules;
using Globals;
using Godot;
using Levels.OverworldLevels;
using Levels.UtilityLevels.UserInterfaceComponents;
using Models;
using Root;
using Scenes.UI.PlayerSelectScene;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Levels.UtilityLevels
{
	public partial class GameRulesScreenManager : Control
	{
		private RootSceneSwapper _rootSceneSwapper;

		public GameRules CurrentGameRules = new GameRules();

		#region Options

		private List<string> _levelSizeOptions = new List<string>()
        {
           "Small",
           "Medium",
           "Large"
        };

        private List<string> _biomeOptions = new List<string>()
        {
           "Castle",
		   "Cave",
		   "Swamp",
		   "Frost"
        };

        #endregion

        #region Components

        private Timer _inputTimer;

		private TextEditBox _rulesetNameEdit;
        private Button _addButton;
        private Button _loadButton;
        private Button _saveButton;
        private Button _deleteButton;

        private OptionSelectorMultiSelect _levelSizeMultiSelector;
        private Button _levelSizeButton;

        private OptionSelector _numberOfLevelsOptionSelector;
        private Button _numberOfLevelsButton;

        private OptionSelectorMultiSelect _biomeTypeMultiSelector;
		private OptionSelectorMultiSelect _spawnProximityMultiSelector;
        private OptionSelectorMultiSelect _switchProximityMultiSelector;
		
		private Button _returnButton;

		#endregion

		#region Signals

		[Signal]
		public delegate void GoToTitleScreenEventHandler();

		[Signal]
		public delegate void GoToPlayModeScreenEventHandler();

		[Signal]
		public delegate void GoToPlayerCharacterSelectScreenEventHandler();

		#endregion

		public override void _Ready()
		{
			_rootSceneSwapper = GetTree().Root.GetNode<RootSceneSwapper>("RootSceneSwapper");

            _inputTimer = FindChild("InputTimer") as Timer;

			_rulesetNameEdit = GetNode<TextEditBox>("RulesetNameEditBox");
            _addButton = GetNode<Button>("AddRulesetButton");
            _loadButton = GetNode<Button>("LoadRulesetButton");
            _saveButton = GetNode<Button>("SaveRulesetButton");
            _deleteButton = GetNode<Button>("DeleteRulesetButton");


            _levelSizeMultiSelector = GetNode<OptionSelectorMultiSelect>("LevelSizeOptionSelectorMultiSelect");
            _levelSizeButton = GetNode<Button>("LevelSizeButton");

            _numberOfLevelsOptionSelector = GetNode<OptionSelector>("NumberOfLevelsOptionSelector");
            _numberOfLevelsButton = GetNode<Button>("NumberOfLevelsButton");

            _biomeTypeMultiSelector = GetNode<OptionSelectorMultiSelect>("BiomeTypeOptionSelectorMultiSelect");
            _switchProximityMultiSelector = GetNode<OptionSelectorMultiSelect>("SpawnProximityOptionSelectorMultiSelect");
            
			_returnButton = GetNode<Button>("ReturnButton");

            GrabFocusOfTopButton();
        }

		public override void _Process(double delta)
		{
			GetButtonPressInput();

			GetNavigationInput();
		}

		private void GetButtonPressInput()
		{
			if (UniversalInputHelper.IsActionJustPressed(InputType.StartButton) || UniversalInputHelper.IsActionJustPressed(InputType.SouthButton))
			{
				if (_returnButton.HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

					ReturnToPriorScene();
				}
			}

			if (UniversalInputHelper.IsActionJustPressed(InputType.EastButton))
			{
				if (!_rulesetNameEdit.HasFocus())
				{
                    ReturnToPriorScene();
                }
			}
		}

		private void GetNavigationInput()
		{
			if (_inputTimer.IsStopped() && Input.IsKeyPressed(Key.Tab))
			{
                //Row 1 - Ruleset Edit Row
                if (_rulesetNameEdit.HasFocus())
                {
                    _addButton.GrabFocus();
                }
                else if (_addButton.HasFocus())
                {
                    _loadButton.GrabFocus();
                }
                else if (_loadButton.HasFocus())
                {
                    _saveButton.GrabFocus();
                }
                else if (_saveButton.HasFocus())
                {
                    _deleteButton.GrabFocus();
                }

                _inputTimer.Start();
            }
			else if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveSouth) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadSouth)))
			{
				//Row 1 - Ruleset Edit Row
				if (_rulesetNameEdit.GetFocusHolder().HasFocus())
				{
					_levelSizeMultiSelector.GetFocusHolder().GrabFocus();
                }

                //Row 2

                else if (_levelSizeMultiSelector.HasFocus())
                {
                    _numberOfLevelsOptionSelector.GrabFocus();
                }

                //Row 3

                else if (_numberOfLevelsOptionSelector.HasFocus())
                {
                    _biomeTypeMultiSelector.GrabFocus();
                }

                //Row 4


                //Row 5


                //Row 6



                _inputTimer.Start();
			}
            else if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveEast) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadEast)))
            {
				//Row 1

				if (_rulesetNameEdit.GetFocusHolder().HasFocus())
				{

				}
				else if (_addButton.HasFocus())
				{
					_loadButton.GrabFocus();
				}
                else if (_loadButton.HasFocus())
                {
                    _saveButton.GrabFocus();
                }
                else if (_saveButton.HasFocus())
                {
                    _deleteButton.GrabFocus();
                }

                _inputTimer.Start();
            }
            else if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveNorth) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadNorth)))
			{


				_inputTimer.Start();
			}
            else if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveWest) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadWest)))
            {
           

                _inputTimer.Start();
            }
        }

		public void GrabFocusOfTopButton()
		{
            _addButton.GrabFocus();
            //_rulesetNameEdit.GetFocusHolder().GrabFocus();
        }

		private void ReturnToPriorScene()
		{
			//SaveOutGameRules();

			if (_rootSceneSwapper.PriorSceneName == ScreenNames.Title)
			{
				EmitSignal(SignalName.GoToTitleScreen);
			}
			else if (_rootSceneSwapper.PriorSceneName == ScreenNames.PlayMode)
			{
				EmitSignal(SignalName.GoToPlayModeScreen);
			}
            else if (_rootSceneSwapper.PriorSceneName == ScreenNames.PlayerCharacterSelect)
            {
                EmitSignal(SignalName.GoToPlayerCharacterSelectScreen);
            }
        }

		private void SaveOutGameRules()
		{
			//foreach (var enumValue in Enum.GetValues(typeof(BiomeType)))
			//{
			//	var enumDescription = UniversalEnumHelper.GetEnumDescription(enumValue);

			//	if (enumDescription != "None" && enumDescription == _biomeTypeSelector.GetOptionButton().Text)
			//	{
			//		CurrentGameRules.BiomeType = (BiomeType)enumValue;
			//	}
			//}

			//foreach (var enumValue in Enum.GetValues(typeof(RelativePlayerSpawnDistanceType)))
			//{
			//	var enumDescription = UniversalEnumHelper.GetEnumDescription(enumValue);

			//	if (enumDescription != "None" && enumDescription == _relativePlayerSpawnDistanceSelector.GetOptionButton().Text)
			//	{
			//		CurrentGameRules.CurrentRelativePlayerSpawnDistanceType = (RelativePlayerSpawnDistanceType)enumValue;
			//	}
			//}

			//CurrentGameRules.NumberOfLevels = _numberSpinner.GetNumberSpinnerButton().Text;

			//foreach (var enumValue in Enum.GetValues(typeof(LevelSize)))
			//{
			//	var enumDescription = UniversalEnumHelper.GetEnumDescription(enumValue);

			//	if (enumDescription != "None" && enumDescription == _levelSizeSelector.GetOptionButton().Text)
			//	{
			//		CurrentGameRules.CurrentLevelSize = (LevelSize)enumValue;
			//	}
			//}

			_rootSceneSwapper.CurrentGameRules = CurrentGameRules;
		}
	}
}
