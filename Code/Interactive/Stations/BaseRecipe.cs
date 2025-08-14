using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Interactive.Items;

namespace SoYouWANNAJam2025.Code.Interactive.Stations;

public enum EWorkType
{
    SpamButton,
    Inputs,
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

    [ExportGroup("Inputs/Outputs")]
    [Export] public ERecipeType RecipeType = ERecipeType.Standard;
    [Export] public Array<GenericItemTemplate> ItemInputs = [];
    [Export] public Array<GenericItemTemplate> ItemOutputs = [];
    [Export] public ModularPartTemplate PartOutput;

    [ExportGroup("Minigame")]
    [Export] public EWorkType WorkType = EWorkType.Instant;
    [Export] public float TimeToComplete = 1.5f;
    [Export] public int SequenceLength = 6;
}