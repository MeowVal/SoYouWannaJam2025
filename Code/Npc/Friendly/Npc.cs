using System;
using Godot;
using SoYouWANNAJam2025.Code.Interactive.Inventory;
using SoYouWANNAJam2025.Code.Npc.Hostile;

namespace SoYouWANNAJam2025.Code.Npc.Friendly;
[Tool]
public partial class Npc : CharacterBody2D
{
    [Signal]
    public delegate void DoBehaviorEnabledEventHandler(); // Signal for enabling behaviours 
    [Export] public Timer MoodTimer; // How often the npc's mode decreases
    [Export] public float MoodDecreaseAmount = 10; // How much the npc's mood decreases by
    [Export] private float _speed = 30f; // How fast the npc moves
    public string State = "idle"; // The animation stat the npc is in
    public Vector2 Direction = Vector2.Down; // The direction the npc is facing
    private string _directionName= "Down"; // Variable for storing npc direction
    public bool DoBehaviour = true; // Enables npc behaviours in the scene
    [Export] public Node2D Target; // The target the npc move towards when spawned 
    [Export] public float Mood = 100; // The mood of the npc 
    private NavigationAgent2D _navAgent; // navigation agent for pathfinding
    private NpcResource _npcResource ; // Local variable to store the npc resource
    [Export] public float Health = 100;
    [Export]
    public NpcResource NpcResource // The npc resource
    {
        get => _npcResource;
        set => _setNpcResource(value);
    }

    private AnimationPlayer _animationPlayer; // The animation player for playing the animations on the npc
    private Sprite2D _sprite2D; // The sprite of the npc
    public Inventory NpcInventory;
    private Vector2 _lastTargetPosition = Vector2.Zero;  // Store last target position
    private float _stopThreshold = 5.0f;  // The threshold distance at which the NPC will stop or no longer recalculate the path
    private float _pathUpdateThreshold = 32.0f;  // The distance at which to update the path
    [Export] public Node2D LeaveAreaNode; // The node for where the npc should go to when it leaves when it's not satisfied 
    public bool StartMoodTimer = true;
    public override void _Ready()
    {
        // gets the different nodes in the tree that is a child of the npc 
        _navAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        _animationPlayer = GetNode<AnimationPlayer>("NpcInteractor/AnimationPlayer");
        _sprite2D = GetNode<Sprite2D>("NpcInteractor/Sprite2D");
        if (GetNode("NpcInteractor/Inventory") is Inventory slot) NpcInventory = slot;
        MoodTimer = GetNode<Timer>("MoodTimer");
        if (NpcResource != null)
            SetupNpc();
        // So that the editor does not run the code below it. 
        if(Engine.IsEditorHint()) return;
        // Connects to the MoodTimer 
        MoodTimer.Timeout += UpdateMood; 
        // Sets up the navigation agent after the first physics frame 
        CallDeferred("SetupNavAgent");
        // Starts the MoodTimer
        MoodTimer.Start();
        // Connects to the signal from the navigation agent 
        Callable callable = new Callable(this, "_VelocityComputed");
        _navAgent.Connect("velocity_computed", callable);
    }
    
    // Used for Npc the pathfinding 
    public override void _PhysicsProcess(double delta)
    {
        
        // So that the editor does not run the code below it. 
        if(Engine.IsEditorHint()) return;
        if (_navAgent == null) return;
        //if (_navAgent.IsNavigationFinished() && Mood <=0) QueueFree();
        if (_navAgent.IsNavigationFinished())
        {
            State = "idle";
            if (Target is not HostileNpc) return;
        }
        if (Target == null) return;
        State = "walk";
        
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


        /*if (currentAgentPosition.DistanceTo(nextPathPosition) <= _stopThreshold)
        {
            newVelocity = Vector2.Zero; // Stop the NPC if close enough
            
        }*/
             
        
        // Set the correct velocity
        if (_navAgent.AvoidanceEnabled)
        {
            _navAgent.SetVelocity(newVelocity);
        }
        else
        {
            _VelocityComputed(newVelocity);
        }
        State = "walk";
        Direction = dir;
        UpdateDirection(GlobalPosition + dir);
        UpdateAnimation();
        MoveAndSlide();
        _sprite2D.GlobalPosition = GlobalPosition.Round();
        NpcInventory.GlobalPosition = GlobalPosition.Round();
    }
    
    // Sets up the navigation agent for the pathfinder
    private async void SetupNavAgent()
    {
        try
        {
            // waits until the first physics frame has rendered 
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
    
    // The velocity calculated by the navigation agent for avoiding an obstacle
    private void _VelocityComputed(Vector2 safeVelocity)
    {
        if(Engine.IsEditorHint()) return;
        Velocity = safeVelocity;
    }
    
    // Used to call the right animation on the animation player
    public void UpdateAnimation()
    {
        if(_animationPlayer == null) return;
        // The names if the animations is a combo of State and _directionName like the state idle and the _directionName Left 
        _animationPlayer.Play( State + _directionName );
    }

    // Used to get the direction that the npc is moving
    public void UpdateDirection(Vector2 targetPosition)
    {
        Direction = GlobalPosition.DirectionTo(targetPosition);
        UpdateDirectionName();
        
    }

    // Timer to update the mood of the npc 
    public void UpdateMood()
    {
        
            
        Mood -= MoodDecreaseAmount;
        //GD.Print("Mood: " + Mood);
        //GD.Print(Mood <= 0);
        if(Mood <= 0)
        {
            //GD.Print("Leaving");
            Target = LeaveAreaNode;
            if (Target == null) return;
            _navAgent.TargetPosition = Target.GlobalPosition;
            StartMoodTimer =  false;
            MoodTimer.Stop();
            
        }
        
    }

    // Used for determining the animation depending on the direction the npc is moving
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

    // Sets the sprite of the npc from the npc resource
    private void SetupNpc()
    {
        if (NpcResource != null)
            _sprite2D.Texture = _npcResource.Sprite2D;
        
    }
    
    // Used to set the npc resource
    private void _setNpcResource(NpcResource npc)
    {
        if (npc == null) return;
        _npcResource = npc;
        if (IsInsideTree() && _sprite2D != null)
            SetupNpc();
    }
}