using System.Diagnostics;
using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.RecipeSystem;
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
    public Timer RecipeTimer;
    //private Player.CharacterControl _player;
    private CraftingStationInterface _interactionInterface;
    private Node2D _interfaceLocation;
    
    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;
        base._Ready();
        _interfaceLocation = GetNode<Node2D>("InterfaceLocation");
        RecipeTimer = GetNode<Timer>("RecipeTimer");
        RecipeTimer.Timeout += _OnRecipeTimer;
        Interact += OnInteractMethod;
        
        if (FindChild("Inventory") is Interactive.Inventory.Inventory inv) Inventory=inv;
        Inventory.RecipeWhitelist = Recipes;
        Inventory.CompileWhitelist();
    }

    public bool AttemptCraft()
    {
        if (!Inventory.HasItem()) return false;
        foreach (var recipe in Recipes)
        {
            GD.Print($"Attemping Recipe {recipe.DisplayName}");

            if (!Inventory.ContainItem(recipe.ItemInputs, true)) continue;
            CurrentRecipe = recipe;

            GD.Print($"Starting WorkType {CurrentRecipe.WorkType}.");
            switch (CurrentRecipe.WorkType)
            {
                case EWorkType.Instant:
                    _RecipeComplete();
                    return true;
                case EWorkType.SpamButton:
                    return true;
                case EWorkType.Timer:
                    RecipeTimer.Start(recipe.TimeToComplete);
                    CreateInteractionUi("res://Scenes/UI/Interactions/CraftingTimer.tscn");
                    return true;
                case EWorkType.ButtonHold:
                    return true;
            }
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

    private void OnCraftingStationExited(Node2D body)
    {
        if (RecipeTimer.IsStopped()) return;
        RecipeTimer.Stop();
        _interactionInterface.QueueFree();
        GD.Print("Recipe ended before completion");
    }

    private void _OnRecipeTimer()
    {
        GD.Print("Recipe Timer Completed");
        _RecipeComplete();
    }

    private void _RecipeComplete()
    {
        if (_interactionInterface != null) _interactionInterface!.QueueFree();

        switch (CurrentRecipe.RecipeType)
        {
            case ERecipeType.Standard:
                // Destroy input items from inventory
                if (!Inventory.DestroyItem(CurrentRecipe.ItemInputs))
                {
                    GD.Print($"Failed to delete recipe: {CurrentRecipe.DisplayName}");
                    return;
                }

                GD.Print($"Completed recipe {CurrentRecipe.DisplayName} with {CurrentRecipe.ItemOutputs.Count} outputs:");
                foreach (var item in CurrentRecipe.ItemOutputs)
                {
                    // Create output items
                    var newItemScene = GD.Load<PackedScene>("res://Entities/Interactive/Items/GenericItem.tscn");
                    var newItem = newItemScene.Instantiate<GenericItem>();
                    newItem.ItemResource = item;
                    GetNode("/root/Node2D/Isometric").AddChild(newItem);
                    // Add outputs to inventory
                    Inventory.PickupItem(newItem, true);
                    GD.Print($"- {newItem.ItemResource.DisplayName}");
                }
                return;

            case ERecipeType.ModularPartSwap:

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
                if (targetItem == null) return;

                // Destroy input items from inventory, except for weapon
                if (!Inventory.DestroyItem(deleteList))
                {
                    GD.Print($"Failed to delete recipe: {CurrentRecipe.DisplayName}");
                    return;
                }

                // Add part to weapon
                GD.Print($"Completed recipe {CurrentRecipe.DisplayName}.");
                targetItem.AddPart(CurrentRecipe.PartOutput);
                return;

            case ERecipeType.ModularStatChange:
                return;
        }

    }

    private void OnInteractMethod(Node2D node, TriggerType trigger)
    {
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
}