using Godot;

namespace SoYouWANNAJam2025.Code.RecipeSystem;

[GlobalClass]
public partial class CraftingStationInterface : Node2D
{
    public CraftingStation Station;

    public virtual void Init(CraftingStation station)
    {
        Station = station;
    }

    public virtual void Update()
    {

    }
}