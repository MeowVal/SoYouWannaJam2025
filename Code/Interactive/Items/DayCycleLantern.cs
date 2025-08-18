using Godot;
using System;
using System.Collections.Generic;
using SoYouWANNAJam2025.Code;
using SoYouWANNAJam2025.Code.World;
using SoYouWANNAJam2025.Scenes.UI;

public partial class DayCycleLantern : Node2D
{
    [Signal]
    public delegate void CycleLanternEventHandler(bool onCycleLantern, float dayLenght);

    [Export] public Node2D Lighting;
    [Export] public double CycleTime = 120;
    [Export] public double DawnTime = 5;
    [Export] public double DawnFrame = 0.2;
    [Export] public double DuskFrame = 0.8;
    
    private Interactible _interact;
    private AnimationPlayer _animator;
    private bool _dawnHandled = false;
    private bool _duskHandled = false;
    private bool _isDayTime = false;
    
    private List<Sprite2D> _lanternSprites;

    public override void _Ready()
    {
        _interact = this.GetNode<Interactible>("Interactible");
        _interact.Interact += OnInteractMethod;

        _animator = Lighting.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        _animator.Play("DayNightCycle");//Init Animation

        _lanternSprites = new List<Sprite2D>();
        
        foreach (var node in GetTree().GetNodesInGroup("Lantern"))
        {
            _lanternSprites.Add(node as Sprite2D);
        }
        
        if (Engine.IsEditorHint() || Engine.IsEmbeddedInEditor())
        {
            //Make default start time Mid-Day for testing in Editor.
            _animator.Seek(0.5, true);
            EnvironmentLight(false);
            this.GetNode<Sprite2D>("Interactible/Lantern").Frame = 2;
        } 
        else _animator.Seek(0, true);
        
        _animator.Stop(true);
    }

 
    private void OnInteractMethod(Node2D node, TriggerType trigger)
    {
        if (trigger is TriggerType.UseAction && !_animator.IsPlaying())
        {
            Global.GameDay += 1;
            
            _interact.IsInteractive = false;
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
        EmitSignal(SignalName.CycleLantern, true, CycleTime);
    }

    private void OnDuskFrame()
    {
        _duskHandled = true;
        GD.Print("Day is Dusk, closing shop");
        EmitSignal(SignalName.CycleLantern, false, CycleTime);
    }

    private void OnMidnight()
    {
        GD.Print("DayNightCycle Timed Out");
        //EmitSignal(SignalName.CycleLantern, false);
        _animator.Stop();
        _dawnHandled = false;
        
        //Turn Interaction back on to trigger next day.
        Global.GameTimer = -1;
        _interact.IsInteractive = true;
        this.GetNode<Sprite2D>("Interactible/Lantern").Frame = 2;
    }

    private void EnvironmentLight(bool isOn)
    {
        if (isOn)
        {
            GD.Print("TURN ON LIGHT");
            //Global.Player.GetNode<PointLight2D>("PointLight2D").Energy = 0.65f;
            foreach (var lantern in _lanternSprites)
            {
                lantern.Frame = 0;
                lantern.GetNode<PointLight2D>("PointLight2D2").Enabled = true;
            }
            _isDayTime = false;
            GD.Print(_isDayTime, "_isDayTime");
        }
        else
        {
            GD.Print("TURN OFF LIGHT");
            //Global.Player.GetNode<PointLight2D>("PointLight2D").Energy = 0.25f;
            foreach (var lantern in _lanternSprites)
            {
                lantern.Frame = 1;
                lantern.GetNode<PointLight2D>("PointLight2D2").Enabled = false;
            }
            _isDayTime = true;
            GD.Print(_isDayTime, "_isDayTime");
        }
    }

    private bool _isFrozen = false;
    
    public override void _Process(double delta)
    {
        if (Global.FreezeDay && !_isFrozen)
        {
            _isFrozen = true;
            _animator.Pause();
        }
        else if (!Global.FreezeDay && _isFrozen)
        {
            _isFrozen = false;
            _animator.Play();
        }
        if (_animator.IsPlaying())
        {
            
            if (_animator.CurrentAnimationPosition is < 0.25 or > 0.7)// || _isDayTime)
            {
                if (_isDayTime) EnvironmentLight(true);
                
            }
            else if (_animator.CurrentAnimationPosition is > 0.25 or < 0.7)
            {
                if (!_isDayTime) EnvironmentLight(false);
            }

            Global.GameTimer = (float)_animator.CurrentAnimationPosition;
            
            if (_animator.CurrentAnimationPosition > DawnFrame && !_dawnHandled)
            {
                GD.Print("DawnFrameHandled?");
                OnDawnFrame();
            }
            else if (_animator.CurrentAnimationPosition > DuskFrame && _dawnHandled && !_duskHandled)
            {
                GD.Print("DuskFrameHandled?");
                OnDuskFrame();
            }
            else if  (_animator.CurrentAnimationPosition > 0.9995)
            {   
                GD.Print("MidnightFrameHandled?");
                OnMidnight();
            }
        }
    }
}

