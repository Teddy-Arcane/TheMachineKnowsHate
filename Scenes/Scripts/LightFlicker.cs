using Godot;
using System;
using System.Resources;

public class LightFlicker : CanvasModulate
{
	[Export] private float _lerpRate = 0.05f;
	
	private Timer _timer;
	private Timer _soundTimer;
	private Timer _lightsOnTimer;
	private MainUI _ui;
	private AudioStreamPlayer _flash;
	
	public static float TimeLeft = 0f;
	public static bool LightsOn = false;
	
	[Signal] public delegate void LightsToggled(bool on);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_ui = GetTree().Root.GetNode<Control>("GameWorld/MainUI") as MainUI;
		_timer = GetNode<Timer>("FlashTimer");
		_soundTimer = GetNode<Timer>("FlashSoundTimer");
		_lightsOnTimer = GetNode<Timer>("LightsOnTimer");
		_flash = GetNode<AudioStreamPlayer>("Flash");

		_on_Timer_timeout();
	}

	public override void _Process(float delta)
	{
		TimeLeft = _timer.TimeLeft;
		_ui.SetHint(TimeLeft.ToString("0.0"));
	}

	public override void _PhysicsProcess(float delta)
	{
		Color = Color.LinearInterpolate(Colors.Black, _lerpRate);
	}
	
	private void _on_Timer_timeout()
	{
		_flash.Play();
		Color = Colors.White;
		_lightsOnTimer.Start();
		_soundTimer.Start();
		
		EmitSignal("LightsToggled", true);
	}
	
	private void _on_LightsOnTimer_timeout()
	{
		EmitSignal("LightsToggled", false);
	}
}
