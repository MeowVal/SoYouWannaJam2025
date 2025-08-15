using System.Diagnostics;
using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.World;

namespace SoYouWANNAJam2025.Scenes.UI;

public partial class HudManager : CanvasLayer
{
    public static string Test = "Hello World";

    private static bool _runClock;
    private static float _clock;
    
    public override void _Ready()
    {
        // var canvas = GetParent<CanvasLayer>();
        // canvas.SetFollowViewport(true);
        //GetViewport().AddChild(this);
        //Position = Vector2.Zero;

        this.GetNode<Label>("%TimeOfDay").Text = "00:00";
        _runClock = false;
        
    }
    public override void _Process(double delta)
    {
        if (Global.GameTimer > 0)
        {
            // Convert minutes to hours and minutes
            float currentTime = Global.GameTimer * 86400f;
            
            int hours = Mathf.FloorToInt(currentTime / 3600);
            int minutes = Mathf.FloorToInt((currentTime % 3600)/60);
            // Example: Print the current time in HH:MM format
            
            this.GetNode<Label>("%TimeOfDay").Text = $"{hours:D2}:{minutes:D2}";
        }
    }
}