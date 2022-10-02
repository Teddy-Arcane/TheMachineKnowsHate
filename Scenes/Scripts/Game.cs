using Godot;
using System;

public class Game : Node
{
	public int Level = 0;

	private LightFlicker _lightFlicker;
	private Timer _flashTimer;
	
	public override void _Ready()
	{
		_lightFlicker = GetNode <LightFlicker>("LightFlicker");
		_flashTimer = GetNode <Timer>("LightFlicker/FlashTimer");
		
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
		player.Position = new Vector2(entrance.Position.x, entrance.Position.y + 23);
		player.PlayAnimation("idle");
		
		_lightFlicker.StartNewLevel();
	}
}
