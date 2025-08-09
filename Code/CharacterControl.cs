using Godot;
namespace SoYouWANNAJam2025.Code;

public partial class CharacterControl : CharacterBody2D
{
    private int Speed = 32;

    public override void _PhysicsProcess(double delta)
    {
        var _charDir = Input.GetVector("left", "right", "up", "down");

        if (_charDir != Vector2.Zero)
        {

            var _char = GetNode<Sprite2D>("Character");
            Velocity = _charDir * Speed;
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