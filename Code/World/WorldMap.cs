using Godot;
using System;
using System.Collections.Generic;

namespace SoYouWANNAJam2025.Code;

public partial class WorldMap : TileMapLayer
{
    //private List<TileMapLayer> _obstacles = new List<TileMapLayer>();

    public override void _Ready()
    {
        
            //await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Callable.From(NotifyRuntimeTileDataUpdate).CallDeferred();
        
       
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
        var globalPos = MapToLocal(coords);

        foreach (Node node in GetTree().GetNodesInGroup("Obstacles"))
        {
            if (node is TileMapLayer obstacleLayer)
            {
                var localTileCoords = obstacleLayer.LocalToMap(globalPos);
                var sourceId = obstacleLayer.GetCellSourceId(localTileCoords);
                if (sourceId == -1)
                    continue;

                var tileSet = obstacleLayer.TileSet;
                var source = tileSet.GetSource(sourceId);

                if (source is TileSetAtlasSource atlasSource)
                {
                    var atlasCoords = obstacleLayer.GetCellAtlasCoords(localTileCoords);
                    var alternative = obstacleLayer.GetCellAlternativeTile(localTileCoords);

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
