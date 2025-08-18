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
	private AudioStreamPlayer2D _audioStreamPlayer;
	
	public override void _Ready()
	{
		_interactor = GetNode<Player.PlayerInteractor>("PlayerInteractor");
		_charSprite = GetNode<Sprite2D>("CharSprite");
		
		_camera = GetTree().Root.GetCamera2D(); // Gives controls over Camera properties and get a reference to it.
		_camera.Scale = new Vector2(World.Global.GameScale, World.Global.GameScale);
		Global.Player = GetNode<CharacterControl>(".");
		_audioStreamPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
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
		

		if (moveDir != Vector2.Zero)
		{
			Velocity = Velocity.MoveToward(moveDir * (MoveSpeed * World.Global.GameScale), (float)(AccelerationSpeed * World.Global.GameScale));
			if (!_audioStreamPlayer.Playing) _audioStreamPlayer.Play();
			
		}
		else
		{
			Velocity = Velocity.MoveToward(Vector2.Zero, (float)(DecelerationSpeed * World.Global.GameScale));
			if (_audioStreamPlayer.Playing) _audioStreamPlayer.Stop();
		}
		MoveAndSlide();
	}

	private bool _canPress = true; // to Delete, camera target change test
	public override void _PhysicsProcess(double delta)
	{
		PlayerMovement(delta); //cleaned up _PhysicsProcess and moved player movement to its own function.
	}
}
