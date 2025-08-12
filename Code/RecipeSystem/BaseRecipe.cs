using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Items;

namespace SoYouWANNAJam2025.Code.RecipeSystem;

public enum WorkType
{
    SpamButton,
    Timer,
    Instant,
    ButtonHold
}

[GlobalClass]
public partial class BaseRecipe : Resource
{
    [ExportGroup("Info")]
    [Export] public string DisplayName = "Unknown";
    [Export] public string Description = "You need to actually set this lol.";
    [Export] public Texture2D Icon;

    [ExportGroup("Recipe Data")]
    [Export] public WorkType WorkType = WorkType.Instant;
    [Export] public float TimeToComplete = 1.5f;
    [Export] public Array<BaseItem> Inputs = [];
    [Export] public Array<BaseItem> Outputs = [];
}