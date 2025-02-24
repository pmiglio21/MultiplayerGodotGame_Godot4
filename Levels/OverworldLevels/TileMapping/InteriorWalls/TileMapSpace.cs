using Enums;
using Godot;

public partial class TileMapSpace 
{
	public Node2D InteriorBlock = null;

	public bool IsSpawnPoint = false;

    public bool IsAdjacentToSpawnPoint = false;

    public bool IsAdjacentToGenerationStartPoint = false;

    public int LastNumberToClearSpace = -1;

    public int SpawnPointNumber = -1;

    public Node2D TestText = null;

	public Vector2I TileMapPosition = Vector2I.Zero;

    public Vector2 ActualGlobalPosition = Vector2I.Zero; //Needed because can't access InteriorBlock's global position if it is queued for deletion

	public TileMapSpaceType TileMapSpaceType = TileMapSpaceType.None;
}
