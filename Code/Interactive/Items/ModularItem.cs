using Godot;
using Godot.Collections;

namespace SoYouWANNAJam2025.Code.Interactive.Items;

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

public enum EPartState
{
    New,
    Used,
    Broken
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
    public Dictionary<EPartType, ModularPartTemplate> Parts = [];
    public Npc.Friendly.Npc OwningNpc = null;

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
            Parts.Add(pair.Key, newItem);
            GD.Print($"Added {newItem!.DisplayName} to slot {pair.Key}.");
        }

        DrawSprite();
    }

    public bool HasPart(EPartType partType)
    {
        return Parts.ContainsKey(partType);
    }

    public bool AddPart(ModularPartTemplate part)
    {
        if (!Parts.ContainsKey(part.PartType)) return false;
        Parts[part.PartType] = part;
        GD.Print($"Added {part.DisplayName} to {ItemResource.DisplayName}.");
        DrawSprite();
        return true;
    }

    public override void DrawSprite()
    {
        //GD.Print("MODULAR DRAW");
        var img = Image.CreateEmpty(32, 32, false, Image.Format.Rgba8);
        
        foreach (var (partType, part) in Parts)
        {
            //GD.Print($"Part {part.DisplayName}");
            var partImg = part.GetItemImage();
            img.BlendRect(partImg, new Rect2I(0,0,32,32), Vector2I.Zero);
        }
        Sprite.Texture = ImageTexture.CreateFromImage(img);
    }
}