using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Items;

namespace SoYouWANNAJam2025.Code.RecipeSystem;

[Tool]
public partial class CraftingStation : Interactible
{
    [ExportGroup("Info")]
    [Export] public string DisplayName = "Unknown Crafting Station";
    [Export] public string Description = "You need to actually set this lol.";
    [Export] public Texture2D Icon;
    [Export] public Array<BaseRecipe> Recipes = [];

    private BaseRecipe _currentRecipe;
    private Timer _recipeTimer;
    private CharacterControl _player;

    public InventorySlot InventorySlot;
    
    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;
        base._Ready();
        _recipeTimer = GetNode<Timer>("RecipeTimer");
        _recipeTimer.Timeout += _OnRecipeTimer;
        Interact += OnInteractMethod;
        
        if (FindChild("InventorySlot") is InventorySlot slot) InventorySlot=slot;
    }

    public bool AttemptCraft()
    {
        foreach (var recipe in Recipes)
        {
            if (!recipe.Inputs.Contains(InventorySlot.Item.ItemResource)) continue;

            _currentRecipe = recipe;
            GD.Print($"Starting WorkType {_currentRecipe.WorkType}.");
            switch (_currentRecipe.WorkType)
            {
                case WorkType.Instant:
                    _RecipeComplete();
                    return true;
                case WorkType.SpamButton:
                    return true;
                case WorkType.Timer:
                    _recipeTimer.Start(recipe.TimeToComplete);
                    return true;
                case WorkType.ButtonHold:
                    return true;
            }
        }
        return false;
    }

    private void OnCraftingStationExited(Node2D body)
    {
        if (_recipeTimer.IsStopped()) return;
        _recipeTimer.Stop();
        GD.Print("Recipe ended before completion");
    }

    private void _OnRecipeTimer()
    {
        GD.Print("Recipe Timer Completed");
        _RecipeComplete();
    }

    private void _RecipeComplete()
    {
        InventorySlot.Item.Free();
        var newItem = new GenericItem();
        newItem.ItemResource = _currentRecipe.Outputs[0];
        InventorySlot.PickupItem(newItem);
        GD.Print($"Completed recipe {_currentRecipe.DisplayName} outputting {newItem.Name}");
    }

    private void OnInteractMethod(Node2D node)
    {
        if (node is not PlayerInteractor interactor) return;
        if (InventorySlot.HasItem())
        {
            interactor.InventorySlot.TransferTo(InventorySlot);
        }
        else
        {
            InventorySlot.TransferTo(interactor.InventorySlot);
        }
    }
}