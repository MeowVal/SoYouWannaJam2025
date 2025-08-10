using Godot;
using System;
using SoyouWANNAJam2025.Code.ModularWeapons;

[GlobalClass]
public partial class BaseWeaponModifier : Resource, IBaseWeaponModifier
{
    [ExportGroup("Info")]
    [Export] public string DisplayName = "Unknown";
    [Export] public string Description = "You need to actually set this lol.";
    [Export] public Texture2D Icon;


    public void ApplyModifier(ModularWeapon ModWep)
    {
        throw new NotImplementedException();
    }

    public string GetDescription()
    {
        return DisplayName;
    }
}
