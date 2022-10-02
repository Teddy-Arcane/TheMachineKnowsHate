using Godot;
using System;

public class BlueLaser : RayCast2D
{
	private bool _isCasting;
	private Tween _tween;
	private Line2D _line;
	private Player _player;
	private LightFlicker _lightFlicker;
	private AudioPlayer _audio;
	
	public override void _Ready()
	{
		_tween = GetNode<Tween>("Tween");
		_line = GetNode<Line2D>("Line2D");
		_player = GetTree().Root.GetNode<Player>("GameWorld/Player");
		_lightFlicker = GetTree().Root.GetNode<LightFlicker>("GameWorld/LightFlicker");
		_audio = GetNode<AudioPlayer>("AudioPlayer");

		_lightFlicker.Connect("LightsToggled", this, "LightsToggled");

		SetPhysicsProcess(false);

		_line.Points[1] = Vector2.Zero;

		SetIsCasting(true);
	}

	public override void _PhysicsProcess(float delta)
	{
		var castPoint = CastTo;

		ForceRaycastUpdate();

		if (IsColliding())
		{
			var hit = GetCollider();
			var test = hit as KinematicBody2D;
			if (test != null)
				_player.LaserKill();

			castPoint = ToLocal(GetCollisionPoint());
		}

		_line.SetPointPosition(1, castPoint);

		Visible = true;
	}

	private void SetIsCasting(bool cast)
	{
		_isCasting = cast;

		if (_isCasting)
		{
			Appear();
		}
		else
		{
			Dissapear();
		}

		SetPhysicsProcess(_isCasting);
	}

	private void Appear()
	{
		_tween.StopAll();
		_tween.InterpolateProperty(_line, "width", 0f, 2f, 0.1f);
		_tween.Start();
	}

	private void Dissapear()
	{
		_tween.StopAll();
		_tween.InterpolateProperty(_line, "width", 2f, 0f, 1f);
		_tween.Start();
		_audio.Play("Off");
	}

	private void LightsToggled(bool on)
	{
		if (on)
		{
			CollisionMask = 1;
			Appear();

		}
		else
		{
			CollisionMask = 10;
			Dissapear();
		}

		Visible = on;
	}
}
