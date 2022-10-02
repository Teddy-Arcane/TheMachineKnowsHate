using Godot;
using System;

public class MovingPlatform : Node2D
{
	[Export] private float _idleDuration = 1f;
	[Export] private Vector2 _moveTo = Vector2.Right * 192;
	[Export] private float _speed = 1f;
	
	private Tween _moveTween;
	private KinematicBody2D _platform;

	public override void _Ready()
	{
		_moveTween = GetNode<Tween>("MoveTween");
		_platform = GetNode<KinematicBody2D>("Platform");

		InitTween();
	}

	private void InitTween()
	{
		var duation = _moveTo.Length() / (_speed * 64f);
		_moveTween.InterpolateProperty(_platform, "position", Vector2.Zero, _moveTo, duation, Tween.TransitionType.Linear, Tween.EaseType.InOut, _idleDuration);
		_moveTween.InterpolateProperty(_platform, "position", _moveTo, Vector2.Zero, duation, Tween.TransitionType.Linear, Tween.EaseType.InOut, duation + _idleDuration * 2);
		_moveTween.Start();
	}
}
