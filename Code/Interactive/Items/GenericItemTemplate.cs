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
    [Export] public bool AllowUseAction = false;
    [Export] public PackedScene SceneOverride;

    public int PartId = -1;
    
    public virtual Image GetItemImage(Array<Texture2D> textures = null)
    {
        textures = textures ?? Textures;
        var img = Image.CreateEmpty(32, 32, false, Image.Format.Rgba8);
        for (var i = 0; i < textures.Count; i++) 
        {
            
            var modulateColor = i < Colours.Count ? Colours[i] :  Colors.White;
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

