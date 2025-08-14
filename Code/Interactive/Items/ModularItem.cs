using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Interactive.Items;
using SoYouWANNAJam2025.Code.Npc;

namespace SoYouWANNAJam2025.Entities.Interactive.Items;

public enum EPartType
{
    Blade,
    Grip,
    Pummel,
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

public enum EModularItemType
{
    Sword,
    Spear,
    Shield,
    Helmet,
    Chestplate,
    Staff,
    Cloak
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
        foreach (var sprite in Sprites)
        {
            sprite.Free();
        }
        Sprites.Clear();
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
        UpdateModularSprites();
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
        UpdateModularSprites();
        return true;
    }

    public void UpdateModularSprites()
    {
        // Remove all old sprites
        foreach (var sprite in Sprites)
        {
            sprite.Free();
        }

        // Add in new ones
        foreach (var (partType, part) in _parts)
        {
            var newSprite = new Sprite2D();
            newSprite.Texture = part.Sprite;
            newSprite.Modulate = part.SpriteColour;
            AddChild(newSprite);
        }

        // Recalculate stats for highlight
        base.UpdateSprites();
    }


}