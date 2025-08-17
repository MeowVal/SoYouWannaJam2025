using Godot;

namespace SoYouWANNAJam2025.Code.World;

public partial class Camera : Camera2D
{
    private Node2D _followingNode;
    private Vector2 _globalPosition;

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
        _globalPosition =  GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        _globalPosition = _globalPosition.Lerp(FollowingNode.GlobalPosition, (float)(SmoothingSpeed * delta));
        GlobalPosition = _globalPosition.Round();
    }
}
