using Enums;
using Globals;
using Godot;
using MobileEntities.PlayerCharacters.Scripts;
using Enums.GameRules;
using MultiplayerGodotGameGodot4.Levels.OverworldLevels.TileMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MobileEntities.Enemies.Scripts;
using Root;

public partial class BaseDungeonLevel : Node
{
	private DungeonLevelSwapper _parentDungeonLevelSwapper;

    #region Signals

    [Signal]
    public delegate void GoToGameOverScreenEventHandler();

    #endregion

    #region TileMap Level Generation

    private Dictionary<int,Vector2I> _possibleFloorPositionsByIndex = new Dictionary<int,Vector2I>();
    private Dictionary<Vector2I, int> _possibleIndicesByFloorPositions = new Dictionary<Vector2I, int>();
    private Dictionary<Vector2I, TileMapSpace> _possibleTileMapSpacesByFloorPosition = new Dictionary<Vector2I, TileMapSpace>();

    private RandomNumberGenerator _rng = new RandomNumberGenerator();

	private List<TileMapSpace> _existingFloorGridSpaces = new List<TileMapSpace>();

	private int _maxNumberOfTiles = 0;

	private PackedScene _interiorBlockScene = GD.Load<PackedScene>("res://Levels/OverworldLevels/TileMapping/InteriorWalls/InteriorWallBlock.tscn");

	private PackedScene _interiorBlockTestTextScene = GD.Load<PackedScene>("res://Levels/OverworldLevels/TileMapping/InteriorWalls/InteriorBlockTestText.tscn");

	#endregion

	#region Key Level Object Generation

	private PackedScene _portalScene = GD.Load<PackedScene>("res://Levels/OverworldLevels/KeyLevelObjects/Portal/Portal.tscn");

	private PackedScene _portalSwitchScene = GD.Load<PackedScene>("res://Levels/OverworldLevels/KeyLevelObjects/PortalSwitch/PortalSwitch.tscn");

	#endregion

	#region Enemy Generation

	private int _enemyCountMax = 5;
	private int _enemyCount = 0;

	#endregion

	#region Component Nodes

	private Node _baseFloor;

	private TileMap _tileMap;

	private Timer _enemyRespawnTimer;

	#endregion

	public override void _Ready()
	{
		#region Initialize Properties

		_rng.Randomize();

        RootSceneSwapper rootSceneSwapper = GetTree().Root.GetNode<RootSceneSwapper>("RootSceneSwapper");

        _parentDungeonLevelSwapper = rootSceneSwapper.GetDungeonLevelSwapper();

        _baseFloor = this.GetNode<Node>("BaseFloor");

		_tileMap = _baseFloor.GetNode<TileMap>("TileMap");

		_enemyRespawnTimer = this.GetNode<Timer>("EnemyRespawnTimer");

		#endregion

		SetPossibleFloorTiles();

		RunProceduralPathGeneration();

		while (_enemyCount < _enemyCountMax)
		{
			SpawnEnemy();
		}	 
		_enemyRespawnTimer.Start();
	}

	#region Floor Generation

	private void SetPossibleFloorTiles()
	{
		if (_parentDungeonLevelSwapper.CurrentGameRules.CurrentLevelSize == LevelSize.Small)
		{
			_maxNumberOfTiles = 20;
		}
		else if (_parentDungeonLevelSwapper.CurrentGameRules.CurrentLevelSize == LevelSize.Medium)
		{
			_maxNumberOfTiles = 40;
		}
		else if (_parentDungeonLevelSwapper.CurrentGameRules.CurrentLevelSize == LevelSize.Large)
		{
			_maxNumberOfTiles = 60;
		}
		else if (_parentDungeonLevelSwapper.CurrentGameRules.CurrentLevelSize == LevelSize.Varied)
		{
			var floorSizeOptions = new List<int>() { 20, 40, 60 };

			_maxNumberOfTiles = floorSizeOptions[_rng.RandiRange(0, floorSizeOptions.Count - 1)];
		}

		int overallCounter = 0;

		for (int x = 0; x < _maxNumberOfTiles; x++)
        {
			for (int y = 0; y < _maxNumberOfTiles; y++)
            {
                _possibleFloorPositionsByIndex.Add(overallCounter, new Vector2I(x, y));
                _possibleIndicesByFloorPositions.Add(new Vector2I(x, y), overallCounter);

                overallCounter++;
            }
        }
    }

