using System.Linq;
using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.RecipeSystem;
using SoYouWANNAJam2025.Code.Interactive.Items;

namespace SoYouWANNAJam2025.Code.Interactive.Inventory;

[GlobalClass]
public partial class InventorySlot : Node2D
{
    public GenericItem Item;
    private TileMapLayer _grid;

    [ExportGroup("Whitelist")]
    [Export] // Ignore whitelist entirely.
    public bool AllowAll = false;
    [Export] // Filter items by a set list.
    public Array<GenericItemTemplate> ItemWhitelist = [];
    [Export] // Filter items by the inputs of the given recipes.
    public Array<BaseRecipe> RecipeWhitelist = [];

    [ExportGroup("Settings")]
    [Export] // Number of ZIndex layers to add to the occupying item.
    public int ZIndexBonus = 0;

    public Array<GenericItemTemplate> Whitelist = [];

    public override void _Ready()
    {
        _grid = GetTree().GetRoot().GetNode<TileMapLayer>("Node2D/Isometric/WorldMap");
        CompileWhitelist();
    }

    public virtual void CompileWhitelist()
    {
        // Compile the complete list of all items accepted
        Whitelist = ItemWhitelist;
        foreach (var recipe in RecipeWhitelist)
        {
            foreach (var input in recipe.ItemInputs)
            {
                Whitelist.Add(input);
            }
        }
    }

    // Moves item from one slot directly to another, returns true if successful.
    public virtual bool TransferTo(InventorySlot slot)
    {
        if (!HasItem() || !slot.HasSpace()) return false;

        Item.SetZIndexOffset(0);
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
        if (!(Whitelist.Contains(item.ItemResource) || AllowAll || forceAdd)) return false;

        // Add item to slot and position in new owning scene.
        Item = item;
        Item.SetHighlight(false);
        Item.SetZIndexOffset(1);
        Item.Reparent(this);
        Item.GlobalPosition = GlobalPosition.Round();
        Item.IsInteractive = false;
        GD.Print($"Picked up {item.ItemResource.DisplayName}");
        return true;
    }

    // Removes item from the slot, returns true if successful. Pass false to force  it to drop where ever it is.
    public virtual bool DropItem(Vector2 position, bool snapToNearestTile = true)
    {
        // Ensure item *can* be removed from the slot.
        if (!HasItem()) return false;

        var localPos = _grid.ToLocal(position);
        Item.SetZIndexOffset(0);
        Item.Reparent(_grid);
        if (snapToNearestTile)
        {
            Item.Position = _grid.MapToLocal(_grid.LocalToMap(localPos));
        }
        else
        {
            Item.Position = localPos;
        }

        Item.IsInteractive = true;
        GD.Print($"Dropped {Item.ItemResource.DisplayName}");
        Item = null;
        return true;
    }

    public virtual bool DestroyItem(Array<GenericItemTemplate> items)
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
    
    public virtual bool ContainItem(Array<GenericItemTemplate> items, bool all = false)
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