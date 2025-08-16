using System.Linq;
using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Interactive.Inventory;
using SoYouWANNAJam2025.Code.Interactive.Items;
using SoYouWANNAJam2025.Code.Player;
using SoYouWANNAJam2025.Code.World;

namespace SoYouWANNAJam2025.Code.Interactive.Stations;

[Tool]
public partial class Dispenser : Interactible
{
    private GenericItemTemplate _itemTemplate;
    private Sprite2D _sprite;
    
    [Export]
    public GenericItemTemplate ItemResource
    {
        get => _itemTemplate;
        set
        {
            _itemTemplate = value;
            _updateSprite();
        }
        
    }
    
    public override void _Ready()
    {
        base._Ready();
        _updateSprite();
        
        if (Engine.IsEditorHint()) return;
        Interact += OnInteractMethod;
    }

    private void _updateSprite()
    {
        if (_sprite == null) _sprite = GetNode<Sprite2D>("Sprite2D");
        if (_itemTemplate == null)
        {
            _sprite.Texture = GD.Load<Texture2D>("res://Assets/Sprites/Unknown.png");
            _sprite.Modulate = Colors.White;
            return;
        }
            
        _sprite.Texture = ImageTexture.CreateFromImage(ItemResource.GetItemImage());
    }
    
    private void OnInteractMethod(Node2D node, TriggerType trigger)
    {
        if (node is not PlayerInteractor interactor) return;

        switch (trigger)
        {
            case TriggerType.PickupDrop:
                if (interactor.InventorySlot.HasSpace())
                {
                    var newItemScene = GD.Load<PackedScene>("res://Entities/Interactive/Items/GenericItem.tscn");
                    var newItem = newItemScene.Instantiate<GenericItem>();
                    newItem.ItemResource = ItemResource;
                    Global.Grid.AddChild(newItem);
                    interactor.InventorySlot.PickupItem(newItem, true);
                }
                else if (interactor.InventorySlot.HasItem())
                {
                    if (interactor.InventorySlot.Item.ItemResource != ItemResource) break;
                    
                    Array<GenericItemTemplate> itemArray = [ItemResource];
                    interactor.InventorySlot.DestroyItem(itemArray);
                }
                break;
            case TriggerType.UseAction:
                break;
        }
    }
    
}