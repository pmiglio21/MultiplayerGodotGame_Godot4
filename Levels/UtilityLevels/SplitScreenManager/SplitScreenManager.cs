using Godot;
using MobileEntities.PlayerCharacters.Scripts;
using Enums.GameRules;
using System.Collections.Generic;

namespace Levels.UtilityLevels.UserInterfaceComponents
{
	public partial class SplitScreenManager : GridContainer
	{
		private LevelHolder _parentDungeonLevelSwapper;

        private List<Camera2D> _subViewportCameras = new List<Camera2D>();
		private List<SubViewport> _availableSubViewports = new List<SubViewport>();
		private List<SubViewportContainer> _availableSubViewportContainers = new List<SubViewportContainer>();

		private bool hasSceneLoaded = false;

		public override void _Ready()
		{
            _parentDungeonLevelSwapper = GetParent() as LevelHolder;

            _availableSubViewports = GetSubViewports();
			_availableSubViewportContainers = GetSubViewportContainers();

			SetSubViewportWorlds();

			//Lets subviewports scale as the window changes size
			if (_parentDungeonLevelSwapper.CurrentGameRules.CurrentSplitScreenMergingType == SplitScreenMergingType.ScreenPerPlayer)
			{
				GetTree().Root.SizeChanged += AdjustScreenPerPlayerCameraView;
			}

			//Call it on its own to set cameras initially
			if (_parentDungeonLevelSwapper.CurrentGameRules.CurrentSplitScreenMergingType == SplitScreenMergingType.ScreenPerPlayer ||
				(_parentDungeonLevelSwapper.ActivePlayers.Count == 1))
			{
				SetCamerasToPlayers();
			}

			RunScreenAdjustmentProcess();
		}

		public override void _Process(double delta)
		{
			if (!hasSceneLoaded)
			{
				//Do this to load in scene and adjust the screen size right away.
				//Root.SizeChanged event doesn't happen after the scene loads, so we have to adjust the screen ourselves at the start of the scene.
				if (_parentDungeonLevelSwapper.CurrentGameRules.CurrentSplitScreenMergingType == SplitScreenMergingType.ScreenPerPlayer)
				{
					SetCamerasToPlayers();

					AdjustScreenPerPlayerCameraView();
				}

				hasSceneLoaded = true;
			}
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
			if (_parentDungeonLevelSwapper.CurrentGameRules.CurrentSplitScreenMergingType == SplitScreenMergingType.ScreenPerPlayer)
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
			foreach (SubViewport subViewport in _availableSubViewports)
			{
				subViewport.World2D = _availableSubViewports[0].World2D;
			}
		}

		private void SetCamerasToPlayers()
		{
			int playerCount = 0;

			foreach (BaseCharacter player in _parentDungeonLevelSwapper.ActivePlayers)
			{
				player.playerCamera = _subViewportCameras[playerCount];

				playerCount++;
			}
		}

