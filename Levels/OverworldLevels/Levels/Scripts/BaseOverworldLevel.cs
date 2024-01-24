using Globals.PlayerManagement;
using Godot;
using MobileEntities.PlayerCharacters.Scripts;
using MultiplayerGodotGameGodot4.Levels.OverworldLevels.TileMapping;
using Scenes.UI.PlayerSelectScene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class BaseOverworldLevel : Node
{
	#region TileMap Level Generation
	
	private List<Vector2I> _floorTileList = new List<Vector2I>();
	
	private RandomNumberGenerator _rng = new RandomNumberGenerator();

	private List<TileMapFloorGridSpace> _existingFloorGridSpaces = new List<TileMapFloorGridSpace>();

	private PackedScene _interiorBlockScene = GD.Load<PackedScene>("res://Levels/OverworldLevels/TileMapping/InteriorWalls/InteriorWallBlock.tscn");

	private PackedScene _interiorBlockTestTextScene = GD.Load<PackedScene>("res://Levels/OverworldLevels/TileMapping/InteriorWalls/InteriorBlockTestText.tscn");

	#endregion

	#region Component Nodes

	private Node2D _baseFloor = null;

	private TileMap _tileMap = null;

	#endregion

	public override void _Ready()
	{
		_baseFloor = GetNode<Node2D>("BaseFloor");

		_tileMap = _baseFloor.GetNode<TileMap>("TileMap");

		_floorTileList = _tileMap.GetUsedCellsById(0, TileMappingMagicNumbers.TileMapFloorSpriteId).ToList();

		 GenerateInteriorWallsForLevel();
	}

	private void GenerateInteriorWallsForLevel()
	{
		_rng.Randomize();

		GenerateInteriorBlocksOnAllFloorTiles();

		GenerateSpawnPoints();

		//If multiplayer
		if (_existingFloorGridSpaces.Count(x => x.IsSpawnPoint) > 1)
		{
			CreatePathsBetweenSpawnPoints();
		}
		//TODO: If singleplayer
		else
		{

		}

		float percentageOfFloorToCover = 0;

		switch (PlayerManager.ActivePlayers.Count)
        {
			case 1:
				percentageOfFloorToCover = .125f;
				break;
			case 2:
				percentageOfFloorToCover = .25f;
				break;
			case 3:
				percentageOfFloorToCover = .333f;
				break;
			case 4:
				percentageOfFloorToCover = .4f;
				break;
			default:
				percentageOfFloorToCover = .25f;
				break;
		}

		//TODO: Get this to work concurrently
		while (_existingFloorGridSpaces.Count(x => x.InteriorBlock.IsQueuedForDeletion()) < (percentageOfFloorToCover * _existingFloorGridSpaces.Count))
        {
			CreatePathsBetweenPoints();
		}

        GenerateKeyMapItems();

        SpawnPlayers();
	}

	#region Path Generation
	private void GenerateInteriorBlocksOnAllFloorTiles()
	{
		foreach (var tilePosition in _floorTileList)
		{
			//Generate InteriorWallBlock at that position
			var tempBlock = _interiorBlockScene.Instantiate();
			var interiorBlock = tempBlock as Node2D;
			AddChild(interiorBlock);

			var tempTestText = _interiorBlockTestTextScene.Instantiate();
			var testText = tempTestText as Node2D;
			AddChild(testText);

			var positionInLevel = interiorBlock.ToGlobal(_tileMap.MapToLocal(new Vector2I(tilePosition.X, tilePosition.Y)));
			interiorBlock.GlobalPosition = new Vector2(positionInLevel.X, positionInLevel.Y);
			testText.GlobalPosition = new Vector2(positionInLevel.X, positionInLevel.Y);

			_existingFloorGridSpaces.Add(new TileMapFloorGridSpace() { InteriorBlock = interiorBlock, TestText = testText });
		}
	}

	private void GenerateSpawnPoints()
	{
		for (int spawnPointGeneratedCount = 0; spawnPointGeneratedCount < PlayerManager.ActivePlayers.Count; spawnPointGeneratedCount++)
		{
			var floorTileIndex = (int)Math.Floor(_rng.RandfRange(0, _floorTileList.Count));

			var currentFloorTileGridMapPosition = _floorTileList[floorTileIndex];

			if (_existingFloorGridSpaces.Any(x => x.InteriorBlock.GlobalPosition == _tileMap.MapToLocal(currentFloorTileGridMapPosition)))
			{
				var floorGridSpaceWithMatchingPosition = _existingFloorGridSpaces.FirstOrDefault(x => x.InteriorBlock.GlobalPosition == _tileMap.MapToLocal(currentFloorTileGridMapPosition));

				floorGridSpaceWithMatchingPosition.IsSpawnPoint = true;

				floorGridSpaceWithMatchingPosition.InteriorBlock.QueueFree();
				floorGridSpaceWithMatchingPosition.NumberOfSpawnPointWhoClearedIt = spawnPointGeneratedCount;

				var richTextLabel = floorGridSpaceWithMatchingPosition.TestText.GetNode("RichTextLabel") as RichTextLabel;
				richTextLabel.Text = spawnPointGeneratedCount.ToString();

				//Clear out spawn point areas
				foreach (var currentFloorGridSpace in _existingFloorGridSpaces)
				{
					if (floorGridSpaceWithMatchingPosition.InteriorBlock.GlobalPosition.DistanceTo(currentFloorGridSpace.InteriorBlock.GlobalPosition) <= TileMappingMagicNumbers.DiagonalDistanceBetweenInteriorBlocks)
					{
						currentFloorGridSpace.InteriorBlock.QueueFree();
						currentFloorGridSpace.NumberOfSpawnPointWhoClearedIt = spawnPointGeneratedCount;

						var rtl = currentFloorGridSpace.TestText.GetNode("RichTextLabel") as RichTextLabel;
						rtl.Text = spawnPointGeneratedCount.ToString();
					}
				}
			}
		}
	}

	//TODO: Check if this method and the other one are similar enough to just make into one method
	private void CreatePathsBetweenSpawnPoints()
	{
		List<TileMapFloorGridSpace> spawnPoints = _existingFloorGridSpaces.Where(x => x.IsSpawnPoint).ToList();

		foreach (TileMapFloorGridSpace startingSpawnPoint in spawnPoints)
		{
			TileMapFloorGridSpace walkingFloorSpace = startingSpawnPoint;

			List<TileMapFloorGridSpace> availableSpawnPoints = spawnPoints.Where(x => x != startingSpawnPoint).ToList();

			var targetSpawnPoint = availableSpawnPoints[_rng.RandiRange(0, availableSpawnPoints.Count-1)];

			//For some reason, trig circle is flipped across its y axis... whatever...
			var angleFromWalkingFloorSpaceToTargetSpawnPoint = walkingFloorSpace.InteriorBlock.GlobalPosition.AngleToPoint(targetSpawnPoint.InteriorBlock.GlobalPosition);

			var weightedValues = GetWeightedValues(angleFromWalkingFloorSpaceToTargetSpawnPoint);

			var changeInX = 0;
			var changeInY = 0;

			int iterationCount = 0;

			//Do this until the walkingFloorSpace is no longer on the starting spawnPoint
			while (true)
			{
				if (iterationCount < TileMappingMagicNumbers.NumberOfIterationsBeforeChangingAngle)
                {
					angleFromWalkingFloorSpaceToTargetSpawnPoint = walkingFloorSpace.InteriorBlock.GlobalPosition.AngleToPoint(targetSpawnPoint.InteriorBlock.GlobalPosition);

					weightedValues = GetWeightedValues(angleFromWalkingFloorSpaceToTargetSpawnPoint);

					iterationCount = 0;
                }

				if (_rng.RandfRange(0, 1) <= .5)
				{
					changeInX = SetRandomChangeInDirection(weightedValues.Item1);

					if (changeInX == 0)
					{
						changeInY = SetRandomChangeInDirection(weightedValues.Item2);
					}
					else
					{
						changeInY = 0;
					}
				}
				else
				{
					changeInY = SetRandomChangeInDirection(weightedValues.Item2);

					if (changeInY == 0)
					{
						changeInX = SetRandomChangeInDirection(weightedValues.Item1);
					}
					else
					{
						changeInX = 0;
					}
				}

				var newPositionToCheck = new Vector2I(_tileMap.LocalToMap(walkingFloorSpace.InteriorBlock.GlobalPosition).X + changeInX, _tileMap.LocalToMap(walkingFloorSpace.InteriorBlock.GlobalPosition).Y + changeInY);

				//Set walkingFloorSpace to somewhere adjacent to spawn
				if (_existingFloorGridSpaces.Any(x => _tileMap.LocalToMap(x.InteriorBlock.GlobalPosition) == _tileMap.LocalToMap(newPositionToCheck)))
				{
					var floorGridSpaceWithMatchingPosition = _existingFloorGridSpaces.FirstOrDefault(x => _tileMap.LocalToMap(x.InteriorBlock.GlobalPosition) == newPositionToCheck);

					if (floorGridSpaceWithMatchingPosition != null && floorGridSpaceWithMatchingPosition != startingSpawnPoint)
					{
						if (!floorGridSpaceWithMatchingPosition.InteriorBlock.IsQueuedForDeletion())
						{
							floorGridSpaceWithMatchingPosition.InteriorBlock.QueueFree();
						}

						var numberOfSpawnPointWhoClearedMatchingFloorSpace = floorGridSpaceWithMatchingPosition.NumberOfSpawnPointWhoClearedIt;

						walkingFloorSpace = floorGridSpaceWithMatchingPosition;
						walkingFloorSpace.NumberOfSpawnPointWhoClearedIt = startingSpawnPoint.NumberOfSpawnPointWhoClearedIt;

						var rtl = walkingFloorSpace.TestText.GetNode("RichTextLabel") as RichTextLabel;
						rtl.Text = walkingFloorSpace.NumberOfSpawnPointWhoClearedIt.ToString();

						if (numberOfSpawnPointWhoClearedMatchingFloorSpace != -1 &&
							numberOfSpawnPointWhoClearedMatchingFloorSpace != startingSpawnPoint.NumberOfSpawnPointWhoClearedIt)
						{
							break;
						}
					}
				}

				iterationCount++;
			}
		}
	}

	private void CreatePathsBetweenPoints()
	{
		var availableStartSpaces = _existingFloorGridSpaces.Where(x => x.InteriorBlock.IsQueuedForDeletion()).ToList();

		TileMapFloorGridSpace startingPoint = availableStartSpaces[_rng.RandiRange(0, availableStartSpaces.Count - 1)];

		TileMapFloorGridSpace walkingFloorSpace = startingPoint;

		var availableTargetSpaces = _existingFloorGridSpaces.Where(x => !x.InteriorBlock.IsQueuedForDeletion()).ToList();

		var targetPoint = availableTargetSpaces[_rng.RandiRange(0, availableTargetSpaces.Count - 1)];

		//For some reason, trig circle is flipped across its y axis... whatever...
		var angleFromWalkingFloorSpaceToTargetPoint = walkingFloorSpace.InteriorBlock.GlobalPosition.AngleToPoint(targetPoint.InteriorBlock.GlobalPosition);

		var weightedValues = GetWeightedValues(angleFromWalkingFloorSpaceToTargetPoint);

		var changeInX = 0;
		var changeInY = 0;

		int iterationCount = 0;

		//Do this until the walkingFloorSpace is no longer on the starting spawnPoint
		while (iterationCount < TileMappingMagicNumbers.NumberOfIterationsBeforeChangingAngle)
		{
			if (_rng.RandfRange(0, 1) <= .5)
			{
				changeInX = SetRandomChangeInDirection(weightedValues.Item1);

				if (changeInX == 0)
				{
					changeInY = SetRandomChangeInDirection(weightedValues.Item2);
				}
				else
				{
					changeInY = 0;
				}
			}
			else
			{
				changeInY = SetRandomChangeInDirection(weightedValues.Item2);

				if (changeInY == 0)
				{
					changeInX = SetRandomChangeInDirection(weightedValues.Item1);
				}
				else
				{
					changeInX = 0;
				}
			}

			var newPosition = new Vector2I(_tileMap.LocalToMap(walkingFloorSpace.InteriorBlock.GlobalPosition).X + changeInX, _tileMap.LocalToMap(walkingFloorSpace.InteriorBlock.GlobalPosition).Y + changeInY);

			//Set walkingFloorSpace to somewhere adjacent to spawn
			if (_existingFloorGridSpaces.Any(x => _tileMap.LocalToMap(x.InteriorBlock.GlobalPosition) == _tileMap.LocalToMap(newPosition)))
			{
				var floorGridSpaceWithMatchingPosition = _existingFloorGridSpaces.FirstOrDefault(x => _tileMap.LocalToMap(x.InteriorBlock.GlobalPosition) == newPosition);

				if (floorGridSpaceWithMatchingPosition != null && floorGridSpaceWithMatchingPosition != startingPoint)
				{
					if (!floorGridSpaceWithMatchingPosition.InteriorBlock.IsQueuedForDeletion())
					{
						floorGridSpaceWithMatchingPosition.InteriorBlock.QueueFree();
					}

					var numberOfSpawnPointWhoClearedMatchingFloorSpace = floorGridSpaceWithMatchingPosition.NumberOfSpawnPointWhoClearedIt;

					walkingFloorSpace = floorGridSpaceWithMatchingPosition;
					walkingFloorSpace.NumberOfSpawnPointWhoClearedIt = 99;

					var rtl = walkingFloorSpace.TestText.GetNode("RichTextLabel") as RichTextLabel;
					rtl.Text = walkingFloorSpace.NumberOfSpawnPointWhoClearedIt.ToString();

					if (numberOfSpawnPointWhoClearedMatchingFloorSpace != -1 &&
						numberOfSpawnPointWhoClearedMatchingFloorSpace != 99)
					{
						break;
					}
				}
			}

			iterationCount++;
		}
	}

	#endregion

	#region Key Object Generation

	private void GenerateKeyMapItems()
	{
		GeneratePortal();

		GenerateSwitches();
	}

	private void GeneratePortal() { }

	private void GenerateSwitches() { }

	private void SpawnPlayers()
	{
		List<TileMapFloorGridSpace> spawnPoints = _existingFloorGridSpaces.Where(x => x.IsSpawnPoint).ToList();

		int playerCount = 0;

		foreach (BaseCharacter character in PlayerManager.ActivePlayers)
		{
			GD.Print($"P: {character.PlayerNumber}, D: {character.DeviceIdentifier}, C: {character.CharacterClassName}");

			character.GlobalPosition = spawnPoints[playerCount].InteriorBlock.GlobalPosition;

			AddChild(character);

			playerCount++;
		}

		PlayerCharacterPickerManager.ActivePickers.Clear();
	}

	#endregion

	#region Direction-Changing Utility Methods

	private int SetRandomChangeInDirection(int weightedValue = 1)
	{
		var randChance = _rng.RandfRange(0, 1);

		if (randChance < .25)
		{
			return -weightedValue;
		}
		else if (randChance >= .25 && randChance < .5)
		{
			return 0;
		}
		else
		{
			return weightedValue;
		}
	}

	private Tuple<int, int> GetWeightedValues(float angleFromWalkingFloorSpaceToTargetSpawnPoint)
    {
		var weightedXValue = 0;
		var weightedYValue = 0;

		if (angleFromWalkingFloorSpaceToTargetSpawnPoint > 0 && angleFromWalkingFloorSpaceToTargetSpawnPoint <= (Math.PI / 2f))
		{
			weightedXValue = 1;
			weightedYValue = 1;
		}
		else if (angleFromWalkingFloorSpaceToTargetSpawnPoint > (Math.PI / 2f) && angleFromWalkingFloorSpaceToTargetSpawnPoint <= Math.PI)
		{
			weightedXValue = -1;
			weightedYValue = 1;
		}
		else if (angleFromWalkingFloorSpaceToTargetSpawnPoint < 0 && angleFromWalkingFloorSpaceToTargetSpawnPoint >= -(Math.PI / 2f))
		{
			weightedXValue = 1;
			weightedYValue = -1;
		}
		else if (angleFromWalkingFloorSpaceToTargetSpawnPoint < -(Math.PI / 2f) && angleFromWalkingFloorSpaceToTargetSpawnPoint >= -Math.PI)
		{
			weightedXValue = -1;
			weightedYValue = -1;
		}
		else
		{
			weightedXValue = 1;
			weightedYValue = 1;
		}

		return new Tuple<int, int>(weightedXValue, weightedYValue); ;
	}

	#endregion
}






