using System.Linq;
using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Interactive.Items;

namespace SoYouWANNAJam2025.Code.Interactive.Inventory;

[GlobalClass]
public partial class Inventory : InventorySlot
{
    public Array<InventorySlot> Slots;
    private int _lastSlot = 0;
    private int _count = 0;

    private void CalculateCount()
    {
        _count = Slots.Count(slot => slot.HasItem());
    }

    private int FindSlot(bool empty = true, bool reversed = false)
    {
        for (var i = 0; i < Slots.Count; i++)
        {
            var x = reversed ?  Slots.Count-1-i: i;
            if ((Slots[x].HasSpace() && empty) || (Slots[x].HasItem() && !empty)) return x;
        }

        return -1;
    }
    
    public override void _Ready()
    {
        Slots = new Array<InventorySlot>(GetChildren()
            .Where(child => child is InventorySlot)
            .Select(child => child)
            .Cast<InventorySlot>()
        );
    }
    
    public override void CompileWhitelist()
    {
        base.CompileWhitelist();
        foreach (var slot in Slots)
        {
            slot.RecipeWhitelist = RecipeWhitelist;
            slot.CompileWhitelist();
        }
    }
    
    // Moves item from one slot directly to another, returns true if successful.
    public override bool TransferTo(InventorySlot slot)
    {
        GD.Print("INVENTORY transfer to");
        if (!HasItem() || !slot.HasSpace()) return false;
        
        if (!Slots[_lastSlot].TransferTo(slot)) return false;
        CalculateCount();
        _lastSlot = FindSlot(empty:false, reversed: true);
        GD.Print("Inventory transfer");
        return true;
    }

    // Add item to the slot, returns true if successful.
    public override bool PickupItem(GenericItem item, bool forceAdd = false)
    {
        GD.Print("INVENTORY Pickup item");
        if (!HasSpace()) return false;
        var index = FindSlot();
        if (index == -1 || !Slots[index].PickupItem(item, forceAdd)) return false;
        _lastSlot = index;
        CalculateCount();
        return true;
    }

    // Removes item from the slot, returns true if successful.
    public override bool DropItem(Vector2 position, bool snapToNearestTile = true)
    {
        if (!HasItem()) return false;
        if (!Slots[_lastSlot].DropItem(position, snapToNearestTile)) return false;
        CalculateCount();
        _lastSlot = FindSlot(empty:false, reversed: true);
        return true;
    }
    
    public override bool DestroyItem(Array<GenericItemTemplate> items)
    {
        if (!HasItem() || items.Count > _count || !ContainItem(items, true)) return false;
        
        var dupeItems = new Array<GenericItemTemplate>(items);
        foreach (var slot in Slots)
        {
            if (slot.HasItem())
            {
                var slotItemResource = slot.Item.ItemResource;
                if (slot.DestroyItem(dupeItems)) dupeItems.Remove(slotItemResource);
            }
            
        }
        CalculateCount();
        _lastSlot = FindSlot(empty:false, reversed: true);
        return true;
    }
    
    public override bool ContainItem(Array<GenericItemTemplate> items, bool all = false)
    {
        if (!HasItem() || (all && items.Count > _count)) return false;
        if (!all) return Slots.Any(slot => slot.ContainItem(items));

        var dupeItems = new Array<GenericItemTemplate>(items);
        foreach (var slot in Slots)
        {
            if (slot.ContainItem(dupeItems))
            {
                dupeItems.Remove(slot.Item.ItemResource);
            }
        }

        return dupeItems.Count == 0;

    }

    public override bool HasItem()
    {
        return _count != 0;
    }
    
    public override bool HasSpace()
    {
        return _count < Slots.Count;
    }
    
    
}