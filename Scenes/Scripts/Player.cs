using Godot;
using System;

public class Player : KinematicBody2D
{
	[Export] private float _speed = 200f;
	[Export] private float _jumpVelocity = 200f;
	[Export] private float _gravity = 200f;
	[Export] private float _sprintModifier = 1.5f;
	[Export] private float _idleLightRadius = 0.2f;
	[Export] private float _walkLightRadius = 0.4f;
	[Export] private float _sprintLightRadius = 1f;
	[Export] private float _fallDamageThresholdVelocity = 950f;
	[Export] private float _gruntThreshold = 500f;
	[Export] private float _downwardGravityModifier = 3f;
	[Export] private float _blinkDistance = 30f;

	private AnimatedSprite _animator;
	private bool _grounded = true;
	private Vector2 _initialPosition;
	private Vector2 _lastDirection;
	private Timer _jumpBuffer;
	private Timer _coyoteTimer;
	private Light2D _pointLight;
	private Vector2 _velocity = Vector2.Zero;
	private AudioPlayer _audio;
	private bool _hasJumped = false;
	private Vector2 _lastVelocity;
	private Timer _deathTimer;
	private bool _isDead = false;
	private CollisionShape2D _collider;
	private RayCast2D _blinkCastTop;
	private RayCast2D _blinkCastBottom;

	public static bool MovementDisabled = false;

	public static bool TerminalVelocity = false;
	
	[Signal] public delegate void PlayerRespawned();

	public override void _Ready()
	{
		_initialPosition = Position;
		_animator = GetNode<AnimatedSprite>("AnimatedSprite");
		_jumpBuffer = GetNode<Timer>("JumpBuffer");
		_coyoteTimer = GetNode<Timer>("CoyoteTimer");
		_pointLight = GetNode<Light2D>("Light2D");
		_audio = GetNode<AudioPlayer>("AudioPlayer");
		_deathTimer = GetNode<Timer>("DeathTimer");
		_collider = GetNode<CollisionShape2D>("CollisionShape2D");
		_blinkCastTop = GetNode<RayCast2D>("BlinkCastTop");
		_blinkCastBottom = GetNode<RayCast2D>("BlinkCastBottom");
	}

	public override void _PhysicsProcess(float delta)
	{
		if (_isDead || MovementDisabled)
		{
			_audio.Stop("Run");
			return;
		}

		CalculateFallDamage();
		HandleMovement();
		HandleJump();
		HandleGravity(delta);
		HandleDash();
		var wasOnFloor = IsOnFloor();
		_velocity = MoveAndSlide(_velocity, Vector2.Up);
		CheckForFallingPlatforms();
		CheckCoyoteAndBuffer(wasOnFloor);
		HandleDirection(_velocity);
		SetLightSize();
	}

	private void HandleDash()
	{
		if (Input.IsActionJustPressed("dash") && !IsOnFloor())
		{
			if (_velocity.x != 0)
			{
				var direction = _velocity.x > 0 ? _blinkDistance : _blinkDistance * -1;
				
				Position = IsDashColliding()
					? DashCollisionPoint()
					: new Vector2(Position.x + direction, Position.y);
				
				_audio.Play("DashTump");
				
				_velocity = Vector2.Zero;
			}
		}
	}

	private Vector2 DashCollisionPoint()
	{
		if (_blinkCastBottom.IsColliding())
			return _blinkCastBottom.GetCollisionPoint();
		else if (_blinkCastTop.IsColliding())
			return _blinkCastTop.GetCollisionPoint();
		else
			return Position;
	}

	private bool IsDashColliding()
	{
		return _blinkCastBottom.IsColliding() || _blinkCastTop.IsColliding();
	}

	private void HandleMovement()
	{
		var sprinting = Input.IsActionPressed("sprint");
		var speed = sprinting ? (_speed * _sprintModifier) : _speed;

		if (Input.IsActionPressed("move_right"))
		{
			if(_grounded && _jumpBuffer.IsStopped())
				_audio.ToggleRun(true);
			
			_velocity.x += speed;
		}
		else if (Input.IsActionPressed("move_left"))
		{
			if(_grounded && _jumpBuffer.IsStopped())
				_audio.ToggleRun(true);
			
			_velocity.x -= speed;
		}
		else
		{
			_audio.ToggleRun(false);
		}
		
		// if in air, exit
		if (!_grounded || !IsOnFloor())
		{
			_grounded = false;
			_animator.Play("jump");
			_audio.ToggleRun(false);
		}

		// trigger animation
		_animator.Play((_velocity.x == 0) ? "idle" : "run");
	}

