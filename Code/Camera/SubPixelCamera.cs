using Godot;

namespace SoYouWANNAJam2025.Code.Camera;

public partial class SubPixelCamera : Camera2D
{
    [Export] public Node2D FollowingNode;
    [Export] public double FollowSpeed = 5.0;
    
    private Vector2 _actualCamPos;

    public override void _Ready()
    {
        _actualCamPos = FollowingNode.GlobalPosition;
    }

    public override void _Process(double delta)
    {
        _actualCamPos = _actualCamPos.Lerp(FollowingNode.GlobalPosition, (float)(FollowSpeed*delta));
        //_actualCamPos = FollowingNode.GlobalPosition;

        if (FindParent("SubViewportContainer") is SubViewportContainer { Material: ShaderMaterial shaderMaterial })
        {
            shaderMaterial.SetShaderParameter("cam_offset", _actualCamPos.Round() - _actualCamPos);
        }

        GlobalPosition = _actualCamPos.Round();
    }
}