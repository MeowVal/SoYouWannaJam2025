using System;
using System.Collections.Generic;
using Godot;

namespace SoYouWANNAJam2025.Code.Npc;
[Tool]
public partial class NpcBehaviourWander : NpcBehaviour
{
    public static readonly Vector2[] Directions = [Vector2.Up, Vector2.Right, Vector2.Down, Vector2.Left];
    private CollisionShape2D _collisionShape;
    
    private int _wanderRange = 2;
    [Export] 
    public int WanderRange {
        get => _wanderRange;
        set => _setWanderRange(value);
    }

    [Export] public float WanderSpeed = 30.0f;
    [Export] public float WanderDuration = 1.0f;
    [Export] public float IdleDuration = 1.0f;

    private Vector2 _originalPosition;

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;
        base._Ready();
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        _collisionShape.QueueFree();
        _originalPosition = Npc.GlobalPosition;
    }

    public override async void  Start()
    {
        try
        {
            if(Npc.DoBehaviour == false) return;
            Npc.State = "idle";
            Npc.Velocity = Vector2.Zero;
            Npc.UpdateAnimation();
            await ToSignal(GetTree().CreateTimer(GD.Randf() * IdleDuration + IdleDuration * 0.5), "timeout");
            Npc.State = "walk";
            var dir = Directions[GD.RandRange(0, 3)];
            Npc.Direction = dir;
            Npc.Velocity = WanderSpeed * dir;
            Npc.UpdateDirection(GlobalPosition + dir);
            Npc.UpdateAnimation();
            
            await ToSignal(GetTree().CreateTimer(GD.Randf() * WanderDuration + WanderDuration * 0.5), "timeout");
            if (Npc.DoBehaviour == false) return; 
            Start();
        }
        catch (Exception e)
        {
            throw; // TODO handle exception
        }
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint()) return;
        if (Math.Abs(GlobalPosition.DistanceTo(_originalPosition)) > _wanderRange * 32)
        {
            Npc.Velocity *= -1;
            Npc.Direction *= -1;
            Npc.UpdateDirection(GlobalPosition + Npc.Direction);
            Npc.UpdateAnimation();
        }
    }

    private void _setWanderRange(int v)
    {
        _wanderRange = v;
        
        if (_collisionShape?.Shape is CircleShape2D circleShape)
        {
            circleShape.Radius = v * 32.0f;
        }
    }

}


