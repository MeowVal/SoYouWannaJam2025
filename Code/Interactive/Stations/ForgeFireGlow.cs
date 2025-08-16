using Godot;
using System;

public partial class ForgeFireGlow : PointLight2D
{
    public override void _Process(double delta)
    {
        this.SetEnergy((float)GD.RandRange(0.7, 1.6)*(float)delta);
    }
}
