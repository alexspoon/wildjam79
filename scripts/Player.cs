using Godot;
using System;

public partial class Player : CharacterBody3D
{
    //Player signals
    [Signal] public delegate void PlayerDeathEventHandler();
    [Signal] public delegate void PlayerSpawnEventHandler();
    [Signal] public delegate void PlayerDamageEventHandler(int damage);
    [Signal] public delegate void PlayerHealEventHandler(int healing);
    
    //Reference variables
    private MeshInstance3D _mesh;
    private CollisionShape3D _collision;
    private Camera3D _camera;
    private Node3D _head;
    private RayCast3D _grabRay;
    private Marker3D _grabMarker;
    
    //Movement variables
    [Export] private float _speed;
    [Export] private float _jumpForce;
    [Export] private float sprintMultiplier;
    private float _initialSpeed;
    private float _sprintSpeed;
    private float _currentSpeed;
    private Basis _basis;
    
    //Look variables
    [Export] private float _mouseSensitivity;
    
    //State variables
    private bool _alive;
    private bool _walking;
    private bool _sprinting;
    private bool _jumping;
    private bool _falling;

    //Health variables
    [Export] private int _maxHealth;
    [Export] private int _damageResistance;
    [Export] private int _antiHeal;
    private int _currentHealth;
    
    //Grab variables
    [Export] private float _grabRange;
    private Vector3 _defaultGrabMarkerPosition;
    private int _grabbedCount;
    private PIDController _pid;
    private RigidBody3D _grabbedBody;
    
    //Called when node first enters scene tree
    public override void _Ready()
    {
        //Connect player signals to functions
        PlayerDamage += OnPlayerDamage;
        PlayerHeal += OnPlayerHeal;
        
        //Capture mouse input
        Input.MouseMode = Input.MouseModeEnum.Captured;
        
        //Get references to nodes
        _mesh = GetNode<MeshInstance3D>("Mesh");
        _collision = GetNode<CollisionShape3D>("Collision");
        _head = GetNode<Node3D>("Head");
        _camera = GetNode<Camera3D>("Head/Camera");
        _grabRay = GetNode<RayCast3D>("Head/GrabRay");
        _grabMarker = GetNode<Marker3D>("Head/GrabMarker");
        _pid = GetNode<PIDController>("PIDController");
        
        //Initialize player
        PlayerInitialize();
    }
    
    //Process unhandled input
    public override void _UnhandledInput(InputEvent @event)
    {
        //Handle player head rotation with mouse motion
        if (@event is InputEventMouseMotion)
        {
            var motion = (InputEventMouseMotion)@event;

            //Rotate player body on Y axis with mouse relative X motion
            RotateY(-motion.Relative.X * _mouseSensitivity * 0.01f);
            //Rotate player head on X axis with mouse relative Y motion
            _head.RotateX(-motion.Relative.Y * _mouseSensitivity * 0.01f);
            //Clamp head rotation so that it does not allow for 360
            _head.Rotation = new Vector3((float)Mathf.Clamp(_head.Rotation.X, -Math.PI / 2, Math.PI / 2), _head.Rotation.Y, _head.Rotation.Z); 
        }
    }
    
    //Called every frame
    public override void _Process(double delta)
    {
    }

    //Called every physics tick
    public override void _PhysicsProcess(double delta)
    {
        //If player is dead, queue for deletion and stop calling functions
        if (!_alive)
        {
            EmitSignalPlayerDeath();
            QueueFree();
            return;
        }
        
        //Call functions while player is alive
        UpdateStates();
        PlayerMovement(delta);
        PlayerGrab();
        HandleGrabbedBody(delta);
        PlayerInventory();
    }
    
    //Initialize player properties
    private void PlayerInitialize()
    {
        EmitSignalPlayerSpawn();
        _alive = true;
        _currentHealth = _maxHealth;
        _initialSpeed = _speed;
        _sprintSpeed = _initialSpeed * sprintMultiplier;
        _defaultGrabMarkerPosition =  _grabMarker.Position;
        _grabRay.TargetPosition = new Vector3(0,0, -_grabRange);
    }
    
    //Handles player movement
    private void PlayerMovement(double delta)
    {
        //Update class variable basis to equal the current basis
        _basis = GlobalTransform.Basis;
        //Create a vector with WASD input for movement direction
        var inputVector2 = Input.GetVector("inputA", "inputD", "inputW", "inputS");
        //Convert input vector2 into vector3 for 3D movement using Y value from the vector2 as the Z value
        var inputVector3 = new Vector3(inputVector2.X, 0, inputVector2.Y).Normalized();
        //Align player input vector to treat basis forward direction as forward movement direction
        inputVector3 = _basis *  inputVector3;
        //If sprinting, increase speed
        if (_sprinting)
        {
            _speed = _sprintSpeed;
        } else _speed = _initialSpeed;
        //Create a target velocity using the input direction, and multiply it by speed value
        var targetVelocity = inputVector3 * _speed * (float)delta;
        //If player is in air, apply gravity
        if (!IsOnFloor())
        {
            targetVelocity.Y -= 9.82f;
        }
        //If jump input is pressed, apply jump force
        if (Input.IsActionJustPressed("inputSpace"))
        {
            targetVelocity.Y = _jumpForce;
        }
        //Set current velocity to target velocity
        Velocity = targetVelocity;
        //Apply movement
        MoveAndSlide();
    }