		private void AdjustScreenPerPlayerCameraView()
		{
			Vector2I mainViewportSize = GetTree().Root.Size;

			if (_parentDungeonLevelSwapper.ActivePlayers.Count == 1)
			{
				//GridContainer only needs one column
				this.Columns = 1;
				
				//Remove all but the first SubviewportContainer
				for (int i = 1; i < _availableSubViewportContainers.Count; i++)
				{
					this.RemoveChild(_availableSubViewportContainers[i]);

					_availableSubViewportContainers.RemoveAt(i);
				}

				_availableSubViewportContainers[0].SizeFlagsHorizontal = SizeFlags.ShrinkCenter | SizeFlags.Expand;
				_availableSubViewportContainers[0].SizeFlagsVertical = SizeFlags.ShrinkCenter | SizeFlags.Expand;
				_availableSubViewports[0].Size = mainViewportSize;
			}
			else if (_parentDungeonLevelSwapper.ActivePlayers.Count == 2)
			{
				//Remove all but the first and second SubviewportContainer
				for (int i = 2; i < _availableSubViewportContainers.Count; i++)
				{
					this.RemoveChild(_availableSubViewportContainers[i]);

					_availableSubViewportContainers.RemoveAt(i);
				}

				_availableSubViewportContainers[0].SizeFlagsHorizontal = SizeFlags.ShrinkEnd | SizeFlags.Expand;
				_availableSubViewportContainers[0].SizeFlagsVertical = SizeFlags.ShrinkCenter | SizeFlags.Expand;
				_availableSubViewports[0].Size = new Vector2I(mainViewportSize.X / 2, mainViewportSize.Y);

				_availableSubViewportContainers[1].SizeFlagsHorizontal = SizeFlags.ShrinkBegin | SizeFlags.Expand;
				_availableSubViewportContainers[1].SizeFlagsVertical = SizeFlags.ShrinkCenter | SizeFlags.Expand;
				_availableSubViewports[1].Size = new Vector2I(mainViewportSize.X / 2, mainViewportSize.Y);
			}
			else if (_parentDungeonLevelSwapper.ActivePlayers.Count == 3)
			{
				_availableSubViewportContainers[0].SizeFlagsHorizontal = SizeFlags.ShrinkEnd | SizeFlags.Expand;
				_availableSubViewportContainers[0].SizeFlagsVertical = SizeFlags.ShrinkEnd | SizeFlags.Expand;
				_availableSubViewports[0].Size = new Vector2I(mainViewportSize.X / 2, mainViewportSize.Y / 2);

				_availableSubViewportContainers[1].SizeFlagsHorizontal = SizeFlags.ShrinkBegin | SizeFlags.Expand;
				_availableSubViewportContainers[1].SizeFlagsVertical = SizeFlags.ShrinkEnd | SizeFlags.Expand;
				_availableSubViewports[1].Size = new Vector2I(mainViewportSize.X / 2, mainViewportSize.Y / 2);

				_availableSubViewportContainers[2].SizeFlagsHorizontal = SizeFlags.ShrinkEnd | SizeFlags.Expand;
				_availableSubViewportContainers[2].SizeFlagsVertical = SizeFlags.ShrinkBegin | SizeFlags.Expand;
				_availableSubViewports[2].Size = new Vector2I(mainViewportSize.X / 2, mainViewportSize.Y / 2);
				
				_availableSubViewports[3].Size = Vector2I.Zero;
			}
			else if (_parentDungeonLevelSwapper.ActivePlayers.Count == 4)
			{
				_availableSubViewportContainers[0].SizeFlagsHorizontal = SizeFlags.ShrinkEnd | SizeFlags.Expand;
				_availableSubViewportContainers[0].SizeFlagsVertical = SizeFlags.ShrinkEnd | SizeFlags.Expand;
				_availableSubViewports[0].Size = new Vector2I(mainViewportSize.X / 2, mainViewportSize.Y / 2);
				
				_availableSubViewportContainers[1].SizeFlagsHorizontal = SizeFlags.ShrinkBegin | SizeFlags.Expand;
				_availableSubViewportContainers[1].SizeFlagsVertical = SizeFlags.ShrinkEnd | SizeFlags.Expand;
				_availableSubViewports[1].Size = new Vector2I(mainViewportSize.X / 2, mainViewportSize.Y / 2);

				_availableSubViewportContainers[2].SizeFlagsHorizontal = SizeFlags.ShrinkEnd | SizeFlags.Expand;
				_availableSubViewportContainers[2].SizeFlagsVertical = SizeFlags.ShrinkBegin | SizeFlags.Expand;
				_availableSubViewports[2].Size = new Vector2I(mainViewportSize.X / 2, mainViewportSize.Y / 2);

				_availableSubViewportContainers[3].SizeFlagsHorizontal = SizeFlags.ShrinkBegin | SizeFlags.Expand;
				_availableSubViewportContainers[3].SizeFlagsVertical = SizeFlags.ShrinkBegin | SizeFlags.Expand;
				_availableSubViewports[3].Size = new Vector2I(mainViewportSize.X / 2, mainViewportSize.Y / 2);
			}
		}

		private void AdjustSharedScreenCameraView()
		{
			Vector2I mainViewportSize = GetWindow().Size;

			_availableSubViewports[0].Size = mainViewportSize;
			_availableSubViewports[1].Size = new Vector2I(0, 0);
			_availableSubViewports[2].Size = new Vector2I(0, 0);
			_availableSubViewports[3].Size = new Vector2I(0, 0);

			GD.Print($"Size shared {_availableSubViewports[0].Size}");
		}
    }
}
