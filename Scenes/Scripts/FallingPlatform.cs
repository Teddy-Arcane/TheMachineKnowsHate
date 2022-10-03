using Godot;
using System;

public class FallingPlatform : KinematicBody2D
{
	private AudioPlayer _audioPlayer;
	private Timer _fallTimer;
	private AnimatedSprite _animated;
	private CollisionShape2D _collider;
	private bool _cracked = false;

	public override void _Ready()
	{
		_audioPlayer = GetNode<AudioPlayer>("AudioPlayer");
		_fallTimer = GetNode<Timer>("FallTimer");
		_animated = GetNode<AnimatedSprite>("AnimatedSprite");
		_collider = GetNode<CollisionShape2D>("CollisionShape2D");
	}

	public void Crack()
	{
		if (!_cracked)
		{
			_audioPlayer.Play("Crack");
			_animated.Play("crack");
			_fallTimer.Start();
			_cracked = true;
		}
	}

	public void Respawn()
	{
		_collider.Disabled = false;
		_animated.Play("normal");
		Visible = true;
	}

	private void _on_FallTimer_timeout()
	{
		_audioPlayer.Play("Break");
		_collider.Disabled = true;
		Visible = false;
	}
}