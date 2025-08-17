using System;
using Godot;
using SoYouWANNAJam2025.Code;
using SoYouWANNAJam2025.Code.Interactive.Inventory;
using SoYouWANNAJam2025.Code.Interactive.Items;
using SoYouWANNAJam2025.Code.Player;

namespace SoYouWANNAJam2025.Entities.Interactive;

public partial class Backpack : GenericItem
{
    public Inventory Inventory;

    public override void _Ready()
    {
        if (FindChild("Inventory") is Inventory inv) Inventory = inv;
        Inventory.CompileWhitelist();
        Interact += OnInteractMethod;
        UpdateSprites();
    }

    public override void DrawSprite()
    {
    }

    private void OnInteractMethod(Node2D node, TriggerType trigger)
    {
        if (node is PlayerInteractor interactor)
        {
            switch (trigger)
            {
                case TriggerType.PickupDrop:
                    if (Inventory.HasItem() && interactor.InventorySlot.HasSpace())
                    {
                        Inventory.TransferTo(interactor.InventorySlot);
                    }
                    else if (Inventory.HasSpace() && interactor.InventorySlot.HasItem())
                    {
                        interactor.InventorySlot.TransferTo(Inventory);
                    }
                    break;
                case TriggerType.UseAction:
                    if (interactor.InventorySlot.HasSpace())
                    {
                        interactor.InventorySlot.PickupItem(this);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(trigger), trigger, null);
            }
        }

    }
}