using Godot;

namespace SoYouWANNAJam2025.Code;

public partial class Interactible : Area2D
{

    private bool _interactive = true;

    [Export]
    public bool IsInteractive
    {
        set 
        { 
            _interactive = value;
            Monitoring = value;
        }
        get => _interactive;
    }

    [Signal]
    public delegate void InteractEventHandler(Node2D node);

    public void TriggerInteraction(Node2D node)
    {
        EmitSignal(SignalName.Interact, node);
    }
}