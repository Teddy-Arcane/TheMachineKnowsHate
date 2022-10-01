using Godot;
using System;

public class MainMenu : Control
{
	private Game _game;
	
	private void _on_Start_pressed()
	{
		GetTree().ChangeScene("res://Scenes/Levels/Main.tscn");
	}

	private void _on_Options_pressed()
	{
		GetTree().ChangeScene("res://Scenes/Options.tscn");
	}

	private void _on_Quit_pressed()
	{
		GetTree().Quit();
	}
}
