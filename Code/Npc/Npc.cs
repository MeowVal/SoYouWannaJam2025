using System;
using Godot;

namespace SoYouWANNAJam2025.Code.Npc;
[Tool]
public partial class Npc : CharacterBody2D
{
    [Signal]
    public delegate void DoBehaviorEnabledEventHandler();
    
    private Texture2D _lastTexture;
    
    [Export] private float _speed = 30f;
    [Export] public float NavTimer = 0.5f; 
    public string State = "idle";
    public Vector2 Direction = Vector2.Down;
    private string _directionName= "Down";
    public bool DoBehaviour = true;
    [Export]
    public Node2D Target;
    
    private NavigationAgent2D _navAgent;
    private NpcResource _npcResource ;
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
        _navAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _sprite2D = GetNode<Sprite2D>("Sprite2D");
        _navAgent.TargetPosition = Target.Position;
        if (NpcResource != null)
            SetupNpc();
        if(Engine.IsEditorHint()) return;
        EmitSignal(SignalName.DoBehaviorEnabled);
        PathTimer();
    }


    public override void _PhysicsProcess(double delta)
    {
        if(Engine.IsEditorHint()) return;
        if (_navAgent == null) return;
        if (_navAgent.IsTargetReached()) return;
        
        var targetPosition = _navAgent.GetTargetPosition();
        var directionToTarget = targetPosition - GlobalPosition;
        if (directionToTarget.Length() < _speed )
        {
            Velocity = Vector2.Zero;
            Direction = Vector2.Zero;
            return; // Stop movement if close enough
        }
        var navPointDir = directionToTarget.Normalized();
        Velocity = navPointDir * _speed ; 
        Direction = navPointDir;
        UpdateDirection(GlobalPosition + navPointDir);
        UpdateAnimation();
        MoveAndSlide();
    }

    private void MakePath()
    {
        _navAgent.TargetPosition = Target.GlobalPosition;
    }

    private async void PathTimer()
    {
        while (true)
        {
            MakePath();
            await ToSignal(GetTree().CreateTimer( NavTimer), "timeout");
        }
        
    }
    

    public void UpdateAnimation()
    {
        if(_animationPlayer == null) return;
        _animationPlayer.Play( State + _directionName );
    }

    public void UpdateDirection(Vector2 targetPosition)
    {
        Direction = GlobalPosition.DirectionTo(targetPosition);
        UpdateDirectionName();
        //GD.Print(Direction+ _directionName);
        
    }

    private void UpdateDirectionName()
    {
        const float threshold = 0.45f;
        
        if (Direction.Y < -threshold)
        {
            _directionName = "Up";
        }
        else if (Direction.Y > threshold)
        {
            _directionName = "Down";
        }
        else if (Direction.X > threshold)
        {
            _directionName = "Right";
        }
        else if (Direction.X < -threshold)
        {
            _directionName = "Left";
        }
    
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