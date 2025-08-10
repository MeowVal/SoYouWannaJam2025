using System.Numerics;
using Godot;
using Vector2 = Godot.Vector2;

namespace SoYouWANNAJam2025.Code;

public partial class CharacterControl : CharacterBody2D
{
    private int _speed = 32;

    public override void _Input(InputEvent @event)
    {
        var _cam = GetNode<Camera2D>("Character/Camera2D");
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
            {
                var oldZoom = _cam.GetZoom();
                _cam.SetZoom(new Vector2(Mathf.Clamp(oldZoom.X+1, 1,10),Mathf.Clamp(oldZoom.Y+1, 1,10)));
            }
            else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
            {
                var oldZoom = _cam.GetZoom();
                _cam.SetZoom(new Vector2(Mathf.Clamp(oldZoom.X-1, 1,10),Mathf.Clamp(oldZoom.Y-1, 1,10)));
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        var _char = GetNode<Sprite2D>("Character");
        var _charDir = Input.GetVector("left", "right", "up", "down");
        
        if (_charDir != Vector2.Zero)
        {
            GD.Print(Input.GetActionStrength("down"));
            GD.Print(Input.GetActionStrength("up"));
            GD.Print(Input.GetActionStrength("right"));
            GD.Print(Input.GetActionStrength("left"));

            Velocity = _charDir * _speed;
            if (_charDir.X == 1)
            {
                _char.RegionRect = new Rect2(0, 32, 32, 32); // character left sprite
            }
            else if (_charDir.X == -1)
            {
                _char.RegionRect = new Rect2(32, 32, 32, 32); // character right sprite
            }
            else if (_charDir.Y == 1)
            {
                _char.RegionRect = new Rect2(0, 0, 32, 32); // character up sprite
            }
            else if (_charDir.Y == -1)
            {
                _char.RegionRect = new Rect2(32, 0, 32, 32); // character down sprite
            }

        } 
        else
        {
            Velocity = Vector2.Zero;
        }
        MoveAndSlide();
    }
}