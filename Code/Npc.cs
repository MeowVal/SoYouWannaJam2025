using Godot;

namespace SoYouWANNAJam2025.Code;

public partial class Npc : CharacterBody2D
{
    private string _state = "idle";
    private Vector2 _direction = Vector2.Down;
    private string _directionSprite = "down";
    [Export]
    public NpcResource NpcResource;
    public override void _Ready()
    {
        
        base._Ready();
    }
}