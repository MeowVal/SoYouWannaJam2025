using System.Linq;
using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Interactive.Inventory;
using SoYouWANNAJam2025.Code.Interactive.Items;
using SoYouWANNAJam2025.Code.Interactive.Stations;
using SoYouWANNAJam2025.Code.Npc.Hostile;
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
    [Signal]
    public delegate void ModularItemRepairRequestEventHandler(EPartType partType, ModularPartTemplate  part);
    [Signal]
    public delegate void ModularItemUpgradeRequestEventHandler(EPartType partType, ModularPartTemplate  part);
    [Signal]
    public delegate void RequestGivenEventHandler();
    [Signal]
    public delegate void RequestCompleteEventHandler(Npc npc);
    [Signal]
    public delegate void CombatEndedEventHandler(Npc npc, HostileNpc hostileNpc);

    //private Godot.Collections.Array _itemParts = [];
    private EPartType _modularItemPartKey;
    private ModularPartTemplate _modularItemPart;
    //public Interactive.Inventory.InventorySlot InventorySlot = null;
    //public Interactive.Inventory.InventorySlot InventorySlot;
    public Inventory Inventory;
    public BaseRecipe CurrentRecipe;
    //public Timer RecipeTimer;
    private Player.CharacterControl _player;
    //private CraftingStationInterface _interactionInterface;
    private bool _requestGiven = false;
    private bool _requestComplete = false;
    private Node2D _interfaceLocation;
    [Export] public Array<BaseRecipe> Recipes = [];
    private Timer _combatTimer;
    private HostileNpc _hostileNpc;
    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;
        base._Ready();
        if (FindChild("Inventory") is Interactive.Inventory.Inventory inv) Inventory=inv;
        if (FindChild("CombatTimer") is Timer timer) _combatTimer=timer;
        _combatTimer.Timeout += OnCombatTimerTimeout;
        Inventory.RecipeWhitelist = Recipes;
        Inventory.CompileWhitelist();
        _npc = GetParent<Npc>();
        Interact += OnInteractMethod;
        BodyEntered += OnInteractableEntered;
        AreaEntered += OnInteractableEntered;
        RequestGiven += OnRequestGiven;
        RequestComplete += OnRequestComplete;
        
    }

    private void OnCombatTimerTimeout()
    {
        EmitSignal(SignalName.CombatEnded, _npc,_hostileNpc);
    }

    private void OnRequestComplete(Npc npc)
    {
        _npc.MoodTimer.Stop();
    }

    private void OnRequestGiven()
    {
        _npc.Mood = 100;
        _npc.MoodTimer.Start();
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
            else if (unknownTarget is NpcInteractor { _npc: HostileNpc hostileNpc })
            {
                //_combatTimer.WaitTime = _hostileNpc.Health - _weaponDamage;
                _combatTimer.WaitTime = 10;
                _combatTimer.Start();
                _hostileNpc = hostileNpc; 
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
                    if (_requestGiven && interactor.InventorySlot.Item is ModularItem item )
                    {
                        if (item.Parts[_modularItemPartKey] != _modularItemPart) EmitSignal(SignalName.RequestComplete);
                        else return;
                    }
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
        if (_requestGiven) return;
        foreach (var slot in Inventory.Slots)
        {
            if (!slot.HasItem()) continue;
            if (slot.Item is ModularItem item)
            {
                foreach (var (partType, part) in item.Parts)
                {
                    if (part.PartState != EPartState.Broken) continue;
                    EmitSignal(SignalName.ModularItemRepairRequest, (int)partType, part);
                    EmitSignal(SignalName.RequestGiven);
                    _modularItemPartKey = partType;
                    _modularItemPart = part;
                    return;
                }
                var keyList = item.Parts.Keys.ToList();
                var index = GD.RandRange(0,keyList.Count -1);
                var partKey = (EPartType)(int)keyList[index];
                var itemPart = item.Parts[partKey];
                EmitSignal(SignalName.ModularItemUpgradeRequest, (int)partKey, itemPart);
                EmitSignal(SignalName.RequestGiven);
                _modularItemPartKey = partKey;
                _modularItemPart = itemPart;
                return;
            };
        }
        GD.Print("Attempting give request... to be implemented");
    }
  
}