using Godot;
using System;

public class Exit : Area2D
{
	private Game _game;
	private int _levelId;

	public override void _Ready()
	{
		_game = GetTree().Root.GetNode("GameWorld") as Game;
		_levelId = _game.Level;
	}

	private void _on_Exit_body_entered(Node2D body)
	{
		if(body.Name == "Player")
		{
			_game.NextLevel();
			_levelId = _game.Level;
		}
	}
}
