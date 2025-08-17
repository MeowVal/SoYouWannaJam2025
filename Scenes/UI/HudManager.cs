using System.Diagnostics;
using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.World;

namespace SoYouWANNAJam2025.Scenes.UI;

public partial class HudManager : CanvasLayer
{
    private static bool _isDayUpdated;
    private Global _global;
    public override void _EnterTree()
    {
        _global = GetNode<Global>("/root/Global");
        _global.CurrentSceneChanged += GlobalOnCurrentSceneChanged;
        if (_global.CurrentScene == "MainMenu") this.Visible = false;
    }

    private void GlobalOnCurrentSceneChanged(string sceneName)
    {
        this.Visible = sceneName switch
        {
            "MainMenu" => false,
            "Island" => true,
            //_ => this.Visible
        };
    }

    public override void _Ready()
    {
        // var canvas = GetParent<CanvasLayer>();
        // canvas.SetFollowViewport(true);
        //GetViewport().AddChild(this);
        //Position = Vector2.Zero;

        this.GetNode<Label>("%Date").Text = "Night: " + Global.GameDay.ToString();
        this.GetNode<Label>("%TimeOfDay").Text = "00:00";
    }
    
    public override void _Process(double delta)
    {
        if (Global.GameTimer > 0)
        {
            if (!_isDayUpdated)
            {
                _isDayUpdated = true;
                this.GetNode<Label>("%Date").Text = "Day:" + Global.GameDay.ToString();
            }
            // Convert minutes to hours and minutes
            float currentTime = Global.GameTimer * 86400f;
            
            int hours = Mathf.FloorToInt(currentTime / 3600);
            int minutes = Mathf.FloorToInt((currentTime % 3600)/60);
            // Example: Print the current time in HH:MM format
            
            this.GetNode<Label>("%TimeOfDay").Text = $"{hours:D2}:{minutes:D2}";
        }
        else
        {
            if (_isDayUpdated)
            {
                _isDayUpdated = false; 
                this.GetNode<Label>("%Date").Text = "Night:" + Global.GameDay.ToString();
            } 
        }
    }
}