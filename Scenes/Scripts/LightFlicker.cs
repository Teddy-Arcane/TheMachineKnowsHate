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
			if (!_timer.IsStopped())
			{
				_ui.SetHint(_timer.TimeLeft.ToString("0.0"));
				
				if (_timer.TimeLeft.ToString("0.00").Contains("1."))
				{
					if(!tick1)
						_audio.Play("Tick");
				
					tick1 = true;
				}
				if (_timer.TimeLeft.ToString("0.00").Contains("0."))
				{
					if(!tick2)
						_audio.Play("Tick");
				
					tick2 = true;
				}
			}
		}
		else
		{
			_ui.SetHint(string.Empty);
			
			if (_levelStartTimer.TimeLeft.ToString("0.00").Contains("1."))
			{
				if(!tick1)
					_audio.Play("Tick");
				
				tick1 = true;
			}
			if (_levelStartTimer.TimeLeft.ToString("0.00").Contains("0."))
			{
				if(!tick2)
					_audio.Play("Tick");
				
				tick2 = true;
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

	public void StartDialogLevel()
	{
		Color = Colors.Black;
		
		_timer.Stop();
		_flashSoundTimer.Stop();
		_lightsOnTimer.Stop();
		_levelStartTimer.Stop();
	}

	public void StartNewLevel()
	{
		tick1 = false;
		tick2 = false;
		
		Color = Colors.Black;
		
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
		_audio.Play("Fan");
		Color = Colors.White;
		_lightsOnTimer.Start();
		_soundTimer.Start();
		
		EmitSignal("LightsToggled", true);
	}
	
	private void _on_LightsOnTimer_timeout()
	{
		if(GetSignalConnectionList("LightsToggled").Count > 0)
			_audio.Play("Off");
		
		tick1 = false;
		tick2 = false;
		
		EmitSignal("LightsToggled", false);
	}
	
	private void _on_LevelStartTimer_timeout()
	{
		tick1 = false;
		tick2 = false;
		
		_timer.Start(_timer.WaitTime);
		_flashSoundTimer.Start(_flashSoundTimer.WaitTime);
		_lightsOnTimer.Start(_lightsOnTimer.WaitTime);
		
		_on_Timer_timeout();
	}
}
