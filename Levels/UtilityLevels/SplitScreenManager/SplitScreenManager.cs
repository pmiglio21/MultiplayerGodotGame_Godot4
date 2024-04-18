using Globals;
using Globals.PlayerManagement;
using Godot;
using MobileEntities.PlayerCharacters.Scripts;
using Enums.GameRules;
using System;
using System.Collections.Generic;

namespace Levels.UtilityLevels.UserInterfaceComponents
{
	public partial class SplitScreenManager : CanvasLayer
	{

		private List<Camera2D> _subViewportCameras = new List<Camera2D>();
		private Node _level = null;

		public override void _Ready()
		{
			GlobalGameComponents.AvailableSubViewports = GetSubViewports();
			_level = GlobalGameComponents.AvailableSubViewports[0].GetNode("Level");
			SetSubViewportWorlds();

			if (CurrentSaveGameRules.CurrentSplitScreenMergingType == SplitScreenMergingType.ScreenPerPlayer ||
				(PlayerManager.ActivePlayers.Count == 1))
			{
				SetCamerasToPlayers();
			}

			if (CurrentSaveGameRules.CurrentSplitScreenMergingType == SplitScreenMergingType.ScreenPerPlayer)
			{
				AdjustScreenPerPlayerCameraView();
			}
			else 
			{
				AdjustSharedScreenCameraView();
			}
		}

		public override void _Process(double delta)
		{
		}

		private List<SubViewport> GetSubViewports()
		{
			GridContainer gridContainer = GetNode<GridContainer>("GridContainer");

			List<SubViewport> subViewports = new List<SubViewport>();

			foreach (var child in gridContainer.GetChildren())
			{
				if (child is SubViewportContainer)
				{
					SubViewport subViewport = child.GetNode("SubViewport") as SubViewport;

					subViewports.Add(subViewport);

					_subViewportCameras.Add(subViewport.GetNode<Camera2D>("Camera2D"));
				}
			}

			return subViewports;
		}

		private void SetSubViewportWorlds()
		{
			foreach (SubViewport subViewport in GlobalGameComponents.AvailableSubViewports)
			{
				subViewport.World2D = GlobalGameComponents.AvailableSubViewports[0].World2D;
			}
		}

		private void SetCamerasToPlayers()
		{
			int playerCount = 0;

			foreach (BaseCharacter player in PlayerManager.ActivePlayers)
			{
				player.playerCamera = _subViewportCameras[playerCount];

				//GD.Print($"Setting camera for {player.PlayerNumber}");

				playerCount++;
			}
		}

		private void AdjustScreenPerPlayerCameraView()
		{
			Vector2I mainViewportSize = GetWindow().Size;

			if (PlayerManager.ActivePlayers.Count == 1)
			{
				GlobalGameComponents.AvailableSubViewports[0].Size = mainViewportSize;
			}
			else if (PlayerManager.ActivePlayers.Count == 2)
			{
				GlobalGameComponents.AvailableSubViewports[0].Size = new Vector2I((mainViewportSize.X / 2), mainViewportSize.Y);
				GlobalGameComponents.AvailableSubViewports[1].Size = new Vector2I((mainViewportSize.X / 2), mainViewportSize.Y);
			}
			else if (PlayerManager.ActivePlayers.Count == 3)
			{
				GlobalGameComponents.AvailableSubViewports[0].Size = new Vector2I((mainViewportSize.X / 2), (mainViewportSize.Y / 2));
				GlobalGameComponents.AvailableSubViewports[1].Size = new Vector2I((mainViewportSize.X / 2), (mainViewportSize.Y / 2));
				GlobalGameComponents.AvailableSubViewports[2].Size = new Vector2I((mainViewportSize.X / 2), (mainViewportSize.Y / 2));
				GlobalGameComponents.AvailableSubViewports[3].Size = new Vector2I(0, 0);
			}
			else if (PlayerManager.ActivePlayers.Count == 4)
			{
				GlobalGameComponents.AvailableSubViewports[0].Size = new Vector2I((mainViewportSize.X / 2), (mainViewportSize.Y / 2));
				GlobalGameComponents.AvailableSubViewports[1].Size = new Vector2I((mainViewportSize.X / 2), (mainViewportSize.Y / 2));
				GlobalGameComponents.AvailableSubViewports[2].Size = new Vector2I((mainViewportSize.X / 2), (mainViewportSize.Y / 2));
				GlobalGameComponents.AvailableSubViewports[3].Size = new Vector2I((mainViewportSize.X / 2), (mainViewportSize.Y / 2));
			}

			GD.Print($"Size per {GlobalGameComponents.AvailableSubViewports[0].Size}");
		}

		private void AdjustSharedScreenCameraView()
		{
			Vector2I mainViewportSize = GetWindow().Size;

			GlobalGameComponents.AvailableSubViewports[0].Size = mainViewportSize;
			GlobalGameComponents.AvailableSubViewports[1].Size = new Vector2I(0, 0);
			GlobalGameComponents.AvailableSubViewports[2].Size = new Vector2I(0, 0);
			GlobalGameComponents.AvailableSubViewports[3].Size = new Vector2I(0, 0);

			GD.Print($"Size shared {GlobalGameComponents.AvailableSubViewports[0].Size}");
		}
	}
}
