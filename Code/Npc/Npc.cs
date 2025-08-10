using Godot;

namespace SoYouWANNAJam2025.Code.Npc;
[Tool]
public partial class Npc : CharacterBody2D
{
    [Signal]
    public delegate void DoBehaviorEnabledEventHandler();
    
    private Texture2D _lastTexture;
    
    public string State = "idle";
    public Vector2 Direction = Vector2.Down;
    private string _directionName= "down";
    public bool DoBehaviour = true;
    
    private NpcResource _npcResource;
    [Export]
    public NpcResource NpcResource
    {
        get => _npcResource;
        set => _setNpcResource(value);
    }

    private AnimationPlayer _animationPlayer;
    private Sprite2D _sprite2D;
    
    public override void _Ready()
    {
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _sprite2D = GetNode<Sprite2D>("Sprite2D");
        if (NpcResource != null)
            SetupNpc();
        if(Engine.IsEditorHint()) return;
        EmitSignal(SignalName.DoBehaviorEnabled);
    }


    public override void _PhysicsProcess(double delta)
    {
        MoveAndSlide();
    }

    public void UpdateAnimation()
    {
        _animationPlayer.Play( State + "_" + _directionName );
    }

    public void UpdateDirection(Vector2 targetPosition)
    {
        Direction = GlobalPosition.DirectionTo(targetPosition);
        UpdateDirectionName();
        
    }

    private void UpdateDirectionName()
    {
        const float threshold = 0.45f;
        _directionName = Direction.Y switch
        {
            < -threshold => "up",
            > threshold => "down",
            _ => Direction.X switch
            {
                < -threshold => "left",
                > threshold => "right",
                _ => _directionName
            }
        };
    }

    private void SetupNpc()
    {
        if (NpcResource != null)
        {
            _sprite2D.Texture = _npcResource.Sprite2D;
        }
    }
    
    private void _setNpcResource(NpcResource npc)
    {
        if (npc == null) return;
        _npcResource = npc;
        if (IsInsideTree() && _sprite2D != null)
            SetupNpc();
    }
}