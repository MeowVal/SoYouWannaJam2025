using Godot;
using Godot.Collections;

namespace SoYouWANNAJam2025.Code.Interactive.Items;

[GlobalClass][Tool]
public partial class ModularItemTemplate : GenericItemTemplate
{
    [ExportGroup("Setup")]
    [Export] // The different modular parts you can attach to this item.
    public Godot.Collections.Dictionary<EPartType, ModularPartTemplate> DefaultParts { get; set; }
    //public Array<EPartType> PartSlots;
    [Export]
    public EModularItemType ModularItemType;
    [Export] // The action an NPC can use this for.
    public EUseType UseType;

    [ExportCategory("Modular Stats")]
    [ExportGroup("Damage")] [Export] // The root base damage of the weapon
    public float BaseDamage = 10;
    [Export] // The amount of the base damage that is converted to fire damage.
    public float FireMultiplier = 0;
    [Export] // The amount of the base damage that is converted to poison damage.
    public float PoisonMultiplier = 0;
    [Export] // The amount of armour the weapon ignores.
    public float BaseArmourPenetration = 1;

    [ExportGroup("Durability")] [Export] // The baseline amount of durability the item has.
    public float BaseDurability = 20;
    [Export] // The chance for durability to not be consumed on use.
    public float UnbreakingChance = 1;

    [ExportGroup("Critical Hits")] [Export] // The percent chance for a critial hit to trigger per attack
    public float CritChance = 5;
    [Export] // The amount that base damage is multiplied by when crits trigger
    public float CritMultiplier = 2;
    [Export] // The amount of additional base damage that is applied on crits.
    public float CritBonus = 5;
}