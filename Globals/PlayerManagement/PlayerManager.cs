using MobileEntities.PlayerCharacters.Scripts;
using System.Collections.Generic;

namespace Globals.PlayerManagement
{
	public static class PlayerManager
	{
		public static List<BaseCharacter> ActivePlayers = new List<BaseCharacter>();

		#region Player Selection Properties

		public static List<string> AvailablePlayerSceneOptions = new List<string>()
		{
            "res://MobileEntities/PlayerCharacters/Knight/Knight.tscn",
			"res://MobileEntities/PlayerCharacters/Mage/Mage.tscn",
			"res://MobileEntities/PlayerCharacters/Rogue/Rogue.tscn",
			"res://MobileEntities/PlayerCharacters/Cleric/Cleric.tscn"
		};

		public static string DefaultPickerImageOption = "res://Levels/EarlyLevels/PlayerSelectScreen/PlayerCharacterPicker/Animations/BaseCharacter_PickerIcon.png";

		public static List<string> AvailablePlayerImageOptions = new List<string>()
		{
			"res://Levels/EarlyLevels/PlayerSelectScreen/PlayerCharacterPicker/Animations/TreeFrog_PickerIcon.png",
			"res://Levels/EarlyLevels/PlayerSelectScreen/PlayerCharacterPicker/Animations/BullFrog_PickerIcon.png",
			"res://Levels/EarlyLevels/PlayerSelectScreen/PlayerCharacterPicker/Animations/ChorusFrog_PickerIcon.png",
			"res://Levels/EarlyLevels/PlayerSelectScreen/PlayerCharacterPicker/Animations/WoodFrog_PickerIcon.png"
		};

		#endregion
	}
}
