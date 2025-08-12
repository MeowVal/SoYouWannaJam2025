using System.Collections.Generic;
using Godot;

namespace SoYouWANNAJam2025.Code;

public partial class BuildingMap : TileMapLayer
{
    private List<TileMapLayer> _obstacles = new List<TileMapLayer>();

    public override async void _Ready()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        CallDeferred(nameof(NotifyRuntimeTileDataUpdate));
    }
    
    public override bool _UseTileDataRuntimeUpdate(Vector2I coords)
    {
        return IsObstructedByObstacle(coords);
    }
    public override void _TileDataRuntimeUpdate(Vector2I coords, TileData tileData)
    {
        
        if (IsObstructedByObstacle(coords))
        {
            tileData.SetNavigationPolygon(0, null); // Remove nav polygon for this tile
        }
    }
    
    private bool IsObstructedByObstacle(Vector2I coords)
    {
        Vector2 globalPos = MapToLocal(coords);

        foreach (Node node in GetTree().GetNodesInGroup("Obstacles"))
        {
            if (node is TileMapLayer obstacleLayer)
            {
                Vector2I localTileCoords = obstacleLayer.LocalToMap(globalPos);
                int sourceId = obstacleLayer.GetCellSourceId(localTileCoords);
                if (sourceId == -1)
                    continue;

                var tileSet = obstacleLayer.TileSet;
                var source = tileSet.GetSource(sourceId);

                if (source is TileSetAtlasSource atlasSource)
                {
                    var atlasCoords = obstacleLayer.GetCellAtlasCoords(localTileCoords);
                    int alternative = obstacleLayer.GetCellAlternativeTile(localTileCoords);

                    var tileData = atlasSource.GetTileData(atlasCoords, alternative);
                    if (tileData != null && tileData.GetCollisionPolygonsCount(0) > 0)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
