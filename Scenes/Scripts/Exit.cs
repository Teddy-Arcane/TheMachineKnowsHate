using Godot;
using System;

public class Exit : Area2D
{
	[Export] private string _levelName;
	[Export] private string _message;
	[Export] private bool _deathMeansNextLevel = false;

	private Game _game;
	private int _levelId;
	private MainUI _mainUI;
	private Timer _timer;
	private Player _player;
	private Label _hint;
	private AudioPlayer _audio;
	
	public override void _Ready()
	{
		_game = GetTree().Root.GetNode("GameWorld") as Game;
		_levelId = _game.Level;
		_mainUI = GetTree().Root.GetNode<MainUI>("GameWorld/MainUI");
		_timer = GetNode<Timer>("Timer");
		_hint = GetTree().Root.GetNode<Label>("GameWorld/MainUI/CanvasLayer/Hint");
		_audio = GetNode<AudioPlayer>("AudioPlayer");
		
		_mainUI.SetLevelName(_levelName);
	}

	public override void _Process(float delta)
	{
		if (Player.MovementDisabled && !string.IsNullOrWhiteSpace(_message))
		{
			_mainUI.SetTutorial(_message);
		}
		else
		{
			_mainUI.SetTutorial(string.Empty);
		}
	}

	private void _on_Exit_body_entered(Node2D body)
	{
		if(body.Name == "Player")
		{
			if (!Player.TerminalVelocity)
			{
				_game.PlaySuccess();
				_game.NextLevel();
				_levelId = _game.Level;
			}
			else if(_deathMeansNextLevel)
			{
				_hint.Visible = false;
				_player = body as Player;
				_timer.Start();
			}
		}
	}
	
	private void _on_Timer_timeout()
	{
		_player.ActuallyKill();
		_game.NextLevel();
		_levelId = _game.Level;
	}
}
