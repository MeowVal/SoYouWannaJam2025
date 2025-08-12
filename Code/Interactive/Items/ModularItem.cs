using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Interactive.Items;

namespace SoYouWANNAJam2025.Entities.Interactive.Items;

public enum EPartType
{
    Blade,
    Hilt,
    Crossguard,
    Pole,
    Tip,
    FrontPlate,
    Trim,
    Body,
    Accessory,
    Shoulders,
    Cape,
    Hood
}

public enum EUseType
{
    Chopping,
    Mining,
    Fishing,
    RangedCombat,
    MeleeCombat,
    Armour
}

[Tool] [GlobalClass]
public partial class ModularItem : GenericItem
{
    // Defines what slots this item will have to use
    private Dictionary<EPartType, ModularItem> _parts;

    public override void _Ready()
    {
        base._Ready();
        if (ItemResource is not ModularItemTemplate template) return;
        // foreach (var type in template.PartSlots)
        // {
        //     _parts.Add(type, null);
        // }
    }
}