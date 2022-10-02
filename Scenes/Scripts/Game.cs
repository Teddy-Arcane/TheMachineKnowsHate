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

	public async void LoadLevel()
	{
		var scene = GD.Load<PackedScene>($"res://Scenes/Maps/Level{Level}.tscn").Instance();
		var entrance = scene.GetNode<Node2D>("Entrance");
		var player = GetNode<Player>("Player");
		player.SetInitialPosition(new Vector2(entrance.Position.x, entrance.Position.y ));
		player.Position = player.GetSpawnPoint();
		player.PlayAnimation("idle");
		player.ZeroVelocity();
		
		await ToSignal(GetTree(), "idle_frame");
		
		AddChild(scene);

		_lightFlicker.StartNewLevel();
	}
}
