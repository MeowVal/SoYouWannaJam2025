using System.Diagnostics;
using Godot;

namespace SoYouWANNAJam2025.Code.World;

public partial class MainMenu : Control
{
    private Global _global;
    public override void _EnterTree()
    {
        _global = GetNode<Global>("/root/Global");
        _global.CurrentSceneChanged += GlobalOnCurrentSceneChanged;
        GetNode<Button>("%Button").Pressed += OnPressed;
        GetNode<Button>("%Button2").Pressed += OnPressed2;
    }

    public override void _ExitTree()
    {
        _global.CurrentSceneChanged -= GlobalOnCurrentSceneChanged;
        GetNode<Button>("%Button").Pressed -= OnPressed;
        GetNode<Button>("%Button2").Pressed -= OnPressed2;
    }

    private void OnPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Maps/Island.tscn");
        _global.CurrentScene = "Island";
    }
    
    private void OnPressed2()
    {
        GetTree().Quit();
    }

    private void GlobalOnCurrentSceneChanged(string sceneName)
    {
        GD.Print("Current scene changed to: " + sceneName);
    }
}