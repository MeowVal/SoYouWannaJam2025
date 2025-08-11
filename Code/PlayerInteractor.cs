using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Items;

namespace SoYouWANNAJam2025.Code;

public partial class PlayerInteractor : Area2D
{
    public Array<Interactible> PossibleTargets = [];
    public int CurrentTarget = 0;
    private Timer _distanceCheckTimer;
    private CharacterControl _player;
    public override void _Ready()
    {
        _distanceCheckTimer = GetNode<Timer>("./DistanceCheckTimer");
        _distanceCheckTimer.Timeout += OnDistanceCheckTimer;
        _player = GetParent<CharacterControl>();

        BodyEntered += OnInteractableEntered;
        BodyExited += OnInteractableExited;
        AreaEntered += OnInteractableEntered;
        AreaExited += OnInteractableExited;
    }

    public override void _Draw()
    {
        foreach (var target in PossibleTargets)
        {
            DrawLine(target.GlobalPosition, _player.GlobalPosition, Colors.Yellow, 10f, true);
        }
    }

    private void OnDistanceCheckTimer()
    {
        if (PossibleTargets.Count == 1)
        {
            PossibleTargets[0].SetHighlight(true);
            return;
        }

        int nearestTarget = 0;
        float nearestDistance = float.MaxValue;

        //GD.Print(PossibleTargets);
        for (var i = 0; i < PossibleTargets.Count; i++)
        {
            var distance = PossibleTargets[i].GlobalPosition.DistanceTo(_player.GlobalPosition);
            GD.Print(PossibleTargets[i] + " | " + (distance < nearestDistance) + " | " +  distance);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTarget = i;
            }
        }

        CurrentTarget = nearestTarget;
        for (var i = 0; i < PossibleTargets.Count; i++)
        {
            PossibleTargets[i].SetHighlight(i == CurrentTarget);
        }
    }
    private void OnInteractableEntered(Node2D unknownTarget)
    {
        if (unknownTarget is not Interactible target) return;
        PossibleTargets.Add(target);
    }

    private void OnInteractableExited(Node2D unknownTarget)
    {
        if (unknownTarget is not Interactible target) return;
        PossibleTargets.Remove(target);
        target.SetHighlight(false);
    }

    public void TriggerInteraction()
    {
        if (PossibleTargets.Count > 0)
        {
            PossibleTargets[CurrentTarget].TriggerInteraction(this);
        }
    }
}