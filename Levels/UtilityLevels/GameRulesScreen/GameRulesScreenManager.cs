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
using System.ComponentModel;
using System.Linq;

namespace Levels.UtilityLevels
{
	public partial class GameRulesScreenManager : Control
	{
		private RootSceneSwapper _rootSceneSwapper;

		public GameRules CurrentGameRules = new GameRules();

		#region Components

		private Timer _inputTimer;

		private TextEdit _rulesetNameEdit;
        private Button _addButton;
        private Button _loadButton;
        private Button _saveButton;
        private Button _deleteButton;

        private OptionSelectorMultiSelect _biomeTypeMultiSelector;
		private OptionSelectorMultiSelect _spawnProximityMultiSelector;
        private OptionSelectorMultiSelect _switchProximityMultiSelector;
        private OptionSelector _levelNumberOptionSelector;
		private OptionSelectorMultiSelect _levelSizeMultiSelector;
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

			_rulesetNameEdit = GetNode<TextEdit>("RulesetNameEdit");
            _addButton = GetNode<Button>("AddRulesetButton");
            _loadButton = GetNode<Button>("LoadRulesetButton");
            _saveButton = GetNode<Button>("SaveRulesetButton");
            _deleteButton = GetNode<Button>("DeleteRulesetButton");

            _biomeTypeMultiSelector = GetNode<OptionSelectorMultiSelect>("BiomeTypeOptionSelectorMultiSelect");
            _switchProximityMultiSelector = GetNode<OptionSelectorMultiSelect>("SpawnProximityOptionSelectorMultiSelect");
            _levelNumberOptionSelector = GetNode<OptionSelector>("LevelNumberOptionSelector");
            _levelSizeMultiSelector = GetNode<OptionSelectorMultiSelect>("LevelSizeOptionSelectorMultiSelect");
			_returnButton = GetNode<Button>("ReturnButton");

			_rulesetNameEdit.GrabFocus();
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
			//if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveSouth) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadSouth)))
			//{
			//	if (_biomeTypeMultiSelector.HasFocus())
			//	{
			//		_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

			//		_relativePlayerSpawnDistanceSelector.GetOptionButton().GrabFocus();
			//	}
			//	else if (_relativePlayerSpawnDistanceSelector.GetOptionButton().HasFocus())
			//	{
			//		_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

			//		_levelSizeSelector.GetOptionButton().GrabFocus();
			//	}
			//	else if (_levelSizeSelector.GetOptionButton().HasFocus())
			//	{
			//		_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

			//		_numberSpinner.GetNumberSpinnerButton().GrabFocus();
			//	}
			//	else if (_numberSpinner.GetNumberSpinnerButton().HasFocus())
			//	{
			//		_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

			//		_returnButton.GrabFocus();
			//	}
			//	else if (_returnButton.HasFocus() && _continueButton.Visible)
			//	{
			//		_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

			//		_continueButton.GrabFocus();
			//	}

			//	_inputTimer.Start();
			//}
			//else if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveNorth) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadNorth)))
			//{
			//	if (_continueButton.HasFocus() && _continueButton.Visible)
			//	{
			//		_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

			//		_returnButton.GrabFocus();
			//	}
			//	else if (_returnButton.HasFocus())
			//	{
			//		_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

			//		_numberSpinner.GetNumberSpinnerButton().GrabFocus();
			//	}
			//	else if (_numberSpinner.GetNumberSpinnerButton().HasFocus())
			//	{
			//		_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

			//		_levelSizeSelector.GetOptionButton().GrabFocus();
			//	}
			//	else if (_levelSizeSelector.GetOptionButton().HasFocus())
			//	{
			//		_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

			//		_relativePlayerSpawnDistanceSelector.GetOptionButton().GrabFocus();
			//	}
			//	else if (_relativePlayerSpawnDistanceSelector.GetOptionButton().HasFocus())
			//	{
			//		_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

   //                 _biomeTypeMultiSelector.GetOptionButton().GrabFocus();
			//	}

			//	_inputTimer.Start();
			//}
		}

		public void GrabFocusOfTopButton()
		{
			_rulesetNameEdit.GrabFocus();
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
