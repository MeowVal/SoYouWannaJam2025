using Godot;
using System;

public partial class CraftingStation : Area2D
{
    [ExportGroup("Info")]
    [Export] public string Name = "Unknown Recipe";
    [Export] public string Description = "You need to actually set this lol.";
    [Export] public Texture2D Icon;
    [Export] public BaseRecipe Recipe;
}
