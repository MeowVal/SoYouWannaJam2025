using Godot;

namespace SoYouWANNAJam2025.Code.Camera;

public partial class SubPixelCamera : Camera2D
{
    [Export] public Node2D FollowingNode;
    [Export(PropertyHint.Range, "1, 10")] public double SmoothingSpeed = 5.0;
    
    private Vector2 _actualCamPos;

    public override void _Ready()
    {
        _actualCamPos = FollowingNode.GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
      if (FollowingNode != null)
      {
          var weight = 0.0f;
          var camPos = FollowingNode.GlobalPosition;
          weight = (float)((11-SmoothingSpeed)/100);
          _actualCamPos.X = Mathf.Lerp(GlobalPosition.X, FollowingNode.GlobalPosition.X, weight);
          _actualCamPos.Y = Mathf.Lerp(GlobalPosition.Y, FollowingNode.GlobalPosition.Y, weight);
      }
      GlobalPosition = _actualCamPos.Floor();
    }
}