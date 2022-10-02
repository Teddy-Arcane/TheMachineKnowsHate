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
		if(Input.IsActionJustPressed("skip"))
			NextLevel();
	}

	public void NextLevel()
	{
		if(Level > 0)
			GetTree().Root.GetNode($"GameWorld/Level{Level}").QueueFree();

		Level++;
		
		CallDeferred("LoadLevel");
	}

	public void LoadLevel()
	{
		var scene = GD.Load<PackedScene>($"res://Scenes/Maps/Level{Level}.tscn").Instance();
		AddChild(scene);
		
		var entrance = scene.GetNode<Node2D>("Entrance");
		var player = GetNode<Player>("Player");
		player.SetInitialPosition(entrance.Position);
		player.Position = entrance.Position;
	}
}
