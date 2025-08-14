using Godot;

namespace SoYouWANNAJam2025.Code.Interactive.Items;

public partial class GenericItem : Interactible
{
    private Interactive.Items.GenericItemTemplate _itemTemplate;
    
    [Export]
    public Interactive.Items.GenericItemTemplate ItemResource
    {
        set
        { 
            _itemTemplate = value;
            var sprite = GetNode<Sprite2D>("ItemSprite");
            if (value != null)
            {
                sprite.Texture = value.Sprite;
                sprite.Modulate = value.SpriteColour;
                ColliderRadius = value.Size;
            }
            else
            {
                sprite.Texture = GD.Load<Texture2D>("res://Assets/Sprites/Unknown.png");
                sprite.Modulate = Colors.White;
                ColliderRadius = 16f;
            }
        }
        get => _itemTemplate;
    }

    private Player.CharacterControl _player;

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;
        base._Ready();
        Interact += OnInteractMethod;
    }
    
    private void OnInteractMethod(Node2D node, TriggerType trigger)
    {
        if (node is not Player.PlayerInteractor interactor) return;
        if (trigger is TriggerType.PickupDrop)
        {
            interactor.InventorySlot.PickupItem(this);
        }
    }
}