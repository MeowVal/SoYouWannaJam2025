using System.Collections.Generic;
using Godot;
using SoYouWANNAJam2025.Code.World;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Interactive.Stations;
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
        _recipeTimer.Start(Station.CurrentRecipe.TimeToComplete);
        _label.Text = Station.CurrentRecipe.DisplayName;

        _pressRequired = (int)(Station.CurrentRecipe.SpamPerSecond * Station.CurrentRecipe.TimeToComplete);
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
        
        _strengthBar.Value = (float)_pressCount / _pressRequired;
        
        /*if (_strength <= 0)
        {
            Station.RecipeAbort();
        }*/
        if (_pressCount >= _pressRequired)
        {
            Station.RecipeComplete();
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (Input.IsActionJustPressed("UseAction"))
        {
            _pressCount++;
        }
    }

    private void _OnRecipeTimer()
    {
        if ((float)_pressCount / _pressRequired > 0.75)
        {
            Station.RecipeComplete();
        }
        else
        {
            Station.RecipeAbort();
        }
        
    }


}