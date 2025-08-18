using System.Linq;
using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Interactive.Items;
using SoYouWANNAJam2025.Code.Npc.Friendly;
using SoYouWANNAJam2025.Code.World;
using SoYouWANNAJam2025.Scenes.UI.Interactions;
using RecipeSelectionUi = SoYouWANNAJam2025.Scenes.UI.Interactions.RecipeSelection.RecipeSelectionUi;

//using SoYouWANNAJam2025.Scenes.UI.Interactions;

namespace SoYouWANNAJam2025.Code.Interactive.Stations;

[Tool]
public partial class CraftingStation : Interactible
{
    [ExportGroup("Info")]
    [Export] public string DisplayName = "Unknown Crafting Station";
    [Export] public string Description = "You need to actually set this lol.";
    [Export] public Texture2D Icon;
    [Export] public bool AutoCraft = false;
    [Export] public Array<BaseRecipe> Recipes = [];
    [Export] public string RecipesFolder = "";

    public Inventory.Inventory Inventory;
    public BaseRecipe CurrentRecipe;
    public bool IsCrafting = false;
    private CraftingStationInterface _interactionInterface;
    private Node2D _interfaceLocation;
    
    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;
        base._Ready();
        _interfaceLocation = GetNode<Node2D>("InterfaceLocation");
        Interact += OnInteractMethod;
        
        if (RecipesFolder != "") Global.GameManager.TraverseDirectory(RecipesFolder,
            file => {
                var resource = ResourceLoader.Load(file);
                if (resource is not BaseRecipe recipe) return;
                Recipes.Add(recipe);
            }
        );
        
