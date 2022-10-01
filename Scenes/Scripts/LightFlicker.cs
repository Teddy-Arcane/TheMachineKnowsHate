using Godot;
using System;

public class LightFlicker : CanvasModulate
{
	[Export] private bool _lightsOn = true;
	[Export] private float _lerpRate = 0.05f;
	
	private Timer _timer;
	private MainUI _ui;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_ui = GetTree().Root.GetNode<Control>("GameWorld/MainUI") as MainUI;
		_timer = GetNode<Timer>("Timer");
	}

	public override void _Process(float delta)
	{
		_ui.SetHint(_timer.TimeLeft.ToString("0.0"));
	}

	public override void _PhysicsProcess(float delta)
	{
		if (_lightsOn)
			Color = Colors.White;
		else
		{
			Color = Color.LinearInterpolate(Colors.Black, _lerpRate);
		}
	}
	
	private void _on_Timer_timeout()
	{
		Color = Colors.White;
	}
}