	#endregion

	private void RunProceduralPathGeneration()
	{
		if (_parentDungeonLevelSwapper.CurrentGameRules.CurrentRelativePlayerSpawnDistanceType == RelativePlayerSpawnDistanceType.SuperClose)
		{
			GenerateSingleSpawnPoint();
		}
		else
		{
			if (_parentDungeonLevelSwapper.ActivePlayers.Count > 1)
			{
				GenerateMultipleSpawnPoints();

				CreatePathsBetweenSpawnPoints();
			}
			else
			{
				GenerateSingleSpawnPoint();
			}
		}

		float percentageOfFloorToCover = 0;

		switch (_parentDungeonLevelSwapper.ActivePlayers.Count)
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
		while (_existingFloorGridSpaces.Count(x => x.InteriorBlock.IsQueuedForDeletion()) < (percentageOfFloorToCover * _possibleFloorPositionsByIndex.Count))
		{
			CreatePathsBetweenPoints();
		}

		#region Spawn Key Objects

		GenerateKeyMapItems();

		if (_parentDungeonLevelSwapper.CurrentGameRules.CurrentRelativePlayerSpawnDistanceType == RelativePlayerSpawnDistanceType.SuperClose)
		{
			SpawnPlayersSuperClose();
		}
		else
		{
			SpawnPlayersNormal();
		}

		#endregion

		DrawOverviewAndWalls();
	}

    #region Procedural Path Generation

    #region Spawn Point Generation

    private void GenerateSingleSpawnPoint(int playerNumber = 0)
    {
        //Get a random space in possible floor spaces to pick as spawn point
        var floorTileIndex = _rng.RandiRange(0, _possibleFloorPositionsByIndex.Count - 1);

        //Create Spawn Point
        var newSpawnPoint_TileMapSpace = new TileMapSpace();

        if (_existingFloorGridSpaces.Any(x => x.TileMapPosition == _possibleFloorPositionsByIndex[floorTileIndex]))
        {
            newSpawnPoint_TileMapSpace = _existingFloorGridSpaces.FirstOrDefault(x => x.TileMapPosition == _possibleFloorPositionsByIndex[floorTileIndex]);
        }
        else
        {
            newSpawnPoint_TileMapSpace = CreateDefaultTileMapSpace(_possibleFloorPositionsByIndex[floorTileIndex]);

            _possibleTileMapSpacesByFloorPosition.Add(_possibleFloorPositionsByIndex[floorTileIndex], newSpawnPoint_TileMapSpace);

            _existingFloorGridSpaces.Add(newSpawnPoint_TileMapSpace);
        }

        newSpawnPoint_TileMapSpace.IsSpawnPoint = true;

        newSpawnPoint_TileMapSpace.InteriorBlock.QueueFree();
        newSpawnPoint_TileMapSpace.NumberOfSpawnPointWhoClearedIt = playerNumber;
        newSpawnPoint_TileMapSpace.IsCleared = true;

        _possibleTileMapSpacesByFloorPosition[newSpawnPoint_TileMapSpace.TileMapPosition].NumberOfSpawnPointWhoClearedIt = playerNumber;

        var richTextLabel = newSpawnPoint_TileMapSpace.TestText.GetNode("RichTextLabel") as RichTextLabel;
        richTextLabel.Text = playerNumber.ToString();

		DrawOnTileMap(newSpawnPoint_TileMapSpace.TileMapPosition);

        //Clear out area near spawn point
        var floorSpacesAdjacentToSpawnPoint = _possibleFloorPositionsByIndex.Values.Where(
            floorSpace => (floorSpace != newSpawnPoint_TileMapSpace.TileMapPosition &&
                          ((Vector2)newSpawnPoint_TileMapSpace.TileMapPosition).DistanceTo(floorSpace) <= Math.Sqrt(2))).ToList();

        foreach (Vector2I floorSpaceAdjacentToSpawnPoint in floorSpacesAdjacentToSpawnPoint)
        {
            var nearSpawnPoint_TileMapSpace = new TileMapSpace();

            if (_existingFloorGridSpaces.Any(x => x.TileMapPosition == floorSpaceAdjacentToSpawnPoint))
            {
                nearSpawnPoint_TileMapSpace = _existingFloorGridSpaces.FirstOrDefault(x => x.TileMapPosition == floorSpaceAdjacentToSpawnPoint);
            }
            else
            {
                nearSpawnPoint_TileMapSpace = CreateDefaultTileMapSpace(floorSpaceAdjacentToSpawnPoint);

                _possibleTileMapSpacesByFloorPosition.Add(floorSpaceAdjacentToSpawnPoint, nearSpawnPoint_TileMapSpace);

                _existingFloorGridSpaces.Add(nearSpawnPoint_TileMapSpace);
            }

            nearSpawnPoint_TileMapSpace.InteriorBlock.QueueFree();
            nearSpawnPoint_TileMapSpace.NumberOfSpawnPointWhoClearedIt = playerNumber;
            nearSpawnPoint_TileMapSpace.IsCleared = true;

            _possibleTileMapSpacesByFloorPosition[nearSpawnPoint_TileMapSpace.TileMapPosition].NumberOfSpawnPointWhoClearedIt = playerNumber;

            var rtl = nearSpawnPoint_TileMapSpace.TestText.GetNode("RichTextLabel") as RichTextLabel;
            rtl.Text = playerNumber.ToString();

            DrawOnTileMap(nearSpawnPoint_TileMapSpace.TileMapPosition);
        }
    }

