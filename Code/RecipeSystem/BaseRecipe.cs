using Godot;
using System;
using Godot.Collections;

public enum WorkType
{
    SpamButton,
    Timer,
    Instant
}

public partial class Recipe : Resource
{
    [ExportGroup("Info")]
    [Export] public string Name = "Unknown Recipe";
    [Export] public string Description = "You need to actually set this lol.";
    [Export] public Texture2D Icon;

    [ExportGroup("Recipe Data")]
    [Export] public WorkType WorkType = WorkType.Instant;
    [Export] public float TimeToComplete = 1.5f;
    [Export] public Array<BaseWeaponModifier> Inputs = [];
    [Export] public Array<BaseWeaponModifier> Outputs = [];
}