    //Handles player inventory
    private void PlayerInventory()
    {
        
    }
    
    //Updates player states
    private void UpdateStates()
    {
        //Alive state
        if (_currentHealth <= 0)
            _alive = false;
        
        //Calculate walking state
        if (Velocity.Length() > 0)
            _walking = true;
        else _walking = false;
        
        //Calculate jumping state
        if (Velocity.Y > 0)
            _jumping = true;
        else _jumping = false;
        
        //Falling state
        if (Velocity.Y < 0)
            _falling = true;
        else _falling = false;
        
        //Sprinting state
        if (Input.IsActionPressed("inputShift"))
            _sprinting = true;
        else _sprinting = false;
    }

    //Handle player grab
    private void PlayerGrab()
    {
        if (Input.IsActionJustPressed("inputE") && _grabbedCount > 0)
        {
            if (_grabbedBody is BodyPart)
            {
                var grabbedPart = _grabbedBody as BodyPart;
                grabbedPart.Grabbed = false;
            }
            _grabbedBody = null;
            _grabbedCount = 0;
            _grabMarker.Position = _defaultGrabMarkerPosition;
            return;
        }
        
        if (Input.IsActionJustPressed("inputE") && _grabbedCount == 0)
        {
            if (!_grabRay.IsColliding())
                return;

            if (_grabRay.GetCollider() is RigidBody3D)
            {
                _grabbedBody = _grabRay.GetCollider() as RigidBody3D;
                _grabbedCount = 1;
                _pid.ValueLastX = _grabbedBody.Position.X;
                _pid.ValueLastY = _grabbedBody.Position.Y;
                _pid.ValueLastZ = _grabbedBody.Position.Z;
                GD.Print("grabbed");
            }
        }
    }

    private void HandleGrabbedBody(double delta)
    {
        if (!IsInstanceValid(_grabbedBody))
        {
            _grabbedCount = 0;
            if (_grabbedBody is BodyPart)
            {
                var grabbedPart = _grabbedBody as BodyPart;
                grabbedPart.Grabbed = false;
            }
            _grabbedBody = null;
            return;
        }

        if (_grabbedBody.Freeze)
        {
            _grabbedCount = 0;
            if (_grabbedBody is BodyPart)
            {
                var grabbedPart = _grabbedBody as BodyPart;
                grabbedPart.Grabbed = false;
            }
            _grabbedBody = null;
            return;
        }
        
        if (_grabbedBody == null)
            return;

        if (_grabbedCount == 0)
            return;
        
        if (Input.IsActionJustPressed("inputScrollUp"))
        {
            var markerPos = _grabMarker.Position;
            markerPos.Z -= 0.5f;
            _grabMarker.Position = markerPos;
        }
        if (Input.IsActionJustPressed("inputScrollDown"))
        {
            var markerPos = _grabMarker.Position;
            markerPos.Z += 0.5f;
            if (markerPos.Z > -1f)
                markerPos.Z = -1f;
            _grabMarker.Position = markerPos;
        }

        if (Input.IsActionJustPressed("inputMiddleMouse"))
        {
            _grabMarker.Position = _defaultGrabMarkerPosition;
        }
        
        var objPos = _grabbedBody.GlobalPosition;
        var targetPos = _grabMarker.GlobalPosition;
        var targetMove = new Vector3(_pid.UpdatePIDX(objPos.X, targetPos.X, (float)delta), _pid.UpdatePIDY(objPos.Y, targetPos.Y, (float)delta), _pid.UpdatePIDZ(objPos.Z, targetPos.Z, (float)delta));
        
        var objVel = _grabbedBody.LinearVelocity;
        objVel = targetMove;
        _grabbedBody.LinearVelocity = objVel;
        _grabbedBody.LookAt(_head.GlobalPosition);

        if (_grabbedBody is BodyPart)
        {
            var grabbedPart = _grabbedBody as BodyPart;
            grabbedPart.Grabbed = true;
        }
    }
    
    //Handle player damage
    private void OnPlayerDamage(int damage)
    {
        damage -= _damageResistance;
        _currentHealth -= damage;
    }

    //Handle player healing
    private void OnPlayerHeal(int healing)
    {
        healing -= _antiHeal;
        _currentHealth += healing;
    }
}
