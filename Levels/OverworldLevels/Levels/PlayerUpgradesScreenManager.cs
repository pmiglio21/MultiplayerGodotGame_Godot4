using Godot;
using Levels.UtilityLevels.UserInterfaceComponents;
using System;

namespace Levels.OverworldLevels
{
    public partial class PlayerUpgradesScreenManager : Control
    {
        private RichTextLabel _player1StatsText;
        private RichTextLabel _player2StatsText;
        private RichTextLabel _player3StatsText;
        private RichTextLabel _player4StatsText;

        public override void _Ready()
        {
            _player1StatsText = GetNode<RichTextLabel>("Player1Stats");
            _player2StatsText = GetNode<RichTextLabel>("Player2Stats");
            _player3StatsText = GetNode<RichTextLabel>("Player3Stats");
            _player4StatsText = GetNode<RichTextLabel>("Player4Stats");
        }

        public override void _Process(double delta)
        {
        }
    }
}
																   