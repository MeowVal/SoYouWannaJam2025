using Godot;

namespace SoYouWANNAJam2025.Code.Interactive.Stations;

[GlobalClass]
public partial class CraftingStationInterface : Node2D
{
    public Interactible Station;

    public virtual void Init(Interactible station)
    {
        Station = station;
    }

    public virtual void Update(double delta)
    {

    }
}