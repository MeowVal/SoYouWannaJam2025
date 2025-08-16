using System.Text.RegularExpressions;
using Godot;
using SoYouWANNAJam2025.Code.World;
using Vector2 = Godot.Vector2;


namespace SoYouWANNAJam2025.Code.Player;

public partial class CharacterControl : CharacterBody2D
{
	[ExportGroup("Info")]
	[Export] public string DisplayName = "PlayerName";

	[ExportGroup("Movement")]
	[Export] public int MoveSpeed = 72;
	[Export] public double AccelerationSpeed = 10;
	[Export] public double DecelerationSpeed = 15;
	private double _currentSpeed;
	private Sprite2D _charSprite;
	private string _direction = "down";
	private Vector2 _targetPos;
	private int _moveThreshold = 3;

	public bool Frozen = false;

	[ExportGroup("Inventory")]
	private Player.PlayerInteractor _interactor;

	private Node2D _camera;
	private string _target; 
	
	public override void _Ready()
	{
		_interactor = GetNode<Player.PlayerInteractor>("PlayerInteractor");
		_charSprite = GetNode<Sprite2D>("CharSprite");
		
		_camera = GetTree().Root.GetCamera2D(); // Gives controls over Camera properties and get a reference to it.
		_camera.Scale = new Vector2(World.Global.GameScale, World.Global.GameScale);
		Global.Player = GetNode<CharacterControl>(".");
	}

	public override void _Input(InputEvent @event)
	{
		// if (@event is InputEventMouse mouseEvent)
		// {
		// 	GD.Print(mouseEvent);
		// }
		
		if (@event.IsActionPressed("UseAction"))
		{
			_interactor.TriggerInteraction(TriggerType.UseAction);
		}
		else if (@event.IsActionPressed("PickupDrop"))
		{
			_interactor.TriggerInteraction(TriggerType.PickupDrop);
			if (_interactor.InventorySlot.HasItem())
			{
				var item = _interactor.InventorySlot.Item;
				//set item Y value to be visually above the character's head
				item.GlobalPosition = new Vector2(item.GlobalPosition.X, item.GlobalPosition.Y - 35);
				_charSprite.Frame = _direction switch
				{
					"right" => 3,
					"left" => 2,
					_ => _charSprite.Frame
				};
			}
			else
			{
				_charSprite.Frame = _direction switch
				{
					"right" => 1,
					"left" => 0,
					_ => _charSprite.Frame
				};
			}
		}
		else if (@event.IsActionPressed("MoveRight") && !Frozen)
		{
			_direction = "right";
			_charSprite.Frame = 1;
			if (_interactor.InventorySlot.HasItem()) _charSprite.Frame = 3;
		}
		else if (@event.IsActionPressed("MoveLeft") && !Frozen)
		{
			_direction = "left";
			_charSprite.Frame = 0;
			if (_interactor.InventorySlot.HasItem()) _charSprite.Frame = 2;
		}
		else if (@event.IsActionPressed("MoveUp") && !Frozen)
		{
			//_direction = "up";
		}
		else if (@event.IsActionPressed("MoveDown") && !Frozen)
		{
			//_direction = "down";
		}
	}

	private void PlayerMovement(double delta)
	{
		var moveDir = Vector2.Zero;
		moveDir.X = Input.GetActionStrength("MoveRight") - Input.GetActionStrength("MoveLeft");
		moveDir.Y = Input.GetActionStrength("MoveDown") - Input.GetActionStrength("MoveUp");
		moveDir = moveDir.Normalized();
		
		if (Frozen) moveDir = Vector2.Zero;
		
		if (Input.IsMouseButtonPressed(MouseButton.Left))
		{
			_targetPos = GetGlobalMousePosition();
			if (_targetPos != Vector2.Zero)
			{
				var direction = (_targetPos - GlobalPosition).Normalized();
				var distance = GlobalPosition.DistanceTo(_targetPos);
				
				if (distance < _moveThreshold) //idle
				{
					_targetPos = Vector2.Zero;
				}
				else //movement
				{
					moveDir = direction;
				}
			}
		}

		if (moveDir != Vector2.Zero)
		{
			//moveDir = moveDir.Rotated(Mathf.DegToRad(30));
			Velocity = Velocity.MoveToward(moveDir * (MoveSpeed * World.Global.GameScale), (float)(AccelerationSpeed * World.Global.GameScale));
			//GD.Print(Velocity);
		}
		else
		{
			Velocity = Velocity.MoveToward(Vector2.Zero, (float)(DecelerationSpeed * World.Global.GameScale));
			/*switch (_direction)
			{
				case "right":
					_charSprite.Play("idleNE");
					break;
				case "left":
					_charSprite.Play("idleSW");
					break;
				case "up":
					_charSprite.Play("idleNW");
					break;
				case "down":
					_charSprite.Play("idleSE");
					break;
			}*/
		}
		MoveAndSlide();
	}

	private bool _canPress = true; // to Delete, camera target change test
	public override void _PhysicsProcess(double delta)
	{
		PlayerMovement(delta); //cleaned up _PhysicsProcess and moved player movement to its own function.
	}
}
