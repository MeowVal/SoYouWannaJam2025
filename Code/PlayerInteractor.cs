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
    private Node2D _itemOffset;
    public GenericItem _heldItem;
    public override void _Ready()
    {
        _distanceCheckTimer = GetNode<Timer>("./DistanceCheckTimer");
        _distanceCheckTimer.Timeout += OnDistanceCheckTimer;
        _player = GetParent<CharacterControl>();
        _itemOffset = GetNode<Node2D>("ItemOffset");

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
            var distance = PossibleTargets[i].GlobalPosition.DistanceTo(_player.GlobalPosition);
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
            PossibleTargets[CurrentTarget].TriggerInteraction(this);
        }
    }

    public void PickupItem(GenericItem item)
    {
        if (_heldItem != null) return;
        _heldItem = item;
        _heldItem.Reparent(_itemOffset);
        _heldItem.Position = Vector2.Zero;
        _heldItem.IsInteractive = false;
        _heldItem.SetHighlight(false);
    }

    public void DropItem()
    {
        if (_heldItem == null) return;
        _heldItem.Reparent(GetParent().GetParent(), true);
        _heldItem.GlobalPosition = _player.GlobalPosition;
        _heldItem.IsInteractive = true;
        _heldItem = null;
    }
}