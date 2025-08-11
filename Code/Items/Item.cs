using Godot;

namespace SoYouWANNAJam2025.Code.Items;

public partial class Item : Interactible
{
    [Export] public string DisplayName = "Unknown";
    [Export] public string Description = "You need to actually set this lol.";
    [Export] public Texture2D Icon;

    private void OnInteractMethod(Node2D node)
    {
        if (node is not CharacterControl character) return;

        GD.Print(character.DisplayName + " interacted with item ", DisplayName);
    }


}