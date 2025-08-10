using System;
using System.Collections.Generic;
using Godot;

namespace SoYouWANNAJam2025.Code.Npc;
[Tool]
public partial class NpcBehaviourWander : NpcBehaviour
{
    public static readonly Vector2[] Directions = [Vector2.Up, Vector2.Right, Vector2.Down, Vector2.Left];
    [Export]
    private float _scale = 32f;
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
        CollisionShape2D collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        collisionShape.QueueFree();
        FixedUpdate();
        _originalPosition = Npc.GlobalPosition;
    }

    public override async void  Start()
    {
        try
        {
            while (true)
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
            }
        }
        catch (Exception e)
        {
            GD.Print("This is bad...");
            GD.PrintErr(e);
        }
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint()) return;
    }

    private async void FixedUpdate()
    {
        try
        {
            while (true)
            {
                // Calculates the distance to the center
                var distanceToCenter = GlobalPosition.DistanceTo(_originalPosition);
                // Radius of the wander circle
                var circleRadius = _wanderRange * _scale;
                // Distance to the edge of the circle 
                var distanceFromEdge = distanceToCenter - circleRadius;
                
                var tolerance = 0.01f;
                
                if (distanceFromEdge> tolerance)
                {
                    // Calculates the direction to the center of the wander circle from the npc 
                    Vector2 directionToCenter = GlobalPosition.DirectionTo(_originalPosition).Normalized();
                    
                    var cardinalDirection = GetClosestCardinalDirection(directionToCenter);
             
                    Npc.Velocity = cardinalDirection * WanderSpeed;
                    Npc.Direction = cardinalDirection;
                    Npc.UpdateDirection(GlobalPosition + Npc.Direction);
                    Npc.UpdateAnimation();

                }
                

                // Wait for the next fixed update interval
                await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
            }
        }
        catch (Exception e)
        {
            GD.Print("This is bad... It will break the wander update loop...");
            GD.PrintErr(e);
        }
    }
    
    private static Vector2 GetClosestCardinalDirection(Vector2 direction)
    {
        // Checks if X is bigger than Y to determine if it should go left and right or up and down
        if (Math.Abs(direction.X) > Math.Abs(direction.Y))
        {
            // Move horizontally (either Left or Right)
            return direction.X > 0 ? Vector2.Right : Vector2.Left;
        } 
        else
        {
            // Move vertically (either Up or Down)
            return direction.Y < 0 ? Vector2.Up : Vector2.Down;
        }
    }
    

    private void _setWanderRange(int v)
    {
        _wanderRange = v;
        if (GetNode<CollisionShape2D>("CollisionShape2D").Shape is CircleShape2D shape)
        {
            shape.Radius = v * _scale;
        }
    }

}


