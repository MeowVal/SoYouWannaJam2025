using Godot;
using SoYouWANNAJam2025.Code.Interactive.Stations;
using SoYouWANNAJam2025.Code.Interactive.Stations;

namespace SoYouWANNAJam2025.Scenes.UI.Interactions;

public partial class CraftingTimer : CraftingStationInterface
{
    private ProgressBar _bar;
    private Label _label;
    private Timer _recipeTimer;

    public override void _Ready()
    {
        _bar = GetNode<ProgressBar>("VBoxContainer/RecipeProgress");
        _label = GetNode<Label>("VBoxContainer/HBoxContainer/RecipeName");
        _recipeTimer = GetNode<Timer>("RecipeTimer");
        _recipeTimer.Timeout += _OnRecipeTimer;
        _recipeTimer.Start(Station.CurrentRecipe.TimeToComplete);
        _label.Text = Station.CurrentRecipe.DisplayName;
    }

    public override void Init(CraftingStation station)
    {
        base.Init(station);
    }

    public override void _Process(double delta)
    {
        Update(delta);
    }

    public override void Update(double delta)
    {
        base.Update(delta);
        if (Station == null) return;
        _bar.Value = 1 - (_recipeTimer.TimeLeft / _recipeTimer.WaitTime);
    }
    
    private void _OnRecipeTimer()
    {
        GD.Print("Recipe Timer Completed");
        Station.RecipeComplete();
    }
}