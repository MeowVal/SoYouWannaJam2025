using Godot;
using System;
using SoYouWANNAJam2025.Code;

public partial class DayCycleLantern : Node2D
{
    [Signal]
    public delegate void CycleLanternEventHandler(bool onCycleLantern);

    [Export] public Node2D Lighting;
    [Export] public float CycleTime = 120f;

    private Interactible _interact;
    private Timer _timer;
    private AnimationPlayer _animator;

    public override void _Ready()
    {
        _timer = this.GetNode<Timer>("Timer");
        _timer.Timeout += OnTimerTimeout;

        _interact = this.GetNode<Interactible>("Interactible");
        _interact.Interact += OnInteractMethod;

        _animator = Lighting.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        _animator.Play("DayNightCycle");//Init Animation
        _animator.Stop();
        
        //_animator.Seek(0.5, true);
        
        _timer.SetWaitTime(CycleTime);
    }

    private async void OnInteractMethod(Node2D node, TriggerType trigger)
    {
        GD.Print("test");
        if (trigger is TriggerType.UseAction && _timer.IsStopped())
        {
            GD.Print("Started DayNightCycle");
            _timer.Start();
            _animator.PlaySection("DayNightCycle",0,1,-1,1/CycleTime);
            EmitSignal(SignalName.CycleLantern, true);
        }
    }

    private void OnTimerTimeout()
    {
        GD.Print("DayNightCycle Timed Out");
        EmitSignal(SignalName.CycleLantern, false);
        _animator.Stop();
    }
}
