namespace SoyouWANNAJam2025.Code.ModularWeapons;

public interface IBaseWeaponModifier
{
    void ApplyModifier(ModularWeapon ModWep);

    public string GetDescription();
}