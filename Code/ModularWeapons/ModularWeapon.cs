using System.Collections.Generic;
using Godot;

namespace SoYouWANNAJam2025.Code.ModularWeapons;

public partial class ModularWeapon : Node2D
{
    [ExportGroup("Info")]
    [Export] public string DisplayName = "Unknown Modular Weapon";
    [Export] public string Description = "You need to actually set this lol.";
    [Export] public Texture2D Icon;

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

    public List<BaseWeaponModifier> Modifiers = new List<BaseWeaponModifier>();
    public List<SoYouWANNAJam2025.Code.RecipeSystem.BaseRecipe> CompletedRecipes = new List<SoYouWANNAJam2025.Code.RecipeSystem.BaseRecipe>();

    // Call this to update the final state of the weapon
    void CalculateModifiers()
    {
        foreach (var wepMod in Modifiers)
        {
            wepMod.ApplyModifier(this);
        }
    }
}