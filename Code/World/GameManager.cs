using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Npc;

namespace SoYouWANNAJam2025.Code.World;

public partial class GameManager : Node2D
{
	public Resource[] PossibleTargets;
	public Godot.Collections.Array<NpcResource> NpcResources = [];
	[Export]
	public PackedScene NpcScene { get; set; }

	public float gold = 0; 
	
	[Export] public DayCycleLantern _cycleLantern;
	
	public override void _Ready()
	{
		var tileMap =(TileMapLayer)FindChild("WorldMap");
		var container = (Node2D)FindChild("OccluderContainer");
		//NpcResources[0] = ResourceLoader.LoadThreadedRequest<NpcResource>("res://Resources/Npcs/Npc01.tres");
		
		
		if(tileMap != null&& container != null)
			GenerateTileOccluders(tileMap, container);
		DirContents("res://Resources/Npcs/");

		GD.Print(_cycleLantern);
		_cycleLantern.CycleLantern += LanternChanged;
	}
	
	private void LanternChanged(bool onCycleLantern)
	{
		if (onCycleLantern)
		{
			GD.Print("IT'S DAY TIME !");
		}
		else
		{
			GD.Print("AWW IT'S NIGHT TIME !");
		}
	}
	
	public void DirContents(string path)
	{
		using var dir = DirAccess.Open(path);
		if (dir != null)
		{
			dir.ListDirBegin();
			string fileName = dir.GetNext();
			while (fileName != "")
			{
				if (dir.CurrentIsDir())
				{
					GD.Print($"Found directory: {fileName}");
				}
				else
				{
					var resource = ResourceLoader.Load<NpcResource>("res://Resources/Npcs/"+fileName);
					if (resource != null)
					{
						NpcResources.Add(resource);
					}
					else 
					{
						GD.Print("Resource not found at path: "+"res://Resources/Npcs/"+fileName);
					}
				}
				fileName = dir.GetNext();
			}
		}
		else
		{
			GD.Print("An error occurred when trying to access the path.");
		}
	}

	private void OnNpcTimerTimeout()
	{
		
		Npc.Npc npc = NpcScene.Instantiate<Npc.Npc>();
		npc.NpcResource = NpcResources[GD.RandRange(0,NpcResources.Count -1)];
		
		var npcSpawnLocation = GetNode<PathFollow2D>("NpcPath/NpcSpawnLocations");
		npcSpawnLocation.ProgressRatio = GD.Randf();
		npc.Position = npcSpawnLocation.Position;
		npc.Scale = Vector2.One * 4;
		npc.Target = GetNode<Area2D>("Isometric/CraftingBench");
		npc.LeaveAreaNode = GetNode<Node2D>("LeaveArea");
		AddChild(npc);
	}
	
	// Called when the node enters the scene tree for the first time.
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	

	private void GenerateTileOccluders(TileMapLayer tileMap, Node2D container)
	{
		var tileSet = tileMap.TileSet;
		var usedCells = tileMap.GetUsedCells(); // Layer 0

		var occluderCache = new Dictionary<int, OccluderPolygon2D>();

		foreach (var cell in usedCells)
		{
			int tileId = tileMap.GetCellSourceId(cell);
			if (tileId == -1) continue;

			// Cache occluders per tile type
			if (!occluderCache.TryGetValue(tileId, out var occluderPolygon))
			{
				var texture = tileSet.GetSource(tileId) as TileSetAtlasSource;
				if (texture == null) continue;

				// Assuming a single tile per source image
				var image = texture.Texture;
				if (image == null) continue;

				occluderPolygon = AlphaOccluderGenerator.GenerateOccluder(image);
				if (occluderPolygon == null) continue;

				occluderCache[tileId] = occluderPolygon;
			}

			var lightOccluder = new LightOccluder2D();
			lightOccluder.Occluder = occluderPolygon.Duplicate() as OccluderPolygon2D;

			// Convert tile coords to world position
			var pos = tileMap.MapToLocal(cell);
			lightOccluder.Position = pos;

			container.AddChild(lightOccluder);
		}
	}
}