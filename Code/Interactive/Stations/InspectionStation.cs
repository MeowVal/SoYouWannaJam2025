using System.Linq;
using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Interactive.Inventory;
using SoYouWANNAJam2025.Code.Interactive.Items;
using SoYouWANNAJam2025.Code.Player;

namespace SoYouWANNAJam2025.Code.Interactive.Stations;

public partial class InspectionStation : Interactible
{
    public InventorySlot Inventory;

    private Node2D _interactionInterface;
    private Node2D _interfaceLocation;
    private ProgressBar _researchBar;
    private Timer _researchTimer;

    private bool _isResearching = false;
    private bool _showUi = true;
    
    public override void _Ready()
    {
        base._Ready();
        _interfaceLocation = GetNode<Node2D>("InterfaceLocation");
        Interact += OnInteractMethod;
        
        Inventory = GetNode<InventorySlot>("InventorySlot");
        AreaEntered += OnInteractableEntered;
        AreaExited += OnInteractableExited;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        if (_interactionInterface == null || !_isResearching) return;
        _researchBar.Value = 1 - (_researchTimer.TimeLeft / _researchTimer.WaitTime);
    }

    public void ResearchBegin()
    {
        CreateUiScene("res://Scenes/UI/Interactions/ResearchUITimer.tscn");
        _isResearching = true;
        
        _researchBar = _interactionInterface.GetNode<ProgressBar>("Control/MarginContainer/VBoxContainer/ProgressBar");
        _researchTimer = _interactionInterface.GetNode<Timer>("ResearchTimer");
        _researchTimer.Timeout += ResearchComplete;
        _researchTimer.Start(5);
        var label = _interactionInterface.GetNode<Label>("Control/MarginContainer/VBoxContainer/ItemName");
        label.Text = Inventory.Item.ItemResource.DisplayName;
    }
    
    public void ResearchAbort()
    {
        if (_interactionInterface != null) _interactionInterface!.QueueFree();
        _isResearching = false;
        _interactionInterface = null;
        _researchTimer = null;
        _researchBar = null;
    }

    public void ResearchComplete()
    {
        if (_interactionInterface != null) _interactionInterface!.QueueFree();
        _isResearching = false;
        _researchTimer = null;
        _researchBar = null;

        CreateUiScene("res://Scenes/UI/Interactions/ResearchUI.tscn");
        if (_interactionInterface == null || !Inventory.HasItem()) return;
        
        _interactionInterface.SetVisible(_showUi);
        
        var container = _interactionInterface.GetNode<Control>("Control/MarginContainer/VBoxContainer");
        
        var itemTexture = container.GetNode<TextureRect>("HBoxContainer/ItemTexture");
        var itemName = container.GetNode<Label>("HBoxContainer/ItemName");
        itemTexture.Texture = Inventory.Item.Sprite.Texture;
        itemName.Text = Inventory.Item.ItemResource.DisplayName;
        
        if (Inventory.Item is not ModularItem modularItem) return;
        
        var mContainer = new MarginContainer();
        var vBox = new VBoxContainer();
        container.AddChild(mContainer);
        mContainer.AddChild(vBox);

        foreach (var part in modularItem.Parts)
        {
            var hBox = new HBoxContainer();
            var partTexture = new TextureRect();
            var partName = new Label();
            
            partTexture.Texture = ImageTexture.CreateFromImage(part.Value.GetItemImage());
            partName.Text = part.Value.DisplayName;
            
            vBox.AddChild(hBox);
            hBox.AddChild(partTexture);
            hBox.AddChild(partName);
        }
    }


    private void OnInteractMethod(Node2D node, TriggerType trigger)
    {
        if (node is not PlayerInteractor interactor) return;

        switch (trigger)
        {
            case TriggerType.PickupDrop:
                if (Inventory.HasItem() && interactor.InventorySlot.HasSpace())
                {
                    Inventory.TransferTo(interactor.InventorySlot);
                    ResearchAbort();
                }
                else if (Inventory.HasSpace() && interactor.InventorySlot.HasItem())
                {
                    if (interactor.InventorySlot.Item.ItemResource is not (ModularPartTemplate or ModularItemTemplate)) return;
                    interactor.InventorySlot.TransferTo(Inventory);
                    ResearchBegin();
                }
                break;
            case TriggerType.UseAction:
                break;
        }
    }

    private void CreateUiScene(string path)
    {
        var uiScene = GD.Load<PackedScene>(path);
        _interactionInterface = uiScene.Instantiate<Node2D>();
        GetViewport().AddChild(_interactionInterface);
        _interactionInterface.GlobalPosition = _interfaceLocation.GlobalPosition;
    }

    private void OnInteractableEntered(Node2D node)
    {
        GD.Print("ENTER");
        if (node is not PlayerInteractor) return;
        _showUi = true;
        if (!(_isResearching || _interactionInterface == null)) _interactionInterface.Show();
    }
    
    private void OnInteractableExited(Node2D node)
    {
        GD.Print("Exit");
        if (node is not PlayerInteractor) return;
        _showUi = false;
        if (!(_isResearching || _interactionInterface == null)) _interactionInterface.Hide();
    }
    
}