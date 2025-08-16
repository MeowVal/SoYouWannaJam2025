using Godot;
using SoYouWANNAJam2025.Code;
using SoYouWANNAJam2025.Code.Interactive.Inventory;
using SoYouWANNAJam2025.Code.Player;

namespace SoYouWANNAJam2025.Entities.Interactive;

public partial class StorageCrate : Interactible
{
    public Inventory Inventory;

    public override void _Ready()
    {
        base._Ready();
        if (FindChild("Inventory") is Inventory inv) Inventory = inv;
        Inventory.CompileWhitelist();
        Interact += OnInteractMethod;
    }

    private void OnInteractMethod(Node2D node, TriggerType trigger)
    {
        if (node is PlayerInteractor interactor && trigger == TriggerType.PickupDrop)
        {
            if (Inventory.HasItem() && interactor.InventorySlot.HasSpace())
            {
                Inventory.TransferTo(interactor.InventorySlot);
            }
            else if (Inventory.HasSpace() && interactor.InventorySlot.HasItem())
            {
                interactor.InventorySlot.TransferTo(Inventory);
            }
        }
    }
}