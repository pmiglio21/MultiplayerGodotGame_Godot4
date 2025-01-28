using Enums;
using Globals;
using Godot;
using Levels.UtilityLevels.UserInterfaceComponents;
using Models;
using Newtonsoft.Json;
using Root;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Levels.UtilityLevels
{
	public partial class GameRulesScreenManager : Control
	{
        private RootSceneSwapper _rootSceneSwapper;

        private string _rulesetFolderPath = "user://rulesets.txt";   //user: is at %APPDATA%\Godot\app_userdata\[project_name]
        private List<GameRules> _availableRulesets = new List<GameRules>(); 

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
        private GenericButton _addButton;
        private GenericButton _loadButton;
        private GenericButton _saveButton;
        private GenericButton _deleteButton;

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

        private OptionSelector _miniBossOptionSelector;
        private Button _miniBossButton;

        private OptionSelector _bossOptionSelector;
        private Button _bossButton;

        private OptionSelector _friendlyFireOptionSelector;
        private Button _friendlyFireButton;
		
		private GenericButton _returnButton;

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
            _rulesetNameEdit.GetFocusHolder().FocusEntered += _rulesetNameEdit.PlayOnFocusAnimation;
            _rulesetNameEdit.GetFocusHolder().FocusExited += _rulesetNameEdit.PlayLoseFocusAnimation;

            _addButton = GetNode<GenericButton>("AddRulesetButton");
            _addButton.Pressed += AddRuleSet;

            _loadButton = GetNode<GenericButton>("LoadRulesetButton");
            _loadButton.Pressed += LoadRuleSet;

            _saveButton = GetNode<GenericButton>("SaveRulesetButton");
            _saveButton.Pressed += SaveRuleSet;

            _deleteButton = GetNode<GenericButton>("DeleteRulesetButton");
            _deleteButton.Pressed += DeleteRuleSet;

            _levelSizeMultiSelector = GetNode<OptionSelectorMultiSelect>("LevelSizeOptionSelectorMultiSelect");
            _levelSizeMultiSelector.GetFocusHolder().FocusEntered += _levelSizeMultiSelector.PlayOnFocusAnimation;
            _levelSizeMultiSelector.GetFocusHolder().FocusExited += _levelSizeMultiSelector.PlayLoseFocusAnimation;
            _levelSizeButton = GetNode<Button>("LevelSizeButton");

            _numberOfLevelsOptionSelector = GetNode<OptionSelector>("NumberOfLevelsOptionSelector");
            _numberOfLevelsOptionSelector.GetFocusHolder().FocusEntered += _numberOfLevelsOptionSelector.PlayOnFocusAnimation;
            _numberOfLevelsOptionSelector.GetFocusHolder().FocusExited += _numberOfLevelsOptionSelector.PlayLoseFocusAnimation;
            _numberOfLevelsButton = GetNode<Button>("NumberOfLevelsButton");

            _endlessLevelsOptionSelector = GetNode<OptionSelector>("EndlessLevelsOptionSelector");
            _endlessLevelsOptionSelector.GetFocusHolder().FocusEntered += _endlessLevelsOptionSelector.PlayOnFocusAnimation;
            _endlessLevelsOptionSelector.GetFocusHolder().FocusExited += _endlessLevelsOptionSelector.PlayLoseFocusAnimation;
            _endlessLevelsButton = GetNode<Button>("EndlessLevelsButton");

            _biomeMultiSelector = GetNode<OptionSelectorMultiSelect>("BiomeOptionSelectorMultiSelect");
            _biomeMultiSelector.GetFocusHolder().FocusEntered += _biomeMultiSelector.PlayOnFocusAnimation;
            _biomeMultiSelector.GetFocusHolder().FocusExited += _biomeMultiSelector.PlayLoseFocusAnimation;
            _biomeButton = GetNode<Button>("BiomeButton");

            _spawnProximityMultiSelector = GetNode<OptionSelectorMultiSelect>("SpawnProximityOptionSelectorMultiSelect");
            _spawnProximityMultiSelector.GetFocusHolder().FocusEntered += _spawnProximityMultiSelector.PlayOnFocusAnimation;
            _spawnProximityMultiSelector.GetFocusHolder().FocusExited += _spawnProximityMultiSelector.PlayLoseFocusAnimation;
            _spawnProximityButton = GetNode<Button>("SpawnProximityButton");

            _switchProximityMultiSelector = GetNode<OptionSelectorMultiSelect>("SwitchProximityOptionSelectorMultiSelect");
            _switchProximityMultiSelector.GetFocusHolder().FocusEntered += _switchProximityMultiSelector.PlayOnFocusAnimation;
            _switchProximityMultiSelector.GetFocusHolder().FocusExited += _switchProximityMultiSelector.PlayLoseFocusAnimation;
            _switchProximityButton = GetNode<Button>("SwitchProximityButton");

            _miniBossOptionSelector = GetNode<OptionSelector>("MiniBossOptionSelector");
            _miniBossOptionSelector.GetFocusHolder().FocusEntered += _miniBossOptionSelector.PlayOnFocusAnimation;
            _miniBossOptionSelector.GetFocusHolder().FocusExited += _miniBossOptionSelector.PlayLoseFocusAnimation;
            _miniBossButton = GetNode<Button>("MiniBossButton");

            _bossOptionSelector = GetNode<OptionSelector>("BossOptionSelector");
            _bossOptionSelector.GetFocusHolder().FocusEntered += _bossOptionSelector.PlayOnFocusAnimation;
            _bossOptionSelector.GetFocusHolder().FocusExited += _bossOptionSelector.PlayLoseFocusAnimation;
            _bossButton = GetNode<Button>("BossButton");

            _friendlyFireOptionSelector = GetNode<OptionSelector>("FriendlyFireOptionSelector");
            _friendlyFireOptionSelector.GetFocusHolder().FocusEntered += _friendlyFireOptionSelector.PlayOnFocusAnimation;
            _friendlyFireOptionSelector.GetFocusHolder().FocusExited += _friendlyFireOptionSelector.PlayLoseFocusAnimation;
            _friendlyFireButton = GetNode<Button>("FriendlyFireButton");

            _returnButton = GetNode<GenericButton>("ReturnButton");

            GetAvailableRuleSets();

            GrabFocusOfTopButton();
        }

		public override void _Process(double delta)
		{
			GetButtonPressInput();

			GetNavigationInput();
		}

		private void GetButtonPressInput()
		{
			if (UniversalInputHelper.IsActionJustPressed(InputType.SouthButton))
			{
                #region Press Button while focusing on XxxxOptionSelector

                //Row 1
                if (_rulesetNameEdit.GetFocusHolder().HasFocus())
                {
                    _rulesetNameEdit.GetTextEditBox().GrabFocus();
                }

                //Row 2
                else if (_levelSizeMultiSelector.GetFocusHolder().HasFocus())
                {
                    _levelSizeButton.GrabFocus();
                }
                else if (_spawnProximityMultiSelector.GetFocusHolder().HasFocus())
                {
                    _spawnProximityButton.GrabFocus();
                }
                else if (_friendlyFireOptionSelector.GetFocusHolder().HasFocus())
                {
                    _friendlyFireButton.GrabFocus();
                }

                //Row 3
                else if (_numberOfLevelsOptionSelector.GetFocusHolder().HasFocus())
                {
                    _numberOfLevelsButton.GrabFocus();
                }
                else if (_switchProximityMultiSelector.GetFocusHolder().HasFocus())
                {
                    _switchProximityButton.GrabFocus();
                }

                //Row 4
                else if (_endlessLevelsOptionSelector.GetFocusHolder().HasFocus())
                {
                    _endlessLevelsButton.GrabFocus();
                }
                else if (_miniBossOptionSelector.GetFocusHolder().HasFocus())
                {
                    _miniBossButton.GrabFocus();
                }

                //Row 5
                else if (_biomeMultiSelector.GetFocusHolder().HasFocus())
                {
                    _biomeButton.GrabFocus();
                }
                else if (_bossOptionSelector.GetFocusHolder().HasFocus())
                {
                    _bossButton.GrabFocus();
                }

                #endregion

                #region Press Button while focused on XxxxButton

                //Row 1
                if (_addButton.HasFocus())
                {
                    AddRuleSet();
                }
                else if (_loadButton.HasFocus())
                {
                    LoadRuleSet();
                }
                else if (_saveButton.HasFocus())
                {
                    SaveRuleSet();
                }
                else if (_deleteButton.HasFocus())
                {
                    DeleteRuleSet();
                }

                //Row 2
                else if (_levelSizeButton.HasFocus())
                {
                    OnLevelSizeOptionSelect_OptionButtonPressed();
                }
                else if (_spawnProximityButton.HasFocus())
                {
                    OnSpawnProximityOptionSelect_OptionButtonPressed();
                }
                else if (_friendlyFireButton.HasFocus())
                {
                    OnFriendlyFireOptionSelector_EitherArrowClicked();
                }

                //Row 3
                else if (_numberOfLevelsButton.HasFocus())
                {
                    OnNumberOfLevelsOptionSelector_RightArrowClicked();
                }
                else if (_switchProximityButton.HasFocus())
                {
                    OnSwitchProximityOptionSelect_OptionButtonPressed();
                }

                //Row 4
                else if (_endlessLevelsButton.HasFocus())
                {
                    OnEndlessLevelsOptionSelector_EitherArrowClicked();
                }
                else if (_miniBossButton.HasFocus())
                {
                    OnMiniBossOptionSelector_EitherArrowClicked();
                }

                //Row 5
                else if (_biomeButton.HasFocus())
                {
                    OnBiomeOptionSelect_OptionButtonPressed();
                }
                else if (_bossButton.HasFocus())
                {
                    OnBossOptionSelector_EitherArrowClicked();
                }
                else if (_returnButton.HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiReturnToPreviousScreenSoundPath);

					ReturnToPriorScene();
				}

                #endregion
            }
            else if (UniversalInputHelper.IsActionJustPressed(InputType.EastButton))
			{
                #region Press Button while focused on XxxxButton

                //Row 2
                if (_levelSizeButton.HasFocus())
                {
                    _levelSizeMultiSelector.GetFocusHolder().GrabFocus();
                }
                else if (_spawnProximityButton.HasFocus())
                {
                    _spawnProximityMultiSelector.GetFocusHolder().GrabFocus();
                }
                else if (_friendlyFireButton.HasFocus())
                {
                    _friendlyFireOptionSelector.GetFocusHolder().GrabFocus();
                }

                //Row 3
                else if (_numberOfLevelsButton.HasFocus())
                {
                    _numberOfLevelsOptionSelector.GetFocusHolder().GrabFocus();
                }
                else if (_switchProximityButton.HasFocus())
                {
                    _switchProximityMultiSelector.GetFocusHolder().GrabFocus();
                }

                //Row 4
                else if (_endlessLevelsButton.HasFocus())
                {
                    _endlessLevelsOptionSelector.GetFocusHolder().GrabFocus();
                }
                else if (_miniBossButton.HasFocus())
                {
                    _miniBossOptionSelector.GetFocusHolder().GrabFocus();
                }

                //Row 5
                else if (_biomeButton.HasFocus())
                {
                    _biomeMultiSelector.GetFocusHolder().GrabFocus();
                }
                else if (_bossButton.HasFocus())
                {
                    _bossOptionSelector.GetFocusHolder().GrabFocus();
                }

                #endregion

                else if (!_rulesetNameEdit.GetTextEditBox().HasFocus())
				{
                    ReturnToPriorScene();
                }
			}
        }

		private void GetNavigationInput()
		{
            
            if (_inputTimer.IsStopped() && Input.IsKeyPressed(Key.Enter))
            {
                //Enter into text edit
                if (_rulesetNameEdit.GetFocusHolder().HasFocus())
                {
                    _rulesetNameEdit.GetTextEditBox().GrabFocus();
                    _rulesetNameEdit.GetTextEditBox().SetCaretColumn(_rulesetNameEdit.GetTextEditBox().Text.Length);
                }
                else if (_rulesetNameEdit.GetTextEditBox().HasFocus())
                {
                    _rulesetNameEdit.GetFocusHolder().GrabFocus();
                }

                _inputTimer.Start();
            }
            else if (_inputTimer.IsStopped() && Input.IsKeyPressed(Key.Escape))
            {
                //Exit text edit
                if (_rulesetNameEdit.GetTextEditBox().HasFocus())
                {
                    _rulesetNameEdit.GetFocusHolder().GrabFocus();
                }

                _inputTimer.Start();
            }
            //Go forward
            else if (_inputTimer.IsStopped() && Input.IsKeyPressed(Key.Tab) && !Input.IsKeyPressed(Key.Shift))
			{
                //Row 1
                if (_rulesetNameEdit.GetFocusHolder().HasFocus())
                {
                    _addButton.GrabFocus();
                }
                if (_rulesetNameEdit.GetTextEditBox().HasFocus())
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
                else if (_deleteButton.HasFocus())
                {
                    _levelSizeMultiSelector.GetFocusHolder().GrabFocus();
                }

                //Row 2
                else if (_levelSizeMultiSelector.GetFocusHolder().HasFocus())
                {
                    _spawnProximityMultiSelector.GetFocusHolder().GrabFocus();
                }
                else if (_spawnProximityMultiSelector.GetFocusHolder().HasFocus())
                {
                    _friendlyFireOptionSelector.GetFocusHolder().GrabFocus();
                }
                else if (_friendlyFireOptionSelector.GetFocusHolder().HasFocus())
                {
                    _numberOfLevelsOptionSelector.GetFocusHolder().GrabFocus();
                }

                //Row 3
                else if (_numberOfLevelsOptionSelector.GetFocusHolder().HasFocus())
                {
                    _switchProximityMultiSelector.GetFocusHolder().GrabFocus();
                }
                else if (_switchProximityMultiSelector.GetFocusHolder().HasFocus())
                {
                    _endlessLevelsOptionSelector.GetFocusHolder().GrabFocus();
                }

                //Row 4
                else if (_endlessLevelsOptionSelector.GetFocusHolder().HasFocus())
                {
                    _miniBossOptionSelector.GetFocusHolder().GrabFocus();
                }
                else if (_miniBossOptionSelector.GetFocusHolder().HasFocus())
                {
                    _biomeMultiSelector.GetFocusHolder().GrabFocus();
                }

                //Row 5
                else if (_biomeMultiSelector.GetFocusHolder().HasFocus())
                {
                    _bossOptionSelector.GetFocusHolder().GrabFocus();
                }
                else if (_bossOptionSelector.GetFocusHolder().HasFocus())
                {
                    _returnButton.GrabFocus();
                }

                _inputTimer.Start();
            }
            //Go backward
            else if (_inputTimer.IsStopped() && Input.IsKeyPressed(Key.Tab) && Input.IsKeyPressed(Key.Shift))
            {
                //Row 1
                if (_deleteButton.HasFocus())
                {
                    _saveButton.GrabFocus();
                }
                else if (_saveButton.HasFocus())
                {
                    _loadButton.GrabFocus();
                }
                else if (_loadButton.HasFocus())
                {
                    _addButton.GrabFocus();
                }
                else if (_addButton.HasFocus())
                {
                    _rulesetNameEdit.GetFocusHolder().GrabFocus();
                }

                //Row 2
                else if (_friendlyFireOptionSelector.GetFocusHolder().HasFocus())
                {
                    _spawnProximityMultiSelector.GetFocusHolder().GrabFocus();
                }
                else if (_spawnProximityMultiSelector.GetFocusHolder().HasFocus())
                {
                    _levelSizeMultiSelector.GetFocusHolder().GrabFocus();
                }
                else if (_levelSizeMultiSelector.GetFocusHolder().HasFocus())
                {
                    _deleteButton.GrabFocus();
                }

                //Row 3
                else if (_switchProximityMultiSelector.GetFocusHolder().HasFocus())
                {
                    _numberOfLevelsOptionSelector.GetFocusHolder().GrabFocus();
                }
                else if (_numberOfLevelsOptionSelector.GetFocusHolder().HasFocus())
                {
                    _friendlyFireOptionSelector.GetFocusHolder().GrabFocus();
                }

                //Row 4
                else if (_miniBossOptionSelector.GetFocusHolder().HasFocus())
                {
                    _endlessLevelsOptionSelector.GetFocusHolder().GrabFocus();
                }
                else if (_endlessLevelsOptionSelector.GetFocusHolder().HasFocus())
                {
                    _switchProximityMultiSelector.GetFocusHolder().GrabFocus();
                }

                //Row 5
                else if (_bossOptionSelector.GetFocusHolder().HasFocus())
                {
                    _biomeMultiSelector.GetFocusHolder().GrabFocus();
                }
                else if (_biomeMultiSelector.GetFocusHolder().HasFocus())
                {
                    _miniBossOptionSelector.GetFocusHolder().GrabFocus();
                }

                //Row 6
                else if (_returnButton.HasFocus())
                {
                    _bossOptionSelector.GetFocusHolder().GrabFocus();
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
                else if (_addButton.HasFocus())
                {
                    _spawnProximityMultiSelector.GetFocusHolder().GrabFocus();
                }
                else if (_loadButton.HasFocus())
                {
                    _spawnProximityMultiSelector.GetFocusHolder().GrabFocus();
                }
                else if (_saveButton.HasFocus())
                {
                    _friendlyFireOptionSelector.GetFocusHolder().GrabFocus();
                }
                else if (_deleteButton.HasFocus())
                {
                    _friendlyFireOptionSelector.GetFocusHolder().GrabFocus();
                }

                //Row 2
                else if (_levelSizeMultiSelector.GetFocusHolder().HasFocus())
                {
                    _numberOfLevelsOptionSelector.GetFocusHolder().GrabFocus();
                }
                else if (_spawnProximityMultiSelector.GetFocusHolder().HasFocus())
                {
                    _switchProximityMultiSelector.GetFocusHolder().GrabFocus();
                }
                else if (_friendlyFireOptionSelector.GetFocusHolder().HasFocus())
                {
                    _returnButton.GrabFocus();
                }

                //Row 3
                else if (_numberOfLevelsOptionSelector.GetFocusHolder().HasFocus())
                {
                    _endlessLevelsOptionSelector.GetFocusHolder().GrabFocus();
                }
                else if (_switchProximityMultiSelector.GetFocusHolder().HasFocus())
                {
                    _miniBossOptionSelector.GetFocusHolder().GrabFocus();
                }

                //Row 4
                else if (_endlessLevelsOptionSelector.GetFocusHolder().HasFocus())
                {
                    _biomeMultiSelector.GetFocusHolder().GrabFocus();
                }
                else if (_miniBossOptionSelector.GetFocusHolder().HasFocus())
                {
                    _bossOptionSelector.GetFocusHolder().GrabFocus();
                }

                //Row 5
                else if (_biomeMultiSelector.GetFocusHolder().HasFocus())
                {
                    _returnButton.GrabFocus();
                }
                else if (_bossOptionSelector.GetFocusHolder().HasFocus())
                {
                    _returnButton.GrabFocus();
                }


                _inputTimer.Start();
			}
            else if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveEast) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadEast)))
            {
                //Row 1
                if (_rulesetNameEdit.GetFocusHolder().HasFocus())
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

                //Row 2
                else if (_levelSizeMultiSelector.GetFocusHolder().HasFocus())
                {
                    _spawnProximityMultiSelector.GetFocusHolder().GrabFocus();
                }
                else if (_spawnProximityMultiSelector.GetFocusHolder().HasFocus())
                {
                    _friendlyFireOptionSelector.GetFocusHolder().GrabFocus();
                }

                //Row 3
                else if (_numberOfLevelsOptionSelector.GetFocusHolder().HasFocus())
                {
                    _switchProximityMultiSelector.GetFocusHolder().GrabFocus();
                }

                //Row 4
                else if (_endlessLevelsOptionSelector.GetFocusHolder().HasFocus())
                {
                    _miniBossOptionSelector.GetFocusHolder().GrabFocus();
                }

                //Row 5
                else if (_biomeMultiSelector.GetFocusHolder().HasFocus())
                {
                    _bossOptionSelector.GetFocusHolder().GrabFocus();
                }

                #region Navigating Options

                //Row 2
                else if (_levelSizeButton.HasFocus())
                {
                    OnLevelSizeOptionSelectorMultiSelect_RightArrowClicked();
                }
                else if (_spawnProximityButton.HasFocus())
                {
                    OnSpawnProximityOptionSelectorMultiSelect_RightArrowClicked();
                }
                else if (_friendlyFireButton.HasFocus())
                {
                    OnFriendlyFireOptionSelector_EitherArrowClicked();
                }

                //Row 3
                else if (_numberOfLevelsButton.HasFocus())
                {
                    OnNumberOfLevelsOptionSelector_RightArrowClicked();
                }
                else if (_switchProximityButton.HasFocus())
                {
                    OnSwitchProximityOptionSelectorMultiSelect_RightArrowClicked();
                }

                //Row 4
                else if (_endlessLevelsButton.HasFocus())
                {
                    OnEndlessLevelsOptionSelector_EitherArrowClicked();
                }
                else if (_miniBossButton.HasFocus())
                {
                    OnMiniBossOptionSelector_EitherArrowClicked();
                }

                //Row 5
                else if (_biomeButton.HasFocus())
                {
                    OnBiomeOptionSelectorMultiSelect_RightArrowClicked();
                }
                else if (_bossButton.HasFocus())
                {
                    OnBossOptionSelector_EitherArrowClicked();
                }

                #endregion

                _inputTimer.Start();
            }
            else if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveNorth) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadNorth)))
			{
                //Row 2
                if (_levelSizeMultiSelector.GetFocusHolder().HasFocus())
                {
                    _rulesetNameEdit.GetFocusHolder().GrabFocus();
                }
                else if (_spawnProximityMultiSelector.GetFocusHolder().HasFocus())
                {
                    _addButton.GrabFocus();
                }
                else if (_friendlyFireOptionSelector.GetFocusHolder().HasFocus())
                {
                    _saveButton.GrabFocus();
                }

                //Row 3
                else if (_numberOfLevelsOptionSelector.GetFocusHolder().HasFocus())
                {
                    _levelSizeMultiSelector.GetFocusHolder().GrabFocus();
                }
                else if (_switchProximityMultiSelector.GetFocusHolder().HasFocus())
                {
                    _spawnProximityMultiSelector.GetFocusHolder().GrabFocus();
                }

                //Row 4
                else if (_endlessLevelsOptionSelector.GetFocusHolder().HasFocus())
                {
                    _numberOfLevelsOptionSelector.GetFocusHolder().GrabFocus();
                }
                else if (_miniBossOptionSelector.GetFocusHolder().HasFocus())
                {
                    _switchProximityMultiSelector.GetFocusHolder().GrabFocus();
                }

                //Row 5
                else if (_biomeMultiSelector.GetFocusHolder().HasFocus())
                {
                    _endlessLevelsOptionSelector.GetFocusHolder().GrabFocus();
                }
                else if (_bossOptionSelector.GetFocusHolder().HasFocus())
                {
                    _miniBossOptionSelector.GetFocusHolder().GrabFocus();
                }

                //Row 6
                else if (_returnButton.HasFocus())
                {
                    _bossOptionSelector.GetFocusHolder().GrabFocus();
                }

                _inputTimer.Start();
			}
            else if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveWest) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadWest)))
            {
                //Row 1
                if (_deleteButton.HasFocus())
                {
                    _saveButton.GrabFocus();
                }
                else if (_saveButton.HasFocus())
                {
                    _loadButton.GrabFocus();
                }
                else if (_loadButton.HasFocus())
                {
                    _addButton.GrabFocus();
                }
                else if (_addButton.HasFocus())
                {
                    _rulesetNameEdit.GetFocusHolder().GrabFocus();
                }

                //Row 2
                else if (_friendlyFireOptionSelector.GetFocusHolder().HasFocus())
                {
                    _spawnProximityMultiSelector.GetFocusHolder().GrabFocus();
                }
                else if (_spawnProximityMultiSelector.GetFocusHolder().HasFocus())
                {
                    _levelSizeMultiSelector.GetFocusHolder().GrabFocus();
                }

                //Row 3
                else if (_switchProximityMultiSelector.GetFocusHolder().HasFocus())
                {
                    _numberOfLevelsOptionSelector.GetFocusHolder().GrabFocus();
                }

                //Row 4
                else if (_miniBossOptionSelector.GetFocusHolder().HasFocus())
                {
                    _endlessLevelsOptionSelector.GetFocusHolder().GrabFocus();
                }

                //Row 5
                else if (_bossOptionSelector.GetFocusHolder().HasFocus())
                {
                    _biomeMultiSelector.GetFocusHolder().GrabFocus();
                }

                #region Navigating Options

                //Row 2
                if (_levelSizeButton.HasFocus())
                {
                    OnLevelSizeOptionSelectorMultiSelect_LeftArrowClicked();
                }
                else if (_spawnProximityButton.HasFocus())
                {
                    OnSpawnProximityOptionSelectorMultiSelect_LeftArrowClicked();
                }
                else if (_friendlyFireButton.HasFocus())
                {
                    OnFriendlyFireOptionSelector_EitherArrowClicked();
                }

                //Row 3
                else if (_numberOfLevelsButton.HasFocus())
                {
                    OnNumberOfLevelsOptionSelector_LeftArrowClicked();
                }
                else if (_switchProximityButton.HasFocus())
                {
                    OnSwitchProximityOptionSelectorMultiSelect_LeftArrowClicked();
                }

                //Row 4
                else if (_endlessLevelsButton.HasFocus())
                {
                    OnEndlessLevelsOptionSelector_EitherArrowClicked();
                }
                else if (_miniBossButton.HasFocus())
                {
                    OnMiniBossOptionSelector_EitherArrowClicked();
                }

                //Row 5
                else if (_biomeButton.HasFocus())
                {
                    OnBiomeOptionSelectorMultiSelect_LeftArrowClicked();
                }
                else if (_bossButton.HasFocus())
                {
                    OnBossOptionSelector_EitherArrowClicked();
                }

                #endregion

                _inputTimer.Start();
            }
        }

		public void GrabFocusOfTopButton()
		{
            _rulesetNameEdit.GetFocusHolder().GrabFocus();
        }

		private void ReturnToPriorScene()
		{
            _rootSceneSwapper.CurrentGameRules = CurrentGameRules;

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

        #region Ruleset Modification

        private void GetAvailableRuleSets()
        {
            try
            {
                if (FileAccess.FileExists(_rulesetFolderPath))
                {
                    _availableRulesets.Clear();

                    using (var rulesetFile = FileAccess.Open(_rulesetFolderPath, FileAccess.ModeFlags.Read))
                    {
                        var currentLine = rulesetFile.GetLine();

                        while (!string.IsNullOrWhiteSpace(currentLine))
                        {
                            GameRules deserializedRuleset = JsonConvert.DeserializeObject<GameRules>(currentLine);

                            _availableRulesets.Add(deserializedRuleset);

                            currentLine = rulesetFile.GetLine();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PushError(ex.Message);
            }
        }

        private void AddRuleSet()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_rulesetNameEdit.GetTextEditBox().Text))
                {
                    using (var rulesetFile = FileAccess.Open(_rulesetFolderPath, FileAccess.ModeFlags.ReadWrite))
                    {
                        CurrentGameRules.RulesetName = _rulesetNameEdit.GetTextEditBox().Text;

                        string serializedRuleset = JsonConvert.SerializeObject(CurrentGameRules);

                        //Forces the file writer to write at the end of the file instead of the beginning.
                        //Necessary to call or else the file's first line will be overwritten by StoreLine.
                        rulesetFile.SeekEnd();

                        rulesetFile.StoreLine(serializedRuleset);
                    }

                    GetAvailableRuleSets();
                }
            }
            catch (Exception ex)
            {
                GD.PushError(ex.Message);
            }
        }

        private void LoadRuleSet()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_rulesetNameEdit.GetTextEditBox().Text))
                {
                    GameRules matchingRuleset = _availableRulesets.FirstOrDefault(x => x.RulesetName == _rulesetNameEdit.GetTextEditBox().Text);

                    if (matchingRuleset != null)
                    {
                        // Row 2
                        var isCurrentLevelSizeOptionSelected = matchingRuleset.LevelSizes.FirstOrDefault(x => x.Key == _levelSizeButton.Text).Value;

                        if (isCurrentLevelSizeOptionSelected)
                        {
                            _levelSizeMultiSelector.PlayActivatedOnOptionSelect();
                        }
                        else
                        {
                            _levelSizeMultiSelector.PlayDeactivatedOnOptionSelect();
                        }

                        //Have to set Dictionaries this way instead of CurrentGameRules.LevelSizes = matchingRuleset.LevelSizes,
                        //because the second way would be setting by reference instead of by value.
                        CurrentGameRules.LevelSizes = new Dictionary<string, bool>();

                        foreach (KeyValuePair<string, bool> levelSizePair in matchingRuleset.LevelSizes)
                        {
                            CurrentGameRules.LevelSizes.Add(levelSizePair.Key, levelSizePair.Value);
                        }

                        var isCurrentSpawnProximityOptionSelected = matchingRuleset.SpawnProximityTypes.FirstOrDefault(x => x.Key == _spawnProximityButton.Text).Value;

                        if (isCurrentSpawnProximityOptionSelected)
                        {
                            _spawnProximityMultiSelector.PlayActivatedOnOptionSelect();
                        }
                        else
                        {
                            _spawnProximityMultiSelector.PlayDeactivatedOnOptionSelect();
                        }

                        CurrentGameRules.SpawnProximityTypes = new Dictionary<string, bool>();

                        foreach (KeyValuePair<string, bool> spawnProximityPair in matchingRuleset.SpawnProximityTypes)
                        {
                            CurrentGameRules.SpawnProximityTypes.Add(spawnProximityPair.Key, spawnProximityPair.Value);
                        }

                        _friendlyFireButton.Text = matchingRuleset.IsFriendlyFireOn ? _offOnOptions[1] : _offOnOptions[0];

                        CurrentGameRules.IsFriendlyFireOn = matchingRuleset.IsFriendlyFireOn;


                        //Row 3
                        _numberOfLevelsButton.Text = matchingRuleset.NumberOfLevels.ToString();

                        CurrentGameRules.NumberOfLevels = matchingRuleset.NumberOfLevels;

                        var isCurrentSwitchProximityOptionSelected = matchingRuleset.SwitchProximityTypes.FirstOrDefault(x => x.Key == _switchProximityButton.Text).Value;

                        if (isCurrentSwitchProximityOptionSelected)
                        {
                            _switchProximityMultiSelector.PlayActivatedOnOptionSelect();
                        }
                        else
                        {
                            _switchProximityMultiSelector.PlayDeactivatedOnOptionSelect();
                        }

                        CurrentGameRules.SwitchProximityTypes = new Dictionary<string, bool>();

                        foreach (KeyValuePair<string, bool> switchProximityPair in matchingRuleset.SwitchProximityTypes)
                        {
                            CurrentGameRules.SwitchProximityTypes.Add(switchProximityPair.Key, switchProximityPair.Value);
                        }


                        //Row 4
                        _endlessLevelsButton.Text = matchingRuleset.IsEndlessLevelsOn ? _offOnOptions[1] : _offOnOptions[0];

                        _miniBossButton.Text = matchingRuleset.CanMinibossSpawn ? _offOnOptions[1] : _offOnOptions[0];


                        //Row 5
                        var isCurrentBiomeOptionSelected = matchingRuleset.BiomeTypes.FirstOrDefault(x => x.Key == _biomeButton.Text).Value;

                        if (isCurrentBiomeOptionSelected)
                        {
                            _biomeMultiSelector.PlayActivatedOnOptionSelect();
                        }
                        else
                        {
                            _biomeMultiSelector.PlayDeactivatedOnOptionSelect();
                        }

                        CurrentGameRules.BiomeTypes = new Dictionary<string, bool>();

                        foreach (KeyValuePair<string, bool> biomeTypePair in matchingRuleset.BiomeTypes)
                        {
                            CurrentGameRules.BiomeTypes.Add(biomeTypePair.Key, biomeTypePair.Value);
                        }

                        _bossButton.Text = matchingRuleset.CanBossSpawn ? _offOnOptions[1] : _offOnOptions[0];
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PushError(ex.Message);
            }
        }

        private void SaveRuleSet()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_rulesetNameEdit.GetTextEditBox().Text))
                {
                    GameRules matchingRuleset = _availableRulesets.FirstOrDefault(x => x.RulesetName == _rulesetNameEdit.GetTextEditBox().Text);

                    if (matchingRuleset != null)
                    {
                        CurrentGameRules.RulesetName = _rulesetNameEdit.GetTextEditBox().Text;

                        matchingRuleset = CurrentGameRules;

                        //Rewrite all available rulesets, with the matching ruleset now updated
                        using (var rulesetFile = FileAccess.Open(_rulesetFolderPath, FileAccess.ModeFlags.Write))    //Using Write to truncate table, instead of ReadWrite
                        {
                            foreach (GameRules ruleset in _availableRulesets)
                            {
                                string serializedRuleset = string.Empty;

                                if (ruleset.RulesetName == _rulesetNameEdit.GetTextEditBox().Text)
                                {
                                    serializedRuleset = JsonConvert.SerializeObject(CurrentGameRules);
                                }
                                else
                                {
                                    serializedRuleset = JsonConvert.SerializeObject(ruleset);
                                }

                                //Forces the file writer to write at the end of the file instead of the beginning.
                                //Necessary to call or else the file's first line will be overwritten by StoreLine.
                                rulesetFile.SeekEnd();

                                rulesetFile.StoreLine(serializedRuleset);
                            }
                        }

                        GetAvailableRuleSets();
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PushError(ex.Message);
            }
        }

        private void DeleteRuleSet()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_rulesetNameEdit.GetTextEditBox().Text))
                {
                    GameRules matchingRuleset = _availableRulesets.FirstOrDefault(x => x.RulesetName == _rulesetNameEdit.GetTextEditBox().Text);

                    if (matchingRuleset != null)
                    {
                        _availableRulesets.Remove(matchingRuleset);

                        //Rewrite all available rulesets, with the matching ruleset now removed
                        using (var rulesetFile = FileAccess.Open(_rulesetFolderPath, FileAccess.ModeFlags.Write))   //Using Write to truncate table, instead of ReadWrite
                        {
                            foreach (GameRules ruleset in _availableRulesets)
                            {
                                string serializedRuleset = JsonConvert.SerializeObject(ruleset);

                                //Forces the file writer to write at the end of the file instead of the beginning.
                                //Necessary to call or else the file's first line will be overwritten by StoreLine.
                                rulesetFile.SeekEnd();

                                rulesetFile.StoreLine(serializedRuleset);
                            }
                        }

                        GetAvailableRuleSets();
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PushError(ex.Message);
            }
        }

        #endregion

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

            CurrentGameRules.NumberOfLevels = int.Parse(_numberOfLevelsButton.Text);
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

            CurrentGameRules.NumberOfLevels = int.Parse(_numberOfLevelsButton.Text);
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

        public void SetOptionButtonsWhenEnteringGameRulesScreen()
        {
            if (CurrentGameRules.LevelSizes[_levelSizeButton.Text])
            {
                _levelSizeMultiSelector.PlayActivatedOnOptionSelect();
            }

            if (CurrentGameRules.SpawnProximityTypes[_spawnProximityButton.Text])
            {
                _spawnProximityMultiSelector.PlayActivatedOnOptionSelect();
            }

            _friendlyFireButton.Text = CurrentGameRules.IsFriendlyFireOn ? _offOnOptions[1] : _offOnOptions[0];

            _numberOfLevelsButton.Text = CurrentGameRules.NumberOfLevels.ToString();

            if (CurrentGameRules.SwitchProximityTypes[_switchProximityButton.Text])
            {
                _switchProximityMultiSelector.PlayActivatedOnOptionSelect();
            }

            _endlessLevelsButton.Text = CurrentGameRules.IsEndlessLevelsOn ? _offOnOptions[1] : _offOnOptions[0];

            _miniBossButton.Text = CurrentGameRules.CanMinibossSpawn ? _offOnOptions[1] : _offOnOptions[0];

            if (CurrentGameRules.BiomeTypes[_biomeButton.Text])
            {
                _biomeMultiSelector.PlayActivatedOnOptionSelect();
            }

            _bossButton.Text = CurrentGameRules.CanBossSpawn ? _offOnOptions[1] : _offOnOptions[0];
        }
    }
}
