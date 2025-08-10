using Godot;

namespace SoYouWANNAJam2025.Code.Items;

public partial class Item : Interactible
{
    [Export] public string DisplayName = "Unknown";
    [Export] public string Description = "You need to actually set this lol.";
    [Export] public Texture2D Icon;
    
    private CharacterControl _player;
    
    public override void _Ready()
    {
        base._Ready();
        BodyEntered += OnItemBodyEntered;
        BodyExited += OnItemBodyExited;
        Interact += OnInteractMethod;
    }

    private void OnItemBodyEntered(Node2D body)
    {
        if (body is not CharacterControl character) return;
        _player = character;
        
        _player.Interactable.Add(GetNode<Interactible>("."));
        GD.Print(_player.Interactable);
    }
    
    private void OnItemBodyExited(Node2D body)
    {
        if (body is not CharacterControl character) return;
        
        _player.Interactable.Remove(this);
        GD.Print(_player.Interactable);
    }
    
    private void OnInteractMethod(Node2D node)
    {
        if (node is not CharacterControl character) return;
        
        GD.Print("Interacted with: ", DisplayName);
    }
}