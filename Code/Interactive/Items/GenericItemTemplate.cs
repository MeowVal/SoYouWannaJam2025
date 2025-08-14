using Godot;

namespace SoYouWANNAJam2025.Code.Interactive.Items;

[GlobalClass][Tool]
public partial class GenericItemTemplate: Resource
{
    [Export] public string DisplayName = "Unknown";
    [Export] public string Description = "You need to actually set this lol.";
    [Export] public Texture2D Sprite = GD.Load<Texture2D>("res://Assets/Sprites/Unknown.png");
    [Export] public Color SpriteColour = Colors.White;
    [Export] public float Size = 10;
}