using Godot;

namespace SoYouWANNAJam2025.Code.Npc;
[Tool]
public partial class NpcBehaviour : Node2D
{
    protected  Npc Npc;


    public override void _Ready()
    {
        if(Engine.IsEditorHint()) return;
        if (GetParent() is Npc npc)
        {
            Npc = npc;
            Npc.DoBehaviorEnabled += Start;
        
        }
    }

    public virtual async void Start()
    {
        if(Engine.IsEditorHint()) return;
    }
}