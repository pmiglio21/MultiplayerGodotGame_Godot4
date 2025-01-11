using Enums;
using Globals;
using Godot;
using Levels.UtilityLevels.UserInterfaceComponents;
using Models;
using Root;
using System.Collections.Generic;

namespace Levels.UtilityLevels
{
	public partial class GameRulesScreenManager : Control
	{
		private RootSceneSwapper _rootSceneSwapper;

		public GameRules CurrentGameRules = new GameRules();

		#region Options

		private readonly List<string> _levelSizeOptions = new List<string>()
        {
           "Small",
           "Medium",
           "Large"
        };

        private readonly List<string> _biomeOptions = new List<string>()
        {
           "Castle",
		   "Cave",
		   "Swamp",
		   "Frost"
        };

        private readonly List<string> _spawnProximityOptions = new List<string>()
        {
           "Super Close",
           "Close",
           "Normal",
           "Far"
        };

        private readonly List<string> _switchProximityOptions = new List<string>()
        {
           "Super Close",
           "Close",
           "Normal",
           "Far"
        };

        private readonly List<string> _offOnOptions = new List<string>()
        {
           "OFF",
           "ON"
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

        private OptionSelector _endlessLevelsOptionSelector;
        private Button _endlessLevelsButton;

        private OptionSelectorMultiSelect _biomeMultiSelector;
        private Button _biomeButton;

        private OptionSelectorMultiSelect _spawnProximityMultiSelector;
        private Button _spawnProximityButton;

        private OptionSelectorMultiSelect _switchProximityMultiSelector;
        private Button _switchProximityButton;

        private Button _miniBossButton;
        private Button _bossButton;
        private Button _friendlyFireButton;
		
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

            _endlessLevelsButton = GetNode<Button>("EndlessLevelsButton");

            _biomeMultiSelector = GetNode<OptionSelectorMultiSelect>("BiomeOptionSelectorMultiSelect");
            _biomeButton = GetNode<Button>("BiomeButton");

            _spawnProximityMultiSelector = GetNode<OptionSelectorMultiSelect>("SpawnProximityOptionSelectorMultiSelect");
            _spawnProximityButton = GetNode<Button>("SpawnProximityButton");

            _switchProximityMultiSelector = GetNode<OptionSelectorMultiSelect>("SwitchProximityOptionSelectorMultiSelect");
            _switchProximityButton = GetNode<Button>("SwitchProximityButton");

            _miniBossButton = GetNode<Button>("MiniBossButton");
            _bossButton = GetNode<Button>("BossButton");
            _friendlyFireButton = GetNode<Button>("FriendlyFireButton");


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
                    _biomeMultiSelector.GrabFocus();
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

        #region Signal Receptions

        public void OnLevelSizeOptionSelectorMultiSelect_LeftArrowClicked()
        {
            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

            var indexOfCurrentLevelSize = _levelSizeOptions.IndexOf(_levelSizeButton.Text);
            var nextIndexOfLevelSizeOptions = _levelSizeOptions.Count - 1;

            if (indexOfCurrentLevelSize != 0)
            {
                nextIndexOfLevelSizeOptions = indexOfCurrentLevelSize - 1;
            }

            _levelSizeButton.Text = $"{_levelSizeOptions[nextIndexOfLevelSizeOptions]}";

            if (CurrentGameRules.LevelSizes.ContainsKey(_levelSizeButton.Text))
            {
                if (CurrentGameRules.LevelSizes[_levelSizeButton.Text])
                {
                    _levelSizeMultiSelector.PlayActivatedOnOptionSelect();
                }
                else
                {
                    _levelSizeMultiSelector.PlayDeactivatedOnOptionSelect();
                }
            }
        }

        public void OnLevelSizeOptionSelectorMultiSelect_RightArrowClicked()
        {
            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

            var indexOfCurrentLevelSize = _levelSizeOptions.IndexOf(_levelSizeButton.Text);
            var nextIndexOfLevelSizeOptions = 0;

            if (indexOfCurrentLevelSize != _levelSizeOptions.Count - 1)
            {
                nextIndexOfLevelSizeOptions = indexOfCurrentLevelSize + 1;
            }

            _levelSizeButton.Text = $"{_levelSizeOptions[nextIndexOfLevelSizeOptions]}";

            if (CurrentGameRules.LevelSizes.ContainsKey(_levelSizeButton.Text))
            {
                if (CurrentGameRules.LevelSizes[_levelSizeButton.Text])
                {
                    _levelSizeMultiSelector.PlayActivatedOnOptionSelect();
                }
                else
                {
                    _levelSizeMultiSelector.PlayDeactivatedOnOptionSelect();
                }
            }
        }

        public void OnLevelSizeOptionSelect_OptionButtonPressed()
        {
            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

            if (CurrentGameRules.LevelSizes.ContainsKey(_levelSizeButton.Text))
            {
                CurrentGameRules.LevelSizes[_levelSizeButton.Text] = !CurrentGameRules.LevelSizes[_levelSizeButton.Text];

                if (CurrentGameRules.LevelSizes[_levelSizeButton.Text])
                {
                    _levelSizeMultiSelector.PlayActivatedOnOptionSelect();
                }
                else
                {
                    _levelSizeMultiSelector.PlayDeactivatedOnOptionSelect();
                }
            }
        }

        private void OnNumberOfLevelsOptionSelector_LeftArrowClicked()
        {
            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

            int numberOfLevels = int.Parse(_numberOfLevelsButton.Text);

            if (numberOfLevels != 1)
            {
                _numberOfLevelsButton.Text = (numberOfLevels - 1).ToString();
            }
            else
            {
                _numberOfLevelsButton.Text = "100";
            }
        }

        private void OnNumberOfLevelsOptionSelector_RightArrowClicked()
        {
            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

            int numberOfLevels = int.Parse(_numberOfLevelsButton.Text);

            if (numberOfLevels != 100)
            {
                _numberOfLevelsButton.Text = (numberOfLevels + 1).ToString();
            }
            else
            {
                _numberOfLevelsButton.Text = "1";
            }
        }

        private void OnEndlessLevelsOptionSelector_EitherArrowClicked()
        {
            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

            if (_endlessLevelsButton.Text == _offOnOptions[0])
            {
                _endlessLevelsButton.Text = _offOnOptions[1];

                _endlessLevelsButton.AddThemeColorOverride("default_color", new Color(Colors.DarkGray));
            }
            else
            {
                _endlessLevelsButton.Text = _offOnOptions[0];
            }

            CurrentGameRules.IsEndlessLevelsOn = _endlessLevelsButton.Text == _offOnOptions[1];
        }

        private void OnBiomeOptionSelectorMultiSelect_LeftArrowClicked()
        {
            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

            var indexOfCurrentBiome = _biomeOptions.IndexOf(_biomeButton.Text);
            var nextIndexOfBiomeOptions = _biomeOptions.Count - 1;

            if (indexOfCurrentBiome != 0)
            {
                nextIndexOfBiomeOptions = indexOfCurrentBiome - 1;
            }

            _biomeButton.Text = $"{_biomeOptions[nextIndexOfBiomeOptions]}";

            if (CurrentGameRules.BiomeTypes.ContainsKey(_biomeButton.Text))
            {
                if (CurrentGameRules.BiomeTypes[_biomeButton.Text])
                {
                    _biomeMultiSelector.PlayActivatedOnOptionSelect();
                }
                else
                {
                    _biomeMultiSelector.PlayDeactivatedOnOptionSelect();
                }
            }
        }

        private void OnBiomeOptionSelectorMultiSelect_RightArrowClicked()
        {
            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

            var indexOfCurrentBiome = _biomeOptions.IndexOf(_biomeButton.Text);
            var nextIndexOfBiomeOptions = 0;

            if (indexOfCurrentBiome != _biomeOptions.Count - 1)
            {
                nextIndexOfBiomeOptions = indexOfCurrentBiome + 1;
            }

            _biomeButton.Text = $"{_biomeOptions[nextIndexOfBiomeOptions]}";

            if (CurrentGameRules.BiomeTypes.ContainsKey(_biomeButton.Text))
            {
                if (CurrentGameRules.BiomeTypes[_biomeButton.Text])
                {
                    _biomeMultiSelector.PlayActivatedOnOptionSelect();
                }
                else
                {
                    _biomeMultiSelector.PlayDeactivatedOnOptionSelect();
                }
            }
        }

        public void OnBiomeOptionSelect_OptionButtonPressed()
        {
            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

            if (CurrentGameRules.BiomeTypes.ContainsKey(_biomeButton.Text))
            {
                CurrentGameRules.BiomeTypes[_biomeButton.Text] = !CurrentGameRules.BiomeTypes[_biomeButton.Text];

                if (CurrentGameRules.BiomeTypes[_biomeButton.Text])
                {
                    _biomeMultiSelector.PlayActivatedOnOptionSelect();
                }
                else
                {
                    _biomeMultiSelector.PlayDeactivatedOnOptionSelect();
                }
            }
        }

        private void OnSpawnProximityOptionSelectorMultiSelect_LeftArrowClicked()
        {
            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

            var indexOfCurrentSpawnProximity = _spawnProximityOptions.IndexOf(_spawnProximityButton.Text);
            var nextIndexOfSpawnProximityOptions = _spawnProximityOptions.Count - 1;

            if (indexOfCurrentSpawnProximity != 0)
            {
                nextIndexOfSpawnProximityOptions = indexOfCurrentSpawnProximity - 1;
            }

            _spawnProximityButton.Text = $"{_spawnProximityOptions[nextIndexOfSpawnProximityOptions]}";

            if (CurrentGameRules.SpawnProximityTypes.ContainsKey(_spawnProximityButton.Text))
            {
                if (CurrentGameRules.SpawnProximityTypes[_spawnProximityButton.Text])
                {
                    _spawnProximityMultiSelector.PlayActivatedOnOptionSelect();
                }
                else
                {
                    _spawnProximityMultiSelector.PlayDeactivatedOnOptionSelect();
                }
            }
        }

        private void OnSpawnProximityOptionSelectorMultiSelect_RightArrowClicked()
        {
            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

            var indexOfCurrentSpawnProximity = _spawnProximityOptions.IndexOf(_spawnProximityButton.Text);
            var nextIndexOfSpawnProximityOptions = 0;

            if (indexOfCurrentSpawnProximity != _spawnProximityOptions.Count - 1)
            {
                nextIndexOfSpawnProximityOptions = indexOfCurrentSpawnProximity + 1;
            }

            _spawnProximityButton.Text = $"{_spawnProximityOptions[nextIndexOfSpawnProximityOptions]}";

            if (CurrentGameRules.SpawnProximityTypes.ContainsKey(_spawnProximityButton.Text))
            {
                if (CurrentGameRules.SpawnProximityTypes[_spawnProximityButton.Text])
                {
                    _spawnProximityMultiSelector.PlayActivatedOnOptionSelect();
                }
                else
                {
                    _spawnProximityMultiSelector.PlayDeactivatedOnOptionSelect();
                }
            }
        }

        public void OnSpawnProximityOptionSelect_OptionButtonPressed()
        {
            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

            if (CurrentGameRules.SpawnProximityTypes.ContainsKey(_spawnProximityButton.Text))
            {
                CurrentGameRules.SpawnProximityTypes[_spawnProximityButton.Text] = !CurrentGameRules.SpawnProximityTypes[_spawnProximityButton.Text];

                if (CurrentGameRules.SpawnProximityTypes[_spawnProximityButton.Text])
                {
                    _spawnProximityMultiSelector.PlayActivatedOnOptionSelect();
                }
                else
                {
                    _spawnProximityMultiSelector.PlayDeactivatedOnOptionSelect();
                }
            }
        }

        private void OnSwitchProximityOptionSelectorMultiSelect_LeftArrowClicked()
        {
            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

            var indexOfCurrentSwitchProximity = _switchProximityOptions.IndexOf(_switchProximityButton.Text);
            var nextIndexOfSwitchProximityOptions = _switchProximityOptions.Count - 1;

            if (indexOfCurrentSwitchProximity != 0)
            {
                nextIndexOfSwitchProximityOptions = indexOfCurrentSwitchProximity - 1;
            }

            _switchProximityButton.Text = $"{_switchProximityOptions[nextIndexOfSwitchProximityOptions]}";

            if (CurrentGameRules.SwitchProximityTypes.ContainsKey(_switchProximityButton.Text))
            {
                if (CurrentGameRules.SwitchProximityTypes[_switchProximityButton.Text])
                {
                    _switchProximityMultiSelector.PlayActivatedOnOptionSelect();
                }
                else
                {
                    _switchProximityMultiSelector.PlayDeactivatedOnOptionSelect();
                }
            }
        }

        private void OnSwitchProximityOptionSelectorMultiSelect_RightArrowClicked()
        {
            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

            var indexOfCurrentSwitchProximity = _switchProximityOptions.IndexOf(_switchProximityButton.Text);
            var nextIndexOfSwitchProximityOptions = 0;

            if (indexOfCurrentSwitchProximity != _switchProximityOptions.Count - 1)
            {
                nextIndexOfSwitchProximityOptions = indexOfCurrentSwitchProximity + 1;
            }

            _switchProximityButton.Text = $"{_switchProximityOptions[nextIndexOfSwitchProximityOptions]}";

            if (CurrentGameRules.SwitchProximityTypes.ContainsKey(_switchProximityButton.Text))
            {
                if (CurrentGameRules.SwitchProximityTypes[_switchProximityButton.Text])
                {
                    _switchProximityMultiSelector.PlayActivatedOnOptionSelect();
                }
                else
                {
                    _switchProximityMultiSelector.PlayDeactivatedOnOptionSelect();
                }
            }
        }

        public void OnSwitchProximityOptionSelect_OptionButtonPressed()
        {
            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

            if (CurrentGameRules.SwitchProximityTypes.ContainsKey(_switchProximityButton.Text))
            {
                CurrentGameRules.SwitchProximityTypes[_switchProximityButton.Text] = !CurrentGameRules.SwitchProximityTypes[_switchProximityButton.Text];

                if (CurrentGameRules.SwitchProximityTypes[_switchProximityButton.Text])
                {
                    _switchProximityMultiSelector.PlayActivatedOnOptionSelect();
                }
                else
                {
                    _switchProximityMultiSelector.PlayDeactivatedOnOptionSelect();
                }
            }
        }

        public void OnMiniBossOptionSelector_EitherArrowClicked()
        {
            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

            if (_miniBossButton.Text == _offOnOptions[0])
            {
                _miniBossButton.Text = _offOnOptions[1];

                _miniBossButton.AddThemeColorOverride("default_color", new Color(Colors.DarkGray));
            }
            else
            {
                _miniBossButton.Text = _offOnOptions[0];
            }

            CurrentGameRules.CanMinibossSpawn = _miniBossButton.Text == _offOnOptions[1];
        }

        public void OnBossOptionSelector_EitherArrowClicked()
        {
            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

            if (_bossButton.Text == _offOnOptions[0])
            {
                _bossButton.Text = _offOnOptions[1];

                _bossButton.AddThemeColorOverride("default_color", new Color(Colors.DarkGray));
            }
            else
            {
                _bossButton.Text = _offOnOptions[0];
            }

            CurrentGameRules.CanBossSpawn = _bossButton.Text == _offOnOptions[1];
        }

        public void OnFriendlyFireOptionSelector_EitherArrowClicked()
        {
            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

            if (_friendlyFireButton.Text == _offOnOptions[0])
            {
                _friendlyFireButton.Text = _offOnOptions[1];

                _friendlyFireButton.AddThemeColorOverride("default_color", new Color(Colors.DarkGray));
            }
            else
            {
                _friendlyFireButton.Text = _offOnOptions[0];
            }

            CurrentGameRules.IsFriendlyFireOn = _friendlyFireButton.Text == _offOnOptions[1];
        }

        public void OnReturnButtonPressed()
        {
            _rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

            ReturnToPriorScene();
        }

        #endregion
    }
}
