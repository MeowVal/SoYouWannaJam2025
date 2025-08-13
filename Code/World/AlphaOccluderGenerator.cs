using System.Collections.Generic;
using Godot;

namespace SoYouWANNAJam2025.Code.World;

public abstract class AlphaOccluderGenerator
{
    /// Converts a Texture2D into an OccluderPolygon2D based on its alpha
    public static OccluderPolygon2D GenerateOccluder(Texture2D texture, double alphaThreshold = 0.1)
    {
        Godot.Image image = texture.GetImage();
        image.Convert(Image.Format.L8);  // Alpha-only
        

        int w = image.GetWidth();
        int h = image.GetHeight();

        // Create binary grid from alpha
        bool[,] grid = new bool[w, h];
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
            grid[x, y] = image.GetPixel(x, y).R > alphaThreshold;

        // Extract contour via marching squares
        var points = MarchingSquares(grid, w, h);
        if (points.Count < 3)
            return null;

        List<Vector2> poly = [];
        poly.AddRange(points);

        return new OccluderPolygon2D { Polygon = poly.ToArray() };
    }

    private static List<Vector2> MarchingSquares(bool[,] g, int w, int h)
    {
        var pts = new List<Vector2>();

        // Simple border-following: find first solid pixel
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (g[x, y])
                {
                    FollowContour(g, w, h, x, y, pts);
                    return pts;
                }
            }
        }
        return pts;
    }

    private static void FollowContour(bool[,] grid, int w, int h, int startX, int startY, List<Vector2> outPts)
    {
        int dir = 0;
        int x = startX, y = startY;

        // Direction offsets: right, down, left, up
        Vector2[] dirs = {
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(-1, 0),
            new Vector2(0, -1)
        };

        var visited = new HashSet<(int, int)>();

        do
        {
            // Record contour vertex
            if (!visited.Contains((x, y)))
            {
                outPts.Add(new Vector2(x + 0.5f, y + 0.5f));
                visited.Add((x, y));
            }

            // Try turning right
            int tryDir = (dir + 3) % 4;
            Vector2 np = new Vector2(x + dirs[tryDir].X, y + dirs[tryDir].Y);

            if (np.X >= 0 && np.X < w && np.Y >= 0 && np.Y < h && grid[(int)np.X, (int)np.Y])
                dir = tryDir;
            else
            {
                // If no, keep going straight
                np = new Vector2(x + dirs[dir].X, y + dirs[dir].Y);
                if (!(np.X >= 0 && np.X < w && np.Y >= 0 && np.Y < h && grid[(int)np.X, (int)np.Y]))
                {
                    // If can't move, turn left
                    dir = (dir + 1) % 4;
                    continue;
                }
            }
            x = (int)np.X;
            y = (int)np.Y;

        } while (x != startX || y != startY);
    }
}