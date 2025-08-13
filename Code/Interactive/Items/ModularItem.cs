using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Interactive.Items;
using SoYouWANNAJam2025.Code.Npc;

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
    private Dictionary<EPartType, ModularPartTemplate> _parts = [];
    public Npc OwningNpc = null;

    public override void _Ready()
    {
        base._Ready();
        // Get resource defaults and duplicate (instantiate) them into the item itself.
        if (ItemResource is not ModularItemTemplate template) return;
        GD.Print($"Constructing ModularItem {ItemResource.DisplayName}.");
        foreach (var pair in template.DefaultParts)
        {
            var oldItem = pair.Value;
            var newItem = oldItem.Duplicate(true) as ModularPartTemplate;
            _parts.Add(pair.Key, newItem);
            GD.Print($"Added {newItem!.DisplayName} to slot {pair.Key}.");
        }
    }

    public bool HasPart(EPartType partType)
    {
        return _parts.ContainsKey(partType);
    }

    public bool AddPart(ModularPartTemplate part)
    {
        if (!_parts.ContainsKey(part.PartType)) return false;
        _parts[part.PartType] = part;
        GD.Print($"Added {part.DisplayName} to {ItemResource.DisplayName}.");
        return true;
    }
}