//private int _gridSize = 16;
//private Sprite2D _fog;

//CompressedTexture2D _lightTexture = GD.Load<CompressedTexture2D>("res://MobileEntities/PlayerCharacters/ComponentPieces/CharacterPointLight/CharacterPointLight.png");

//int _display_width = (int)ProjectSettings.GetSetting("display/window/size/viewport_width");
//int _display_height = (int)ProjectSettings.GetSetting("display/window/size/viewport_height");

//Image _fogImage = new Image();
//ImageTexture _fogTexture = new ImageTexture();

//byte[] _lightImage = null;
//Vector2I _lightOffset = Vector2I.Zero;

//public override void _Ready()
//{
//	_fog = GetNode<Sprite2D>("FogOfWar");

//	//_lightImage = _lightTexture.GetData();
//	_lightOffset = new Vector2I(_lightTexture.GetWidth()/2, _lightTexture.GetHeight() / 2);

//	//GD.Print("Display width: "+_display_width);
//	//GD.Print("Display height: " + _display_height);

//	var fog_image_width = _display_width / _gridSize;
//	var fog_image_height = _display_height / _gridSize;

//	_fogImage = Image.Create(fog_image_width, fog_image_height, false, Image.Format.Rgbah);
//	_fogImage.Fill(new Color("BLACK"));

