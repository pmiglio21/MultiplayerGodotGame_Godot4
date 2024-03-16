using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enums;
using Globals;
using Godot;
using Levels.EarlyLevels;

namespace MultiplayerGodotGameGodot4.Levels.EarlyLevels
{
	public partial class PlayerSelectSceneManager : Node2D
	{
		private Button _settingsButton;
		private SettingsScreenManager _settingsManager;

		public override void _Ready()
		{
			_settingsButton = GetNode<Button>("SettingsButton");
			_settingsManager = GetNode<SettingsScreenManager>("SettingsScreen");
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
					_settingsManager.IsSettingsScreenBeingShown = true;
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
