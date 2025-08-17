using Godot;
using SoYouWANNAJam2025.Code.Interactive.Items;
using SoYouWANNAJam2025.Code.Player;
using SoYouWANNAJam2025.Code.World;

namespace SoYouWANNAJam2025.Code.Interactive;

public partial class StorageCrate : Interactible
{
    public Inventory.Inventory Inventory;
    [Export] public GenericItemTemplate Slot1Template;
    [Export] public GenericItemTemplate Slot2Template;

    public override void _Ready()
    {
        base._Ready();
        if (FindChild("Inventory") is Inventory.Inventory inv) Inventory = inv;
        Inventory.CompileWhitelist();
        Interact += OnInteractMethod;
        
        SetSlotItem(Inventory.Slots[0], Slot1Template);
        SetSlotItem(Inventory.Slots[1], Slot2Template);
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

    private void SetSlotItem(Inventory.InventorySlot slot, GenericItemTemplate itemTemplate)
    {
        if (slot == null || slot.HasItem() ||  itemTemplate == null) return;
        
        var (success, newItem) = Global.GameManager.NewItem(itemTemplate);
        if (!success) return;
            
        AddChild(newItem);
        Inventory.PickupItem(newItem, true);
    }
}