//	//(_lightImage as Image).Convert(Image.Format.Rgbah);

//	_fog.Texture = ImageTexture.CreateFromImage(_fogImage);
//	_fog.Scale *= _gridSize;
//}

//private void UpdateFog(Vector2I newGridPosition)
//{
//	//var lightRectangle = new Rect2I(Vector2I.Zero, new Vector2I(_lightTexture.GetWidth(), _lightTexture.GetHeight()));
//	//_fogImage.BlendRect(_lightTexture, lightRectangle, newGridPosition - _lightOffset);
//}


// //  public override void _Process(double delta)
// //  {
//	//UpdateFog(GetLocalMousePosition()/_gridSize);
// //  }

//public override void _Input(InputEvent @event)
//{
//	// Mouse in viewport coordinates.
//	if (@event is InputEventMouseButton eventMouseButton)
//		GD.Print("Mouse Click/Unclick at: ", eventMouseButton.Position);
//	else if (@event is InputEventMouseMotion eventMouseMotion)
//	{
//		GD.Print("Mouse Motion at: ", eventMouseMotion.Position);
//		//UpdateFog(eventMouseMotion.Position / _gridSize);
//	}

//	// Print the size of the viewport.
//	GD.Print("Viewport Resolution is: ", GetViewport().GetVisibleRect().Size);


//}
