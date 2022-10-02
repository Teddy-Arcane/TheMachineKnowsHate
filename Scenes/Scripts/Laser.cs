using Godot;
using System;

public class Laser : RayCast2D
{
	private bool _isCasting;
	private Tween _tween;
	private Line2D _line;
	private Player _player;

	public override void _Ready()
	{
		_tween = GetNode<Tween>("Tween");
		_line = GetNode<Line2D>("Line2D");
		_player = GetTree().Root.GetNode<Player>("GameWorld/Player");
		
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
		_tween.InterpolateProperty(_line, "width", 10f, 0f, 0.1f);
		_tween.Start();
	}
}
