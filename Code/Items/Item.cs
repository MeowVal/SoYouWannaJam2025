using Godot;

namespace SoYouWANNAJam2025.Code.Items;

public partial class Item : Interactible
{
    [Export] public BaseItem BaseItem = new BaseItem();

    private CharacterControl _player;

    public override void _Ready()
    {
        base._Ready();
        BodyEntered += OnItemBodyEntered;
        BodyExited += OnItemBodyExited;
        Interact += OnInteractMethod;

        if (BaseItem.Icon != null)
        {
            GetNode<Sprite2D>("Sprite2D").Texture = BaseItem.Icon;
        }
        /*if (GetNode<CollisionShape2D>("CollisionShape2D").GetShape() is CircleShape2D circleShape)
        {
            circleShape.Radius = BaseItem.Size;
        }*/
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
        
        GD.Print("Interacted with: ", BaseItem.DisplayName);
    }

    private void Pickup()
    {

    }
}