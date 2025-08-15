using Godot;
using Godot.Collections;

namespace SoYouWANNAJam2025.Code.Interactive.Items;

[GlobalClass][Tool]
public partial class GenericItemTemplate: Resource
{
    [Export] public string DisplayName = "Unknown";
    [Export] public string Description = "You need to actually set this lol.";
    [Export] public Array<Texture2D> Textures = [GD.Load<Texture2D>("res://Assets/Sprites/Unknown.png")];
    [Export] public Array<Color> Colours = [Colors.White];
    [Export] public float Size = 10;

    public virtual Image GetItemImage()
    {
        return CreateItemImage(Textures, Colours);
    }

    protected Image CreateItemImage(Array<Texture2D> textures, Array<Color> colours)
    {
        var img = Image.CreateEmpty(32, 32, false, Image.Format.Rgba8);
        for (var i = 0; i < textures.Count; i++)
        {
            var modulateColor = i < colours.Count ? colours[i] :  Colors.White;
            var spriteImg = textures[i].GetImage();

            for (var x = 0; x < 32; x++)
            {
                for (var y = 0; y < 32; y++)
                {
                    var original = spriteImg.GetPixel(x, y);
                    if (original.A == 0) continue;
                    var modulated = new Color(
                        original.R * modulateColor.R,
                        original.G * modulateColor.G,
                        original.B * modulateColor.B,
                        original.A * modulateColor.A
                    );
                    img.SetPixel(x, y, modulated);
                }
            }
        }
        return img;
    }
}

