using System.Collections.Generic;
using Godot;
using SoYouWANNAJam2025.Code.World;
using Godot.Collections;
using SoYouWANNAJam2025.Code;
using SoYouWANNAJam2025.Code.Interactive.Stations;
using SoYouWANNAJam2025.Code.Interactive.Stations;

namespace SoYouWANNAJam2025.Scenes.UI.Interactions;

public partial class CraftingInputs : CraftingStationInterface
{
    private ProgressBar _bar;
    private Label _label;
    private HBoxContainer _container;

    private int _index = 0;
    private int[] _sequence = [];
    private int _lenght = 6;
    private Array<Label> _labels = [];
    
    private bool[] _lastDirection = [false, false, false, false];
    private Timer _recipeTimer;

    
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
        _container = GetNode<HBoxContainer>("Control/MarginContainer/VBoxContainer/ArrowContainer");
        _recipeTimer = GetNode<Timer>("RecipeTimer");
        _recipeTimer.Timeout += _OnRecipeTimer;
        switch (Station)
        {
            case CraftingStation craftingStation:
                _recipeTimer.Start(craftingStation.CurrentRecipe.TimeToComplete);
                _label.Text = craftingStation.CurrentRecipe.DisplayName;
                _lenght = craftingStation.CurrentRecipe.SequenceLength;
                break;
        }
        
        
        _sequence = new int[_lenght];
        
        var rng = new RandomNumberGenerator();
        string[] arrows = ["🠅","🠇","🠄","🠆"];
        

        for (var i = 0; i < _lenght; i++)
        {
            var newLabel = new Label();
            
            newLabel.Name = "Label"+i;
            newLabel.LabelSettings = new LabelSettings();
            newLabel.LabelSettings.FontSize = 32;
            newLabel.Modulate = new Color(1, 0, 0, 1);
            _sequence[i] = rng.RandiRange(0, 3);
            newLabel.Text = arrows[_sequence[i]];
            
            _labels.Add(newLabel);
            _container.AddChild(newLabel);
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
        _bar.Value = 1 - (_recipeTimer.TimeLeft / _recipeTimer.WaitTime);
    }
    
    private void _OnRecipeTimer()
    {
        switch (Station)
        {
            case CraftingStation craftingStation:
                craftingStation.RecipeAbort();
                break;
        }
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        
        var up = Input.IsActionJustPressed("MinigameUp");
        //if (up && Input.GetActionStrength("up") < 0.5) up = false;
        var down = Input.IsActionJustPressed("MinigameDown");
        //if (down && Input.GetActionStrength("down") < 0.5) down = false;
        var left = Input.IsActionJustPressed("MinigameLeft");
        //if (left && Input.GetActionStrength("left") < 0.5) left = false;
        var right = Input.IsActionJustPressed("MinigameRight");
        //if (right && Input.GetActionStrength("right") < 0.5) right = false;
        
        if (Input.IsActionJustPressed("ui_cancel") && Station is CraftingStation craftingStation2)
        {
            craftingStation2.RecipeAbort();
        }
        
        if (!up && !down && !left && !right) return;
        
        bool[] direction = [up, down, left, right];
        if (_lastDirection == direction) return;
        _lastDirection = direction;

        if (direction[_sequence[_index]])
        {
            GD.Print($"{_index} Success");
            _index++;

            if (_index >= _lenght)
            {
                GD.Print("WIN!");
                Global.Player.Frozen = false;
                _recipeTimer.Stop();
                switch (Station)
                {
                    case CraftingStation craftingStation:
                        craftingStation.RecipeComplete();
                        break;
                }
            }
        }
        else
        {
            GD.Print($"{_index} Failed");
            _index = 0;
        }
        
        for (var i = 0; i < _lenght; i++)
        {
            _labels[i].Modulate = i < _index ? new Color(0, 1, 0, 1): new Color(1, 0, 0, 1);
        }
    }
}