        if (FindChild("Inventory") is Inventory.Inventory inv) Inventory=inv;
        Inventory.RecipeWhitelist = Recipes;
        Inventory.CompileWhitelist();
    }

    public bool AttemptCraft()
    {
        if (!Inventory.HasItem() || IsCrafting) return false;
        var recipeList = GetAvailableRecipes();
        if (recipeList == null) return false;
        switch (recipeList.Count)
        {
            case 1:
                RecipeBegin(recipeList[0]);
                return true;
            case 0:
                return false;
        }

        CreateInteractionUi("res://Scenes/UI/Interactions/RecipeSelection/RecipeSelectionUI.tscn");
        if (_interactionInterface is not RecipeSelectionUi scene) return false;
        scene.RecipeList = recipeList;
        
        return false;
    }

    private Array<BaseRecipe> GetAvailableRecipes()
    {
        Array<BaseRecipe> recipeList = [];
        
        foreach (var recipe in Recipes)
        {
            if (!Inventory.ContainItem(recipe.RecipeInputs, true)) continue;
            switch (recipe.RecipeType)
            {
                case ERecipeType.Standard:
                    recipeList.Add(recipe);
                    break;
                case ERecipeType.ModularPartSwap:
                case ERecipeType.ModularPartAdd:
                    if (recipe.RecipeInputs[0] is not ModularPartTemplate)
                    {
                        GD.Print($"Recipe {recipe.DisplayName} at {recipe.ResourcePath} doesn't output a ModularPart Template");
                        break;
                    }

                    var part = recipe.RecipeInputs[0] as ModularPartTemplate;
                    var foundItems = Inventory.Slots
                        .Where(slot => slot.Item is ModularItem)
                        .Select(slot => slot.Item)
                        .Cast<ModularItem>()
                        .ToArray();
                    if (foundItems.Length != 1 || foundItems[0].Parts.Count == 0) continue;
                    var item =  foundItems[0];
                    switch (recipe.RecipeType)
                    {
                        case ERecipeType.ModularPartSwap:
                            if (item.Parts[part.PartType] != null) recipeList.Add(recipe);
                            break;
                        case ERecipeType.ModularPartAdd:
                            if (item.Parts[part.PartType] == null) recipeList.Add(recipe);
                            break;
                    }
                    break;
            }
        }

        return recipeList;
    }

    public void CreateInteractionUi(string path)
    {
        var uiScene = GD.Load<PackedScene>(path);
        _interactionInterface = uiScene.Instantiate<CraftingStationInterface>();
        _interactionInterface.Init(this);
        GetViewport().AddChild(_interactionInterface);
        _interactionInterface.GlobalPosition = _interfaceLocation.GlobalPosition;
    }

    // Begin the process of making a specific recipe
    public virtual void RecipeBegin(BaseRecipe recipe)
    {
        CurrentRecipe = recipe;
        IsCrafting = true;

        GD.Print($"Starting WorkType {CurrentRecipe.WorkType}.");
        switch (CurrentRecipe.WorkType)
        {
            case EWorkType.Instant:
                RecipeComplete();
                return;
            case EWorkType.Inputs:
                CreateInteractionUi("res://Scenes/UI/Interactions/CraftingInputs.tscn");
                return;
            case EWorkType.SpamButton:
                CreateInteractionUi("res://Scenes/UI/Interactions/CraftingButtonSpam.tscn");
                return;
            case EWorkType.Timer:
                CreateInteractionUi("res://Scenes/UI/Interactions/CraftingTimer.tscn");
                return;
            case EWorkType.ButtonHold:
                return;
        }
    }

    // Cancel the current recipe's process
    public virtual bool RecipeAbort()
    {
        FreeUi();
        
        GD.Print("Recipe ended before completion");
        return true;
    }

    public void FreeUi()
    {
        CurrentRecipe = null;
        IsCrafting = false;
        if (_interactionInterface == null) return;
        _interactionInterface.QueueFree();
        _interactionInterface = null;
    }

    // End the recipe and produce the results
    public virtual bool RecipeComplete()
    {
        var val = false;
        switch (CurrentRecipe.RecipeType)
        {
            case ERecipeType.Standard:
                val = RecipeCompleteStandard();
                break;
            case ERecipeType.ModularPartSwap:
            case ERecipeType.ModularPartAdd:
                val = RecipeCompleteModular();
                break;
        }
        FreeUi();
        if (AutoCraft) AttemptCraft();
        return val;
    }

    public virtual void OnInteractMethod(Node2D node, TriggerType trigger)
    {
        if (!AutoCraft && IsCrafting) return;
        if (node is Player.PlayerInteractor interactor)
        {
            switch (trigger)
            {
                case TriggerType.PickupDrop:
                    if (Inventory.HasItem() && interactor.InventorySlot.HasSpace())
                    {
                        Inventory.TransferTo(interactor.InventorySlot);
                        if (AutoCraft) RecipeAbort();
                        if (_interactionInterface == null) break;
                        _interactionInterface.QueueFree();
                        _interactionInterface = null;
                    }
                    else if (Inventory.HasSpace() && interactor.InventorySlot.HasItem())
                    {
                        interactor.InventorySlot.TransferTo(Inventory);
                        if (AutoCraft) AttemptCraft();
                    }
                    break;
                case TriggerType.UseAction:
                    if (!AutoCraft) AttemptCraft();
                    break;
            } 
        } else if (!IsCrafting && node is NpcInteractor npcInteractor)
        {
            switch (trigger)
            {
                case TriggerType.PickupDrop:
                    if (Inventory.HasItem() && npcInteractor.Inventory.HasSpace())
                    {
                        Inventory.TransferTo(npcInteractor.Inventory);
                    }
                    else if (Inventory.HasSpace() && npcInteractor.Inventory.HasItem())
                    {
                        foreach (var slot in Inventory.Slots)
                        {
                            if (slot.Item is ModularItem) return;
                            npcInteractor.Inventory.TransferTo(Inventory); 
                        }
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
        if (!Inventory.DestroyItem(CurrentRecipe.RecipeInputs))
        {
            GD.Print($"Failed to delete recipe: {CurrentRecipe.DisplayName}");
            return false;
        }

        GD.Print($"Completed recipe {CurrentRecipe.DisplayName} with {CurrentRecipe.RecipeOutputs.Count} outputs:");
        foreach (var item in CurrentRecipe.RecipeOutputs)
        {
            var (success, newItem) = Global.GameManager.NewItem(item);
            if (!success) continue;
            
            Global.Grid.AddChild(newItem);
            Inventory.PickupItem(newItem, true);
            GD.Print($"- {newItem.ItemResource.DisplayName}");
        }
        return true;
    }

    private bool RecipeCompleteModular()
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
        if (CurrentRecipe.RecipeInputs[0] is ModularPartTemplate output)
        {
            targetItem.AddPart(output);
        }
        
        
        return true;
    }
}