using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Player;
using SoYouWANNAJam2025.Code.World;

namespace SoYouWANNAJam2025.Code.Interactive;

public partial class GuideShelf : Interactible
{
    private CanvasLayer _hudNode;
    private Control _uiNode;
    private Control _guideBookNode;
    public override void _Ready()
    {
        base._Ready();
        Interact += OnInteractMethod;
        _hudNode = GetNode<CanvasLayer>("/root/HUD");
        _uiNode = GetNode<Control>("/root/HUD/Container");
    }


    private void OnInteractMethod(Node2D node, TriggerType trigger)
    {
        if (node is not PlayerInteractor interactor) return;

        switch (trigger)
        {
            case TriggerType.PickupDrop:
                break;
            case TriggerType.UseAction:
                if (_guideBookNode == null) OpenPopup();
                break;
        }
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (_guideBookNode != null && Input.IsActionJustPressed("ui_cancel"))
        {
            ClosePopup();
        }
    }

    private void OpenPopup()
    {
        if (_uiNode != null) _uiNode.Hide();
        if (_hudNode == null) return;
        Global.Player.Frozen = true;
        var uiScene = GD.Load<PackedScene>("res://Scenes/UI/Guidebook.tscn");
        _guideBookNode = uiScene.Instantiate<Control>();
        _hudNode.AddChild(_guideBookNode);
    }
    
    private void ClosePopup()
    {
        if (_uiNode != null) _uiNode.Show();
        Global.Player.Frozen = false;
        
        if (_guideBookNode == null) return;
        _guideBookNode.QueueFree();
        _guideBookNode = null;
    }
}