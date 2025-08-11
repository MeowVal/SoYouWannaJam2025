using System;
using Godot;

namespace SoYouWANNAJam2025.Code.Npc;
[Tool]
public partial class Npc : CharacterBody2D
{
    [Signal]
    public delegate void DoBehaviorEnabledEventHandler();
    
    private Texture2D _lastTexture;
    [Export] public float MoodDecreaseTimer = 10f;
    [Export] public float MoodDecreaseAmount = 10;
    [Export] private float _speed = 30f;
    public string State = "idle";
    public Vector2 Direction = Vector2.Down;
    private string _directionName= "Down";
    public bool DoBehaviour = true;
    [Export] public Node2D Target;
    [Export] public float Mood = 100;
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
    private Vector2 _lastTargetPosition = Vector2.Zero;  // Store last target position
    private float _stopThreshold = 5.0f;  // The threshold distance at which the NPC will stop or no longer recalculate path
    private float _pathUpdateThreshold = 32.0f;  // The distance at which to update the path
    [Export] public Node2D LeaveAreaNode;
    public override void _Ready()
    {
        _navAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _sprite2D = GetNode<Sprite2D>("Sprite2D");
        CallDeferred("SetupNavAgent");
        if (NpcResource != null)
            SetupNpc();
        if(Engine.IsEditorHint()) return;
        UpdateMood();
        Callable callable = new Callable(this, "_VelocityComputed");
        _navAgent.Connect("velocity_computed", callable);
        //PathTimer();
    }
    
    public override void _PhysicsProcess(double delta)
    {
        if(Engine.IsEditorHint()) return;
        if (_navAgent == null) return;
        if (_navAgent.IsNavigationFinished() && Mood ==0) Free();
        if (_navAgent.IsNavigationFinished()) return;
        if (Target == null) return;
        
        // Get pathfinding information
        var currentAgentPosition = GetGlobalPosition();
        var nextPathPosition = _navAgent.GetNextPathPosition();
        //Makes sure the path only gets updated when necessary 
        if (_lastTargetPosition.DistanceTo(nextPathPosition) > _pathUpdateThreshold)
        {
            _navAgent.TargetPosition = Target.GlobalPosition;
            _lastTargetPosition = nextPathPosition;  
        }
        // Calculate the new direction
        var dir = currentAgentPosition.DirectionTo(nextPathPosition).Normalized();
        var newVelocity = dir * _speed;
        
    
        if (currentAgentPosition.DistanceTo(nextPathPosition) <= _stopThreshold)
            newVelocity = Vector2.Zero;  // Stop the NPC if close enough
        
        // Set the correct velocity
        if (_navAgent.AvoidanceEnabled)
        {
            _navAgent.SetVelocity(newVelocity);
        }
        else
        {
            _VelocityComputed(newVelocity);
        }
        Direction = dir;
        UpdateDirection(GlobalPosition + dir);
        UpdateAnimation();
        MoveAndSlide();
    }

    private async void SetupNavAgent()
    {
        try
        {
            await ToSignal(GetTree(), "physics_frame");
            
            if(Target == null) return;
            _navAgent.TargetPosition = Target.GlobalPosition;
            //Free();
        }
        catch (Exception e)
        {
            GD.Print("This is bad...");
            GD.PrintErr(e);
        }
    }

    private void _VelocityComputed(Vector2 safeVelocity)
    {
        Velocity = safeVelocity;
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

    public async void UpdateMood()
    {
        while ((Mood > 0))
        {
            await ToSignal(GetTree().CreateTimer(MoodDecreaseTimer), "timeout");
            Mood -= MoodDecreaseAmount;
            GD.Print("Mood: " + Mood);
        }
        Target = LeaveAreaNode;
        if (Target == null) return;
        _navAgent.TargetPosition = Target.GlobalPosition;
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