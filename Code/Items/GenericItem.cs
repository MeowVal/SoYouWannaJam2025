using Godot;

namespace SoYouWANNAJam2025.Code.Items;

public partial class GenericItem : Node2D
{
    [Export] public BaseItem BaseItem = new BaseItem();

    private CharacterControl _player;
    private Interactible _interactible;

    public override void _Ready()
    {
        _interactible = GetNode<Interactible>("Interactible");
        _interactible.Interact += OnInteractMethod;

        if (BaseItem.Icon != null)
        {
            GetNode<Sprite2D>("ItemSprite").Texture = BaseItem.Icon;
        }
        /*if (GetNode<CollisionShape2D>("CollisionShape2D").GetShape() is CircleShape2D circleShape)
        {
            circleShape.Radius = BaseItem.Size;
        }*/
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