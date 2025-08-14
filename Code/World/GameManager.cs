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
	[Export]
	public Vector2 TileSize = new Vector2(32, 16);

	public float Gold = 0; 
	
	[Export] public DayCycleLantern CycleLantern;
	private Godot.Collections.Array<Npc.Npc> _npcs;
	[Export] public int NpcMaxCount = 20;
	private Node2D _npcSpawnLocation;
	private Node2D _frontDesk;
	private Area2D _leaveAreaNode;
	
	public override void _Ready()
	{
		DirContents("res://Resources/Npcs/");
		_npcSpawnLocation = (Node2D)FindChild("NpcArrivalArea");
		_frontDesk  = (Node2D)FindChild("FrontDesk");
		_leaveAreaNode = (Area2D)FindChild("LeaveArea");
		var npcInteractor = (NpcInteractor)FindChild("NpcInteractor");

		npcInteractor.NpcLeft += OnNpcLeft;
		GD.Print(CycleLantern);
		if (CycleLantern == null) return;
		CycleLantern.CycleLantern += LanternChanged;
		
	}

	private void OnNpcLeft(Npc.Npc npc)
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
		if (_npcs.Count >= NpcMaxCount) return;
		Npc.Npc npc = NpcScene.Instantiate<Npc.Npc>();
		npc.NpcResource = NpcResources[GD.RandRange(0,NpcResources.Count -1)];
		npc.Position = _npcSpawnLocation.Position;
		npc.Scale = Vector2.One * 4;
		npc.Target = _frontDesk;
		npc.LeaveAreaNode = _leaveAreaNode;
		_npcs.Add(npc);
		AddChild(npc);
	}
	
	// Called when the node enters the scene tree for the first time.
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	
	
}