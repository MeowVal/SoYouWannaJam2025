using System.Linq;
using Godot;
using Godot.Collections;

namespace SoYouWANNAJam2025.Code;


public enum TriggerType
{
    PickupDrop,
    UseAction
}

[GlobalClass][Tool]
public partial class Interactible : Area2D
{

    private bool _interactive = true;
    private Array<int> _defaultZIndices = [];
    public Array<Sprite2D> Sprites = [];

    [Export]
    public bool IsInteractive
    {
        set 
        { 
            _interactive = value;
            Monitorable = value;
        }
        get => _interactive;
    }
    
    private float _colliderRadius = 16f;
    
    [Export]
    public float ColliderRadius
    {
        set 
        {
            _colliderRadius = value;
            if (FindChild("Collider") is CollisionShape2D collider)
            {
                var circle = new CircleShape2D();
                circle.Radius = _colliderRadius;
                collider.SetShape(circle);
            }
        }
        get => _colliderRadius;
    }
    
    [Signal] public delegate void InteractEventHandler(Node2D node, TriggerType trigger);

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;
        
        Interact += OnInteractMethod;
        var foundSprites = GetChildren()
            .Where(child => child is Sprite2D)
            .Select(child => child)
            .Cast<Sprite2D>();
        foreach (var sprite in foundSprites)
        {
            Sprites.Add(sprite);
            _defaultZIndices.Add(sprite.ZIndex);
        }
    }

    private void OnInteractMethod(Node2D node, TriggerType trigger) { }

    public void TriggerInteraction(Node2D node, TriggerType trigger)
    {
        EmitSignal(SignalName.Interact, node, Variant.From(trigger));
    }

    public void SetHighlight(bool enabled)
    {
        foreach (var sprite in Sprites)
        {
            if (enabled)
            {
                sprite.Modulate = new Color(1.5f, 1.5f, 1.5f, 1.5f);
            }
            else
            {
                sprite.Modulate = new Color(1, 1, 1, 1);
            }
        }
    }

    public void SetZIndexOffset(int offset)
    {
        for (var i = 0; i < Sprites.Count; i++)
        {
            Sprites[i].ZIndex = _defaultZIndices[i] + offset;
        }
    }
}