using Godot;

namespace SoYouWANNAJam2025.Code.Npc;
[Tool]
public partial class Npc : CharacterBody2D
{
    [Signal]
    public delegate void DoBehaviorEnabledEventHandler();
    
    private Texture2D _lastTexture;
    
    private string _state = "idle";
    private Vector2 _direction = Vector2.Down;
    private string _directionSprite = "down";
    
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
    }
    
    private void SetupNpc()
    {
        if (NpcResource != null)
        {
            _sprite2D.Texture = _npcResource.Sprite2D;
        }
    }
    
    private void _setNpcResource(NpcResource value)
    {
        if (value == null) return;
        _npcResource = value;
        if (IsInsideTree() && _sprite2D != null)
            SetupNpc();
    }
}