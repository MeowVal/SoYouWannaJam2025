using System.Linq;
using Godot;

namespace SoYouWANNAJam2025.Code;

public partial class Interactible : Area2D
{
    [Signal]
    public delegate void InteractEventHandler(Node2D node);

    public override void _Ready()
    {
        Interact += OnInteractMethod;
    }

    private void OnInteractMethod(Node2D node)
    {
        if (node is not CharacterControl character) return;

        GD.Print("Interacted with: ", character.Name);
    }

    public void TriggerInteraction(Node2D node)
    {
        EmitSignal(SignalName.Interact, node);
    }

    public void SetHighlight(bool enabled)
    {
        var sprites = GetChildren()
            .Where(child => child is Sprite2D)
            .Select(child => child)
            .Cast<Sprite2D>();
        if (enabled)
        {
            foreach (var sprite in sprites)
            {
                sprite.Modulate = new Color(1.5f, 1.5f, 1.5f, 1.5f);
            }
        }
        else
        {
            foreach (var sprite in sprites)
            {
                sprite.Modulate = new Color(1, 1, 1, 1);
            }
        }
    }
}