using Godot;
using System;

public class Game : Node
{
	public int Level = 0;

	private LightFlicker _lightFlicker;
	private Label _hint;
	private Label _levelname;
	private KinematicBody2D _player;
	private AudioPlayer _audio;
	
	public override void _Ready()
	{
		_lightFlicker = GetNode <LightFlicker>("LightFlicker");
		_hint = GetNode<Label>("MainUI/CanvasLayer/Hint");
		_levelname = GetNode<Label>("MainUI/CanvasLayer/LevelName");
		_player = GetNode<KinematicBody2D>("Player");
		_audio = GetNode<AudioPlayer>("AudioPlayer");
		
		_audio.Play("Fan");
		
		NextLevel();
	}
	
	public override void _Process(float delta)
	{
		if(Input.IsActionJustPressed("skip"))
			NextLevel();

		if (Input.IsActionPressed("quit"))
			GetTree().Quit();
	}

	public void NextLevel()
	{
		if(Level > 0)
			GetTree().Root.GetNode($"GameWorld/Level{Level}").QueueFree();

		if (Level == 25)
			Level = 0;
		
		Level++;
		
		CallDeferred("LoadLevel");
	}

	public async void LoadLevel()
	{
		var scene = GD.Load<PackedScene>($"res://Scenes/Maps/Level{Level}.tscn").Instance();
		
		// if a dialog node
		if (scene.FindNode("Control") != null)
		{
			_player.Visible = false;
			_hint.Visible = false;
			_levelname.Visible = false;
			Player.MovementDisabled = true;
			_lightFlicker.StartDialogLevel();
		}
		// if a level node
		else
		{
			var entrance = scene.GetNode<Node2D>("Entrance");
			var player = GetNode<Player>("Player");
			Player.MovementDisabled = true;
			player.Visible = true;
			player.ZeroVelocity();
			player.SetInitialPosition(entrance.Position);
			player.Position = player.GetSpawnPoint();
			player.PlayAnimation("idle");
			player.ZeroVelocity();
			Player.MovementDisabled = true;

			_hint.Visible = true;
			_levelname.Visible = true;
			await ToSignal(GetTree(), "idle_frame");
			_lightFlicker.StartNewLevel();
		}
		
		AddChild(scene);
	}
}