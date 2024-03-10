using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enums;
using Globals;
using Godot;

namespace MultiplayerGodotGameGodot4.Levels.EarlyLevels
{
	public partial class PlayerSelectSceneManager : Node2D
	{
		private Button _settingsButton;

		public override void _Ready()
		{
			_settingsButton = GetNode<Button>("SettingsButton");
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
				if (_settingsButton.HasFocus())
				{
					GlobalGameProperties.CurrentGameType = GameType.LocalCoop;
					GetTree().ChangeSceneToFile(LevelScenePaths.SettingsScreenPath);
					GlobalGameProperties.PriorScene = LevelScenePaths.PlayerSelectLevelPath;
				}
			}
		}

		private void GetNavigationInput()
		{
			if (UniversalInputHelper.IsActionJustPressed(InputType.MoveSouth))
			{
				//if (_competitiveGameButton.HasFocus())
				//{
					_settingsButton.GrabFocus();
				//}
			}
		}
	}
}
