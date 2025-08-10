using Godot;
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

	[ExportGroup("Inventory")]
	public ModularWeapon HeldItem;

	public override void _Ready()
	{
		base._Ready();
		HeldItem = new ModularWeapon();
		var resource = GD.Load<BaseWeaponModifier>("res://Resources/WeaponModifiers/TestModifier.tres");
		HeldItem.Modifiers.Add(resource);
		_charSprite = GetNode<AnimatedSprite2D>("CharSprite");
		GetNode<Camera2D>("Camera2D").Scale = new Vector2(Global.GameScale, Global.GameScale);
	}

	private void PlayerMovement(double delta)
	{
		Vector2 velocity = Velocity;
		//velocity.X = MoveSpeed;//, (float)(AccelerationSpeed);
		//velocity.Y = (MoveSpeed / 2);//, (float)(AccelerationSpeed);
		
		var moveDir = Vector2.Zero;
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
					velocity.X = direction.X * (MoveSpeed * Global.GameScale);
					velocity.Y = direction.Y * (MoveSpeed * Global.GameScale);
					
					Velocity = velocity;
				}
			}
		}
		else
		{
			if (Input.IsActionPressed("left"))
			{
				/*velocity.X = -scaledMoveSpeed;
			velocity.Y = (scaledMoveSpeed / 2);
			Velocity = velocity;*/

				moveDir.X = -1;

				_direction = "left";
				_charSprite.Play("RunSW");
				//GD.Print("LEFT");
			}
			else if (Input.IsActionPressed("right"))
			{
				/*velocity.X = scaledMoveSpeed;
			velocity.Y = -(scaledMoveSpeed / 2);
			Velocity = velocity;*/

				moveDir.X = 1;

				_direction = "right";
				_charSprite.Play("RunNE");
				//GD.Print("right");
			}
			else if (Input.IsActionPressed("up"))
			{
				/*velocity.X = -scaledMoveSpeed;
			velocity.Y = -(scaledMoveSpeed / 2);
			Velocity = velocity;*/

				moveDir.Y = -1;

				_direction = "up";
				_charSprite.Play("RunNW");
				//GD.Print("up");
			}
			else if (Input.IsActionPressed("down"))
			{
				/*velocity.X = scaledMoveSpeed;
			velocity.Y = (scaledMoveSpeed / 2);
			Velocity = velocity;*/

				moveDir.Y = 1;

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
		}
		MoveAndSlide();
	}
	
	public override void _PhysicsProcess(double delta)
	{
		PlayerMovement(delta); //cleaned up _PhysicsProcess and moved player movement to its own function.
	}
}