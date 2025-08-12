using Godot;

namespace SoYouWANNAJam2025.Code.Items;

[Tool]
public partial class GenericItem : Interactible
{
    private BaseItem _baseItem;
    
    [Export]
    public BaseItem ItemResource
    {
        set
        { 
            _baseItem = value;
            if (value != null)
            {
                GetNode<Sprite2D>("ItemSprite").Texture = value.Icon;
                ColliderRadius = value.Size;
            }
            else
            {
                GetNode<Sprite2D>("ItemSprite").Texture = null;
                ColliderRadius = 16f;
            }
        }
        get => _baseItem;
    }

    private CharacterControl _player;

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;
        Interact += OnInteractMethod;
    }
    
    private void OnInteractMethod(Node2D node, TriggerType trigger)
    {
        if (node is not PlayerInteractor interactor) return;
        if (trigger is TriggerType.PickupDrop)
        {
            interactor.InventorySlot.PickupItem(this);
        }
    }
}