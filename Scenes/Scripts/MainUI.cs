using Godot;
using System;

public class MainUI : Control
{
	private Label _hint;
	
	//PackedScene _dialog = GD.Load<PackedScene>("res://Scenes/UI/DialogBox.tscn");

	public override void _Ready()
	{
		_hint = GetNode<Label>("CanvasLayer/Hint");
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

	public void SetHint(string hint)
	{
		_hint.Text = hint;
	}
}
