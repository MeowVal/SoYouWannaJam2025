using Godot;

namespace SoYouWANNAJam2025.Code.Npc.Friendly;
[Tool]
[GlobalClass]
public partial class NpcResource : Resource
{
    [ExportGroup("Setup")]
    [Export]
    public Texture2D Sprite2D;
    [ExportCategory("Npc Stats")]
    [ExportGroup("Stats")]
    [Export] public Vector2 MinMaxHealth;
    [Export] public Vector2 MinMaxSpeed;
    [Export] public Vector2 MinMaxStrength;
    [Export] public Vector2 MinMaxConstitution;
    [Export] public Vector2 MinMaxDexterity;
    [Export] public Vector2 MinMaxIntelligence;
    [Export] public Vector2 MinMaxWisdom;
    [Export] public Vector2 MinMaxCharisma;
    
    [ExportGroup("Name")]
    [Export] public string NpcName = ""; 
    [ExportGroup("Class")]
    [Export] public string Class;

}
