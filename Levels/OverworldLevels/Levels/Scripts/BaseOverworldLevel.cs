using Globals.PlayerManagement;
using Godot;
using MobileEntities.PlayerCharacters.Scripts;
using MultiplayerGodotGameGodot4.Levels.OverworldLevels.TileMapping;
using Scenes.UI.PlayerSelectScene;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BaseOverworldLevel : Node
{
	#region TileMap Level Generation
	
	private List<Vector2I> _floorTileList = new List<Vector2I>();
	
	private RandomNumberGenerator _rng = new RandomNumberGenerator();

	private List<TileMapFloorGridSpace> _existingFloorGridSpaces = new List<TileMapFloorGridSpace>();

	PackedScene scene = GD.Load<PackedScene>("res://Levels/OverworldLevels/TileMapping/InteriorWalls/InteriorWallBlock.tscn");

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
		//If singleplayer
		else
        {

        }

        //

        //GenerateKeyMapItems();

        SpawnPlayers();
		
	}

	private void GenerateInteriorBlocksOnAllFloorTiles()
	{
		foreach (var tilePosition in _floorTileList)
		{
			//Generate InteriorWallBlock at that position
			var tempBlock = scene.Instantiate();
			var interiorBlock = tempBlock as Node2D;
			AddChild(interiorBlock);

			var positionInLevel = interiorBlock.ToGlobal(_tileMap.MapToLocal(new Vector2I(tilePosition.X, tilePosition.Y)));
			interiorBlock.GlobalPosition = new Vector2(positionInLevel.X, positionInLevel.Y);

			_existingFloorGridSpaces.Add(new TileMapFloorGridSpace() { InteriorBlock = interiorBlock });
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

				GD.Print($"Spawn Point: {_tileMap.LocalToMap(floorGridSpaceWithMatchingPosition.InteriorBlock.GlobalPosition).X}, {_tileMap.LocalToMap(floorGridSpaceWithMatchingPosition.InteriorBlock.GlobalPosition).Y}");

				//Clear out spawn point areas
				foreach (var currentFloorGridSpace in _existingFloorGridSpaces)
				{
					if (floorGridSpaceWithMatchingPosition.InteriorBlock.GlobalPosition.DistanceTo(currentFloorGridSpace.InteriorBlock.GlobalPosition) <= TileMappingMagicNumbers.DiagonalDistanceBetweenInteriorBlocks)
					{
						currentFloorGridSpace.InteriorBlock.QueueFree();
					}
				}
			}
		}
	}

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

			GD.Print("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

			//GD.Print($"Angle: {angleFromWalkingFloorSpaceToTargetSpawnPoint}");

			var weightedXValue = 0;
			var weightedYValue = 0;

			if (angleFromWalkingFloorSpaceToTargetSpawnPoint > 0 && angleFromWalkingFloorSpaceToTargetSpawnPoint <= (Math.PI/2f))
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

			//GD.Print($"Weighted values: {weightedXValue}, {weightedYValue}");

			var changeInX = 0;
			var changeInY = 0;
			
			//GD.Print($"Adjusting spawn: {_tileMap.LocalToMap(startingSpawnPoint.InteriorBlock.GlobalPosition).X}, {_tileMap.LocalToMap(startingSpawnPoint.InteriorBlock.GlobalPosition).Y}");

			//Do this until the walkingFloorSpace is no longer on the starting spawnPoint
			while (walkingFloorSpace == startingSpawnPoint)
            {
				changeInX = RandomlySetChangeInDirection(weightedXValue);
				changeInY = RandomlySetChangeInDirection(changeInY);

				//GD.Print($"Change in direction: {changeInX}, {changeInY}");

				var newPosition = new Vector2I(_tileMap.LocalToMap(walkingFloorSpace.InteriorBlock.GlobalPosition).X + changeInX, _tileMap.LocalToMap(walkingFloorSpace.InteriorBlock.GlobalPosition).Y + changeInY);

				//GD.Print($"New Position: {newPosition.X}, {newPosition.Y}");

				//Set walkingFloorSpace to somewhere adjacent to spawn
				if (_existingFloorGridSpaces.Any(x => _tileMap.LocalToMap(x.InteriorBlock.GlobalPosition) == _tileMap.LocalToMap(newPosition)))
				{
					var floorGridSpaceWithMatchingPosition = _existingFloorGridSpaces.FirstOrDefault(x => _tileMap.LocalToMap(x.InteriorBlock.GlobalPosition) == newPosition);

					if (floorGridSpaceWithMatchingPosition != startingSpawnPoint)
                    {
						if (!floorGridSpaceWithMatchingPosition.InteriorBlock.IsQueuedForDeletion())
                        {
							floorGridSpaceWithMatchingPosition.InteriorBlock.QueueFree();
						}

						walkingFloorSpace = floorGridSpaceWithMatchingPosition;

						//GD.Print($"Matching Position in Vector2I: {_tileMap.LocalToMap(floorGridSpaceWithMatchingPosition.InteriorBlock.GlobalPosition).X}, {_tileMap.LocalToMap(floorGridSpaceWithMatchingPosition.InteriorBlock.GlobalPosition).Y}");

						//GD.Print($"Matching Position in Vector2: {floorGridSpaceWithMatchingPosition.InteriorBlock.GlobalPosition.X}, {floorGridSpaceWithMatchingPosition.InteriorBlock.GlobalPosition.Y}");
					}
				}
			}

			GD.Print("Off of Spawn Point");

			int count = 0;

            //Do this until the walkingFloorSpace is no longer on the starting spawnPoint
            while (walkingFloorSpace != targetSpawnPoint )
            {
                changeInX = RandomlySetChangeInDirection(weightedXValue);
                changeInY = RandomlySetChangeInDirection(weightedYValue);

                GD.Print($"Change in direction: {changeInX}, {changeInY}");

                var newPosition = new Vector2I(_tileMap.LocalToMap(walkingFloorSpace.InteriorBlock.GlobalPosition).X + changeInX, _tileMap.LocalToMap(walkingFloorSpace.InteriorBlock.GlobalPosition).Y + changeInY);

                GD.Print($"New Position: {newPosition.X}, {newPosition.Y}");

                //Set walkingFloorSpace to somewhere adjacent to spawn
                if (_existingFloorGridSpaces.Any(x => _tileMap.LocalToMap(x.InteriorBlock.GlobalPosition) == _tileMap.LocalToMap(newPosition)))
                {
                    var floorGridSpaceWithMatchingPosition = _existingFloorGridSpaces.FirstOrDefault(x => _tileMap.LocalToMap(x.InteriorBlock.GlobalPosition) == newPosition);

                    if (floorGridSpaceWithMatchingPosition != null && floorGridSpaceWithMatchingPosition != startingSpawnPoint)
                    {
                        if (!floorGridSpaceWithMatchingPosition.InteriorBlock.IsQueuedForDeletion())
                        {
                            floorGridSpaceWithMatchingPosition.InteriorBlock.QueueFree();
                        }

                        walkingFloorSpace = floorGridSpaceWithMatchingPosition;

                        GD.Print($"Matching Position in Vector2I: {_tileMap.LocalToMap(floorGridSpaceWithMatchingPosition.InteriorBlock.GlobalPosition).X}, {_tileMap.LocalToMap(floorGridSpaceWithMatchingPosition.InteriorBlock.GlobalPosition).Y}");

                        GD.Print($"Matching Position in Vector2: {floorGridSpaceWithMatchingPosition.InteriorBlock.GlobalPosition.X}, {floorGridSpaceWithMatchingPosition.InteriorBlock.GlobalPosition.Y}");

                        if (floorGridSpaceWithMatchingPosition.InteriorBlock.GlobalPosition.DistanceTo(targetSpawnPoint.InteriorBlock.GlobalPosition) <= 100)//)
                        {
                            break;
                        }
                    }
                }

				count++;
			}
        }
	}

    private int RandomlySetChangeInDirection(int weightedValue = 1)
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
