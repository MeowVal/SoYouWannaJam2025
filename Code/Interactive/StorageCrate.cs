using Godot;
using SoYouWANNAJam2025.Code.Player;

namespace SoYouWANNAJam2025.Code.Interactive;

public partial class StorageCrate : Interactible
{
    public Inventory.Inventory Inventory;

    public override void _Ready()
    {
        base._Ready();
        if (FindChild("Inventory") is Inventory.Inventory inv) Inventory = inv;
        Inventory.CompileWhitelist();
        Interact += OnInteractMethod;
    }

    private void OnInteractMethod(Node2D node, TriggerType trigger)
    {
        if (node is PlayerInteractor interactor && trigger == TriggerType.PickupDrop)
        {
            if (Inventory.HasItem() && interactor.InventorySlot.HasSpace())
            {
                //SetZOffset(10);
                Inventory.TransferTo(interactor.InventorySlot);
                //SetZOffset(0);
            }
            else if (Inventory.HasSpace() && interactor.InventorySlot.HasItem())
            {
                interactor.InventorySlot.TransferTo(Inventory);
                //SetZOffset(0);
            }
        }
    }

    private void SetZOffset(int z)
    {
        foreach (var slot in Inventory.Slots)
        {
            if (slot.HasItem()) Inventory.Item.ZIndex = z;
        }
    }
}