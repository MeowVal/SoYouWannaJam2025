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
        //seperated value for funky cam stuff down the road?
        _actualCamPos.X = Mathf.Lerp(_actualCamPos.X, FollowingNode.GlobalPosition.X, (float)(FollowSpeed*delta));
        _actualCamPos.Y = Mathf.Lerp(_actualCamPos.Y, FollowingNode.GlobalPosition.Y, (float)(FollowSpeed*delta));
        
        if (FindParent("SubViewportContainer") is SubViewportContainer { Material: ShaderMaterial shaderMaterial })
        {
            shaderMaterial.SetShaderParameter("cam_offset", _actualCamPos.Round() - _actualCamPos);
        }

        GlobalPosition = _actualCamPos.Round();
    }
}