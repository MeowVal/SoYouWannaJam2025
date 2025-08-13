using System.Collections.Generic;
using Godot;
using SoYouWANNAJam2025.Code.World;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Interactive.Stations;
using SoYouWANNAJam2025.Code.Interactive.Stations;

namespace SoYouWANNAJam2025.Scenes.UI.Interactions;

public partial class CraftingInputs : CraftingStationInterface
{
    public int Lenght = 6;
    
    private ProgressBar _bar;
    private Label _label;
    private HBoxContainer _container;

    private int _index = 0;
    private int[] _sequence = [];
    private Array<Label> _labels = [];

    
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
        _bar = GetNode<ProgressBar>("VBoxContainer/RecipeProgress");
        _label = GetNode<Label>("VBoxContainer/HBoxContainer/RecipeName");
        _container = GetNode<HBoxContainer>("VBoxContainer/ArrowContainer");
        _label.Text = Station.CurrentRecipe.DisplayName;
        
        _sequence = new int[Lenght];
        
        var rng = new RandomNumberGenerator();
        string[] arrows = ["🠅","🠇","🠄","🠆"];
        

        for (var i = 0; i < Lenght; i++)
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

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        
        var up = Input.IsActionJustPressed("up");
        var down = Input.IsActionJustPressed("down");
        var left = Input.IsActionJustPressed("left");
        var right = Input.IsActionJustPressed("right");
        
        if (!up && !down && !left && !right) return;
        
        bool[] direction = [up, down, left, right];

        if (direction[_sequence[_index]])
        {
            GD.Print($"{_index} Success");
            _index++;

            if (_index >= Lenght)
            {
                GD.Print("WIN!");
                Global.Player.Frozen = false;
                Station.RecipeTimer.Stop();
                Station.CompleteRecipe();
            }
        }
        else
        {
            GD.Print($"{_index} Failed");
            _index = 0;
        }
        
        for (var i = 0; i < Lenght; i++)
        {
            _labels[i].Modulate = i < _index ? new Color(0, 1, 0, 1): new Color(1, 0, 0, 1);
        }
    }
}