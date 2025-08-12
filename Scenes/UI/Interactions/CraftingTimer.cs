using Godot;
using SoYouWANNAJam2025.Code.RecipeSystem;
using CraftingStation = SoYouWANNAJam2025.Code.Interactive.Stations.CraftingStation;
using CraftingStationInterface = SoYouWANNAJam2025.Code.Interactive.Stations.CraftingStationInterface;

namespace SoYouWANNAJam2025.Scenes.UI.Interactions;

public partial class CraftingTimer : CraftingStationInterface
{
    private ProgressBar _bar;
    private Label _label;

    public override void _Ready()
    {
        _bar = GetNode<ProgressBar>("VBoxContainer/RecipeProgress");
        _label = GetNode<Label>("VBoxContainer/HBoxContainer/RecipeName");
        _label.Text = Station.CurrentRecipe.DisplayName;
    }

    public override void Init(CraftingStation station)
    {
        base.Init(station);
    }

    public override void _Process(double delta)
    {
        Update();
    }

    public override void Update()
    {
        base.Update();
        if (Station == null) return;
        _bar.Value = 1 - (Station.RecipeTimer.TimeLeft / Station.RecipeTimer.WaitTime);
    }
}