using Godot;
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

		//The id of the floor tiles is 8
		_floorTileList = _tileMap.GetUsedCellsById(0, 8).ToList();

		GenerateInteriorWallsForLevel();
	}

	private void GenerateInteriorWallsForLevel()
	{
		_rng.Randomize();

		GenerateAllInteriorBlocks();

		GenerateSpawnPoints();
	}

	//Generate interior blocks everywhere there is a floor tile
	private void GenerateAllInteriorBlocks()
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

	//Generate spawn points
	private void GenerateSpawnPoints()
	{
		//TODO: Set spawnPointGeneratedCountMax = # of players spawning in
		for (int spawnPointGeneratedCount = 0; spawnPointGeneratedCount < 4; spawnPointGeneratedCount++)
		{
			var floorTileIndex = (int)Math.Floor(_rng.RandfRange(0, _floorTileList.Count));

			var currentFloorTileGridMapPosition = _floorTileList[floorTileIndex];

			if (_existingFloorGridSpaces.Any(x => x.InteriorBlock.GlobalPosition == _tileMap.MapToLocal(currentFloorTileGridMapPosition)))
			{
				var floorGridSpaceWithMatchingPosition = _existingFloorGridSpaces.FirstOrDefault(x => x.InteriorBlock.GlobalPosition == _tileMap.MapToLocal(currentFloorTileGridMapPosition));

				floorGridSpaceWithMatchingPosition.IsSpawnPoint = true;

				floorGridSpaceWithMatchingPosition.InteriorBlock.QueueFree();

				//Clear out spawn point areas
				foreach (var currentFloorGridSpace in _existingFloorGridSpaces)
				{
					if (floorGridSpaceWithMatchingPosition.InteriorBlock.GlobalPosition.DistanceTo(currentFloorGridSpace.InteriorBlock.GlobalPosition) <= 48)
					{
						currentFloorGridSpace.InteriorBlock.QueueFree();
					}
				}
			}
		}
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
