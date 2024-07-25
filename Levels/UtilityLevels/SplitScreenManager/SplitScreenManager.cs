using Globals;
using Globals.PlayerManagement;
using Godot;
using MobileEntities.PlayerCharacters.Scripts;
using Enums.GameRules;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Levels.UtilityLevels.UserInterfaceComponents
{
	public partial class SplitScreenManager : GridContainer
	{

		private List<Camera2D> _subViewportCameras = new List<Camera2D>();
		private Node _level = null;

		public override void _Ready()
		{
			GlobalGameComponents.AvailableSubViewports = GetSubViewports();
            GlobalGameComponents.AvailableSubViewportContainers = GetSubViewportContainers();

            _level = GlobalGameComponents.AvailableSubViewports[0].GetNode("Level");

            SetSubViewportWorlds();

            RunScreenAdjustmentProcess();

			//Lets subviewports scale as the window changes size
            GetTree().Root.SizeChanged += RunScreenAdjustmentProcess;
        }

		private List<SubViewport> GetSubViewports()
		{
			List<SubViewport> subViewports = new List<SubViewport>();

			int cameraCount = 1;

			while (cameraCount < 5)
			{
                SubViewport subViewport = FindChild($"SubViewport{cameraCount}") as SubViewport;

                subViewports.Add(subViewport);

                _subViewportCameras.Add(subViewport.GetNode<Camera2D>($"PlayerCamera{cameraCount}"));

                cameraCount++;
			}

			return subViewports;
		}

        private List<SubViewportContainer> GetSubViewportContainers()
        {
            List<SubViewportContainer> subViewportContainers = new List<SubViewportContainer>();

            int cameraCount = 1;

            while (cameraCount < 5)
            {
                SubViewportContainer subViewportContainer = FindChild($"SubViewportContainer{cameraCount}") as SubViewportContainer;

                subViewportContainers.Add(subViewportContainer);

                cameraCount++;
            }

            return subViewportContainers;
        }

        private void RunScreenAdjustmentProcess()
		{
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
            Vector2I mainViewportSize = (Vector2I)GetViewport().GetVisibleRect().Size;

			//ADJUST ANCHORS SITUATIONALLY!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            if (PlayerManager.ActivePlayers.Count == 1)
			{
				//MAYBE ADJUST VIEWPORT SIZE INSTEAD OF MATCHING SubViewport to Viewport Size??????????????????????????????????????????????????????
				GlobalGameComponents.AvailableSubViewportContainers[0].SizeFlagsHorizontal = SizeFlags.ShrinkEnd | SizeFlags.Expand;
				GlobalGameComponents.AvailableSubViewportContainers[0].SizeFlagsVertical = SizeFlags.ShrinkEnd | SizeFlags.Expand;
				GlobalGameComponents.AvailableSubViewports[0].Size = mainViewportSize;

                GlobalGameComponents.AvailableSubViewports[1].Size = Vector2I.Zero;
				GlobalGameComponents.AvailableSubViewports[2].Size = Vector2I.Zero;
				GlobalGameComponents.AvailableSubViewports[3].Size = Vector2I.Zero;
			}
			else if (PlayerManager.ActivePlayers.Count == 2)
			{
				GlobalGameComponents.AvailableSubViewportContainers[0].SizeFlagsHorizontal = SizeFlags.ShrinkEnd | SizeFlags.Expand;
                GlobalGameComponents.AvailableSubViewportContainers[0].SizeFlagsVertical = SizeFlags.Expand;
                GlobalGameComponents.AvailableSubViewports[0].Size = new Vector2I((mainViewportSize.X / 2), mainViewportSize.Y);

				GlobalGameComponents.AvailableSubViewportContainers[1].SizeFlagsHorizontal = SizeFlags.ShrinkBegin | SizeFlags.Expand;
                GlobalGameComponents.AvailableSubViewportContainers[1].SizeFlagsVertical = SizeFlags.Expand;
                GlobalGameComponents.AvailableSubViewports[1].Size = new Vector2I((mainViewportSize.X / 2), mainViewportSize.Y);
				
				GlobalGameComponents.AvailableSubViewports[2].Size = Vector2I.Zero;
				GlobalGameComponents.AvailableSubViewports[3].Size = Vector2I.Zero;
			}
			else if (PlayerManager.ActivePlayers.Count == 3)
			{
				GlobalGameComponents.AvailableSubViewportContainers[0].SizeFlagsHorizontal = SizeFlags.ShrinkEnd | SizeFlags.Expand;
				GlobalGameComponents.AvailableSubViewportContainers[0].SizeFlagsVertical = SizeFlags.ShrinkEnd | SizeFlags.Expand;
                GlobalGameComponents.AvailableSubViewports[0].Size = new Vector2I((mainViewportSize.X / 2), (mainViewportSize.Y / 2));

				GlobalGameComponents.AvailableSubViewportContainers[1].SizeFlagsHorizontal = SizeFlags.ShrinkBegin | SizeFlags.Expand;
				GlobalGameComponents.AvailableSubViewportContainers[1].SizeFlagsVertical = SizeFlags.ShrinkEnd | SizeFlags.Expand;
				GlobalGameComponents.AvailableSubViewports[1].Size = new Vector2I((mainViewportSize.X / 2), (mainViewportSize.Y / 2));

				GlobalGameComponents.AvailableSubViewportContainers[2].SizeFlagsHorizontal = SizeFlags.ShrinkEnd | SizeFlags.Expand;
				GlobalGameComponents.AvailableSubViewportContainers[2].SizeFlagsVertical = SizeFlags.ShrinkBegin | SizeFlags.Expand;
				GlobalGameComponents.AvailableSubViewports[2].Size = new Vector2I((mainViewportSize.X / 2), (mainViewportSize.Y / 2));
				
				GlobalGameComponents.AvailableSubViewports[3].Size = Vector2I.Zero;
			}
			else if (PlayerManager.ActivePlayers.Count == 4)
			{
                GlobalGameComponents.AvailableSubViewportContainers[0].SizeFlagsHorizontal = SizeFlags.ShrinkEnd | SizeFlags.Expand;
                GlobalGameComponents.AvailableSubViewportContainers[0].SizeFlagsVertical = SizeFlags.ShrinkEnd | SizeFlags.Expand;
                GlobalGameComponents.AvailableSubViewports[0].Size = new Vector2I((mainViewportSize.X / 2), (mainViewportSize.Y / 2));
                
				GlobalGameComponents.AvailableSubViewportContainers[1].SizeFlagsHorizontal = SizeFlags.ShrinkBegin | SizeFlags.Expand;
                GlobalGameComponents.AvailableSubViewportContainers[1].SizeFlagsVertical = SizeFlags.ShrinkEnd | SizeFlags.Expand;
                GlobalGameComponents.AvailableSubViewports[1].Size = new Vector2I((mainViewportSize.X / 2), (mainViewportSize.Y / 2));

                GlobalGameComponents.AvailableSubViewportContainers[2].SizeFlagsHorizontal = SizeFlags.ShrinkEnd | SizeFlags.Expand;
                GlobalGameComponents.AvailableSubViewportContainers[2].SizeFlagsVertical = SizeFlags.ShrinkBegin | SizeFlags.Expand;
                GlobalGameComponents.AvailableSubViewports[2].Size = new Vector2I((mainViewportSize.X / 2), (mainViewportSize.Y / 2));

                GlobalGameComponents.AvailableSubViewportContainers[3].SizeFlagsHorizontal = SizeFlags.ShrinkBegin | SizeFlags.Expand;
                GlobalGameComponents.AvailableSubViewportContainers[3].SizeFlagsVertical = SizeFlags.ShrinkBegin | SizeFlags.Expand;
                GlobalGameComponents.AvailableSubViewports[3].Size = new Vector2I((mainViewportSize.X / 2), (mainViewportSize.Y / 2));
			}
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
