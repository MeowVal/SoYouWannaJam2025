



namespace SoYouWANNAJam2025.Code.ModularWeapons;

public interface IBaseWeaponModifier
{
    void ApplyModifier(SoYouWANNAJam2025.Code.ModularWeapons.ModularWeapon ModWep);

    public string GetDescription();
}