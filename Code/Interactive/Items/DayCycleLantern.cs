using Godot;
using System;
using SoYouWANNAJam2025.Code;
using SoYouWANNAJam2025.Code.World;
using SoYouWANNAJam2025.Scenes.UI;

public partial class DayCycleLantern : Node2D
{
    [Signal]
    public delegate void CycleLanternEventHandler(bool onCycleLantern);

    [Export] public Node2D Lighting;
    [Export] public double CycleTime = 120;
    [Export] public double DawnTime = 5;
    [Export] public double DawnFrame = 0.2;

    private Interactible _interact;
    private AnimationPlayer _animator;
    private bool _dawnHandled = false;

    public override void _Ready()
    {
        _interact = this.GetNode<Interactible>("Interactible");
        _interact.Interact += OnInteractMethod;

        _animator = Lighting.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        _animator.Play("DayNightCycle");//Init Animation
        _animator.Stop();
    }

 
    private void OnInteractMethod(Node2D node, TriggerType trigger)
    {
        if (trigger is TriggerType.UseAction && !_animator.IsPlaying())
        {
            GD.Print("Started DayNightCycle");
            if (DawnFrame > 0.0)
            {
                _animator.SetSpeedScale(1 / (float)DawnTime * (1-(float)DawnFrame));
                _animator.Play();
            }
            else
            {
                _animator.SetSpeedScale(1/(float)CycleTime * (1-(float)DawnFrame));
                _animator.Play();
                EmitSignal(SignalName.CycleLantern, true);
            }
        }
    }

    private void OnDawnFrame()
    {
        _dawnHandled = true;
        _animator.SetSpeedScale(1/(float)CycleTime * (1-(float)DawnFrame));
        EmitSignal(SignalName.CycleLantern, true);
    }

    private void OnMidnight()
    {
        GD.Print("DayNightCycle Timed Out");
        EmitSignal(SignalName.CycleLantern, false);
        _animator.Stop();
    }

    public override void _Process(double delta)
    {
        if (_animator.IsPlaying())
        {
            Global.GameTimer = (float)_animator.CurrentAnimationPosition;
            
            if (_animator.CurrentAnimationPosition > DawnFrame && !_dawnHandled)
            {
                GD.Print("DawnFrameHandled?");
                OnDawnFrame();
            }
            else if  (_animator.CurrentAnimationPosition > 0.9995)
            {   
                GD.Print("MidnightFrameHandled?");
                OnMidnight();
            }
        }
    }
}

