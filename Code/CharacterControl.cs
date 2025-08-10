using System.Numerics;
using Godot;
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

	[ExportGroup("Inventory")]
	public ModularWeapon HeldItem;

	public override void _Ready()
	{
		base._Ready();
		HeldItem = new ModularWeapon();
		var resource = GD.Load<BaseWeaponModifier>("res://Resources/TestModifier.tres");
		HeldItem.Modifiers.Add(resource);
		_charSprite = GetNode<AnimatedSprite2D>("CharSprite");
	}

	private void PlayerMovement(double delta)
	{
		Vector2 velocity = Velocity;
		//velocity.X = MoveSpeed;//, (float)(AccelerationSpeed);
		//velocity.Y = (MoveSpeed / 2);//, (float)(AccelerationSpeed);
		
		if (Input.IsActionPressed("left"))
		{
			velocity.X = -MoveSpeed;
			velocity.Y = (MoveSpeed / 2);
			Velocity = velocity;
			
			_direction = "left";
			_charSprite.Play("RunSW");
			//GD.Print("LEFT");
		}
		else if (Input.IsActionPressed("right"))
		{
			velocity.X = MoveSpeed;
			velocity.Y = -(MoveSpeed / 2);
			Velocity = velocity;
			
			_direction = "right";
			_charSprite.Play("RunNE");
			//GD.Print("right");
		}
		else if (Input.IsActionPressed("up"))
		{
			velocity.X = -MoveSpeed;
			velocity.Y = -(MoveSpeed / 2);
			Velocity = velocity;
			
			_direction = "up";
			_charSprite.Play("RunNW");
			//GD.Print("up");
		}
		else if (Input.IsActionPressed("down"))
		{
			velocity.X = MoveSpeed;
			velocity.Y = (MoveSpeed / 2);
			Velocity = velocity;
			
			_direction = "down";
			_charSprite.Play("RunSE");
			//GD.Print("down");
		}
		else
		{
			Velocity = velocity.MoveToward(Vector2.Zero, (float)(DecelerationSpeed)).Round();
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
	
	public override void _PhysicsProcess(double delta)
	{
		PlayerMovement(delta); //cleaned up _PhysicsProcess and moved player movement to its own function.
	}
}
