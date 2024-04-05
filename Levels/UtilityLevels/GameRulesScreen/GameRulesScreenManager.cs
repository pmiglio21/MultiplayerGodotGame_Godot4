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
	public partial class GameRulesScreenManager : Node2D
	{
		protected PauseScreenManager pauseScreen;

		private OptionSelector _splitScreenOptionSelector;
		private Button _returnButton;
		private Button _continueButton;

		public override void _Ready()
		{
			_splitScreenOptionSelector = GetNode<OptionSelector>("SplitScreenOptionSelector");
			_returnButton = GetNode<Button>("ReturnButton");
			_continueButton = GetNode<Button>("ContinueButton");

			if (GlobalGameComponents.PriorSceneName == LevelScenePaths.PlayerSelectLevelPath)
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
			GetButtonInput();

			GetNavigationInput();
		}

		private void GetButtonInput()
		{
			if (UniversalInputHelper.IsActionJustPressed(InputType.StartButton) || UniversalInputHelper.IsActionJustPressed(InputType.SouthButton))
			{
				if (_splitScreenOptionSelector.GetOptionButton().HasFocus())
				{
					//GlobalGameProperties.CurrentGameType = GameType.LocalCompetitive;
					//GetTree().ChangeSceneToFile(LevelScenePaths.PlayerSelectLevelPath);
				}
				else if (_returnButton.HasFocus())
				{
					ReturnToPriorScene();
				}
				else if (_continueButton.HasFocus())
				{
					GetTree().ChangeSceneToFile(LevelScenePaths.OverworldLevel1Path);
				}
			}

			if (UniversalInputHelper.IsActionJustPressed(InputType.EastButton))
			{
				ReturnToPriorScene();
			}
		}

		private void GetNavigationInput()
		{
			if (UniversalInputHelper.IsActionJustPressed(InputType.MoveSouth))
			{
				if (_splitScreenOptionSelector.GetOptionButton().HasFocus())
				{
					_returnButton.GrabFocus();
				}
				else if (_returnButton.HasFocus() && _continueButton.Visible)
				{
					_continueButton.GrabFocus();
				}
			}

			if (UniversalInputHelper.IsActionJustPressed(InputType.MoveNorth))
			{
				if (_continueButton.HasFocus() && _continueButton.Visible)
				{
					_returnButton.GrabFocus();
				}
				else if (_returnButton.HasFocus())
				{
					_splitScreenOptionSelector.GetOptionButton().GrabFocus();
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
		}
	}
}
