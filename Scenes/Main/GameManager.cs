using Godot;
using SoYouWANNAJam2025.Code.Npc;

namespace SoYouWANNAJam2025.Scenes.Main;

public partial class GameManager : Node2D
{
	public Resource[] PossibleTargets;
	public Godot.Collections.Array<NpcResource> NpcResources = [];
	[Export]
	public PackedScene NpcScene { get; set; }

	public float gold = 0; 
	public override void _Ready()
	{
		//NpcResources[0] = ResourceLoader.LoadThreadedRequest<NpcResource>("res://Resources/Npcs/Npc01.tres");
		DirContents("res://Resources/Npcs/");
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
		
		Npc npc = NpcScene.Instantiate<Npc>();
		GD.Print(GD.RandRange(0,NpcResources.Count ));
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
}