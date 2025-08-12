using System.Diagnostics;
using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.RecipeSystem;
using SoYouWANNAJam2025.Code.Interactive.Items;

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

    public Interactive.Inventory.InventorySlot InventorySlot;
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
        
        if (FindChild("InventorySlot") is Interactive.Inventory.InventorySlot slot) InventorySlot=slot;
        InventorySlot.RecipeWhitelist = Recipes;
        InventorySlot.CompileWhitelist();
    }

    public bool AttemptCraft()
    {
        if (!InventorySlot.HasItem()) return false;
        foreach (var recipe in Recipes)
        {
            GD.Print($"Attemping Recipe {recipe.DisplayName}");
            GD.Print(recipe.Inputs.ToString());
            if (!InventorySlot.ContainItem(recipe.Inputs, true)) continue;

            CurrentRecipe = recipe;
            GD.Print($"Starting WorkType {CurrentRecipe.WorkType}.");
            switch (CurrentRecipe.WorkType)
            {
                case WorkType.Instant:
                    _RecipeComplete();
                    return true;
                case WorkType.SpamButton:
                    return true;
                case WorkType.Timer:
                    RecipeTimer.Start(recipe.TimeToComplete);
                    CreateInteractionUi("res://Scenes/UI/Interactions/CraftingTimer.tscn");
                    return true;
                case WorkType.ButtonHold:
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
        _interactionInterface!.QueueFree();
        if (!InventorySlot.DestroyItem(CurrentRecipe.Inputs))
        {
            GD.Print($"Failed to delete recipe: {CurrentRecipe.DisplayName}");
            return;
        }
        var newItemScene = GD.Load<PackedScene>("res://Entities/Interactive/Items/GenericItem.tscn");
        var newItem = newItemScene.Instantiate<GenericItem>();
        newItem.ItemResource = CurrentRecipe.Outputs[0];
        GetNode("/root/Node2D/Isometric").AddChild(newItem);
        InventorySlot.PickupItem(newItem, true);
        GD.Print($"Completed recipe {CurrentRecipe.DisplayName} outputting {newItem.Name}");
    }

    private void OnInteractMethod(Node2D node, TriggerType trigger)
    {
        if (node is Player.PlayerInteractor interactor)
        {
            switch (trigger)
            {
                case TriggerType.PickupDrop:
                    if (InventorySlot.HasItem() && interactor.InventorySlot.HasSpace())
                    {
                        InventorySlot.TransferTo(interactor.InventorySlot);
                    }
                    else if (InventorySlot.HasSpace() && interactor.InventorySlot.HasItem())
                    {
                        interactor.InventorySlot.TransferTo(InventorySlot);
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
                    if (InventorySlot.HasItem() && npcInteractor.InventorySlot.HasSpace())
                    {
                        InventorySlot.TransferTo(npcInteractor.InventorySlot);
                    }
                    else if (InventorySlot.HasSpace() && npcInteractor.InventorySlot.HasItem())
                    {
                        npcInteractor.InventorySlot.TransferTo(InventorySlot);
                    }

                    break;
                case TriggerType.UseAction:
                    AttemptCraft();
                    break;
            }
        }
    }
}