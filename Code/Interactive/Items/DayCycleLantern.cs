using Godot;
using System;
using SoYouWANNAJam2025.Code;

public partial class DayCycleLantern : Node2D
{
    [Signal]
    public delegate void CycleLanternEventHandler(bool onCycleLantern);

    [Export] public Node2D Lighting;
    [Export] public float CycleTime = 120f;
    [Export] public float DawnTime = 5f;
    [Export] public double DawnFrame = 0.2;

    private Interactible _interact;
    private Timer _timer;
    private Timer _morningTimer;
    private AnimationPlayer _animator;

    public override void _Ready()
    {
        _timer = this.GetNode<Timer>("Timer");
        _timer.Timeout += OnTimerTimeout;
        
        _morningTimer = this.GetNode<Timer>("MorningTimer");
        _morningTimer.Timeout += OnMorningTimeout;

        _interact = this.GetNode<Interactible>("Interactible");
        _interact.Interact += OnInteractMethod;

        _animator = Lighting.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        _animator.Play("DayNightCycle");//Init Animation
        _animator.Stop();
        
        //_animator.Seek(0.5, true);
        
        _timer.SetWaitTime(CycleTime);
        _morningTimer.SetWaitTime(DawnTime);
    }

    private async void OnInteractMethod(Node2D node, TriggerType trigger)
    {
        GD.Print("test");
        if (trigger is TriggerType.UseAction && _timer.IsStopped())
        {
            GD.Print("Started DayNightCycle");
            if (DawnFrame != 0)
            {
                _morningTimer.Start();
                _timer.Start();
                _animator.PlaySection("DayNightCycle", 0, DawnFrame, -1, 1 / DawnTime * (float)DawnFrame);
            }
            else
            {
                _timer.Start();
                _animator.PlaySection("DayNightCycle",0,1,-1,1/CycleTime);
                EmitSignal(SignalName.CycleLantern, true);
            }
        }
    }

    private void OnMorningTimeout()
    {
        _animator.Pause();
        _animator.PlaySection("DayNightCycle",0,1,-1,1/CycleTime*(float)DawnFrame);
        EmitSignal(SignalName.CycleLantern, true);
    }
    
    private void OnTimerTimeout()
    {
        GD.Print("DayNightCycle Timed Out");
        EmitSignal(SignalName.CycleLantern, false);
        _animator.Stop();
    }
}
