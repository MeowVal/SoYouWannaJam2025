using Godot;
using Godot.Collections;

namespace SoYouWANNAJam2025.Code.Items;

[GlobalClass][Tool]
public partial class BaseItem: Resource
{
    [Export] public string DisplayName = "Unknown";
    [Export] public string Description = "You need to actually set this lol.";
    [Export] public Texture2D Icon;
    [Export] public float Size = 10;
}