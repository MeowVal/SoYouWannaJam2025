using Godot;
using SoYouWANNAJam2025.Code.Interactive.Stations;

namespace SoYouWANNAJam2025.Scenes.UI.Interactions.RecipeSelection;

public partial class RecipeRow : PanelContainer
{
    private Label _recipeName;
    private TextureRect _recipeIcon;
    private TextureRect _highlightIcon;

    public override void _Ready()
    {
        _recipeName = GetNode<Label>("HBoxContainer/ItemText");
        _recipeIcon = GetNode<TextureRect>("HBoxContainer/ItemIcon");
        _highlightIcon = GetNode<TextureRect>("HBoxContainer/HighlightArrow");
    }

    public void SetRecipe(BaseRecipe recipe)
    {
        _recipeName.Text = recipe.DisplayName;
        if (recipe.Icon != null)
        {
            _recipeIcon.Texture = recipe.Icon;
        }
        else if (recipe.RecipeOutputs.Count > 0)
        {
            _recipeIcon.Texture = ImageTexture.CreateFromImage(recipe.RecipeOutputs[0].GetItemImage());
        }
    }

    public void SetHighlighted(bool highlighted)
    {
        if (highlighted)
        {
            Modulate = new Color(1f, 1f, 1f);
            _highlightIcon.Visible = true;
        }
        else
        {
            Modulate = new Color(0.7f, 0.7f, 0.7f);
            _highlightIcon.Visible = false;
        }
    }
}