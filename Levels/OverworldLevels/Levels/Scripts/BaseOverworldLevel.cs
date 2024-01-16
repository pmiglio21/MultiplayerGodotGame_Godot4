using Godot;
using System;

public partial class BaseOverworldLevel : Node
{
	#region Component Nodes

	private Node2D _baseFloor = null;

	#endregion

	PackedScene scene = GD.Load<PackedScene>("res://Levels/OverworldLevels/TileMapping/InteriorWalls/InteriorWallBlock.tscn");

	public override void _Ready()
	{
		_baseFloor = GetNode<Node2D>("BaseFloor");

		GenerateInteriorWallsForLevel();
	}

	private void GenerateInteriorWallsForLevel()
	{
		TileMap tileMap = _baseFloor.GetNode<TileMap>("TileMap");

		//var count = tileMap.GetUsedCellsById(0, 8).Count;

		var rng = new RandomNumberGenerator();
		rng.Randomize();

		//Gets all positions of cells being used in TileMap
		foreach (var tilePosition in tileMap.GetUsedCellsById(0, 8))
		{
			var chanceOfInteriorBlockInstantiation = rng.RandfRange(0, 100);

			if (chanceOfInteriorBlockInstantiation < 25)
			{
				//Generate InteriorWallBlock at that position
				var tempBlock = scene.Instantiate();
				var interiorBlock = tempBlock as Node2D;
				AddChild(interiorBlock);

				//Needs to get better positions, Vector2I makes it weird
				var real_pos = interiorBlock.ToGlobal(tileMap.MapToLocal(new Vector2I(tilePosition.X, tilePosition.Y)));
				interiorBlock.GlobalPosition = new Vector2(real_pos.X, real_pos.Y);

				//GD.Print($"Chance {chanceOfInteriorBlockInstantiation}, Tile Position: {tilePosition}, Real Position: {new Vector2(real_pos.X, real_pos.Y)}");
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
}
