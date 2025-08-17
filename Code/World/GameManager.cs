using System;
using System.Collections.Generic;
using System.Diagnostics;
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
	//[Export]
	//public Vector2 TileSize = new Vector2(32, 16);

	public float Gold = 0; 
	
	[Export] public DayCycleLantern CycleLantern;
	private Godot.Collections.Array<Npc.Friendly.Npc> _npcs = [] ;
	private Godot.Collections.Array<Npc.Friendly.Npc> _hostileNpc = [] ;
	[Export] public int MaxNpcCount = 20;
	private Node2D _npcSpawnLocation;
	private Node2D _frontDesk;
	private Node2D _leaveAreaNode;
	private const string NpcResourceFolderPath = "res://Resources/Npcs/Friendly/";
	private const string HostileNpcResourceFolderPath = "res://Resources/Npcs/Hostile/";
	private const string ModularItemFolderPath = "res://Resources/Items/ModularItems/";
	private const string ModularPartsFolderPath = "res://Resources/Items/ModularParts/";
	private PackedScene _modularItemScene;
	private PathFollow2D _hostileNpcSpawnLocation;
	//private List<Vector2> _queuePositions = new List<Vector2>();
	private List<Npc.Friendly.Npc> _npcQueue = new();
	[Export] public Vector2I QueueDirection = new Vector2I(0, 1); // Behind the target
	
	public Dictionary<EModularItemType, Dictionary<EPartType, Godot.Collections.Array<ModularPartTemplate>>> ModularParts = new();
	private TileMapLayer _tileLayer;
	private TileMapLayer _entities;

	public override void _EnterTree()
	{
		Global.Grid = GetNode<TileMapLayer>("/root/GameManager/GameWorld/Entities");
		Global.GameManager = GetNode<GameManager>("/root/GameManager");
		Global.Camera = GetNode<Camera>("/root/GameManager/Camera");
	}

	public override void _ExitTree()
	{
		Global.Grid = null;
		Global.GameManager = null;
		Global.Camera = null;
	}

	public override void _Ready()
	{
		string[] paths = [NpcResourceFolderPath, HostileNpcResourceFolderPath, ModularItemFolderPath, ModularPartsFolderPath];
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
		_leaveAreaNode = (Node2D)FindChild("NpcLeaveArea");
		_modularItemScene = GD.Load<PackedScene>("res://Entities/Interactive/Items/ModularItem.tscn");
		var npcInteractor = (NpcInteractor)FindChild("NpcInteractor");
		_hostileNpcSpawnLocation = (PathFollow2D)FindChild("HostileNpcSpawnLocations");
		_entities = (TileMapLayer)FindChild("Entities");
		_tileLayer = GetNode<TileMapLayer>("/root/GameManager/GameWorld/Ground");
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
			Vector2I cell = startCell + QueueDirection * i*4;
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
	public (ModularItemTemplate wantedTemplate, ModularItemTemplate givenTemplate) RandomItemTemplates(EModularItemType itemType, float brokenPercent = 0.5f, float changePercent = 0.5f)
	{
		if (_modularItemScene==null || ModularParts[itemType].Count == 0 ) return (null, null);
		
		var wantedTemplate = new ModularItemTemplate();
		var givenTemplate = new ModularItemTemplate();
		EPartType[] partlist;
		
		switch (itemType)
		{
			case EModularItemType.Sword:
				partlist = [EPartType.Pummel, EPartType.Grip, EPartType.Crossguard, EPartType.Blade];
				(wantedTemplate, givenTemplate) = GenerateItemTemplate(EModularItemType.Sword, partlist, brokenPercent, changePercent);
				break;
			case EModularItemType.Spear:
				partlist = [EPartType.Pole, EPartType.Blade];
				(wantedTemplate, givenTemplate) = GenerateItemTemplate(EModularItemType.Spear, partlist, brokenPercent, changePercent);
				break;
			case EModularItemType.Chestplate:
				partlist = [EPartType.Pauldron, EPartType.Plate, EPartType.Trim];
				(wantedTemplate, givenTemplate) = GenerateItemTemplate(EModularItemType.Chestplate, partlist, brokenPercent, changePercent);
				break;
			case EModularItemType.Staff:
				partlist = [EPartType.Pole, EPartType.Tip, EPartType.Trim];
				(wantedTemplate, givenTemplate) = GenerateItemTemplate(EModularItemType.Staff, partlist, brokenPercent, changePercent);
				break;
			case EModularItemType.Bow:
				partlist = [EPartType.Grip, EPartType.Limb, EPartType.String];
				(wantedTemplate, givenTemplate) = GenerateItemTemplate(EModularItemType.Bow, partlist, brokenPercent, changePercent);
				break;
			case EModularItemType.Shield:
			case EModularItemType.Helmet:
			case EModularItemType.Cloak:
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null);
		}
		if (wantedTemplate == null || givenTemplate == null) return (null, null);
		return (wantedTemplate, givenTemplate);
	}

	private (ModularItemTemplate wantedTemplate, ModularItemTemplate givenTemplate) GenerateItemTemplate(EModularItemType itemType, EPartType[] partTypes, float brokenPercent, float changePercent)
	{
		var brokenCount = (int)(partTypes.Length*brokenPercent);
		if (brokenCount == 0) brokenCount = 1;
		
		var states = new EPartState[partTypes.Length];
		while (brokenCount > 0)
		{
			var randIndex = GD.RandRange(0, partTypes.Length-1);
			if (states[randIndex] != EPartState.New) continue;
			brokenCount--;
			states[randIndex] = EPartState.Broken;
		}
		
		var changeCount = (int)(partTypes.Length*changePercent);
		if (changeCount == 0) brokenCount = 1;
		
		var changes = new bool[partTypes.Length];
		while (changeCount > 0)
		{
			var randIndex = GD.RandRange(0, partTypes.Length-1);
			if (changes[randIndex] != false) continue;
			changeCount--;
			changes[randIndex] = true;
		}
		
		var wantedTemplate = new ModularItemTemplate()
		{
			DefaultParts = [],
			ModularItemType = itemType,
		};
		var givenTemplate = new ModularItemTemplate()
		{
			DefaultParts = [],
			ModularItemType = itemType,
		};
		
		for (var i = 0; i < partTypes.Length; i++)
		{
			if (ModularParts[itemType].Count == 0 || ModularParts[itemType][partTypes[i]].Count == 0) return (null, null);
			
			var availableParts = ModularParts[itemType][partTypes[i]];
			var newPart = availableParts.PickRandom();
			
			givenTemplate.DefaultParts[partTypes[i]] = newPart.Duplicate() as ModularPartTemplate;
			givenTemplate.DefaultParts[partTypes[i]].PartState = states[i];
			
			if (changes[i])
			{
				wantedTemplate.DefaultParts[partTypes[i]] = availableParts.PickRandom();
			}
			else
			{
				wantedTemplate.DefaultParts[partTypes[i]] = newPart.Duplicate() as ModularPartTemplate;
			}
			wantedTemplate.DefaultParts[partTypes[i]].PartState = EPartState.New;
		}
		
		return (wantedTemplate, givenTemplate);
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
			//Example of changing the Camera Target.
			//Global.Camera.SmoothingSpeed = 1;
			//Global.Camera.FollowingNode = GetNode("GameWorld/Entities/Dispenser14") as Node2D;
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
		if (!NpcSpawning(npc)) return;
		EModularItemType[] itemTypes = [EModularItemType.Sword, EModularItemType.Spear, EModularItemType.Bow,  EModularItemType.Chestplate, EModularItemType.Staff];
		//EModularItemType[] itemTypes = [EModularItemType.Bow];
		ModularItemTemplate wantedTemplate;
		ModularItemTemplate givenTemplate;
		
		(wantedTemplate, givenTemplate) = RandomItemTemplates(itemTypes[GD.RandRange(0, itemTypes.Length - 1)]);
		if (wantedTemplate == null || givenTemplate == null) return;
		
		var modularItemScene = GD.Load<PackedScene>("res://Entities/Interactive/Items/ModularItem.tscn");
		var newItem = modularItemScene.Instantiate<ModularItem>();
		newItem.ModularItemType = givenTemplate.ModularItemType;
		newItem.ItemResource = givenTemplate;
		
		newItem.OwningNpc = npc;
		npc.WantedItemTemplate = wantedTemplate;
		
		Global.Grid.AddChild(newItem);
		npc.NpcInventory.PickupItem(newItem, true);
	}
	private void OnHostileNpcTimerTimeout()
	{
		HostileNpc hostileNpc = HostileNpcScene.Instantiate<HostileNpc>();
		NpcSpawning(hostileNpc);
	}

	private bool NpcSpawning( dynamic npc)
	{
		//GD.Print(npc.GetType() );
		if (npc is HostileNpc npcHostile)
		{
			if (_hostileNpc.Count >= MaxNpcCount) return false;
			npcHostile.NpcResource = HostileNpcResources[GD.RandRange(0,HostileNpcResources.Count -1)];
			_hostileNpcSpawnLocation.ProgressRatio = GD.Randf();
			npcHostile.Position = _hostileNpcSpawnLocation.Position;
			npcHostile.Scale = Vector2.One;
			_hostileNpc.Add(npcHostile);
			
			_entities.AddChild(npcHostile);
			
		}
		else if  (npc is Npc.Friendly.Npc npcFriendly)
		{
			if (_npcs.Count >= MaxNpcCount)return false;
			npcFriendly.NpcResource = NpcResources[GD.RandRange(0,NpcResources.Count -1)];
			npcFriendly.Position = _npcSpawnLocation.Position;
			npcFriendly.Scale = Vector2.One;
			//npc.Target = _frontDesk;
			npcFriendly.LeaveAreaNode = _leaveAreaNode;
			_npcs.Add(npcFriendly);
			_entities.AddChild(npcFriendly);
			var npcInteractor = (NpcInteractor)npcFriendly.FindChild("NpcInteractor");
			RequestToJoin(npcFriendly);
			npcInteractor.RequestComplete += OnRequestComplete;
			npcFriendly.LeftQueue += LeaveQueue;
			npcInteractor.CombatEnded += OnCombatEnded;
		}
		return true;
	}

	private void OnCombatEnded(Npc.Friendly.Npc npc, HostileNpc hostileNpc)
	{
		_hostileNpc.Remove(hostileNpc);
		_npcs.Remove(npc);
		hostileNpc.QueueFree();
		npc.QueueFree();
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