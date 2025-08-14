using Godot;

namespace SoYouWANNAJam2025.Code.World;

public partial class Sun : DirectionalLight2D
{
	[Export] public Light2D SunLight;
	[Export] public float OrbitRadius = 5000f;
	[Export] public float CycleDuration = 60f; // seconds for full day
	[Export] public Gradient LightGradient; // Color over time (create in editor)
	[Export] public Curve EnergyCurve; 
	private float _time;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	

	public override void _Process(double delta)
	{
		_time += (float)delta;
		float t = (_time % CycleDuration) / CycleDuration;
		
		SunLight.Color = LightGradient.Sample(t);
		SunLight.Energy = EnergyCurve.Sample(t);

		// Convert time to angle (0 = sunrise, 0.5 = noon, 1 = night)
		float angle = Mathf.Lerp(-90f, 270f, t); // 360-degree orbit

		// Convert angle to radians and calculate sun position
		float radians = Mathf.DegToRad(angle);
		Vector2 center = GetViewport().GetVisibleRect().Size / 2f;
		Vector2 offset = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * OrbitRadius;

		SunLight.Position = center + offset;
		SunLight.Rotation = radians + Mathf.Pi; // Aim toward center
	}
}