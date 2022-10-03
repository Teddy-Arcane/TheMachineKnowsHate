using Godot;
using System;

public class Warning : Control
{
	private void _on_Timer_timeout()
	{
		GetTree().ChangeScene("res://Scenes/MainMenu.tscn");
	}
	
	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("skip_video"))
		{
			GetTree().ChangeScene("res://Scenes/MainMenu.tscn");
		}
	}
}
