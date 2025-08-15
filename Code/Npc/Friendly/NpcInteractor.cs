using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Interactive.Inventory;
using SoYouWANNAJam2025.Code.Interactive.Stations;
using BaseRecipe = SoYouWANNAJam2025.Code.Interactive.Stations.BaseRecipe;

namespace SoYouWANNAJam2025.Code.Npc.Friendly;

public partial class NpcInteractor : Interactible
{
    //public Array<Interactible> PossibleTargets = [];
    //public int CurrentTarget = 0;
    private Timer _distanceCheckTimer;
    private Friendly.Npc _npc;
    [Signal]
    public delegate void NpcLeftEventHandler(Npc npc);

    //public Interactive.Inventory.InventorySlot InventorySlot = null;
    //public Interactive.Inventory.InventorySlot InventorySlot;
    public Inventory Inventory;
    public BaseRecipe CurrentRecipe;
    //public Timer RecipeTimer;
    private Player.CharacterControl _player;
    //private CraftingStationInterface _interactionInterface;
    private Node2D _interfaceLocation;
    [Export] public Array<BaseRecipe> Recipes = [];
    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;
        base._Ready();
        if (FindChild("Inventory") is Interactive.Inventory.Inventory inv) Inventory=inv;
        
        Inventory.RecipeWhitelist = Recipes;
        Inventory.CompileWhitelist();
        _npc = GetParent<Npc>();
        Interact += OnInteractMethod;
        BodyEntered += OnInteractableEntered;
        AreaEntered += OnInteractableEntered;
        
    }
    private void OnInteractableEntered(Node2D unknownTarget)
    {
        if (unknownTarget is not CraftingStation target)
        {
            if (unknownTarget.Name == "LeaveArea")
            {
                EmitSignal(SignalName.NpcLeft, _npc);
                _npc.QueueFree();
            }
            return;
        }
        _npc.Mood = 100;
        _npc.MoodTimer.Start();
        target.TriggerInteraction(this,TriggerType.PickupDrop);
        //GD.Print("Crafting table reached");

    }
    private void OnInteractMethod(Node2D node, TriggerType trigger)
    {
        if (node is not Player.PlayerInteractor interactor) return;

        switch (trigger)
        {
            case TriggerType.PickupDrop:
                if (Inventory.HasItem() && interactor.InventorySlot.HasSpace())
                {
                    Inventory.TransferTo(interactor.InventorySlot);
                }
                else if (Inventory.HasSpace() && interactor.InventorySlot.HasItem())
                {
                    interactor.InventorySlot.TransferTo(Inventory);
                }
                break;
            case TriggerType.UseAction:
                AttemptGiveRequest();
                break;
        } 
    }

    private void AttemptGiveRequest()
    {
        GD.Print("Attempting give request... to be implemented");
    }
  
}