	private void HandleJump()
	{
		if (Input.IsActionJustPressed("jump"))
		{
			if (IsOnFloor() || (!_hasJumped && !_coyoteTimer.IsStopped()))
			{
				DoJump();
			}
			else
			{
				_jumpBuffer.Start();
			}
		}
		else if (!_grounded && IsOnFloor())
		{
			if (TerminalVelocity)
			{
				_audio.Stop("Scream");
				_audio.Play("Crunch1");
				_audio.Play("Thump");
				_animator.Play("fall");

				Kill();
			}
			else if (_lastVelocity.y > _gruntThreshold)
			{
				_audio.Play($"Landing{new Random().Next(1, 4)}");
			}
			
			_audio.Play("Thump");

			_grounded = true;
			_hasJumped = false;
			
			GD.Print(_lastVelocity);
		}

		if (_velocity.y > 10f)
		{
			_grounded = false;
		}

		if (Input.IsActionJustReleased("jump") && _velocity.y < 0)
		{
			_velocity.y = 0;
		}
	}

	private void DoJump()
	{
		_audio.ToggleRun(false);
		_audio.Play($"Jump{new Random().Next(1, 5)}");

		_velocity.y -= _jumpVelocity;
		
		_animator.Play("jump");
		//_ui.SetAction("Jumping");
		_hasJumped = true;
		_grounded = false;
	}

	private void HandleGravity(float delta)
	{
		_velocity.y += (_gravity * delta) * (_velocity.y > 0 ? _downwardGravityModifier : 1);
	}

	private void HandleDirection(Vector2 velocity)
	{
		// if idle face last known direction
		if (velocity.x == 0f)
		{
			if (_lastDirection == Vector2.Left)
			{
				_animator.FlipH = true;
			}
			else if (_lastDirection == Vector2.Right)
			{
				_animator.FlipH = false;
			}
		}
		// if moving face running direction
		else
		{
			var movingLeft = Input.IsActionPressed("move_left");
			_animator.FlipH = movingLeft;
			_lastDirection = movingLeft ? Vector2.Left: Vector2.Right;

			_blinkCastBottom.CastTo = movingLeft
				? new Vector2(_blinkDistance * -1, 0f)
				: new Vector2(_blinkDistance, 0f);
			
			_blinkCastTop.CastTo = movingLeft
				? new Vector2(_blinkDistance * -1, 0f)
				: new Vector2(_blinkDistance, 0f);
		}
	}
	
	private async void Respawn()
	{
		EmitSignal("PlayerRespawned");
		
		_lastVelocity = Vector2.Zero;
		PlayAnimation("idle");
		Position = GetSpawnPoint();
		
		await ToSignal(GetTree(), "idle_frame");
		
		_collider.Disabled = false;
		_isDead = false;

		TerminalVelocity = false;
	}
	
	private void CalculateFallDamage()
	{
		if (_velocity.y > 0)
			_lastVelocity = _velocity;

		if (_lastVelocity.y > (_fallDamageThresholdVelocity))
		{
			TerminalVelocity = true;
			_audio.Play("Scream");
		}
		
		_velocity.x = 0;
	}

	private void CheckForFallingPlatforms()
	{
		for (int i = 0; i < GetSlideCount(); i++)
		{
			var collision = GetSlideCollision(i);
			var plat = collision.Collider as FallingPlatform;
			if (plat != null)
			{
				plat.Crack();
			}
		}
	}

	private void SetLightSize()
	{
		if (_velocity.x == 0)
		{
			_pointLight.TextureScale = _idleLightRadius;
		}
		else
		{
			if (Input.IsActionPressed("sprint"))
			{
				_pointLight.TextureScale = _sprintLightRadius;
			}
			else
			{
				_pointLight.TextureScale = _walkLightRadius;
			}
		}
	}

	private void CheckCoyoteAndBuffer(bool wasOnFloor)
	{
		if (wasOnFloor && !IsOnFloor())
		{
			_coyoteTimer.Start();
		}

		if (IsOnFloor() && !_jumpBuffer.IsStopped())
		{
			DoJump();
		}
	}
	
	public void SetInitialPosition(Vector2 initPosit)
	{
		_initialPosition = initPosit;
	}

	public void LaserKill()
	{
		_audio.Stop("Scream");
		_animator.Play("cinders");
		_audio.Play("Zap");
		
		Kill();
	}

	public void Kill()
	{
		var finalExit = GetTree().Root.GetNodeOrNull<Exit>("GameWorld/Level24/Exit");
		if (finalExit != null)
		{
			_collider.Disabled = true;
			_velocity = Vector2.Zero;

			MoveAndSlide(_velocity);

			_isDead = true;
		}
		else
		{
			ActuallyKill();
		}
	}

	public void StopScream()
	{
		_audio.Stop("Scream");
	}

	public void ActuallyKill()
	{
		_collider.Disabled = true;
		_velocity = Vector2.Zero;

		MoveAndSlide(_velocity);

		_isDead = true;
		_deathTimer.Start();
	}

	public void ZeroVelocity()
	{
		_lastVelocity = Vector2.Zero;
		_velocity = Vector2.Zero;
		_audio.ToggleRun(false);
		_audio.Stop("Run");
		_audio.Stop("Scream");
	}

	public void PlayAnimation(string name)
	{
		_animator.Play(name);
	}

	public Vector2 GetSpawnPoint()
	{
		return new Vector2(_initialPosition.x, _initialPosition.y + 25f);
	}
	
	private void _on_DeathTimer_timeout()
	{
		Respawn();
	}
}
