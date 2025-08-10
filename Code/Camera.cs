using Godot;

namespace SoYouWANNAJam2025.Code;

public partial class Camera : Camera2D
{
    private Node2D _followingNode;

    [Export]public Node2D FollowingNode
    {
        get => _followingNode;
        set {_followingNode = value; }
    }
    
    [Export(PropertyHint.Range, "1, 10")] public float SmoothingSpeed = 3.0f;

    
    private Vector2 _oldResolution;

    public override void _Ready()
    {
        GlobalPosition = FollowingNode.GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GetWindow().Size != _oldResolution)
        {
            _oldResolution = GetWindow().Size;
            GetNode<Control>("Hud").Position = -0.5f * _oldResolution;
        }
        
        GlobalPosition = GlobalPosition.Lerp(FollowingNode.GlobalPosition, (float)(SmoothingSpeed*Global.GameScale*delta));
    }
}
