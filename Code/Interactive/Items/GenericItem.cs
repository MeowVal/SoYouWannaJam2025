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
        get => _itemTemplate;
        set
        {
            _itemTemplate = value;
            Sprite ??= GetNode<Sprite2D>("Sprite2D");
            if (_itemTemplate == null)
            {
                //GD.Print("DEFAULT TEMPLATE");
                Sprite.Texture = GD.Load<Texture2D>("res://Assets/Sprites/Unknown.png");
                Sprite.Modulate = Colors.White;
                return;
            }
            ColliderRadius = value.Size;
            
            DrawSprite();
        }
        
    }

    private Player.CharacterControl _player;

    public override void _Ready()
    {
        DrawSprite();
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

    public virtual void DrawSprite()
    {
        if (Sprite == null) Sprite = GetNode<Sprite2D>("Sprite2D");
        var img = _itemTemplate.GetItemImage();
        if (img == null || Sprite == null) return;
        Sprite.Texture = ImageTexture.CreateFromImage(img);
    }
}