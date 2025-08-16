using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Interactive.Inventory;
using SoYouWANNAJam2025.Code.Interactive.Items;
using SoYouWANNAJam2025.Code.Npc;
using SoYouWANNAJam2025.Code.Npc.Friendly;
using SoYouWANNAJam2025.Code.Npc.Hostile;

namespace SoYouWANNAJam2025.Code.World;

public partial class GameManager : Node2D
{
	public Resource[] PossibleTargets;
	public Godot.Collections.Array<NpcResource> NpcResources = [];
	public Godot.Collections.Array<NpcResource> HostileNpcResources = [];
	public Godot.Collections.Array<ModularItemTemplate> ModularItemResources = [];
	[ExportGroup("Scenes")]
	[Export]
	public PackedScene NpcScene { get; set; }
	[Export]
	public PackedScene HostileNpcScene { get; set; }
	[ExportGroup("Other stuff")]
	[Export]
	public Vector2 TileSize = new Vector2(32, 16);

	public float Gold = 0; 
	
	[Export] public DayCycleLantern CycleLantern;
	private Godot.Collections.Array<Npc.Friendly.Npc> _npcs = [] ;
	private Godot.Collections.Array<Npc.Friendly.Npc> _hostileNpc = [] ;
	[Export] public int NpcMaxCount = 20;
	private Node2D _npcSpawnLocation;
	private Node2D _frontDesk;
	private Area2D _leaveAreaNode;
	private string _npcResourceFolderPath = "res://Resources/Npcs/Friendly/";
	private string _hostileNpcResourceFolderPath = "res://Resources/Npcs/Hostile/";
	private string _modularItemFolderPath = "res://Resources/Items/ModularItems/";
	private PackedScene _modularItemScene;
	private PathFollow2D _hostileNpcSpawnLocation; 
	
	public override void _Ready()
	{
		DirContents(_npcResourceFolderPath);
		DirContents(_modularItemFolderPath);
		DirContents(_hostileNpcResourceFolderPath);
		GD.Print(ModularItemResources);
		_npcSpawnLocation = (Node2D)FindChild("NpcArrivalArea");
		_frontDesk  = (Node2D)FindChild("FrontDesk");
		_leaveAreaNode = (Area2D)FindChild("LeaveArea");
		_modularItemScene = GD.Load<PackedScene>("res://Entities/Interactive/Items/ModularItem.tscn");
		var npcInteractor = (NpcInteractor)FindChild("NpcInteractor");
		_hostileNpcSpawnLocation = (PathFollow2D)FindChild("HostileNpcSpawnLocations");
		
		Global.Grid = GetNode<TileMapLayer>("./Isometric/WorldMap");

		npcInteractor.NpcLeft += OnNpcLeft;
		GD.Print(CycleLantern);
		if (CycleLantern == null) return;
		CycleLantern.CycleLantern += LanternChanged;
		
	}

	private void OnNpcLeft(Npc.Friendly.Npc npc)
	{
		_npcs.Remove(npc);
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
		if (path == _npcResourceFolderPath)
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
						var resource = ResourceLoader.Load<NpcResource>(path+fileName);
						if (resource != null)
						{
							NpcResources.Add(resource);
						}
						else 
						{
							GD.Print("Resource not found at path: "+path+fileName);
						}
					}
					fileName = dir.GetNext();
				}
			}
			else
			{
				GD.Print("An error occurred when trying to access the path.");
			}
		} else if (path == _modularItemFolderPath)
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
						var resource = ResourceLoader.Load<ModularItemTemplate>(path+fileName);
						if (resource != null)
						{
							ModularItemResources.Add(resource);
						}
						else 
						{
							GD.Print("Resource not found at path: "+path+fileName);
						}
					}
					fileName = dir.GetNext();
				}
			}
			else
			{
				GD.Print("An error occurred when trying to access the path.");
			}
		}else if (path == _hostileNpcResourceFolderPath)
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
						var resource = ResourceLoader.Load<NpcResource>(path+fileName);
						if (resource != null)
						{
							HostileNpcResources.Add(resource);
						}
						else 
						{
							GD.Print("Resource not found at path: "+path+fileName);
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
		
	}

	private void OnNpcTimerTimeout()
	{
		//GD.Print("NPC count = "+_npcs.Count);
		var newItem = _modularItemScene.Instantiate<GenericItem>();
		if (ModularItemResources != null)
		{
			newItem.ItemResource = ModularItemResources[GD.RandRange(0, NpcResources.Count - 1)];
		}

		GetNode("/root/GameManager/Isometric").AddChild(newItem);

		Npc.Friendly.Npc npc = NpcScene.Instantiate<Npc.Friendly.Npc>();
		NpcSpawning(npc);
		// Add outputs to inventory
		if (newItem.ItemResource == null) return;
		npc.NpcInventory.PickupItem(newItem, true);
	}
	private void OnHostileNpcTimerTimeout()
	{
		
		HostileNpc hostileNpc = HostileNpcScene.Instantiate<HostileNpc>();
		NpcSpawning(hostileNpc);
		
	}

	private void NpcSpawning( dynamic npc)
	{
		//GD.Print(npc.GetType() );
		if (npc is HostileNpc)
		{
			if (_hostileNpc.Count >= NpcMaxCount) return;
			npc.NpcResource = HostileNpcResources[GD.RandRange(0,HostileNpcResources.Count -1)];
			_hostileNpcSpawnLocation.ProgressRatio = GD.Randf();
			npc.Position = _hostileNpcSpawnLocation.Position * 4;
			npc.Scale = Vector2.One * 4;
			_hostileNpc.Add(npc);
			AddChild(npc);
			
		}
		else if  (npc is Npc.Friendly.Npc)
		{
			if (_npcs.Count >= NpcMaxCount)return;
			npc.NpcResource = NpcResources[GD.RandRange(0,NpcResources.Count -1)];
			npc.Position = _npcSpawnLocation.Position * 4 ;
			npc.Scale = Vector2.One * 4;
			npc.Target = _frontDesk;
			npc.LeaveAreaNode = _leaveAreaNode;
			_npcs.Add(npc);
			AddChild(npc);
			var npcInteractor = (NpcInteractor)npc.FindChild("NpcInteractor");
			npcInteractor.RequestComplete += OnRequestComplete;
		}
		
	}

	private void OnRequestComplete(Npc.Friendly.Npc npc)
	{
		var hostileTarget = _hostileNpc[GD.RandRange(0, _hostileNpc.Count - 1)];
		npc.Target = hostileTarget;
	}

	
	
	// Called when the node enters the scene tree for the first time.
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	
	
}