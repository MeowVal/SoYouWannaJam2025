using System.Numerics;
using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.ModularWeapons;
using Vector2 = Godot.Vector2;


namespace SoYouWANNAJam2025.Code;

public partial class CharacterControl : CharacterBody2D
{
	[ExportGroup("Info")]
	[Export] public string DisplayName = "PlayerName";

	[ExportGroup("Movement")]
	[Export] public int MoveSpeed = 72;
	[Export] public double AccelerationSpeed = 10;
	[Export] public double DecelerationSpeed = 15;
	private double _currentSpeed;
	private AnimatedSprite2D _charSprite;
	private string _direction = "down";
	private Vector2 _targetPos;
	private int _moveThreshold = 3;
	
	public Array<Interactible> Interactable = [];

	[ExportGroup("Inventory")]
	public ModularWeapon HeldItem;

	private Node2D _camera;
	private string _target; 
	
	public override void _Ready()
	{
		base._Ready();
		HeldItem = new ModularWeapon();
		var resource = GD.Load<BaseWeaponModifier>("res://Resources/WeaponModifiers/TestModifier.tres");
		HeldItem.Modifiers.Add(resource);
		_charSprite = GetNode<AnimatedSprite2D>("CharSprite");
		
		_camera = GetTree().Root.GetCamera2D(); // Gives controls over Camera properties and get a reference to it.
		_camera.Scale = new Vector2(Global.GameScale, Global.GameScale);
	}

	private void PlayerMovement(double delta)
	{
		
		var moveDir = Input.GetVector("left", "right", "up",  "down");
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

		if (Input.IsActionJustPressed("ui_accept"))
		{
			if (Interactable.Count > 0)
			{
				Interactable[0].TriggerInteraction(this);
			}
		}
		
		if (Input.IsActionPressed("left"))
		{
			_direction = "left";
			_charSprite.Play("RunSW");
			//GD.Print("LEFT");
		}
		else if (Input.IsActionPressed("right"))
		{
			_direction = "right";
			_charSprite.Play("RunNE");
			//GD.Print("right");
		}
		else if (Input.IsActionPressed("up"))
		{
			_direction = "up";
			_charSprite.Play("RunNW");
			//GD.Print("up");
		}
		else if (Input.IsActionPressed("down"))
		{
			_direction = "down";
			_charSprite.Play("RunSE");
			//GD.Print("down");
		}

		if (moveDir != Vector2.Zero)
		{
			//moveDir = moveDir.Rotated(Mathf.DegToRad(30));
			Velocity = Velocity.MoveToward(moveDir * (MoveSpeed * Global.GameScale),
				(float)(AccelerationSpeed * Global.GameScale));
		}
		else
		{
			Velocity = Velocity.MoveToward(Vector2.Zero, (float)(DecelerationSpeed * Global.GameScale));
			switch (_direction)
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
			}
		}
		MoveAndSlide();
	}

	private bool _canPress = true; // to Delete, camera target change test
	public override void _PhysicsProcess(double delta)
	{
		PlayerMovement(delta); //cleaned up _PhysicsProcess and moved player movement to its own function.
		
		//Testing function for camera target change
		if (Input.IsActionPressed("jump"))
		{
			if (_canPress)
			{
				_canPress = false;
				if (_target == "Player")
				{
					_target = "Player2";
					GD.Print(GetParent().GetNode<Node2D>("TestTarget"));
					_camera.Set("FollowingNode", GetParent().GetNode<Node2D>("TestTarget"));
				}
				else
				{
					_target = "Player";
					_camera.Set("FollowingNode", GetParent().GetNode<Node2D>("Character"));
				}
			}
		}
		else
		{
			_canPress = true;
		}
	}
}
