using Godot;
using System;

public class Game : Node
{
	public int Level = 0;

	public override void _Ready()
	{
		NextLevel();
	}
	
	public override void _Process(float delta)
	{
		if(Input.IsActionPressed("skip"))
			NextLevel();
	}

	public void NextLevel()
	{
		Level++;
		CallDeferred("LoadLevel");
	}

	public void LoadLevel()
	{
		var scene = GD.Load<PackedScene>($"res://Scenes/Maps/Level{Level}.tscn").Instance();
		AddChild(scene);
		
		var entrance = scene.GetNode<Node2D>("Entrance");
		var player = GetNode<KinematicBody2D>("Player");
		player.Position = entrance.Position;
	}
}
