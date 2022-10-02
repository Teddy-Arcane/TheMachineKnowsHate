using Godot;
using System;

public class Exit : Area2D
{
	[Export] private string _levelName;
	
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

	private void _on_Exit_body_entered(Node2D body)
	{
		if(body.Name == "Player" && !Player.TerminalVelocity)
		{
			_game.NextLevel();
			_levelId = _game.Level;
		}
	}
}
