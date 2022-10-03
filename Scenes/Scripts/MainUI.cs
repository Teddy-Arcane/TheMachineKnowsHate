using Godot;
using System;

public class MainUI : Control
{
	private Label _hint;
	private Label _leveName;
	private Label _tutorial;
	
	//PackedScene _dialog = GD.Load<PackedScene>("res://Scenes/UI/DialogBox.tscn");

	public override void _Ready()
	{
		_hint = GetNode<Label>("CanvasLayer/Hint");
		_leveName = GetNode<Label>("CanvasLayer/LevelName");
		_tutorial = GetNode<Label>("CanvasLayer/Tutorial");
	}

	// public void StartDialog(string dialogPath)
	// {
	// 	var dialog = _dialog.Instantiate() as Control;
	// 	var box = dialog.GetNode<ColorRect>("CanvasLayer/DialogBox");
	// 	var script = box as DialogBox;
	// 	
	// 	AddChild(dialog);
	//
	// 	script.Start(dialogPath);
	// }

	public void SetLevelName(string name)
	{
		_leveName.Text = name;
	}

	public void SetHint(string hint)
	{
		_hint.Text = hint;
	}

	public void SetTutorial(string message)
	{
		_tutorial.Text = message;
	}
}
