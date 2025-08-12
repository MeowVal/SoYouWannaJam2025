using Godot;
using SoYouWANNAJam2025.Code.Npc;

namespace SoYouWANNAJam2025.Scenes.Main;

public partial class GameManager : Node2D
{
	[Export]
	public PackedScene NpcScene { get; set; }

	private void OnNpcTimerTimeout()
	{
		Npc npc = NpcScene.Instantiate<Npc>();
		
		var npcSpawnLocation = GetNode<PathFollow2D>("NpcPath/NpcSpawnLocations");
		npcSpawnLocation.ProgressRatio = GD.Randf();
		npc.Position = npcSpawnLocation.Position;
		npc.Scale = Vector2.One * 4;
		npc.Target = GetNode<Area2D>("Isometric/CraftingBench");
		npc.LeaveAreaNode = GetNode<Node2D>("LeaveArea");
		AddChild(npc);
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}