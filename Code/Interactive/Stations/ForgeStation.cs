using Godot;
using SoYouWANNAJam2025.Code.Interactive.Items;
using SoYouWANNAJam2025.Code.Player;

namespace SoYouWANNAJam2025.Code.Interactive.Stations;

public partial class ForgeStation : CraftingStation
{
    [Export] public GenericItemTemplate ScrapItem;
    private BaseRecipe _scrapingRecipe;
    public override void _Ready()
    {
        base._Ready();
        _scrapingRecipe = new BaseRecipe()
        {
            DisplayName =  "Scraping",
            RecipeInputs = [],
            RecipeOutputs = [ScrapItem],
            WorkType = EWorkType.Timer,
            TimeToComplete = 3,
        };
    }

    public override bool RecipeComplete()
    {
        var completed = base.RecipeComplete();
        if (Inventory.HasItem() && AttemptCraft() == false)
        {
            if (Inventory.Slots[0].Item.ItemResource == ScrapItem) return false;
            if (_scrapingRecipe == null) return false;
            _scrapingRecipe.RecipeInputs = [Inventory.Slots[0].Item.ItemResource];
            _scrapingRecipe.RecipeOutputs = [ScrapItem];
            RecipeBegin(_scrapingRecipe);
        }
        
        return completed;
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
                    interactor.InventorySlot.TransferTo(Inventory);
                    if (Inventory.HasItem() && AttemptCraft() == false)
                    {
                        if (Inventory.Slots[0].Item.ItemResource == ScrapItem)
                        {
                            _scrapingRecipe.RecipeOutputs = [];
                        }
                        else
                        {
                            _scrapingRecipe.RecipeOutputs = [ScrapItem];
                        }
                        if (_scrapingRecipe == null) break;
                        _scrapingRecipe.RecipeInputs = [Inventory.Slots[0].Item.ItemResource];
                        RecipeBegin(_scrapingRecipe);
                    }
                }
                break;
            case TriggerType.UseAction:
                if (_scrapingRecipe == null || IsCrafting) break;
                if (Inventory.Slots[0].Item.ItemResource == ScrapItem)
                {
                    _scrapingRecipe.RecipeInputs = [Inventory.Slots[0].Item.ItemResource];
                    _scrapingRecipe.RecipeOutputs = [];
                    RecipeBegin(_scrapingRecipe);
                }
                break;
        }
    }
}