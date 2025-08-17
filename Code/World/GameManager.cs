using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

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
	[Export] public int MaxNpcCount = 20;
	private Node2D _npcSpawnLocation;
	private Node2D _frontDesk;
	private Area2D _leaveAreaNode;
	private const string NpcResourceFolderPath = "res://Resources/Npcs/Friendly/";
	private const string HostileNpcResourceFolderPath = "res://Resources/Npcs/Hostile/";
	private const string ModularItemFolderPath = "res://Resources/Items/ModularItems/";
	private const string ModularPartsFolderPath = "res://Resources/Items/ModularParts/";
	private PackedScene _modularItemScene;
	private PathFollow2D _hostileNpcSpawnLocation;
	private List<Vector2> _queuePositions = new List<Vector2>();
	private List<Npc.Friendly.Npc> _npcQueue = new();
	[Export] public Vector2I QueueDirection = new Vector2I(0, 1); // Behind the target
	
	public Dictionary<EModularItemType, Dictionary<EPartType, Godot.Collections.Array<ModularPartTemplate>>> ModularParts = new();
	private TileMapLayer _tileLayer; 
	
	public override void _Ready()
	{
		string[] paths = [ModularPartsFolderPath];
		foreach (var path in paths)
		{
			TraverseDirectory(path,
				file => {
					var resource = ResourceLoader.Load(file);
					if (resource == null) return;
					switch (path)
					{
						case NpcResourceFolderPath:
							if (resource is NpcResource npcResource) NpcResources.Add(npcResource);
							break;
						case HostileNpcResourceFolderPath:
							if (resource is NpcResource hostileNpcResource) HostileNpcResources.Add(hostileNpcResource);
							break;
						case ModularItemFolderPath:
							if (resource is ModularItemTemplate item) ModularItemResources.Add(item);
							break;
						case ModularPartsFolderPath:
							if (resource is not ModularPartTemplate part) break;
							if (!ModularParts.ContainsKey(part.ModularItemType)) ModularParts[part.ModularItemType] = [];
							if (!ModularParts[part.ModularItemType].ContainsKey(part.PartType)) ModularParts[part.ModularItemType][part.PartType] = [];
							ModularParts[part.ModularItemType][part.PartType].Add(part);
							break;
					}
				}
			);
		}
		
		//GD.Print(ModularItemResources);
		_npcSpawnLocation = (Node2D)FindChild("NpcArrivalArea");
		_frontDesk  = (Node2D)FindChild("FrontDesk");
		_leaveAreaNode = (Area2D)FindChild("LeaveArea");
		_modularItemScene = GD.Load<PackedScene>("res://Entities/Interactive/Items/ModularItem.tscn");
		var npcInteractor = (NpcInteractor)FindChild("NpcInteractor");
		_hostileNpcSpawnLocation = (PathFollow2D)FindChild("HostileNpcSpawnLocations");

		//npcInteractor.NpcLeft += OnNpcLeft;
		//GD.Print(CycleLantern);
		if (CycleLantern == null) return;
		CycleLantern.CycleLantern += LanternChanged;
		Global.GameDay = 0;
		
	}

	public void RequestToJoin(Npc.Friendly.Npc npc)
	{
		if (_npcQueue.Contains(npc) || _npcQueue.Count >= MaxNpcCount)
			return;

		_npcQueue.Add(npc);
		AssignQueuePositions();
	}
	private List<Vector2> GenerateQueuePositions(Vector2 targetPos, int count)
	{
		var list = new List<Vector2>();
		var startCell = _tileLayer.LocalToMap(targetPos);

		for (int i = 1; i <= count; i++)
		{
			Vector2I cell = startCell + QueueDirection * i;
			Vector2 world = _tileLayer.MapToLocal(cell);

			list.Add(world);
		}

		return list;
	}

	private bool IsCellWalkable(Vector2I cell)
	{
		// Only returns true if tile is empty or marked as walkable
		// You can customize this to check tile data or metadata
		return _tileLayer.GetCellSourceId(cell) == -1;
	}
	public void LeaveQueue(Npc.Friendly.Npc npc)
	{
		if (!_npcQueue.Contains(npc))
			return;

		_npcQueue.Remove(npc);
		AssignQueuePositions();
	}
	private void AssignQueuePositions()
	{
		var positions = GenerateQueuePositions(_frontDesk.GlobalPosition, _npcQueue.Count);

		for (int i = 0; i < _npcQueue.Count; i++)
		{
			_npcQueue[i].AssignQueueSlot(positions[i]);
		}
	}
	public ModularItem NewItem(EModularItemType itemType)
	{
		if (_modularItemScene==null || ModularParts[itemType].Count == 0 ) return null;
		
		var itemTemplate = new ModularItemTemplate();
		itemTemplate.DefaultParts = [];
		
		switch (itemType)
		{
			case EModularItemType.Sword:
				itemTemplate.DefaultParts[EPartType.Pummel] = _getPartTemplate(itemType, EPartType.Pummel);
				itemTemplate.DefaultParts[EPartType.Grip] = _getPartTemplate(itemType, EPartType.Grip);
				itemTemplate.DefaultParts[EPartType.Crossguard] =_getPartTemplate(itemType, EPartType.Crossguard);
				itemTemplate.DefaultParts[EPartType.Blade] = _getPartTemplate(itemType, EPartType.Blade);
				break;
			case EModularItemType.Spear:
				itemTemplate.DefaultParts[EPartType.Pole] = _getPartTemplate(itemType, EPartType.Pole);
				itemTemplate.DefaultParts[EPartType.Blade] = _getPartTemplate(itemType, EPartType.Blade);
				break;
			case EModularItemType.Chestplate:
				itemTemplate.DefaultParts[EPartType.Pauldron] = _getPartTemplate(itemType, EPartType.Pauldron);
				itemTemplate.DefaultParts[EPartType.Plate] = _getPartTemplate(itemType, EPartType.Plate);
				itemTemplate.DefaultParts[EPartType.Trim] = _getPartTemplate(itemType, EPartType.Trim);
				break;
			case EModularItemType.Staff:
				itemTemplate.DefaultParts[EPartType.Pole] = _getPartTemplate(itemType, EPartType.Pole);
				itemTemplate.DefaultParts[EPartType.Tip] = _getPartTemplate(itemType, EPartType.Tip);
				itemTemplate.DefaultParts[EPartType.Trim] = _getPartTemplate(itemType, EPartType.Trim);
				break;
			case EModularItemType.Bow:
				itemTemplate.DefaultParts[EPartType.Grip] = _getPartTemplate(itemType, EPartType.Grip);
				itemTemplate.DefaultParts[EPartType.Limb] = _getPartTemplate(itemType, EPartType.Limb);
				itemTemplate.DefaultParts[EPartType.String] = _getPartTemplate(itemType, EPartType.String);
				break;
			case EModularItemType.Shield:
			case EModularItemType.Helmet:
			case EModularItemType.Cloak:
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null);
		}
		//newItem.DrawSprite();
		GD.Print("NEW ITEM");
		var newItem = _modularItemScene.Instantiate<ModularItem>();
		newItem.ItemResource = itemTemplate;
		newItem.ModularItemType = itemType;
		return newItem;
	}

	private ModularPartTemplate _getPartTemplate(EModularItemType itemType, EPartType partType, EPartState state = EPartState.New)
	{
		if (ModularParts[itemType].Count == 0 || ModularParts[itemType][partType].Count == 0) return null;
		var stateParts = ModularParts[itemType][partType]
            .Where(part => part.PartState == state)
            .Select(part => part)
            .ToArray();
		if (stateParts.Length == 0) return null;
		
		return ModularParts[itemType][partType][GD.RandRange(0, stateParts.Length - 1)];
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
	
	

	private void OnNpcTimerTimeout()
	{
		//GD.Print("NPC count = "+_npcs.Count);
		Npc.Friendly.Npc npc = NpcScene.Instantiate<Npc.Friendly.Npc>();
		NpcSpawning(npc);

		EModularItemType[] itemTypes = [EModularItemType.Sword, EModularItemType.Spear]; 
		var newItem = NewItem(itemTypes[GD.RandRange(0, itemTypes.Length - 1)]);
		Global.Grid.AddChild(newItem);
		if (newItem.ItemResource == null) return;
		newItem.OwningNpc = npc;
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
			if (_hostileNpc.Count >= MaxNpcCount) return;
			npc.NpcResource = HostileNpcResources[GD.RandRange(0,HostileNpcResources.Count -1)];
			_hostileNpcSpawnLocation.ProgressRatio = GD.Randf();
			npc.Position = _hostileNpcSpawnLocation.Position * 4;
			npc.Scale = Vector2.One * 4;
			_hostileNpc.Add(npc);
			AddChild(npc);
			
		}
		else if  (npc is Npc.Friendly.Npc)
		{
			if (_npcs.Count >= MaxNpcCount)return;
			npc.NpcResource = NpcResources[GD.RandRange(0,NpcResources.Count -1)];
			npc.Position = _npcSpawnLocation.Position * 4 ;
			npc.Scale = Vector2.One * 4;
			//npc.Target = _frontDesk;
			npc.LeaveAreaNode = _leaveAreaNode;
			_npcs.Add(npc);
			AddChild(npc);
			var npcInteractor = (NpcInteractor)npc.FindChild("NpcInteractor");
			RequestToJoin(npc);
			npcInteractor.RequestComplete += OnRequestComplete;
		}
		
	}

	private void OnRequestComplete(Npc.Friendly.Npc npc)
	{
		var hostileTarget = _hostileNpc[GD.RandRange(0, _hostileNpc.Count - 1)];
		LeaveQueue(npc);
		npc.LeaveQueue();
		npc.Target = hostileTarget;
	}
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void TraverseDirectory(string rootPath, Action<string> fileAction)
	{
		var dirStack = new Stack<string>();
		dirStack.Push(rootPath);

		while (dirStack.Count > 0)
		{
			var currentPath = dirStack.Pop();

			using var dir = DirAccess.Open(currentPath);
			if (dir == null) continue;
        
			dir.ListDirBegin();
			var fileName = dir.GetNext();
			while (fileName != "")
			{
				var fullPath = System.IO.Path.Combine(currentPath, fileName);

				if (dir.CurrentIsDir())
				{
					if (fileName != "." && fileName != "..")
					{
						dirStack.Push(fullPath);
					}
				}
				else
				{
					fileAction?.Invoke(fullPath);
				}

				fileName = dir.GetNext();
			}
			dir.ListDirEnd();
		}
	}
	
	public (bool,GenericItem) NewItem(GenericItemTemplate itemTemplate)
	{
		if (itemTemplate == null) return (false, null);
		PackedScene newItemScene = null;
		if (itemTemplate.SceneOverride == null)
		{
			newItemScene = GD.Load<PackedScene>("res://Entities/Interactive/Items/GenericItem.tscn");
		}
		else
		{
			newItemScene = GD.Load<PackedScene>(itemTemplate.SceneOverride.ResourcePath);
		}
		if (newItemScene == null) return (false, null);
            
		var newItem = newItemScene.Instantiate<GenericItem>();
		newItem.ItemResource = itemTemplate;
		
		return (true, newItem);
	}
	
}