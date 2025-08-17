using Godot;
using SoYouWANNAJam2025.Code;
using SoYouWANNAJam2025.Code.World;
using SoYouWANNAJam2025.Code.Interactive.Stations;

namespace SoYouWANNAJam2025.Scenes.UI.Interactions;

public partial class CraftingButtonSpam : CraftingStationInterface
{
    private ProgressBar _bar;
    private Label _label;
    private Timer _recipeTimer;

    
    private int _pressCount = 0;
    private int _pressRequired = 0;
    private ProgressBar _strengthBar;
    
    public override void _EnterTree()
    {
        base._EnterTree();
        Global.Player.Frozen = true;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Global.Player.Frozen = false;
    }

    public override void _Ready()
    {
        _bar = GetNode<ProgressBar>("Control/MarginContainer/VBoxContainer/RecipeProgress");
        _label = GetNode<Label>("Control/MarginContainer/VBoxContainer/HBoxContainer/RecipeName");
        _strengthBar = GetNode<ProgressBar>("Control/MarginContainer/VBoxContainer/ProgressBar");
        _recipeTimer = GetNode<Timer>("RecipeTimer");
        _recipeTimer.Timeout += _OnRecipeTimer;
        
        switch (Station)
        {
            case CraftingStation craftingStation:
                //_recipeTimer.Start(craftingStation.CurrentRecipe.TimeToComplete);
                _label.Text = craftingStation.CurrentRecipe.DisplayName;
                _pressRequired = (int)(craftingStation.CurrentRecipe.SpamPerSecond * craftingStation.CurrentRecipe.TimeToComplete);
                break;
        }
        
    }

    public override void Init(Interactible station)
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

        if (_pressCount > 0)
        {
            _bar.Value = 1 - (_recipeTimer.TimeLeft / _recipeTimer.WaitTime);
            _strengthBar.Value = (float)_pressCount / _pressRequired;   
        }
        
        if (_pressCount >= _pressRequired)
        {
            switch (Station)
            {
                case CraftingStation craftingStation:
                    craftingStation.RecipeComplete();
                    break;
            }
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (Input.IsActionJustPressed("UseAction") || Input.IsActionJustPressed("PickupDrop"))
        {
            if (_pressCount == 0 && _recipeTimer.IsStopped() && Station is CraftingStation craftingStation)
            {
                _recipeTimer.Start(craftingStation.CurrentRecipe.TimeToComplete);
            }
            _pressCount++;
        }

        if (Input.IsActionJustPressed("ui_cancel") && Station is CraftingStation craftingStation2)
        {
            craftingStation2.RecipeAbort();
        }
    }

    private void _OnRecipeTimer()
    {
        var completed = (float)_pressCount / _pressRequired > 0.75;
        switch (Station)
        {
            case CraftingStation craftingStation:
                if (completed) craftingStation.RecipeComplete(); else craftingStation.RecipeAbort();
                break;
        }
        
    }


}