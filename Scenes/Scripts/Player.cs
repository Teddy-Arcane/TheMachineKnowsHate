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

	public static bool MovementDisabled = false;
	
	public override void _Ready()
	{
		_initialPosition = Position;
		_animator = GetNode<AnimatedSprite>("AnimatedSprite");
		_jumpBuffer = GetNode<Timer>("JumpBuffer");
		_coyoteTimer = GetNode<Timer>("CoyoteTimer");
		_pointLight = GetNode<Light2D>("Light2D");
		_audio = GetNode<AudioPlayer>("AudioPlayer");
	}
	
	public override void _PhysicsProcess(float delta)
	{
		if (_isDead || MovementDisabled) return;
		
		if (_velocity.y > 0)
			_lastVelocity = _velocity;
		
		_velocity.x = 0;

		HandleMovement();
		HandleJump();
		HandleGravity(delta);

		var wasOnFloor = IsOnFloor();
		_velocity = MoveAndSlide(_velocity, Vector2.Up);

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

	public void Kill()
	{
		_velocity = Vector2.Zero;;
		
		// start death timer
		_deathTimer = new Timer();;
		_deathTimer.WaitTime = 2;
		_deathTimer.OneShot = true;
		_deathTimer.Autostart = false;
		_isDead = true;
		AddChild(_deathTimer);
		_deathTimer.Connect("timeout", this, "Respawn");
		_deathTimer.Start();
	}

	private void Respawn()
	{
		Position = _initialPosition;
		_deathTimer.QueueFree();
		_isDead = false;
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
			_velocity.x += speed;
		}
		else if(Input.IsActionPressed("move_left"))
		{
			_velocity.x -= speed;
		}
		
		// if in air, exit
		if (!_grounded || !IsOnFloor())
		{
			_animator.Play("jump");
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
			GD.Print(_lastVelocity);
			if (_lastVelocity.y > _fallDamageThresholdVelocity)
			{
				_audio.Play("Crunch1");
				_animator.Play("fall");
				
				Kill();
			}
			else if (_lastVelocity.y > _gruntThreshold)
			{
				_audio.Play($"Landing{new Random().Next(1, 4)}");
			}
				
			_grounded = true;
			_hasJumped = false;
			
			//_audio.Play($"Landing{new Random().Next(1, 3)}");
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
		_audio.Play($"Jump{new Random().Next(1, 5)}");
		_velocity.y -= _jumpVelocity;
		_animator.Play("jump");
		//_ui.SetAction("Jumping");
		_hasJumped = true;
		_grounded = false;
	}
	
	private void HandleGravity(float delta)
	{
		_velocity.y += _gravity * delta;
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
}
