using System;
using System.Linq;
using Godot;

namespace SoYouWANNAJam2025.Code.Interactive.Items;

[Tool] [GlobalClass]
public partial class GenericItem : Interactible
{
    private GenericItemTemplate _itemTemplate;
    public Sprite2D Sprite;
    
    [Export]
    public virtual GenericItemTemplate ItemResource
    {
        set
        {
            _itemTemplate = (GenericItemTemplate)value.Duplicate();
            if (_itemTemplate == null)
            {
                //GD.Print("DEFAULT TEMPLATE");
                Sprite.Texture = GD.Load<Texture2D>("res://Assets/Sprites/Unknown.png");
                Sprite.Modulate = Colors.White;
                return;
            }
            ColliderRadius = value.Size;
            if (Sprite == null) Sprite = GetNode<Sprite2D>("Sprite2D");
            DrawSprite();
        }
        get => _itemTemplate;
    }

    private Player.CharacterControl _player;

    public override void _Ready()
    {
        Sprite = GetNode<Sprite2D>("Sprite2D");
        if (Engine.IsEditorHint()) return;
        base._Ready();
        Interact += OnInteractMethod;
        DrawSprite();
    }

    private void OnInteractMethod(Node2D node, TriggerType trigger)
    {
        if (node is not Player.PlayerInteractor interactor) return;
        if (trigger is TriggerType.PickupDrop)
        {
            interactor.InventorySlot.PickupItem(this);
        }
    }

    public virtual void DrawSprite()
    {
        GD.Print("ITEM DRAW");
        Sprite.Texture = ImageTexture.CreateFromImage(_itemTemplate.GetItemImage());
    }
}