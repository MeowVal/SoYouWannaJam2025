using Godot;

namespace SoYouWANNAJam2025.Code;

public partial class Interactible : Area2D
{
    [Signal]
    public delegate void InteractEventHandler(Node2D node);

    public void TriggerInteraction(Node2D node)
    {
        EmitSignal(SignalName.Interact, node);
    }
}