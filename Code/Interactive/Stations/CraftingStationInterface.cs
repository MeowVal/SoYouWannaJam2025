using Godot;

namespace SoYouWANNAJam2025.Code.Interactive.Stations;

[GlobalClass]
public partial class CraftingStationInterface : Node2D
{
    public Interactive.Stations.CraftingStation Station;

    public virtual void Init(Interactive.Stations.CraftingStation station)
    {
        Station = station;
    }

    public virtual void Update()
    {

    }
}