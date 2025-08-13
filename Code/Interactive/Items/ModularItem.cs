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
    private Dictionary<EPartType, ModularPartTemplate> _parts;
    public Npc OwningNpc = null;

    public override void _Ready()
    {
        base._Ready();
        // Get resource defaults and duplicate (instantiate) them into the item itself.
        if (ItemResource is not ModularItemTemplate template) return;
        foreach (var pair in template.DefaultParts)
        {

            var newItem = (ModularPartTemplate) pair.Value.Duplicate();
            _parts.Add(pair.Key, newItem);
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
        return true;
    }
}