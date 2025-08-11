using Godot;
using SoYouWANNAJam2025.Code.Items;

namespace SoYouWANNAJam2025.Code;

[GlobalClass]
public partial class InventorySlot : Node2D
{
    public GenericItem Item;

    public void TransferTo(InventorySlot slot)
    {
        if (Item == null || slot.Item != null) return;
        slot.Item = Item;
        slot.Item.Reparent(slot);
        slot.Item.GlobalPosition = slot.GlobalPosition;
        slot.Item.IsInteractive = false;
        slot.Item.SetHighlight(false);
        Item = null;
        
    }
    
    public void PickupItem(GenericItem item)
    {
        if (Item != null) return;
        Item = item;
        Item.Reparent(this);
        Item.GlobalPosition = GlobalPosition;
        Item.IsInteractive = false;
        
    }
    
    public void DropItem(Node parentNode, Vector2 globalPosition)
    {
        if (Item == null) return;
        Item.Reparent(parentNode);
        Item.GlobalPosition = globalPosition;
        Item.IsInteractive = true;
        Item = null;
    }
    
}