using System.Collections.Generic;
using Godot;

namespace SoYouWANNAJam2025.Code.Lighting;

public partial class OcclusionPolygonGenerator : Node
{
	[Export]
    public TileMapLayer tileMapLayer;

    [Export]
    public int OcclusionLayerIndex = 1; // The layer index in TileMapLayer to assign polygons to

    public override void _Ready()
    {
        GenerateOcclusionPolygons();
    }

    public void GenerateOcclusionPolygons()
    {
        if (tileMapLayer == null)
        {
            GD.PrintErr("TilemapLayer not assigned!");
            return;
        }

        var usedRect = tileMapLayer.GetUsedRect();

        for (int y = usedRect.Position.Y; y < usedRect.End.Y; y++)
        {
            for (int x = usedRect.Position.X; x < usedRect.End.X; x++)
            {
                Vector2I cellPos = new Vector2I(x, y);
                
                int sourceId = tileMapLayer.GetCellSourceId(cellPos);;

                var source = tileMapLayer.TileSet.GetSource(sourceId) as TileSetAtlasSource;

                var atlasCoord = tileMapLayer.GetCellAtlasCoords(cellPos);
                if(source == null) continue;

                var rect = source.GetTileTextureRegion(atlasCoord);

                var image = source.Texture.GetImage();

                var tileImage = image.GetRegion(rect);
                
                var tex = ImageTexture.CreateFromImage(tileImage);
                TileSet tileSet = tileMapLayer.TileSet;
                //var source = tileSet.GetSource(sourceId);
                

                if (tex == null)
                    continue;

                // Generate polygon from texture alpha
                var polygon = GenerateOcclusionPolygonFromTexture(tex);
            }
        }
        GD.Print("Occlusion polygons generated!");
    }

    private List<Vector2> GenerateOcclusionPolygonFromTexture(Texture2D texture)
    {
        // Get image from texture
        Image image = texture.GetImage();

        // Ensure it's readable and in a format with alpha
        

        int w = image.GetWidth();
        int h = image.GetHeight();

        // Create a bool grid for alpha > 0
        bool[,] grid = new bool[w, h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var color = image.GetPixel(x, y);
                grid[x, y] = color.A > 0.01f;
            }
        }

        

        // Extract polygon using a contour tracing algorithm (like marching squares)
        var points = FollowContour(grid, w, h);

        return points;
    }

    private List<Vector2> FollowContour(bool[,] grid, int w, int h)
    {
        var points = new List<Vector2>();

        int[] dx = { 1, 0, -1, 0 };
        int[] dy = { 0, 1, 0, -1 };

        bool[,] visited = new bool[w, h];

        // Find first solid pixel
        int startX = -1, startY = -1;
        for (int y = 0; y < h && startX == -1; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (grid[x, y])
                {
                    startX = x;
                    startY = y;
                    break;
                }
            }
        }

        if (startX == -1)
            return points; // no solid pixels found

        int xCur = startX;
        int yCur = startY;
        int dir = 0; // direction: right=0, down=1, left=2, up=3

        do
        {
            if (!visited[xCur, yCur])
            {
                points.Add(new Vector2(xCur + 0.5f, yCur + 0.5f)); // center of pixel
                visited[xCur, yCur] = true;
            }

            int tryDir = (dir + 3) % 4; // turn right
            int nx = xCur + dx[tryDir];
            int ny = yCur + dy[tryDir];

            if (nx >= 0 && nx < w && ny >= 0 && ny < h && grid[nx, ny])
                dir = tryDir;
            else
            {
                nx = xCur + dx[dir];
                ny = yCur + dy[dir];
                if (!(nx >= 0 && nx < w && ny >= 0 && ny < h && grid[nx, ny]))
                {
                    dir = (dir + 1) % 4; // turn left
                    continue;
                }
            }
            xCur = nx;
            yCur = ny;

        } while (xCur != startX || yCur != startY);

        return points;
    }
}