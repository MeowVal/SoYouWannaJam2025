using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;
using SoYouWANNAJam2025.Code;

public partial class CraftingStation : Area2D
{
    [ExportGroup("Info")]
    [Export] public string DisplayName = "Unknown Crafting Station";
    [Export] public string Description = "You need to actually set this lol.";
    [Export] public Texture2D Icon;
    [Export] public Array<BaseRecipe> Recipes = [];

    private BaseRecipe _currentRecipe;
    private Timer _recipeTimer;
    private CharacterControl _player;

    public override void _Ready()
    {
        base._Ready();
        _recipeTimer = GetNode<Timer>("RecipeTimer");
        _recipeTimer.Timeout += _OnRecipeTimer;
        BodyEntered += OnCraftingStationBodyEntered;
        BodyExited += OnCraftingStationBodyExited;
    }

    private void OnCraftingStationBodyEntered(Node2D body)
    {
        if (body is not CharacterControl character) return;
        _player = character;
        if (_player.HeldItem is null) return;

        foreach (var recipe in Recipes)
        {
            GD.Print("Attempting to start recipe: " + recipe.DisplayName);
            if( _player.HeldItem.CompletedRecipes.Contains(recipe)) continue;

            var hasInputs = false;
            foreach (var wepMod in recipe.Inputs)
            {
                if (_player.HeldItem.Modifiers.Contains(wepMod))
                {
                    hasInputs = true;
                    continue;
                }
                else
                {
                    hasInputs = false;
                    break;
                }
            }
            if (!hasInputs) continue;
            GD.Print("Weapon has required inputs");
            _currentRecipe = recipe;
            switch (_currentRecipe.WorkType)
            {
                case WorkType.Instant:
                    GD.Print("Starting WorkType Instant.");
                    _RecipeComplete();
                    break;
                case WorkType.SpamButton:
                    GD.Print("idk i didnt add this yet...");
                    break;
                case WorkType.Timer:
                    GD.Print("Starting WorkType Timer.");
                    _recipeTimer.Start(recipe.TimeToComplete);
                    break;
                case WorkType.ButtonHold:
                    GD.Print("idk i didnt add this yet...");
                    break;
            }
            GD.Print("Recipe started.");
            break;
        }
    }

    private void OnCraftingStationBodyExited(Node2D body)
    {
        if (_recipeTimer.IsStopped()) return;
        _recipeTimer.Stop();
        GD.Print("Recipe ended before complete");
    }

    private void _OnRecipeTimer()
    {
        GD.Print("Timer Complete");
        _RecipeComplete();
    }

    private void _RecipeComplete()
    {
        if (_player.HeldItem is null) return;
        foreach (var wepMod in _currentRecipe.Outputs)
        {
            _player.HeldItem.Modifiers.Add(wepMod);
        }
        _player.HeldItem.CompletedRecipes.Add(_currentRecipe);
        GD.Print("Completed recipe: " + _currentRecipe.DisplayName);
    }
}
