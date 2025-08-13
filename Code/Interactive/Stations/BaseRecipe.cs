using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Interactive.Items;

namespace SoYouWANNAJam2025.Code.RecipeSystem;

public enum EWorkType
{
    SpamButton,
    Timer,
    Instant,
    ButtonHold
}

public enum ERecipeType
{
    Standard,
    ModularPartSwap,
    ModularStatChange
}


[GlobalClass]
public partial class BaseRecipe : Resource
{
    [ExportGroup("Info")]
    [Export] public string DisplayName = "Unknown";
    [Export] public string Description = "You need to actually set this lol.";
    [Export] public Texture2D Icon;

    [ExportGroup("Recipe Data")]
    [Export] public EWorkType WorkType = EWorkType.Instant;
    [Export] public float TimeToComplete = 1.5f;
    [Export] public ERecipeType RecipeType = ERecipeType.Standard;
    [Export] public Array<GenericItemTemplate> ItemInputs = [];
    [Export] public Array<GenericItemTemplate> ItemOutputs = [];
}