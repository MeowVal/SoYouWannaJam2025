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
    public delegate void RequestGivenEventHandler();
    [Signal]
    public delegate void RequestCompleteEventHandler(Npc npc);
    [Signal]
    public delegate void CombatEndedEventHandler(Npc npc, HostileNpc hostileNpc);
    
    public Inventory Inventory;
    private Player.CharacterControl _player;
    private Node2D _interfaceLocation;
    private Timer _combatTimer;
    private HostileNpc _hostileNpc;
    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;
        base._Ready();
        if (FindChild("Inventory") is Interactive.Inventory.Inventory inv) Inventory=inv;
        //if (FindChild("CombatTimer") is Timer timer) _combatTimer=timer;
        //_combatTimer.Timeout += OnCombatTimerTimeout;
       
        _npc = GetParent<Npc>();
        Interact += OnInteractMethod;
        BodyEntered += OnInteractableEntered;
        AreaEntered += OnInteractableEntered;
    }

    private void OnCombatTimerTimeout()
    {
        EmitSignal(SignalName.CombatEnded, _npc,_hostileNpc);
    }

    private void OnInteractableEntered(Node2D unknownTarget)
    {
        if (unknownTarget is not FrontDesk target)
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
        _npc.MoodBar.Visible = true;
        //target.TriggerInteraction(this,TriggerType.PickupDrop);
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
                    if (_npc.RequestComplete) break;
                    Inventory.TransferTo(interactor.InventorySlot);
                    
                    if (_npc.RequestGiven == false)
                    {
                        _npc.Mood = 100;
                        _npc.MoodTimer.Start();
                        _npc.MoodBar.Visible = true;
                    }
                    
                }
                else if (Inventory.HasSpace() && interactor.InventorySlot.HasItem())
                {
                    if (_npc.RequestComplete) break;
                    if (interactor.InventorySlot.Item is ModularItem modularItem )
                    {
                        if (modularItem.IsCompleted())
                        {
                            //GD.Print("NPC Complete");
                        
                            _npc.RequestComplete = true;
                            _npc.LeaveQueue();
                            _npc.StartMoodTimer =  false;
                            _npc.MoodTimer.Stop();
                            _npc.MoodBar.Visible = false;
                            _npc.MoodSpriteY.Visible = true;
                            interactor.InventorySlot.TransferTo(Inventory, true);
                        }
                        else if (modularItem.OwningNpc == _npc)
                        {
                            interactor.InventorySlot.TransferTo(Inventory, true);
                        }
                    } 
                }
                break;
            case TriggerType.UseAction:
                break;
        } 
    }
  
}