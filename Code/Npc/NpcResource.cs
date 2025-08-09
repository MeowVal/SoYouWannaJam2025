using Godot;

namespace SoYouWANNAJam2025.Code.Npc;
[Tool]
[GlobalClass]
public partial class NpcResource : Resource
{
    [Export]
    public string NpcName = ""; 
    [Export]
    public Texture2D Sprite2D;
    
}
