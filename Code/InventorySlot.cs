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

    private Array<BaseItem> _whitelist = [];

    public override void _Ready()
    {
        CompileWhitelist();
    }

    public void CompileWhitelist()
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
    public bool TransferTo(InventorySlot slot)
    {
        if (!HasItem() || slot.HasItem()) return false;

        // Ensures the item is accepted before doing anything else
        if (slot.PickupItem(Item))
        {
            // Accepted -> Remove from this slot.
            slot.Item!.SetHighlight(false);
            GD.Print($"Transferred {Item.ItemResource.DisplayName} away.");
            Item = null;

            return true;
        };
        return false;
    }

    // Add item to the slot, returns true if successful.
    public bool PickupItem(GenericItem item)
    {
        // Ensure item *can* be added to the slot.
        if (HasItem()) return false;
        if (!_whitelist.Contains(item.ItemResource) && !AllowAll) return false;

        // Add item to slot and position in new owning scene.
        Item = item;
        Item.Reparent(this);
        Item.GlobalPosition = GlobalPosition;
        Item.IsInteractive = false;
        GD.Print($"Picked up {item.ItemResource.DisplayName}");
        return true;
    }

    // Removes item from the slot, returns true if successful.
    public bool DropItem(Node parentNode, Vector2 globalPosition)
    {
        // Ensure item *can* be removed from the slot.
        if (!HasItem()) return false;

        // Remove the item and place it in the world.
        Item.Reparent(parentNode);
        Item.GlobalPosition = globalPosition;
        Item.IsInteractive = true;
        GD.Print($"Dropped {Item.ItemResource.DisplayName}");
        Item = null;
        return true;
    }

    public bool HasItem()
    {
        return Item != null;
    }
}