using System;
using System.Collections.Generic;
using Godot;

namespace SoYouWANNAJam2025.Code.Lighting;

[Tool]
public partial class HeightMapGenerator : Node2D
{
    [Export]
    public TileMapLayer TileLayer;
    
    [Export]
    public string HeightMapPath = "res://generated_heightmap.png";
    [Export]
    public string IndoorMaskPath = "res://indoor_mask.png";

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) // Avoid running in game
            GenerateAndSaveHeightmapAndIndoorMask();
        GenerateAndSaveHeightmapAndIndoorMask();
    }
    public void GenerateAndSaveHeightmapAndIndoorMask()
    {
        if (TileLayer == null)
        {
            GD.PrintErr("TileLayer not assigned.");
            return;
        }
        
        
        
        var usedCells = TileLayer.GetUsedCells();

        if (usedCells.Count == 0)
        {
            GD.PrintErr("No tiles in this layer.");
            return;
        }

        // Determine bounds
        Vector2I min = usedCells[0];
        Vector2I max = usedCells[0];

        foreach (var pos in usedCells)
        {
            min = new Vector2I(Mathf.Min(min.X, pos.X), Mathf.Min(min.Y, pos.Y));
            max = new Vector2I(Mathf.Max(max.X, pos.X), Mathf.Max(max.Y, pos.Y));
        }

        Vector2I size = max - min + Vector2I.One;
        Rect2I usedRect = new Rect2I(min, size);

        // Create images
        Image heightMap = Image.CreateEmpty(size.X, size.Y, false, Image.Format.L8);
        Image indoorMask = Image.CreateEmpty(size.X, size.Y, false, Image.Format.L8);
        
        for (int y = usedRect.Position.Y; y < usedRect.End.Y; y++)
        {
            for (int x = usedRect.Position.X; x < usedRect.End.X; x++)
            {
                Vector2I cell = new Vector2I(x, y);
                TileData data = TileLayer.GetCellTileData(cell);
                float height = 0f;
                float outdoor = 1f;

                if (data != null)
                {
                    if (data.HasCustomData("height"))
                    {
                        height = Mathf.Clamp(Convert.ToSingle((float)data.GetCustomData("height")), 0f, 1f);
                    }
                        

                    if (data.HasCustomData("indoor") && Convert.ToBoolean((int)data.GetCustomData("indoor")))
                        outdoor = 0f;
                }

                int px = x - usedRect.Position.X;
                int py = y - usedRect.Position.Y;

                heightMap.SetPixel(px, py, new Color(height, height, height));
                indoorMask.SetPixel(px, py, new Color(outdoor, outdoor, outdoor));
            }
        }
        
        var heightSaveResult = heightMap.SavePng(HeightMapPath);
        var maskSaveResult = indoorMask.SavePng(IndoorMaskPath);
        
        if (heightSaveResult == Error.Ok)
            GD.Print($"Heightmap saved to: {HeightMapPath}");
        else
            GD.PrintErr($"Failed to save heightmap: {heightSaveResult}");

        if (maskSaveResult == Error.Ok)
            GD.Print($"Indoor mask saved to: {IndoorMaskPath}");
        else
            GD.PrintErr($"Failed to save indoor mask: {maskSaveResult}");
    }
}