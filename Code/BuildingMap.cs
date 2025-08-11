using Godot;
using System;
using System.Collections.Generic;

public partial class BuildingMap : TileMapLayer
{
    private List<TileMapLayer> _obstacles = new List<TileMapLayer>();

    public override void _Ready()
    {
        GetObstaclesLayer();
    }

    private void GetObstaclesLayer()
    {
        var layers = GetTree().GetNodesInGroup("Obstacles");

        foreach (var node in layers)
        {
            var layer = (Node2D)node;
            if (layer is TileMapLayer mapLayer)
            {
                _obstacles.Add(mapLayer);
                GD.Print("Obstacle Layer Added: " + mapLayer.Name);
            }
        }
    }

    
    public override bool _UseTileDataRuntimeUpdate(Vector2I coords)
    {
        return IsUsedByObstacles(coords);
    }

    private bool IsUsedByObstacles(Vector2I coords)
    {
        foreach (var layer in _obstacles)
        {
            if (layer.GetUsedCells().Contains(coords))
            {
                var isObstacle  = layer.GetCellTileData(coords).GetCollisionPolygonsCount(0) > 0;
                if (isObstacle)
                {
                    return true; 
                }
            }
        }
        return false;
    }

    public override void _TileDataRuntimeUpdate(Vector2I coords, TileData tileData)
    {
        
        if (!IsUsedByObstacles(coords))
        {
            tileData.SetNavigationPolygon(0, null);
        }
    }
}
