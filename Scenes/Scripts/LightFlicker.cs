using Godot;
using System;
using System.Resources;

public class LightFlicker : CanvasModulate
{
	[Export] private float _lerpRate = 0.05f;
	
	private Timer _timer;
	private Timer _soundTimer;
	private Timer _lightsOnTimer;
	private Timer _levelStartTimer;
	private Timer _flashSoundTimer;
	private MainUI _ui;
	private AudioStreamPlayer _flash;
	private AudioPlayer _audio;
	
	
	public static float TimeLeft = 0f;
	
	[Signal] public delegate void LightsToggled(bool on);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_ui = GetTree().Root.GetNode<Control>("GameWorld/MainUI") as MainUI;
		_timer = GetNode<Timer>("FlashTimer");
		_soundTimer = GetNode<Timer>("FlashSoundTimer");
		_lightsOnTimer = GetNode<Timer>("LightsOnTimer");
		_levelStartTimer = GetNode<Timer>("LevelStartTimer");
		_flashSoundTimer = GetNode<Timer>("FlashSoundTimer");
		_flash = GetNode<AudioStreamPlayer>("Flash");
		_audio = GetNode<AudioPlayer>("AudioPlayer");
	}

	private bool tick1 = false;
	private bool tick2 = false;
	public override void _Process(float delta)
	{
		if (_levelStartTimer.IsStopped())
		{
			_ui.SetHint(_timer.TimeLeft.ToString("0.0"));
		}
		else
		{
			if (_levelStartTimer.TimeLeft.ToString("0.00").Contains("2."))
			{
				if(!tick1)
					_audio.Play("Tick");
				
				tick1 = true;
				_ui.SetHint("ready");
			}
			if (_levelStartTimer.TimeLeft.ToString("0.00").Contains("1."))
			{
				if(!tick2)
					_audio.Play("Tick");
				
				tick2 = true;
				_ui.SetHint("set");
			}
		}
	}

	public override void _PhysicsProcess(float delta)
	{
		if (_levelStartTimer.IsStopped())
		{
			Color = Color.LinearInterpolate(Colors.Black, _lerpRate);
		}
	}

	public void StartNewLevel()
	{
		Player.MovementDisabled = true;
		
		_timer.Stop();
		_flashSoundTimer.Stop();
		_lightsOnTimer.Stop();
		
		_levelStartTimer.Start();
	}
	
	private void _on_Timer_timeout()
	{
		Player.MovementDisabled = false;
		
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
	
	private void _on_LevelStartTimer_timeout()
	{
		_timer.Start(_timer.WaitTime);
		_flashSoundTimer.Start(_flashSoundTimer.WaitTime);
		_lightsOnTimer.Start(_lightsOnTimer.WaitTime);
		
		_on_Timer_timeout();
	}
}
