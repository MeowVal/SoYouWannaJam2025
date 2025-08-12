using System.Linq;
using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Items;
using SoYouWANNAJam2025.Code.RecipeSystem;

namespace SoYouWANNAJam2025.Code;

[GlobalClass]
public partial class InventorySlot : Node2D
{
    public GenericItem Item;

    [ExportGroup("Whitelist")]
    [Export] // Ignore whitelist entirely.
    public bool AllowAll = false;
    [Export] // Filter items by a set list.
    public Array<BaseItem> ItemWhitelist = [];
    [Export] // Filter items by the inputs of the given recipes.
    public Array<BaseRecipe> RecipeWhitelist = [];

    public Array<BaseItem> _whitelist = [];

    public override void _Ready()
    {
        CompileWhitelist();
    }

    public virtual void CompileWhitelist()
    {
        // Compile the complete list of all items accepted
        _whitelist = ItemWhitelist;
        foreach (var recipe in RecipeWhitelist)
        {
            foreach (var input in recipe.Inputs)
            {
                _whitelist.Add(input);
            }
        }
    }

    // Moves item from one slot directly to another, returns true if successful.
    public virtual bool TransferTo(InventorySlot slot)
    {
        if (!HasItem() || !slot.HasSpace()) return false;

        // Ensures the item is accepted before doing anything else
        if (slot.PickupItem(Item))
        {
            // Accepted -> Remove from this slot.
            GD.Print($"Transferred {Item.ItemResource.DisplayName} away.");
            Item = null;

            return true;
        };
        return false;
    }

    // Add item to the slot, returns true if successful.
    public virtual bool PickupItem(GenericItem item, bool forceAdd = false)
    {
        // Ensure item *can* be added to the slot.
        if (!HasSpace()) return false;
        if (!(_whitelist.Contains(item.ItemResource) || AllowAll || forceAdd)) return false;

        // Add item to slot and position in new owning scene.
        Item = item;
        Item.SetHighlight(false);
        Item.Reparent(this);
        Item.GlobalPosition = GlobalPosition.Round();
        Item.IsInteractive = false;
        GD.Print($"Picked up {item.ItemResource.DisplayName}");
        return true;
    }

    // Removes item from the slot, returns true if successful.
    public virtual bool DropItem(Node parentNode, Vector2 globalPosition)
    {
        // Ensure item *can* be removed from the slot.
        if (!HasItem()) return false;

        // Remove the item and place it in the world.
        Item.Reparent(parentNode);
        Item.GlobalPosition = globalPosition.Round();
        Item.IsInteractive = true;
        GD.Print($"Dropped {Item.ItemResource.DisplayName}");
        Item = null;
        return true;
    }

    public virtual bool DestroyItem(Array<BaseItem> items)
    {
        // Ensure item exists at all.
        if (!HasItem() || !ContainItem(items, false)) return false;

        Item.Reparent(GetTree().GetRoot());
        Item.IsInteractive = false;
        Item.QueueFree();
        GD.Print($"Destroyed {Item.ItemResource.DisplayName}");
        Item = null;
        return true;
    }
    
    public virtual bool ContainItem(Array<BaseItem> items, bool all = false)
    {
        if (!HasItem() || (all && items.Count > 1)) return false;
        
        return items.Any(item => item == Item.ItemResource);
    }

    public virtual bool HasItem()
    {
        return Item != null;
    }
    
    public virtual bool HasSpace()
    {
        return Item == null;
    }
}