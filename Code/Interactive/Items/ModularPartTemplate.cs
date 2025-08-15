using System;
using Godot;
using Godot.Collections;

namespace SoYouWANNAJam2025.Code.Interactive.Items;

[GlobalClass][Tool]
public partial class ModularPartTemplate : GenericItemTemplate
{
    [ExportGroup("Setup")]
    [Export] // The different modular parts you can attach to this item.
    public EPartType PartType;
    [Export] // The type of Modular Item this can be attached to.
    public EModularItemType ModularItemType;

    /*[ExportCategory("Modular Stats")]
    [ExportGroup("Damage")] [Export] // The root base damage of the weapon
    public float BaseDamage = 0;
    [Export] // The amount of the base damage that is converted to fire damage.
    public float FireMultiplier = 0;
    [Export] // The amount of the base damage that is converted to poison damage.
    public float PoisonMultiplier = 0;
    [Export] // The amount of armour the weapon ignores.
    public float BaseArmourPenetration = 0;

    [ExportGroup("Durability")] [Export] // The baseline amount of durability the item has.
    public float BaseDurability = 0;
    [Export] // The chance for durability to not be consumed on use.
    public float UnbreakingChance = 0;

    [ExportGroup("Critical Hits")] [Export] // The percent chance for a critial hit to trigger per attack
    public float CritChance = 0;
    [Export] // The amount that base damage is multiplied by when crits trigger
    public float CritMultiplier = 0;
    [Export] // The amount of additional base damage that is applied on crits.
    public float CritBonus = 0;*/
}