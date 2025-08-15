using System;
using System.Collections.Immutable;
using System.Linq;
using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Interactive.Items;
using SoYouWANNAJam2025.Entities.Interactive.Items;

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
    ModularPartStateChange
}


[GlobalClass]
public partial class BaseRecipe : Resource
{
    [ExportGroup("Info")]
    [Export] public string DisplayName = "Unknown";
    [Export] public string Description = "You need to actually set this lol.";
    [Export] public Texture2D Icon;

    [ExportGroup("Inputs & Outputs")]
    [Export] public ERecipeType RecipeType = ERecipeType.Standard;

    [ExportSubgroup("Standard")]
    [Export] public Array<GenericItemTemplate> StandardItemInputs = [];
    [Export] public Array<GenericItemTemplate> StandardOutputs = [];

    [ExportSubgroup("PartSwap")]
    [Export] public Array<GenericItemTemplate> PartSwapItemInputs = [];
    [Export] public ModularPartTemplate PartSwapOutput;

    [ExportSubgroup("PartStateChange")]
    [Export] public EModularItemType PartStateChangeItemType;
    [Export] public Array<GenericItemTemplate> PartStateChangeItemInputs = [];
    [Export] public EPartType PartStateChangeTarget;
    [Export] public EPartState PartStateChangeValue = EPartState.New;

    [ExportGroup("Minigame")]
    [Export] public EWorkType WorkType = EWorkType.Instant;
    [Export] public float TimeToComplete = 1.5f;
    [Export] public int SequenceLength = 6;
    [Export] public int SpamPerSecond = 5;

    // Use this to wildcard grab whatever inputs are defined
    public Array<GenericItemTemplate> ItemInputs
    {
        get
        {
            switch (RecipeType)
            {
                case ERecipeType.Standard:
                    return StandardItemInputs;
                case ERecipeType.ModularPartSwap:
                    return PartSwapItemInputs;
                case ERecipeType.ModularPartStateChange:
                    return PartStateChangeItemInputs;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}