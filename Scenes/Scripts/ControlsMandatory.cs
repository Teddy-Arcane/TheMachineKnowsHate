using Godot;
using System;

public class ControlsMandatory : Control
{
	private Button _back;

	public override void _Ready()
	{
		_back = GetNode<Button>("CanvasLayer/ColorRect/OK");
		_back.GrabFocus();
	}
	
	private void _on_OK_pressed()
	{
		GetTree().ChangeScene("res://Scenes/Levels/Main.tscn");
	}

}


