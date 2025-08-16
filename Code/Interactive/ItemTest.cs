using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Interactive.Items;
using SoYouWANNAJam2025.Code.Player;
using SoYouWANNAJam2025.Code.World;

namespace SoYouWANNAJam2025.Code.Interactive;

public partial class ItemTest : Interactible
{
    
    [Export] public EModularItemType ItemType;
    [Export] public ModularItemTemplate DefaultItemTemplate;
    
    private ModularItem _modularItem;
    public override void _Ready()
    {
        var modularItemScene = GD.Load<PackedScene>("res://Entities/Interactive/Items/ModularItem.tscn");
        var newItem = modularItemScene.Instantiate<ModularItem>();
        newItem.ItemResource = DefaultItemTemplate;
        newItem.ModularItemType = ItemType;
        _modularItem = newItem;
        AddChild(newItem);
        
        base._Ready();
        
        Interact += OnInteractMethod;
    }

    private void _updateItem()
    {
        if (_modularItem != null) _modularItem.QueueFree();
        _modularItem = Global.GameManager.NewItem(ItemType);
        AddChild(_modularItem);
    }
    
    private void OnInteractMethod(Node2D node, TriggerType trigger)
    {
        if (node is not PlayerInteractor interactor) return;

        switch (trigger)
        {
            case TriggerType.PickupDrop:
                break;
            case TriggerType.UseAction:
                _updateItem();
                break;
        }
    }
    
}