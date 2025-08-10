using System.Numerics;
using Godot;
using Vector2 = Godot.Vector2;

namespace SoYouWANNAJam2025.Code;

public partial class CharacterControl : CharacterBody2D
{

	[Export]public int MoveSpeed = 72;
	[Export]public double AccelerationSpeed = 10;
	[Export] public double DecelerationSpeed = 15;
	public ModularWeapon HeldItem;

	private double _currentSpeed;
	private Sprite2D _charSprite;

	public override void _Ready()
	{
		base._Ready();
		HeldItem = new ModularWeapon();
		var resource = GD.Load<BaseWeaponModifier>("res://Resources/TestModifier.tres");
		HeldItem.Modifiers.Add(resource);
		_charSprite = GetNode<Sprite2D>("CharSprite");
	}

	public override void _PhysicsProcess(double delta)
	{
		
		var charDir = Input.GetVector("left", "right", "up", "down");
		
		if (charDir != Vector2.Zero)
		{
			Velocity = Velocity.MoveToward(charDir * MoveSpeed, (float)(AccelerationSpeed));
			
			if (charDir.X == 1)
			{
				_charSprite.RegionRect = new Rect2(0, 32, 32, 32); // character left sprite
			}
			else if (charDir.X == -1)
			{
				_charSprite.RegionRect = new Rect2(32, 32, 32, 32); // character right sprite
			}
			else if (charDir.Y == 1)
			{
				_charSprite.RegionRect = new Rect2(0, 0, 32, 32); // character up sprite
			}
			else if (charDir.Y == -1)
			{
				_charSprite.RegionRect = new Rect2(32, 0, 32, 32); // character down sprite
			}

		} 
		else
		{
			Velocity = Velocity.MoveToward(Vector2.Zero, (float)(DecelerationSpeed));
		}
		MoveAndSlide();
	}
}
