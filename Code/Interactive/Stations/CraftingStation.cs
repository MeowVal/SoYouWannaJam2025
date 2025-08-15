using System.Diagnostics;
using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Interactive.Stations;
using SoYouWANNAJam2025.Code.Interactive.Items;
using SoYouWANNAJam2025.Entities.Interactive.Items;

//using SoYouWANNAJam2025.Scenes.UI.Interactions;

namespace SoYouWANNAJam2025.Code.Interactive.Stations;

[Tool]
public partial class CraftingStation : Interactible
{
    [ExportGroup("Info")]
    [Export] public string DisplayName = "Unknown Crafting Station";
    [Export] public string Description = "You need to actually set this lol.";
    [Export] public Texture2D Icon;
    [Export] public Array<BaseRecipe> Recipes = [];

    public Interactive.Inventory.Inventory Inventory;
    public BaseRecipe CurrentRecipe;
    public bool IsCrafting = false;
    //private Player.CharacterControl _player;
    private CraftingStationInterface _interactionInterface;
    private Node2D _interfaceLocation;
    
    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;
        base._Ready();
        _interfaceLocation = GetNode<Node2D>("InterfaceLocation");
        Interact += OnInteractMethod;
        
        if (FindChild("Inventory") is Interactive.Inventory.Inventory inv) Inventory=inv;
        Inventory.RecipeWhitelist = Recipes;
        Inventory.CompileWhitelist();
    }

    public bool AttemptCraft()
    {
        if (!Inventory.HasItem() || IsCrafting) return false;
        // Try each recipe until one triggers successfully
        foreach (var recipe in Recipes)
        {
            GD.Print($"Attemping Recipe {recipe.DisplayName}");
            if (!RecipeBegin(recipe)) continue;
            return true;
        }
        return false;
    }

    private void CreateInteractionUi(string path)
    {
        var uiScene = GD.Load<PackedScene>(path);
        _interactionInterface = uiScene.Instantiate<CraftingStationInterface>();
        _interactionInterface.Init(this);
        GetViewport().AddChild(_interactionInterface);
        _interactionInterface.GlobalPosition = _interfaceLocation.GlobalPosition;
    }

    // Begin the process of making a specific recipe
    public bool RecipeBegin(BaseRecipe recipe)
    {
        if (!Inventory.ContainItem(recipe.ItemInputs, true)) return false;
        CurrentRecipe = recipe;
        IsCrafting = true;

        GD.Print($"Starting WorkType {CurrentRecipe.WorkType}.");
        switch (CurrentRecipe.WorkType)
        {
            case EWorkType.Instant:
                RecipeComplete();
                return true;
            case EWorkType.Inputs:
                CreateInteractionUi("res://Scenes/UI/Interactions/CraftingInputs.tscn");
                return true;
            case EWorkType.SpamButton:
                CreateInteractionUi("res://Scenes/UI/Interactions/CraftingButtonSpam.tscn");
                return true;
            case EWorkType.Timer:
                CreateInteractionUi("res://Scenes/UI/Interactions/CraftingTimer.tscn");
                return true;
            case EWorkType.ButtonHold:
                return true;
        }
        return false;
    }

    // Cancel the current recipe's process
    public bool RecipeAbort()
    {
        if (_interactionInterface == null) return false;
        _interactionInterface.QueueFree();
        IsCrafting = false;
        GD.Print("Recipe ended before completion");
        return true;
    }

    // End the recipe and produce the results
    public bool RecipeComplete()
    {
        if (_interactionInterface != null) _interactionInterface!.QueueFree();
        IsCrafting = false;
        switch (CurrentRecipe.RecipeType)
        {
            case ERecipeType.Standard:
                return RecipeCompleteStandard();

            case ERecipeType.ModularPartSwap:
                return RecipeCompleteModularPartSwap();

            case ERecipeType.ModularPartStateChange:
                return RecipeCompleteModularPartStateChange();
        }
        return false;
    }

    private void OnInteractMethod(Node2D node, TriggerType trigger)
    {
        if (IsCrafting) return;
        if (node is Player.PlayerInteractor interactor)
        {
            switch (trigger)
            {
                case TriggerType.PickupDrop:
                    if (Inventory.HasItem() && interactor.InventorySlot.HasSpace())
                    {
                        Inventory.TransferTo(interactor.InventorySlot);
                    }
                    else if (Inventory.HasSpace() && interactor.InventorySlot.HasItem())
                    {
                        interactor.InventorySlot.TransferTo(Inventory);
                    }
                    break;
                case TriggerType.UseAction:
                    AttemptCraft();
                    break;
            } 
        } else if (node is Npc.NpcInteractor npcInteractor)
        {
            switch (trigger)
            {
                case TriggerType.PickupDrop:
                    if (Inventory.HasItem() && npcInteractor.InventorySlot.HasSpace())
                    {
                        Inventory.TransferTo(npcInteractor.InventorySlot);
                    }
                    else if (Inventory.HasSpace() && npcInteractor.InventorySlot.HasItem())
                    {
                        npcInteractor.InventorySlot.TransferTo(Inventory);
                    }

                    break;
                case TriggerType.UseAction:
                    AttemptCraft();
                    break;
            }
        }
    }

    private bool RecipeCompleteStandard()
    {
        // Destroy input items from inventory
        if (!Inventory.DestroyItem(CurrentRecipe.StandardItemInputs))
        {
            GD.Print($"Failed to delete recipe: {CurrentRecipe.DisplayName}");
            return false;
        }

        GD.Print($"Completed recipe {CurrentRecipe.DisplayName} with {CurrentRecipe.StandardOutputs.Count} outputs:");
        foreach (var item in CurrentRecipe.StandardOutputs)
        {
            // Create output items
            var newItemScene = GD.Load<PackedScene>("res://Entities/Interactive/Items/GenericItem.tscn");
            var newItem = newItemScene.Instantiate<GenericItem>();
            newItem.ItemResource = item;
            GetNode("/root/GameManager/Isometric").AddChild(newItem);
            // Add outputs to inventory
            Inventory.PickupItem(newItem, true);
            GD.Print($"- {newItem.ItemResource.DisplayName}");
        }
        return true;
    }

    private bool RecipeCompleteModularPartSwap()
    {
        // Filter the weapon from the other inputs
        Array<GenericItemTemplate> deleteList = [];
        ModularItem targetItem = null;
        foreach (var slot in Inventory.Slots)
        {
            if (slot.Item is ModularItem modularItem)
            {
                targetItem = modularItem;
            }
            else
            {
                deleteList.Add(slot.Item.ItemResource);
            }
        }
        if (targetItem == null) return false;

        // Destroy input items from inventory, except for weapon
        if (!Inventory.DestroyItem(deleteList))
        {
            GD.Print($"Failed to delete recipe: {CurrentRecipe.DisplayName}");
            return false;
        }

        // Add part to weapon
        GD.Print($"Completed recipe {CurrentRecipe.DisplayName}.");
        targetItem.AddPart(CurrentRecipe.PartSwapOutput);
        return true;
    }

    private bool RecipeCompleteModularPartStateChange()
    {
        // Filter the weapon from the other inputs
        Array<GenericItemTemplate> deleteList = [];
        ModularItem targetItem = null;
        foreach (var slot in Inventory.Slots)
        {
            if (slot.Item is ModularItem modularItem)
            {
                targetItem = modularItem;
            }
            else
            {
                deleteList.Add(slot.Item.ItemResource);
            }
        }
        if (targetItem == null) return false;

        // Destroy input items from inventory, except for weapon
        if (!Inventory.DestroyItem(deleteList))
        {
            GD.Print($"Failed to delete recipe: {CurrentRecipe.DisplayName}");
            return false;
        }

        // Set target part's state to new.
        GD.Print($"Completed recipe {CurrentRecipe.DisplayName}.");
        targetItem.SetPartState(CurrentRecipe.PartStateChangeTarget, CurrentRecipe.PartStateChangeValue);
        return true;
    }
}