using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Interactive.Inventory;
using SoYouWANNAJam2025.Code.Interactive.Items;
using SoYouWANNAJam2025.Code.Player;

namespace SoYouWANNAJam2025.Code.Interactive.Stations;

public partial class TinkeringStation : CraftingStation
{
    private CraftingStationInterface _interactionInterface;
    private Node2D _interfaceLocation;
    
    public override void _Ready()
    {
        base._Ready();
    }

    public void Begin()
    {
        if (AttemptCraft()) return;
        
        var (found, item, part) = FindParts();
        if (!found) return;
        CurrentRecipe = new BaseRecipe()
        {
            TimeToComplete = 6,
            SpamPerSecond = 6,
            DisplayName = "Tinkering"
        };
        IsCrafting = true;
        CreateInteractionUi("res://Scenes/UI/Interactions/CraftingInputs.tscn");
        CurrentRecipe = null;
        GD.Print(item.ModularItemType, part.DisplayName);
    }

    public override bool RecipeComplete()
    {
        if (CurrentRecipe != null) return base.RecipeComplete();
        FreeUi();
        
        var (found, item, part) = FindParts();
        if (!found) return false;
        if (!item.AddPart(part)) return false;
        
        Array<GenericItemTemplate> list = [part];
        
        if (!Inventory.DestroyItem(list)) return false;
        if (AutoCraft) Begin();
        return true;
    }

    private (bool, ModularItem, ModularPartTemplate) FindParts()
    {
        ModularItem targetItem = null;
        ModularPartTemplate targetPart = null;
        foreach (var slot in Inventory.Slots)
        {
            if (!slot.HasItem()) continue;
            if (slot.Item is ModularItem modularItem)
            {
                if (targetItem != null) return (false, null, null);
                targetItem = modularItem;
            }
            else if (slot.Item.ItemResource is ModularPartTemplate modularPartItem)
            {
                if (targetPart != null) return (false, null, null);
                targetPart = modularPartItem;
            }
        }

        if (targetItem == null || targetPart == null) return (false, null, null);
        
        return (true, targetItem, targetPart);
    }
    
    public override void OnInteractMethod(Node2D node, TriggerType trigger)
    {
        if (node is not PlayerInteractor interactor) return;

        switch (trigger)
        {
            case TriggerType.PickupDrop:
                if (Inventory.HasItem() && interactor.InventorySlot.HasSpace())
                {
                    Inventory.TransferTo(interactor.InventorySlot);
                    if (IsCrafting) RecipeAbort();
                }
                else if (Inventory.HasSpace() && interactor.InventorySlot.HasItem())
                {
                    if (interactor.InventorySlot.Item.ItemResource is not (ModularPartTemplate or ModularItemTemplate)) return;
                    interactor.InventorySlot.TransferTo(Inventory);
                    if (AutoCraft) Begin();
                }
                break;
            case TriggerType.UseAction:
                if (!IsCrafting) Begin();
                break;
        }
    }
}