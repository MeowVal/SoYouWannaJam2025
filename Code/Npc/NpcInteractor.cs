using Godot;
using Godot.Collections;

namespace SoYouWANNAJam2025.Code.Npc;

public partial class NpcInteractor : Area2D
{
    public Array<Interactible> PossibleTargets = [];
    public int CurrentTarget = 0;
    private Timer _distanceCheckTimer;
    private Npc _npc;

    public InventorySlot InventorySlot = null;
    public override void _Ready()
    {
        if (GetParent().FindChild("InventorySlot") is InventorySlot slot) InventorySlot=slot;
        
        _distanceCheckTimer = GetNode<Timer>("./DistanceCheckTimer");
        _distanceCheckTimer.Timeout += OnDistanceCheckTimer;
        _npc = GetParent<Npc>();

        BodyEntered += OnInteractableEntered;
        BodyExited += OnInteractableExited;
        AreaEntered += OnInteractableEntered;
        AreaExited += OnInteractableExited;
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

        for (var i = 0; i < PossibleTargets.Count; i++)
        {
            var distance = PossibleTargets[i].GlobalPosition.DistanceTo(_npc.GlobalPosition);
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
        OnDistanceCheckTimer();
    }

    private void OnInteractableExited(Node2D unknownTarget)
    {
        if (unknownTarget is not Interactible target) return;
        PossibleTargets.Remove(target);
        target.SetHighlight(false);
        OnDistanceCheckTimer();
    }

    public void TriggerInteraction()
    {
        if (PossibleTargets.Count > 0 && CurrentTarget < PossibleTargets.Count)
        {
            PossibleTargets[CurrentTarget].TriggerInteraction(this, TriggerType.PickupDrop);
        }
        else if (InventorySlot.Item != null)
        {
            InventorySlot.DropItem();
        }
    }
}