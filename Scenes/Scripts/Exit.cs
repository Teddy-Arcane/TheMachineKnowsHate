using Godot;
using System;

public class Exit : Area2D
{
	[Export] private string _levelName;
	[Export] private string _message;

	private Game _game;
	private int _levelId;
	private MainUI _mainUI;
	
	public override void _Ready()
	{
		_game = GetTree().Root.GetNode("GameWorld") as Game;
		_levelId = _game.Level;
		_mainUI = GetTree().Root.GetNode<MainUI>("GameWorld/MainUI");

		
		
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
		if(body.Name == "Player" && !Player.TerminalVelocity)
		{
			_game.NextLevel();
			_levelId = _game.Level;
		}
	}
}
