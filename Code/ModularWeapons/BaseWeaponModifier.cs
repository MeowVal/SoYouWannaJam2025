using Godot;
using System;
using SoyouWANNAJam2025.Code.ModularWeapons;

[GlobalClass]
public partial class BaseWeaponModifier : Resource, IBaseWeaponModifier
{
    [ExportGroup("Info")] [Export] public string Name = "Base Upgrade";
    [Export] public string Description = "This is an unset base class.";
    [Export] public Texture2D Icon;


    public void ApplyModifier(ModularWeapon ModWep)
    {
        throw new NotImplementedException();
    }

    public string GetDescription()
    {
        return Name;
    }
}
