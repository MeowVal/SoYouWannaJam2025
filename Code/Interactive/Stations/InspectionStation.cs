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
    private ProgressBar _progressBar;
    private Timer _progressTimer;

    private bool _isInspecting = false;
    private bool _showUi = true;

    private PackedScene _partRow = GD.Load<PackedScene>("res://Scenes/UI/Interactions/InspectionTable/PartRow.tscn");
    
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
        
        if (_interactionInterface == null || !_isInspecting) return;
        _progressBar.Value = 1 - (_progressTimer.TimeLeft / _progressTimer.WaitTime);
    }

    public void ResearchBegin()
    {
        CreateUiScene("res://Scenes/UI/Interactions/InspectionTable/InspectingTimer.tscn");
        _isInspecting = true;
        
        _progressBar = _interactionInterface.GetNode<ProgressBar>("Control/MarginContainer/VBoxContainer/ProgressBar");
        _progressTimer = _interactionInterface.GetNode<Timer>("Timer");
        _progressTimer.Timeout += ProgressComplete;
        var label = _interactionInterface.GetNode<Label>("Control/MarginContainer/VBoxContainer/ItemName");
        if (Inventory.Item is ModularItem){
            _progressTimer.Start(5);
            label.Text = Inventory.Item.ItemResource.DisplayName;
        } 
        else if (Inventory.Item.ItemResource is ModularPartTemplate)
        {
            _progressTimer.Start(1);
            label.Text = "...";
        }
        
    }
    
    public void ResearchAbort()
    {
        if (_interactionInterface != null) _interactionInterface!.QueueFree();
        _isInspecting = false;
        _interactionInterface = null;
        _progressTimer = null;
        _progressBar = null;
    }

    public void ProgressComplete()
    {
        if (_interactionInterface != null) _interactionInterface!.QueueFree();
        _isInspecting = false;
        _progressTimer = null;
        _progressBar = null;

        CreateUiScene("res://Scenes/UI/Interactions/InspectionTable/ModularItemBreakdown.tscn");
        if (_interactionInterface == null || !Inventory.HasItem()) return;
        
        _interactionInterface.SetVisible(_showUi);
        
        var container = _interactionInterface.GetNode<VBoxContainer>("Control/VBoxContainer/PartsMarginContainer/PartsVBox");
        var itemText = _interactionInterface.GetNode<Label>("Control/VBoxContainer/MarginContainer2/PanelContainer/MarginContainer/HBoxContainer/ItemName");
        var itemIcon = _interactionInterface.GetNode<TextureRect>("Control/VBoxContainer/MarginContainer2/PanelContainer/MarginContainer/HBoxContainer/ItemTexture");



        if (Inventory.Item is not ModularItem modularItem) return;

        itemText.Text = modularItem.ItemResource.DisplayName;
        itemIcon.Texture = modularItem.Sprite.Texture;

        GD.Print("Compare Item:");
        foreach (var key in modularItem.Parts.Keys)
        {
            var wantedPart = modularItem.OwningNpc.WantedItemTemplate.DefaultParts[key];
            var givenPart = modularItem.Parts[key];
            
            GD.Print($"    {key}: {givenPart.PartState} {givenPart.DisplayName} -> {wantedPart.PartState} {wantedPart.DisplayName} | {givenPart.PartId}/{wantedPart.PartId}");
        }

        foreach (var part in modularItem.Parts)
        {
            var wantedPart = modularItem.OwningNpc.WantedItemTemplate.DefaultParts[part.Key];
            var newRow = _partRow.Instantiate<Scenes.UI.Interactions.InspectionTable.PartRow>();
            var isSameItem = wantedPart.DisplayName == part.Value.DisplayName;
            var isSameState = wantedPart.PartState == part.Value.PartState;

            if (!isSameItem)
            {
                newRow.Initialise(
                    part: part.Value,
                    actionType: Scenes.UI.Interactions.InspectionTable.PartRow.EActionType.Replace,
                    replacement: wantedPart
                );
            }
            else if (isSameItem && !isSameState)
            {
                newRow.Initialise(
                    part: part.Value,
                    actionType: Scenes.UI.Interactions.InspectionTable.PartRow.EActionType.Repair
                );
            }
            else
            {
                newRow.Initialise(
                    part: part.Value,
                    actionType: Scenes.UI.Interactions.InspectionTable.PartRow.EActionType.None
                );
            }


            container.AddChild(newRow);
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
        if (!(_isInspecting || _interactionInterface == null)) _interactionInterface.Show();
    }
    
    private void OnInteractableExited(Node2D node)
    {
        GD.Print("Exit");
        if (node is not PlayerInteractor) return;
        _showUi = false;
        if (!(_isInspecting || _interactionInterface == null)) _interactionInterface.Hide();
    }
    
}