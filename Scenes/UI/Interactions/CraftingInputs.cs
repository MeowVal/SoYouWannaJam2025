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
    private TextureRect _icon;

    private int _index = 0;
    private int[] _sequence = [];
    private int _lenght = 6;
    private Array<TextureRect> _arrowRects = [];
    
    private bool[] _lastDirection = [false, false, false, false];
    private Timer _recipeTimer;

    private Array<Texture2D> _arrows = [
        GD.Load<Texture2D>("res://Assets/Sprites/UI/Icon_Up.png"),
        GD.Load<Texture2D>("res://Assets/Sprites/UI/Icon_Down.png"),
        GD.Load<Texture2D>("res://Assets/Sprites/UI/Icon_Left.png"),
        GD.Load<Texture2D>("res://Assets/Sprites/UI/Icon_Right.png")
    ];

    
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
        _bar = GetNode<ProgressBar>("Control2/VBoxContainer/BarsMarginContainer/BarsVBox/RecipeProgress");
        _label = GetNode<Label>("Control2/VBoxContainer/MarginContainer2/PanelContainer/MarginContainer/HBoxContainer/ItemName");
        _icon = GetNode<TextureRect>("Control2/VBoxContainer/MarginContainer2/PanelContainer/MarginContainer/HBoxContainer/ItemTexture");
        _container = GetNode<HBoxContainer>("Control2/VBoxContainer/BarsMarginContainer/BarsVBox/ArrowContainer");
        _recipeTimer = GetNode<Timer>("RecipeTimer");
        _recipeTimer.Timeout += _OnRecipeTimer;
        switch (Station)
        {
            case CraftingStation craftingStation:
                _recipeTimer.Start(craftingStation.CurrentRecipe.TimeToComplete);
                _label.Text = craftingStation.CurrentRecipe.DisplayName;
                if (craftingStation.CurrentRecipe.Icon != null)
                {
                    _icon.Texture = craftingStation.CurrentRecipe.Icon;
                }
                /*else
                {
                    _icon.Texture = ImageTexture.CreateFromImage(craftingStation.CurrentRecipe.RecipeOutputs[0].GetItemImage());
                }*/
                _lenght = craftingStation.CurrentRecipe.SequenceLength;
                break;
        }
        
        
        _sequence = new int[_lenght];
        
        var rng = new RandomNumberGenerator();

        for (var i = 0; i < _lenght; i++)
        {
            var newArrow = new TextureRect();
            _sequence[i] = rng.RandiRange(0, 3);
            newArrow.Texture = _arrows[_sequence[i]];
            newArrow.SetCustomMinimumSize(new Vector2(16,16));
            _arrowRects.Add(newArrow);
            _container.AddChild(newArrow);
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

        if (_index >= _lenght) return;

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
            if (i < _index)
            {
                _arrowRects[i].Modulate = Colors.Yellow;
            } else if (i > _index)
            {
                _arrowRects[i].Modulate = Colors.DimGray;
            }
            else
            {
                _arrowRects[i].Modulate = Colors.White;
            }
        }
    }
}