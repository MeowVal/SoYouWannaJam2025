using System.Diagnostics;
using Godot;

namespace SoYouWANNAJam2025.Code.World;

public partial class MainMenu : Control
{
    private Global _global;
    private Node _rootNode;
    private Control _guideBookNode;
    
    public override void _EnterTree()
    {
        _global = GetNode<Global>("/root/Global");
        _global.CurrentSceneChanged += GlobalOnCurrentSceneChanged;
        GetNode<Button>("%PlayButton").Pressed += OnPressedPlay;
        GetNode<Button>("%QuitButton").Pressed += OnPressedQuit;
        GetNode<Button>("%GuideButton").Pressed += OnOpenGuideBook;
    }

    public override void _ExitTree()
    {
        _global.CurrentSceneChanged -= GlobalOnCurrentSceneChanged;
        GetNode<Button>("%PlayButton").Pressed -= OnPressedPlay;
        GetNode<Button>("%QuitButton").Pressed -= OnPressedQuit;
        GetNode<Button>("%GuideButton").Pressed -= OnOpenGuideBook;
    }

    public override void _Ready()
    {
        base._Ready();
        _rootNode = GetNode<Node>("/root");
    }

    private void OnPressedPlay()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Maps/Island.tscn");
    }
    
    private void OnPressedQuit()
    {
        GetTree().Quit();
    }

    private void GlobalOnCurrentSceneChanged(string sceneName)
    {
        GD.Print("Current scene changed to: " + sceneName);
    }
    
    private void OnOpenGuideBook()
    {
        if (_rootNode == null) return;
        var uiScene = GD.Load<PackedScene>("res://Scenes/UI/Guidebook.tscn");
        _guideBookNode = uiScene.Instantiate<Control>();
        _rootNode.AddChild(_guideBookNode);
        
        _guideBookNode.GetNode<Button>("%CloseGuidebook").Pressed += OnCloseGuideBook;
    }
    
    private void OnCloseGuideBook()
    {
        if (_guideBookNode == null) return;
        _guideBookNode.GetNode<Button>("%CloseGuidebook").Pressed -= OnCloseGuideBook;
        _guideBookNode.QueueFree();
        _guideBookNode = null;
    }
    
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (_guideBookNode != null && Input.IsActionJustPressed("ui_cancel"))
        {
            OnCloseGuideBook();
        }
    }
    

}