    private void GenerateMultipleSpawnPoints()
	{
		for (int spawnPointGeneratedCount = 0; spawnPointGeneratedCount < _parentDungeonLevelSwapper.ActivePlayers.Count; spawnPointGeneratedCount++)
		{
			GenerateSingleSpawnPoint(spawnPointGeneratedCount);
		}
	}

	//TODO: Check if this method and the other one are similar enough to just make into one method
	private void CreatePathsBetweenSpawnPoints()
	{
		List<TileMapSpace> spawnPoints = _existingFloorGridSpaces.Where(x => x.IsSpawnPoint).ToList();

		foreach (TileMapSpace startingSpawnPoint in spawnPoints)
		{
			TileMapSpace walkingFloorSpace = startingSpawnPoint;

			List<TileMapSpace> availableSpawnPoints = spawnPoints.Where(x => x != startingSpawnPoint).ToList();

			var targetSpawnPoint = availableSpawnPoints[_rng.RandiRange(0, availableSpawnPoints.Count - 1)];

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

				var newPositionToCheck = new Vector2I(walkingFloorSpace.TileMapPosition.X + changeInX, walkingFloorSpace.TileMapPosition.Y + changeInY);

				//Set walkingFloorSpace to somewhere adjacent to spawn
				//Instead of using any, just check that new x and y are within the max dimension range
				if (IsBlockInsideBorders(newPositionToCheck) &&_possibleIndicesByFloorPositions.ContainsKey(newPositionToCheck))
				{
                    var nextWalk_TileMapSpace = new TileMapSpace();

                    if (_existingFloorGridSpaces.Any(x => x.TileMapPosition == newPositionToCheck))
                    {
                        nextWalk_TileMapSpace = _existingFloorGridSpaces.FirstOrDefault(x => x.TileMapPosition == newPositionToCheck);
                    }
                    else
                    {
                        nextWalk_TileMapSpace = CreateDefaultTileMapSpace(newPositionToCheck);

                        _possibleTileMapSpacesByFloorPosition.Add(newPositionToCheck, nextWalk_TileMapSpace);

                        _existingFloorGridSpaces.Add(nextWalk_TileMapSpace);
                    }

					if (nextWalk_TileMapSpace != null && nextWalk_TileMapSpace != startingSpawnPoint)
					{
						if (!nextWalk_TileMapSpace.InteriorBlock.IsQueuedForDeletion())
						{
                            nextWalk_TileMapSpace.InteriorBlock.QueueFree();
                            nextWalk_TileMapSpace.IsCleared = true;
						}

						var numberOfSpawnPointWhoClearedMatchingFloorSpace = nextWalk_TileMapSpace.NumberOfSpawnPointWhoClearedIt;

						walkingFloorSpace = nextWalk_TileMapSpace;
						walkingFloorSpace.NumberOfSpawnPointWhoClearedIt = startingSpawnPoint.NumberOfSpawnPointWhoClearedIt;


                        _possibleTileMapSpacesByFloorPosition[walkingFloorSpace.TileMapPosition].NumberOfSpawnPointWhoClearedIt = startingSpawnPoint.NumberOfSpawnPointWhoClearedIt;

                        var rtl = walkingFloorSpace.TestText.GetNode("RichTextLabel") as RichTextLabel;
						rtl.Text = walkingFloorSpace.NumberOfSpawnPointWhoClearedIt.ToString();

                        DrawOnTileMap(nextWalk_TileMapSpace.TileMapPosition);

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

    private void SpawnPlayersSuperClose()
    {
        List<TileMapSpace> spawnPoints = _existingFloorGridSpaces.Where(x => x.IsSpawnPoint).ToList();

        int playerCount = 0;

        foreach (BaseCharacter character in _parentDungeonLevelSwapper.ActivePlayers)
        {
            if (playerCount == 0)
            {
                character.GlobalPosition = spawnPoints[playerCount].InteriorBlock.GlobalPosition;

                AddChild(character);
            }
            else
            {
                float changeInX = _rng.RandfRange(-30, 30);
                float changeInY = _rng.RandfRange(-30, 30);

                character.GlobalPosition = new Vector2(_parentDungeonLevelSwapper.ActivePlayers[0].GlobalPosition.X + changeInX, _parentDungeonLevelSwapper.ActivePlayers[0].GlobalPosition.Y + changeInY);

                AddChild(character);
            }

            GD.Print($"P: {character.PlayerNumber}, D: {character.DeviceIdentifier}, C: {character.CharacterClassName}");

            playerCount++;
        }

        GD.Print("---------------------------------------");
    }

    private void SpawnPlayersNormal()
    {
        List<TileMapSpace> spawnPoints = _existingFloorGridSpaces.Where(x => x.IsSpawnPoint).ToList();

        int playerCount = 0;

        foreach (BaseCharacter character in _parentDungeonLevelSwapper.ActivePlayers)
        {
            GD.Print($"P: {character.PlayerNumber}, D: {character.DeviceIdentifier}, C: {character.CharacterClassName}");

            character.GlobalPosition = spawnPoints[playerCount].InteriorBlock.GlobalPosition;

            AddChild(character);

            playerCount++;
        }

        GD.Print("---------------------------------------");
    }


    #endregion

    private void CreatePathsBetweenPoints()
	{
		var availableStartSpaces = _existingFloorGridSpaces.Where(x => x.InteriorBlock.IsQueuedForDeletion()).ToList();

		TileMapSpace startingPoint = availableStartSpaces[_rng.RandiRange(0, availableStartSpaces.Count - 1)];

		TileMapSpace walkingFloorSpace = startingPoint;

		var targetPoint = _possibleFloorPositionsByIndex[_rng.RandiRange(0, _possibleFloorPositionsByIndex.Count - 1)];

		//For some reason, trig circle is flipped across its y axis... whatever...
		var angleFromWalkingFloorSpaceToTargetPoint = walkingFloorSpace.InteriorBlock.GlobalPosition.AngleToPoint(_tileMap.LocalToMap(targetPoint));

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

			var newPositionToCheck = new Vector2I(walkingFloorSpace.TileMapPosition.X + changeInX, walkingFloorSpace.TileMapPosition.Y + changeInY);

            //Set walkingFloorSpace to somewhere adjacent to spawn
            if (IsBlockInsideBorders(newPositionToCheck) &&_possibleIndicesByFloorPositions.ContainsKey(newPositionToCheck))
            {
                var nextWalk_TileMapSpace = new TileMapSpace();

                if (_existingFloorGridSpaces.Any(x => x.TileMapPosition == newPositionToCheck))
                {
                    nextWalk_TileMapSpace = _existingFloorGridSpaces.FirstOrDefault(x => x.TileMapPosition == newPositionToCheck);
                }
                else
                {
                    nextWalk_TileMapSpace = CreateDefaultTileMapSpace(newPositionToCheck);

                    _possibleTileMapSpacesByFloorPosition.Add(newPositionToCheck, nextWalk_TileMapSpace);

                    _existingFloorGridSpaces.Add(nextWalk_TileMapSpace);
                }

                if (nextWalk_TileMapSpace != null && nextWalk_TileMapSpace != startingPoint)
                {
                    if (!nextWalk_TileMapSpace.InteriorBlock.IsQueuedForDeletion())
                    {
                        nextWalk_TileMapSpace.InteriorBlock.QueueFree();
                        nextWalk_TileMapSpace.IsCleared = true;
                    }

                    var numberOfSpawnPointWhoClearedMatchingFloorSpace = nextWalk_TileMapSpace.NumberOfSpawnPointWhoClearedIt;

                    walkingFloorSpace = nextWalk_TileMapSpace;
                    walkingFloorSpace.NumberOfSpawnPointWhoClearedIt = 99;

					_possibleTileMapSpacesByFloorPosition[walkingFloorSpace.TileMapPosition].NumberOfSpawnPointWhoClearedIt = 99;

                    var rtl = walkingFloorSpace.TestText.GetNode("RichTextLabel") as RichTextLabel;
                    rtl.Text = walkingFloorSpace.NumberOfSpawnPointWhoClearedIt.ToString();

                    DrawOnTileMap(nextWalk_TileMapSpace.TileMapPosition);

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

	private TileMapSpace CreateDefaultTileMapSpace(Vector2I positionOfTileMapSpace)
	{
		//Generate InteriorWallBlock at that position
		var tempBlock = _interiorBlockScene.Instantiate();
		var interiorBlock = tempBlock as Node2D;
		var interiorWallBlock = tempBlock as InteriorWallBlock;
		AddChild(interiorBlock);

		var tempTestText = _interiorBlockTestTextScene.Instantiate();
		var testText = tempTestText as Node2D;
		AddChild(testText);

		var positionInLevel = interiorBlock.ToGlobal(_tileMap.MapToLocal(positionOfTileMapSpace));
		interiorBlock.GlobalPosition = new Vector2(positionInLevel.X, positionInLevel.Y);
		testText.GlobalPosition = new Vector2(positionInLevel.X, positionInLevel.Y);

		TileMapSpace newTileMapSpace = new TileMapSpace()
		{
			InteriorBlock = interiorBlock,
			TestText = testText,
			TileMapPosition = positionOfTileMapSpace,
			ActualGlobalPosition = _tileMap.MapToLocal(positionOfTileMapSpace),
			ExistingTileMapSpacesIndex = _existingFloorGridSpaces.Count
        };

		return newTileMapSpace;
	}

	private void DrawOverviewAndWalls()
	{
		try
		{
			List<TileMapSpace> tempTileMapSpaces = new List<TileMapSpace>();

			//Create all floor-adjacent wall-tile map spaces
            foreach (Vector2I possiblePosition in _possibleTileMapSpacesByFloorPosition.Keys)
            {
                List<Vector2I> allAdjacentFloorSpacePositions = GetAllAdjacentFloorSpacePositions(possiblePosition);

                foreach (Vector2I adjacentFloorSpacePosition in allAdjacentFloorSpacePositions)
                {
                    if (!_possibleTileMapSpacesByFloorPosition.ContainsKey(adjacentFloorSpacePosition))
                    {
                        var newWall_TileMapSpace = new TileMapSpace();

                        newWall_TileMapSpace = CreateDefaultTileMapSpace(adjacentFloorSpacePosition);

						newWall_TileMapSpace.NumberOfSpawnPointWhoClearedIt = 90;

						//Can't add to list of TileMapSpaces-by-FloorPosition because it will alter the enumeration we're looping through
                        _existingFloorGridSpaces.Add(newWall_TileMapSpace);

                        tempTileMapSpaces.Add(newWall_TileMapSpace);


                        var interiorBlockSprite = newWall_TileMapSpace.InteriorBlock.FindChild("Sprite2D") as Sprite2D;

                        var overviewIndex = _rng.RandiRange(0, 3);

                        Texture2D newTexture = ResourceLoader.Load($"res://Levels/OverworldLevels/TileMapping/InteriorWalls/Castle/Overview/CastleOverview{overviewIndex}.png") as Texture2D;
                        interiorBlockSprite.Texture = newTexture;
					}
                }
			}

			//Add all newly-created tiles to the list of TileMapSpaces-by-FloorPosition
            foreach (TileMapSpace tileMapSpace in tempTileMapSpaces)
            {
                if (!_possibleTileMapSpacesByFloorPosition.ContainsKey(tileMapSpace.TileMapPosition))
                {
                    _possibleTileMapSpacesByFloorPosition.Add(tileMapSpace.TileMapPosition, tileMapSpace);
                }
            }

            foreach (Vector2I tileMapPosition in _possibleTileMapSpacesByFloorPosition.Keys)
            {
				TileMapSpace current_TileMapSpace = _possibleTileMapSpacesByFloorPosition[tileMapPosition];

                List<Vector2I> allAdjacentFloorSpacePositions = GetAllAdjacentFloorSpacePositions(tileMapPosition);

                //if current index isn't the very top
                if (current_TileMapSpace.TileMapPosition.Y != 0)
                {
                    if (allAdjacentFloorSpacePositions.Any(x => x == new Vector2I(current_TileMapSpace.TileMapPosition.X, current_TileMapSpace.TileMapPosition.Y + 1)))
                    {
                        var collisionShape = current_TileMapSpace.InteriorBlock.FindChild("CollisionShape2D") as CollisionShape2D;

                        collisionShape.Shape = new RectangleShape2D() { Size = new Vector2(32, 16) };
                        collisionShape.Position = new Vector2(0, 8);
                    }
                }

                if (current_TileMapSpace.TileMapPosition.Y != _maxNumberOfTiles - 1)
                {
                    if (allAdjacentFloorSpacePositions.Any(x => x == new Vector2I(current_TileMapSpace.TileMapPosition.X, current_TileMapSpace.TileMapPosition.Y - 1)))
                    {
                        var interiorBlockSprite = current_TileMapSpace.InteriorBlock.FindChild("Sprite2D") as Sprite2D;

                        var textureIndex = _rng.RandiRange(0, 5);

						Texture2D newTexture2 = ResourceLoader.Load($"res://Levels/OverworldLevels/TileMapping/InteriorWalls/Castle/Wall/CastleWall{textureIndex}.png") as Texture2D;
                        interiorBlockSprite.Texture = newTexture2;
					}
                }
            }
        }
		catch (Exception ex)
		{

		}

		
	}

    private List<Vector2I> GetAllAdjacentFloorSpacePositions(Vector2I centralFloorSpacePosition)
	{
        List<Vector2I> adjacentFloorSpaces = new List<Vector2I>();

		Vector2I northAdjacentPosition = new Vector2I(centralFloorSpacePosition.X, centralFloorSpacePosition.Y - 1);
        Vector2I northEastAdjacentPosition = new Vector2I(centralFloorSpacePosition.X + 1, centralFloorSpacePosition.Y - 1);
        Vector2I eastAdjacentPosition = new Vector2I(centralFloorSpacePosition.X + 1, centralFloorSpacePosition.Y);
        Vector2I southEastAdjacentPosition = new Vector2I(centralFloorSpacePosition.X + 1, centralFloorSpacePosition.Y + 1);
        Vector2I southAdjacentPosition = new Vector2I(centralFloorSpacePosition.X, centralFloorSpacePosition.Y + 1);
        Vector2I southWestAdjacentPosition = new Vector2I(centralFloorSpacePosition.X - 1, centralFloorSpacePosition.Y + 1);
        Vector2I westAdjacentPosition = new Vector2I(centralFloorSpacePosition.X - 1, centralFloorSpacePosition.Y);
        Vector2I northWestAdjacentPosition = new Vector2I(centralFloorSpacePosition.X - 1, centralFloorSpacePosition.Y - 1);

		CheckIfContainsKey(adjacentFloorSpaces, northAdjacentPosition);
        CheckIfContainsKey(adjacentFloorSpaces, northEastAdjacentPosition);
        CheckIfContainsKey(adjacentFloorSpaces, eastAdjacentPosition);
        CheckIfContainsKey(adjacentFloorSpaces, southEastAdjacentPosition);
        CheckIfContainsKey(adjacentFloorSpaces, southAdjacentPosition);
        CheckIfContainsKey(adjacentFloorSpaces, southWestAdjacentPosition);
        CheckIfContainsKey(adjacentFloorSpaces, westAdjacentPosition);
        CheckIfContainsKey(adjacentFloorSpaces, northWestAdjacentPosition);

        return adjacentFloorSpaces;
    }

	private void CheckIfContainsKey(List<Vector2I> adjacentFloorSpaces, Vector2I newPosition)
	{
        if (_possibleIndicesByFloorPositions.ContainsKey(newPosition))
        {
            adjacentFloorSpaces.Add(newPosition);
        }
    }

    private bool IsBlockInsideBorders(Vector2I vector)
	{
		return vector.X != 0 && vector.Y != 0 && vector.X != _maxNumberOfTiles - 1 && vector.Y != _maxNumberOfTiles - 1;
	}

    private void DrawOnTileMap(Vector2I positionOfTile)
    {
        var atlasId = -1;
        var xAtlasCoord = -1;
        var yAtlasCoord = -1;

        if (_parentDungeonLevelSwapper.CurrentGameRules.BiomeType == BiomeType.Castle)
        {
            atlasId = TileMappingMagicNumbers.TileMapCastleFloorAtlasId;
            xAtlasCoord = _rng.RandiRange(0, 3);
            yAtlasCoord = _rng.RandiRange(0, 0);
        }
        else if (_parentDungeonLevelSwapper.CurrentGameRules.BiomeType == BiomeType.Frost)
        {

        }

        _tileMap.SetCell(0, positionOfTile, atlasId, new Vector2I(xAtlasCoord, yAtlasCoord));
    }

    #endregion

    #region Key Object Generation

    private void GenerateKeyMapItems()
	{
		GeneratePortal();

		//GenerateSwitches();
	}

	private void GeneratePortal() 
	{
		var tempPortal = _portalScene.Instantiate();
		var portal = tempPortal as Node2D;
		AddChild(portal);

		var availableTargetSpaces = _existingFloorGridSpaces.Where(x => x.InteriorBlock.IsQueuedForDeletion() && !x.IsSpawnPoint).ToList();

		//Need to match to make sure portals and switches don't spawn on each other, maybe add "holding something flag" to tile class
		portal.GlobalPosition = availableTargetSpaces[_rng.RandiRange(0, availableTargetSpaces.Count - 1)].InteriorBlock.GlobalPosition;
	}

	private void GenerateSwitches() 
	{
		for (int i = 0; i < 3; i++)
		{
			var tempPortalSwitch = _portalSwitchScene.Instantiate();
			var portalSwitch = tempPortalSwitch as Node2D;
			AddChild(portalSwitch);

			var availableTargetSpaces = _existingFloorGridSpaces.Where(x => x.InteriorBlock.IsQueuedForDeletion() && !x.IsSpawnPoint).ToList();

			portalSwitch.GlobalPosition = availableTargetSpaces[_rng.RandiRange(0, availableTargetSpaces.Count - 1)].InteriorBlock.GlobalPosition;
		}
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

	public override void _Process(double delta)
	{
		if (_parentDungeonLevelSwapper.ActivePlayers.Count > 0 && _parentDungeonLevelSwapper.ActivePlayers.All(x => x.IsDead))
		{
            _parentDungeonLevelSwapper.ActivePlayers.Clear();

			EmitSignal(SignalName.GoToGameOverScreen);
		}

		if (GetTree() != null)
		{
            _enemyCount = GetTree().GetNodesInGroup("Enemy").Count;
        }

		SpawnEnemies();
	}

	#region Enemy Generation

	private List<string> _enemyList = new List<string>()
	{
		EnemyType.Slime.ToString()
	};

	private void SpawnEnemies()
	{
		if (_enemyRespawnTimer.IsStopped() &&_enemyCount < _enemyCountMax)
		{
			int availableEnemyCount = _enemyCountMax - _enemyCount;

			int chanceOfSpawning = _rng.RandiRange(1, 100) + availableEnemyCount * 10;

			if (chanceOfSpawning > 80)
			{
				SpawnEnemy();
			}

			_enemyRespawnTimer.Start();
		}
	}

	private void SpawnEnemy()
	{
		var tempEnemy = GD.Load<PackedScene>(EnemyScenePaths.SlimeScenePath).Instantiate();

		var enemy = tempEnemy as BaseEnemy;
		AddChild(enemy);

		var availableTargetSpaces = _existingFloorGridSpaces.Where(x => x.IsCleared && !x.IsSpawnPoint).ToList();

		enemy.GlobalPosition = availableTargetSpaces[_rng.RandiRange(0, availableTargetSpaces.Count - 1)].ActualGlobalPosition;

		_enemyCount++;
	}

	#endregion
}
