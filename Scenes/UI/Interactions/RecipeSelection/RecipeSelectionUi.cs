using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code;
using SoYouWANNAJam2025.Code.Interactive.Stations;
using SoYouWANNAJam2025.Code.World;

namespace SoYouWANNAJam2025.Scenes.UI.Interactions.RecipeSelection;

public partial class RecipeSelectionUi : CraftingStationInterface
{

    private Array<BaseRecipe> _recipes = [];
    private int _index = 0;
    private bool _pressed = false;

    private PackedScene _rowScene = GD.Load<PackedScene>("res://Scenes/UI/Interactions/RecipeSelection/RecipeRow.tscn");

    private Array<RecipeRow> _rows = [];

    public Array<BaseRecipe> RecipeList
    {
        set
        {
            _recipes = value;
            BuildRecipeList();
        }
        get => _recipes;
    }

    private void BuildRecipeList()
    {
        _rows = [];
        var vBox = GetNode<VBoxContainer>("Control2/VBoxContainer/RecipesMarginContainer/RecipesVBox");
        foreach (var child in vBox.GetChildren())
        {
            child.QueueFree();
        }
        foreach (var recipe in RecipeList)
        {
            GD.Print(recipe.DisplayName);
            var newRow = _rowScene.Instantiate<RecipeRow>();
            vBox.AddChild(newRow);
            newRow.SetRecipe(recipe);
            newRow.SetHighlighted(false);
            _rows.Add(newRow);
        }
        _rows[_index].SetHighlighted(true);
    }

    public override void Init(Interactible station)
    {
        base.Init(station);
    }
    
    public override void _EnterTree()
    {
        base._EnterTree();
        Global.Player.Frozen = true;
    }

    public override void _ExitTree()
    {
        GD.Print("EXIT");
        Global.Player.Frozen = false;
        base._ExitTree();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        
        var up = Input.IsActionPressed("MinigameUp");
        var down = Input.IsActionPressed("MinigameDown");

        if (_pressed && Input.IsActionJustReleased("UseAction"))
        {
            Global.Player.Frozen = false;
            
            switch (Station)
            {
                case CraftingStation craftingStation:
                    craftingStation.FreeUi();
                    QueueFree();
                    craftingStation.RecipeBegin(RecipeList[_index]);
                    break;
            }
            return;
        }
        if (Input.IsActionJustPressed("ui_cancel") && Station is CraftingStation craftingStation2)
        {
            craftingStation2.RecipeAbort();
        }
        _pressed = true;
        if ((up && down) || (!up && !down)) return;
        
        if (up && _index > 0)
        {
            _index -= 1;
        }
        if (down && _index < RecipeList.Count-1)
        {
            _index += 1;
        }

        for (var i = 0; i < _rows.Count; i++)
        {
            _rows[i].SetHighlighted(i == _index);
        }
    }
}