using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Interactive.Inventory;
using SoYouWANNAJam2025.Code.Interactive.Items;
using SoYouWANNAJam2025.Code.Player;

namespace SoYouWANNAJam2025.Code.Interactive.Stations;

public partial class GrindingStation : CraftingStation
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
        if (Inventory.Slots[0].Item is not ModularItem modularItem) return;
        if (!HasBrokenPart(modularItem)) return;
        
        CurrentRecipe = new BaseRecipe()
        {
            RecipeType = ERecipeType.Standard,
            TimeToComplete = 5,
            DisplayName = "Grinding"
        };
        if (AudioPlayer != null) AudioPlayer.Play();
        IsCrafting = true;
        CreateInteractionUi("res://Scenes/UI/Interactions/CraftingTimer.tscn");
        CurrentRecipe = null;
    }

    public override bool RecipeComplete()
    {
        if (CurrentRecipe != null) return base.RecipeComplete();
        FreeUi();
        
        if (!Inventory.Slots[0].HasItem()) return false;
        if (Inventory.Slots[0].Item is ModularItem modularItem)
        {
            foreach (var part in modularItem.Parts)
            {
                if (part.Value.PartState == EPartState.Broken)
                {
                    part.Value.PartState = EPartState.New;
                    break;
                }
            }
            modularItem.DrawSprite();
        }

        //if (AutoCraft) Begin();
        return true;
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
                    if (IsCrafting || interactor.InventorySlot.Item is not ModularItem modularItem) return;
                    if (!HasBrokenPart(modularItem)) return;
                    interactor.InventorySlot.TransferTo(Inventory);
                    Begin();
                }
                break;
            case TriggerType.UseAction:
                if (!IsCrafting) Begin();
                break;
        }
    }

    private static bool HasBrokenPart(ModularItem modularItem)
    {
        var found = false;
        foreach (var part in modularItem.Parts)
        {
            if (part.Value.PartState == EPartState.Broken)
            {
                found = true;
                break;
            }
        }
        return found;
    }
}