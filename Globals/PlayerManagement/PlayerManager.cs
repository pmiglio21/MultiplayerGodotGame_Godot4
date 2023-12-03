using MobileEntities.PlayerCharacters.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Globals.PlayerManagement
{
	public static class PlayerManager
	{
		public static List<BaseCharacter> ActivePlayers = new List<BaseCharacter>();

		#region Player Selection Properties

		public static List<string> AvailablePlayerSceneOptions = new List<string>()
		{
			"res://MobileEntities/PlayerCharacters/TreeFrog/Scenes/TreeFrog.tscn",
			"res://MobileEntities/PlayerCharacters/BullFrog/Scenes/BullFrog.tscn",
			"res://MobileEntities/PlayerCharacters/ChorusFrog/Scenes/ChorusFrog.tscn",
			"res://MobileEntities/PlayerCharacters/WoodFrog/Scenes/WoodFrog.tscn"
		};

		public static string DefaultPickerImageOption = "res://Levels/EarlyLevels/PlayerSelectScene/PlayerCharacterPicker/Animations/BaseCharacter_PickerIcon.png";

		public static List<string> AvailablePlayerImageOptions = new List<string>()
		{
			"res://Levels/EarlyLevels/PlayerSelectScene/PlayerCharacterPicker/Animations/TreeFrog_PickerIcon.png",
			"res://Levels/EarlyLevels/PlayerSelectScene/PlayerCharacterPicker/Animations/BullFrog_PickerIcon.png",
			"res://Levels/EarlyLevels/PlayerSelectScene/PlayerCharacterPicker/Animations/ChorusFrog_PickerIcon.png",
			"res://Levels/EarlyLevels/PlayerSelectScene/PlayerCharacterPicker/Animations/WoodFrog_PickerIcon.png"
		};

		#endregion

		public static void ClearActivePlayers()
		{
			ActivePlayers.Clear();
		}
	}
}
