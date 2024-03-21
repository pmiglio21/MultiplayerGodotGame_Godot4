using Enums;
using Enums.GameRules;
using Globals;
using Godot;
using Levels.OverworldLevels;
using Levels.UtilityLevels.UserInterfaceComponents;
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

		public override void _Ready()
		{
			_splitScreenOptionSelector = GetNode<OptionSelector>("SplitScreenOptionSelector");
			_returnButton = GetNode<Button>("ReturnButton");


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
			}

			if (UniversalInputHelper.IsActionJustPressed(InputType.MoveNorth))
			{
				if (_returnButton.HasFocus())
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

			GetTree().ChangeSceneToFile(LevelScenePaths.TitleLevelPath);
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
