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
		private OptionSelector _biomeTypeSelector;
		private OptionSelector _relativePlayerSpawnDistanceSelector;
		private NumberSpinner _numberSpinner;
		private OptionSelector _levelSizeSelector;
		private Button _returnButton;
		private Button _continueButton;

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
			_biomeTypeSelector = GetNode<OptionSelector>("BiomeTypeOptionSelector");
			_relativePlayerSpawnDistanceSelector = GetNode<OptionSelector>("RelativePlayerSpawnDistanceOptionSelector");
			_numberSpinner = GetNode<NumberSpinner>("NumberSpinner");
			_levelSizeSelector = GetNode<OptionSelector>("LevelSizeOptionSelector");
			_returnButton = GetNode<Button>("ReturnButton");
			_continueButton = GetNode<Button>("ContinueButton");

			_biomeTypeSelector.GetOptionButton().GrabFocus();
		}

		public override void _Process(double delta)
		{
			if (_rootSceneSwapper.PriorSceneName == ScreenNames.PlayMode)
			{
				_continueButton.Show();
			}
			else
			{
				_continueButton.Hide();
			}

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
				else if (_continueButton.HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);

					SaveOutGameRules();

					EmitSignal(SignalName.GoToPlayerCharacterSelectScreen);
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
			}

			if (UniversalInputHelper.IsActionJustPressed(InputType.EastButton))
			{
				ReturnToPriorScene();
			}
		}

		private void GetNavigationInput()
		{
			if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveSouth) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadSouth)))
			{
				if (_biomeTypeSelector.GetOptionButton().HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

					_relativePlayerSpawnDistanceSelector.GetOptionButton().GrabFocus();
				}
				else if (_relativePlayerSpawnDistanceSelector.GetOptionButton().HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

					_levelSizeSelector.GetOptionButton().GrabFocus();
				}
				else if (_levelSizeSelector.GetOptionButton().HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

					_numberSpinner.GetNumberSpinnerButton().GrabFocus();
				}
				else if (_numberSpinner.GetNumberSpinnerButton().HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

					_returnButton.GrabFocus();
				}
				else if (_returnButton.HasFocus() && _continueButton.Visible)
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

					_continueButton.GrabFocus();
				}

				_inputTimer.Start();
			}
			else if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveNorth) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadNorth)))
			{
				if (_continueButton.HasFocus() && _continueButton.Visible)
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

					_returnButton.GrabFocus();
				}
				else if (_returnButton.HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

					_numberSpinner.GetNumberSpinnerButton().GrabFocus();
				}
				else if (_numberSpinner.GetNumberSpinnerButton().HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

					_levelSizeSelector.GetOptionButton().GrabFocus();
				}
				else if (_levelSizeSelector.GetOptionButton().HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

					_relativePlayerSpawnDistanceSelector.GetOptionButton().GrabFocus();
				}
				else if (_relativePlayerSpawnDistanceSelector.GetOptionButton().HasFocus())
				{
					_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

					_biomeTypeSelector.GetOptionButton().GrabFocus();
				}

				_inputTimer.Start();
			}
		}

		public void GrabFocusOfTopButton()
		{
			_biomeTypeSelector.GetOptionButton().GrabFocus();
		}

		private void ReturnToPriorScene()
		{
			SaveOutGameRules();

			if (_rootSceneSwapper.PriorSceneName == ScreenNames.Title)
			{
				EmitSignal(SignalName.GoToTitleScreen);
			}
			else if (_rootSceneSwapper.PriorSceneName == ScreenNames.PlayMode)
			{
				EmitSignal(SignalName.GoToPlayModeScreen);
			}
		}

		private void SaveOutGameRules()
		{
			foreach (var enumValue in Enum.GetValues(typeof(BiomeType)))
			{
				var enumDescription = UniversalEnumHelper.GetEnumDescription(enumValue);

				if (enumDescription != "None" && enumDescription == _biomeTypeSelector.GetOptionButton().Text)
				{
					CurrentGameRules.BiomeType = (BiomeType)enumValue;
				}
			}

			foreach (var enumValue in Enum.GetValues(typeof(RelativePlayerSpawnDistanceType)))
			{
				var enumDescription = UniversalEnumHelper.GetEnumDescription(enumValue);

				if (enumDescription != "None" && enumDescription == _relativePlayerSpawnDistanceSelector.GetOptionButton().Text)
				{
					CurrentGameRules.CurrentRelativePlayerSpawnDistanceType = (RelativePlayerSpawnDistanceType)enumValue;
				}
			}

			CurrentGameRules.NumberOfLevels = _numberSpinner.GetNumberSpinnerButton().Text;

			foreach (var enumValue in Enum.GetValues(typeof(LevelSize)))
			{
				var enumDescription = UniversalEnumHelper.GetEnumDescription(enumValue);

				if (enumDescription != "None" && enumDescription == _levelSizeSelector.GetOptionButton().Text)
				{
					CurrentGameRules.CurrentLevelSize = (LevelSize)enumValue;
				}
			}

			_rootSceneSwapper.CurrentGameRules = CurrentGameRules;
		}
	}
}
