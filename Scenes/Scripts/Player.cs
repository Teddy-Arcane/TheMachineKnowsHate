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
	}

	public override void _PhysicsProcess(float delta)
	{
		if (_isDead || MovementDisabled)
		{
			_audio.Stop("Run");
			return;
		}
		
		if (_velocity.y > 0)
			_lastVelocity = _velocity;

		if (_lastVelocity.y > (_fallDamageThresholdVelocity))
		{
			TerminalVelocity = true;
			_audio.Play("Scream");
		}
		
		_velocity.x = 0;

		HandleMovement();
		HandleJump();
		HandleGravity(delta);

		var wasOnFloor = IsOnFloor();
		_velocity = MoveAndSlide(_velocity, Vector2.Up);

		for (int i = 0; i < GetSlideCount(); i++)
		{
			var collision = GetSlideCollision(i);
			var plat = collision.Collider as FallingPlatform;
			if (plat != null)
			{
				plat.Crack();
			}
		}

		if (wasOnFloor && !IsOnFloor())
		{
			_coyoteTimer.Start();
		}

		if (IsOnFloor() && !_jumpBuffer.IsStopped())
		{
			DoJump();
		}

		HandleDirection(_velocity);

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

	// public void TriggerDialog(string path)
	// {
	// 	//_ui.StartDialog(path);
	// }

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
		if (velocity == Vector2.Zero)
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
			_animator.FlipH = velocity.x < 0;
			_lastDirection = velocity;
		}
	}

	private void _on_DeathTimer_timeout()
	{
		Respawn();
	}
}
