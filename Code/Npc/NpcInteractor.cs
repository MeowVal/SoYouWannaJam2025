using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Items;

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
        // Init Temp Variables
        int nearestTarget = -1;
        float nearestDistance = float.MaxValue;

        // Check Loop
        for (var i = 0; i < PossibleTargets.Count; i++)
        {
            // Ignore target if its an item and we already have one
            if (InventorySlot.HasItem() && PossibleTargets[i] is GenericItem) continue;

            // Get distance between target and us
            var distance = PossibleTargets[i].GlobalPosition.DistanceTo(_npc.GlobalPosition);

            // Save nearest target distance and its index
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTarget = i;
            }
        }

        // Save index and apply highlight to all targets
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

    public void TriggerInteraction(TriggerType trigger)
    {
        switch (trigger)
        {
            case TriggerType.PickupDrop:
                if (PossibleTargets.Count > 0 && CurrentTarget < PossibleTargets.Count && CurrentTarget >= 0)
                {
                    PossibleTargets[CurrentTarget].TriggerInteraction(this, TriggerType.PickupDrop);
                    OnDistanceCheckTimer();
                } else if (InventorySlot.HasItem()) // If we have an item, drop it.
                {
                    if (InventorySlot.Item.OverlapsArea(this))
                    {
                        PossibleTargets.Add(InventorySlot.Item);
                    }
                    InventorySlot.DropItem();
                    OnDistanceCheckTimer();
                }
                break;

            case TriggerType.UseAction:
                if (PossibleTargets.Count > 0 && CurrentTarget < PossibleTargets.Count && CurrentTarget >= 0)
                {
                    PossibleTargets[CurrentTarget].TriggerInteraction(this, TriggerType.UseAction);
                    OnDistanceCheckTimer();
                }
                break;
        }
    }
}