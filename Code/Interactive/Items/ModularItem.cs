using System;
using System.Linq;
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

public enum EPartState
{
    New,
    Worn,
    Broken
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

        DrawSprite();
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
        DrawSprite();
        return true;
    }

    public bool SetPartState(EPartType target, EPartState value)
    {
        if (!_parts.ContainsKey(target)) return false;
        _parts[target].PartState = value;
        GD.Print($"Set Part {target.ToString()} to {value.ToString()}.");
        DrawSprite();
        return true;
    }

    public override void DrawSprite()
    {
        //GD.Print("MODULAR DRAW");
        var img = Image.CreateEmpty(32, 32, false, Image.Format.Rgba8);
        
        foreach (var (partType, part) in _parts)
        {
            //GD.Print($"Part {part.DisplayName}");
            var partImg = part.GetItemImage();
            img.BlendRect(partImg, new Rect2I(0,0,32,32), Vector2I.Zero);
        }
        Sprite.Texture = ImageTexture.CreateFromImage(img);
    }
}