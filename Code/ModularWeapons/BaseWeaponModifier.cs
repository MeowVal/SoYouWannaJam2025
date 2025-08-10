using System;
using Godot;

namespace SoYouWANNAJam2025.Code.ModularWeapons;

[GlobalClass]
public partial class BaseWeaponModifier : Resource, IBaseWeaponModifier
{
    [ExportGroup("Info")]
    [Export] public string DisplayName = "Unknown";
    [Export] public string Description = "You need to actually set this lol.";
    [Export] public Texture2D Icon;


    public void ApplyModifier(SoYouWANNAJam2025.Code.ModularWeapons.ModularWeapon ModWep)
    {
        throw new NotImplementedException();
    }

    public string GetDescription()
    {
        return DisplayName;
    }
}