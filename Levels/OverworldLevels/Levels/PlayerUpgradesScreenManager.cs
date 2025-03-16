using Enums;
using Globals;
using Godot;
using Levels.UtilityLevels.UserInterfaceComponents;
using System;

namespace Levels.OverworldLevels
{
    public partial class PlayerUpgradesScreenManager : Control
    {
        [Signal]
        public delegate void GoToSplitScreenManagerEventHandler();

        #region Properties

        private DungeonLevelSwapper _parentDungeonLevelSwapper;

        private RichTextLabel _player1StatsText;
        private RichTextLabel _player2StatsText;
        private RichTextLabel _player3StatsText;
        private RichTextLabel _player4StatsText;

        #endregion

        public override void _Ready()
        {
            _parentDungeonLevelSwapper = GetParent() as DungeonLevelSwapper;

            _player1StatsText = GetNode<RichTextLabel>("Player1Stats");
            _player2StatsText = GetNode<RichTextLabel>("Player2Stats");
            _player3StatsText = GetNode<RichTextLabel>("Player3Stats");
            _player4StatsText = GetNode<RichTextLabel>("Player4Stats");

            UpgradeAndDisplayStats(0, _player1StatsText);

            UpgradeAndDisplayStats(1, _player2StatsText);

            UpgradeAndDisplayStats(2, _player3StatsText);

            UpgradeAndDisplayStats(3, _player4StatsText);
        }

        private void UpgradeAndDisplayStats(int playerNumber, RichTextLabel statsText)
        {
            if (_parentDungeonLevelSwapper.ActivePlayers.Count > playerNumber)
            {
                _parentDungeonLevelSwapper.ActivePlayers[playerNumber].CharacterStats.CalculateStatsOnLevelUp();

                statsText.Text = $"Player {playerNumber + 1}\n" +
                                         $"Health: {_parentDungeonLevelSwapper.ActivePlayers[playerNumber].CharacterStats.Health}\n" +
                                         $"Attack: {_parentDungeonLevelSwapper.ActivePlayers[playerNumber].CharacterStats.Attack}\n" +
                                         $"Defense: {_parentDungeonLevelSwapper.ActivePlayers[playerNumber].CharacterStats.Defense}\n" +
                                         $"Speed: {_parentDungeonLevelSwapper.ActivePlayers[playerNumber].CharacterStats.Speed}";
            }
            else
            {
                statsText.Hide();
            }
        }

        //TODO: Use this later when screen is more built out
        private void OnPlayerCharacterPicker_TellPlayerCharacterSelectScreenToGoToDungeonLevelSwapper()
        {
            EmitSignal(SignalName.GoToSplitScreenManager);
        }

        public override void _Process(double delta)
        {
            if (UniversalInputHelper.IsActionJustReleased(InputType.SouthButton) || UniversalInputHelper.IsActionJustReleased(InputType.StartButton))
            {
                EmitSignal(SignalName.GoToSplitScreenManager);
            }
        }
    }
}
																   