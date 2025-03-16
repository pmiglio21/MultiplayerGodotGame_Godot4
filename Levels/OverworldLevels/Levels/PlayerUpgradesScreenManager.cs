using Godot;
using Levels.UtilityLevels.UserInterfaceComponents;
using System;

namespace Levels.OverworldLevels
{
    public partial class PlayerUpgradesScreenManager : Control
    {
        private DungeonLevelSwapper _parentDungeonLevelSwapper;

        private RichTextLabel _player1StatsText;
        private RichTextLabel _player2StatsText;
        private RichTextLabel _player3StatsText;
        private RichTextLabel _player4StatsText;

        public override void _Ready()
        {
            _parentDungeonLevelSwapper = GetParent() as DungeonLevelSwapper;

            _player1StatsText = GetNode<RichTextLabel>("Player1Stats");
            _player2StatsText = GetNode<RichTextLabel>("Player2Stats");
            _player3StatsText = GetNode<RichTextLabel>("Player3Stats");
            _player4StatsText = GetNode<RichTextLabel>("Player4Stats");

            if (_parentDungeonLevelSwapper.ActivePlayers.Count > 0)
            {
                _player1StatsText.Text = $"Player 1\n" +
                                         $"Health: {_parentDungeonLevelSwapper.ActivePlayers[0].CharacterStats.Health}\n" +
                                         $"Attack: {_parentDungeonLevelSwapper.ActivePlayers[0].CharacterStats.Attack}\n" +
                                         $"Defense: {_parentDungeonLevelSwapper.ActivePlayers[0].CharacterStats.Defense}\n" +
                                         $"Speed: {_parentDungeonLevelSwapper.ActivePlayers[0].CharacterStats.Speed}";
            }
            
            if (_parentDungeonLevelSwapper.ActivePlayers.Count > 1)
            {
                _player2StatsText.Text = $"Player 2\n" +
                                         $"Health: {_parentDungeonLevelSwapper.ActivePlayers[1].CharacterStats.Health}\n" +
                                         $"Attack: {_parentDungeonLevelSwapper.ActivePlayers[1].CharacterStats.Attack}\n" +
                                         $"Defense: {_parentDungeonLevelSwapper.ActivePlayers[1].CharacterStats.Defense}\n" +
                                         $"Speed: {_parentDungeonLevelSwapper.ActivePlayers[1].CharacterStats.Speed}";
            }
            else
            {
                _player2StatsText.Hide();
            }
            
            if (_parentDungeonLevelSwapper.ActivePlayers.Count > 2)
            {
                _player3StatsText.Text = $"Player 3\n" +
                                         $"Health: {_parentDungeonLevelSwapper.ActivePlayers[2].CharacterStats.Health}\n" +
                                         $"Attack: {_parentDungeonLevelSwapper.ActivePlayers[2].CharacterStats.Attack}\n" +
                                         $"Defense: {_parentDungeonLevelSwapper.ActivePlayers[2].CharacterStats.Defense}\n" +
                                         $"Speed: {_parentDungeonLevelSwapper.ActivePlayers[2].CharacterStats.Speed}";
            }
            else
            {
                _player3StatsText.Hide();
            }

            if (_parentDungeonLevelSwapper.ActivePlayers.Count > 3)
            {
                _player4StatsText.Text = $"Player 4\n" +
                                         $"Health: {_parentDungeonLevelSwapper.ActivePlayers[3].CharacterStats.Health}\n" +
                                         $"Attack: {_parentDungeonLevelSwapper.ActivePlayers[3].CharacterStats.Attack}\n" +
                                         $"Defense: {_parentDungeonLevelSwapper.ActivePlayers[3].CharacterStats.Defense}\n" +
                                         $"Speed: {_parentDungeonLevelSwapper.ActivePlayers[3].CharacterStats.Speed}";
            }
            else
            {
                _player4StatsText.Hide();
            }
        }
            
        public override void _Process(double delta)
        {
        }
    }
}
																   