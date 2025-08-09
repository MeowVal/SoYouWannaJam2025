using Godot;

namespace SoYouWANNAJam2025.Entities.NPC;

public partial class Npc : RigidBody3D
{
    public Sprite3D sprite;
    public override void _Ready()
    {
        sprite = GetNode<Sprite3D>("Sprite");
    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }
    
}