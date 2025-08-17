using System.Linq;
using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code;
using SoYouWANNAJam2025.Code.Interactive.Stations;
using SoYouWANNAJam2025.Code.World;

namespace SoYouWANNAJam2025.Scenes.UI.Interactions;

public partial class RecipeSelectionUi : CraftingStationInterface
{

    private Array<BaseRecipe> _recipes = [];
    private int _index = 0;
    private bool _pressed = false;

    private HBoxContainer[] _hBoxs;

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
        _hBoxs = [];
        var vBox = GetNode<VBoxContainer>("Control/MarginContainer/VBoxContainer");
        foreach (var child in vBox.GetChildren())
        {
            child.QueueFree();
        }
        foreach (var recipe in RecipeList)
        {
            GD.Print(recipe.DisplayName);
            var img = recipe.RecipeOutputs[0].GetItemImage();
            
            var hBox = new HBoxContainer();
            var partTexture = new TextureRect();
            var partName = new Label();
            
            _hBoxs = _hBoxs.Append(hBox).ToArray();
            hBox.Modulate = new Color(0.5f, 0.5f, 0.5f, 1);
            if (recipe.Icon != null)
            {
                partTexture.Texture = recipe.Icon;
            }
            else
            {
                partTexture.Texture = ImageTexture.CreateFromImage(img);
            }
            
            partName.Text = recipe.RecipeOutputs[0].DisplayName;
            
            vBox.AddChild(hBox);
            hBox.AddChild(partTexture);
            hBox.AddChild(partName);
        }

        _hBoxs[0].Modulate = new Color(1, 1, 1, 1);
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

        for (var i = 0; i < _hBoxs.Length; i++)
        {
            var col = new Color(0.5f, 0.5f, 0.5f, 1);
            if (i == _index) col = new Color(1, 1, 1, 1);
            _hBoxs[i].Modulate = col;
        }
    }
}