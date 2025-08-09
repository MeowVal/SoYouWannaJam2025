using Godot;

namespace SoYouWANNAJam2025.Code.Npc;

public partial class NpcBehaviour : Node
{
    private Npc _npc;


    public override void _Ready()
    {
        var p = GetParent(); 
        if (p is Npc npc)
        {
            _npc = npc;
            //connect to signal
        }
        
        
    }
}