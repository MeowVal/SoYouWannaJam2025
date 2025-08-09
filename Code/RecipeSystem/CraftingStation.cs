using Godot;
using System;

public partial class CraftingStation : Area2D
{
    [ExportGroup("Info")]
    [Export] public string DisplayName = "Unknown";
    [Export] public string Description = "You need to actually set this lol.";
    [Export] public Texture2D Icon;
    [Export] public BaseRecipe Recipe;

    private Timer _recipeTimer;
    private CharacterControl _player;

    public override void _Ready()
    {
        base._Ready();
        _recipeTimer = GetNode<Timer>("RecipeTimer");
    }

    private void OnCraftingStationBodyEntered(PhysicsBody2D body)
    {
        if (body is not CharacterControl character) return;
        if (character.HeldItem is null) return;
        _player = character;
        var hasInputs = false;
        foreach (var wepMod in Recipe.Inputs)
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
        if (!hasInputs) return;
        _recipeTimer.Timeout += _OnRecipeTimer;
        _recipeTimer.Start(Recipe.TimeToComplete);
    }

    private void _OnRecipeTimer()
    {
        if (_player.HeldItem is null) return;
        foreach (var wepMod in Recipe.Outputs)
        {
            _player.HeldItem.Modifiers.Add(wepMod);
        }
        GD.Print("Completed recipe: " + Recipe.DisplayName);
    }
}
