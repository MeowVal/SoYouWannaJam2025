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
    private TextureRect _icon;

    
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
        _bar = GetNode<ProgressBar>("Control2/VBoxContainer/BarsMarginContainer/BarsVBox/RecipeProgress");
        _label = GetNode<Label>("Control2/VBoxContainer/MarginContainer2/PanelContainer/MarginContainer/HBoxContainer/ItemName");
        _icon = GetNode<TextureRect>("Control2/VBoxContainer/MarginContainer2/PanelContainer/MarginContainer/HBoxContainer/ItemTexture");
        _strengthBar = GetNode<ProgressBar>("Control2/VBoxContainer/BarsMarginContainer/BarsVBox/ProgressBar");
        _recipeTimer = GetNode<Timer>("RecipeTimer");
        _recipeTimer.Timeout += _OnRecipeTimer;
        
        switch (Station)
        {
            case CraftingStation craftingStation:
                //_recipeTimer.Start(craftingStation.CurrentRecipe.TimeToComplete);
                _label.Text = craftingStation.CurrentRecipe.DisplayName;
                if (craftingStation.CurrentRecipe.Icon != null)
                {
                    _icon.Texture = craftingStation.CurrentRecipe.Icon;
                }
                else
                {
                    _icon.Texture = ImageTexture.CreateFromImage(craftingStation.CurrentRecipe.RecipeOutputs[0].GetItemImage());
                }

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