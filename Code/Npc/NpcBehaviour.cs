using Godot;

namespace SoYouWANNAJam2025.Code.Npc;

public partial class NpcBehaviour : Node2D
{
    protected  Npc Npc;


    public override void _Ready()
    {
        if (GetParent() is Npc npc)
        {
            Npc = npc;
            Npc.DoBehaviorEnabled += Start;
        
        }
    }

    public virtual async void Start()
    {
        // TODO: Implement behavior
    }
}