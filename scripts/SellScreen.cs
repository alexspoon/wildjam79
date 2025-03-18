using Godot;

public partial class SellScreen : StaticBody3D
{
    private SubViewport _guiViewport;
    private MeshInstance3D _screenMesh;
    private Camera3D _guiCamera;
    private Timer _cameraCooldown;
    private Player _player;
    private Camera3D _playerCamera;
    private RayCast3D _playerRay;
    public bool ScreenInUse;
    private Area3D _screenArea;
    private Label _fundsLabel;
    private GameManager _gameManager;
    private CollisionShape3D _screenCollisionShape;

    // Used for checking if the mouse is inside the Area3D.
    private bool _isMouseInside;

    // The last processed input touch/mouse event. Used to calculate relative movement.
    private Vector2 _lastEventPos2D;

    // The time of the last event in seconds since engine start.
    private double _lastEventTime = -1.0;
    
    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/GameManager");
        _guiViewport = GetNode<SubViewport>("GUIViewport");
        _screenMesh = GetNode<MeshInstance3D>("ScreenMesh");
        _screenArea = GetNode<Area3D>("ScreenArea");
        _fundsLabel = GetNode<Label>("GUIViewport/ScreenControl/FundsLabel");
        _guiCamera = GetNode<Camera3D>("GUICamera");
        _cameraCooldown = GetNode<Timer>("CameraCooldown");
        _player = GetNode<Player>("../Player");
        _playerCamera = _player.GetNode<Camera3D>("Head/Camera");
        _playerRay = _player.GetNode<RayCast3D>("Head/GrabRay");
        _gameManager.PartSold += UpdateFundsLabel;
        
        _screenArea.MouseEntered += _MouseEnteredArea;
        _screenArea.MouseExited += _MouseExitedArea;
        _screenArea.InputEvent += _MouseInputEvent;
    }
    
     private void _MouseEnteredArea()
    {
        _isMouseInside = true;
        // Notify the viewport that the mouse is now hovering it.
        _guiViewport.Notification((int)NotificationVpMouseEnter);
    }

    private void _MouseExitedArea()
    {
        // Notify the viewport that the mouse is no longer hovering it.
        _guiViewport.Notification((int)NotificationVpMouseExit);
        _isMouseInside = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // Check if the event is a non-mouse/non-touch event
        if (@event is InputEventMouseButton || @event is InputEventMouseMotion)
        {
            // If the event is a mouse/touch event, then we can ignore it here, because it will be
            // handled via Physics Picking.
            return;
        }
        _guiViewport.PushInput(@event);
    }

    private void _MouseInputEvent(Node camera, InputEvent @event, Vector3 eventPosition, Vector3 normal, long shapeIdx)
    {
        // Get mesh size to detect edges and make conversions. This code only supports PlaneMesh and QuadMesh.
        Vector2 quadMeshSize = ((QuadMesh)_screenMesh.Mesh).Size;

        // Event position in Area3D in world coordinate space.
        Vector3 eventPos3D = eventPosition;

        // Current time in seconds since engine start.
        double now = Time.GetTicksMsec() / 1000.0;

        // Convert position to a coordinate space relative to the Area3D node.
        // NOTE: `AffineInverse()` accounts for the Area3D node's scale, rotation, and position in the scene!
        eventPos3D = _screenMesh.GlobalTransform.AffineInverse() * eventPos3D;
        
        Vector2 eventPos2D = new Vector2();

        if (_isMouseInside)
        {
            // Convert the relative event position from 3D to 2D.
            eventPos2D = new Vector2(eventPos3D.X, -eventPos3D.Y);

            // Right now the event position's range is the following: (-quad_size/2) -> (quad_size/2)
            // We need to convert it into the following range: -0.5 -> 0.5
            eventPos2D.X = eventPos2D.X / quadMeshSize.X;
            eventPos2D.Y = eventPos2D.Y / quadMeshSize.Y;
            // Then we need to convert it into the following range: 0 -> 1
            eventPos2D.X += 0.5f;
            eventPos2D.Y += 0.5f;

            // Finally, we convert the position to the following range: 0 -> viewport.size
            eventPos2D.X *= _guiViewport.Size.X;
            eventPos2D.Y *= _guiViewport.Size.Y;
            // We need to do these conversions so the event's position is in the viewport's coordinate system.
        }
        else if (_lastEventPos2D != Vector2.Zero)
        {
            // Fall back to the last known event position.
            eventPos2D = _lastEventPos2D;
        }

        // Set the event's position and global position.
        var mEvent = @event as InputEventMouse;
        mEvent.Position = eventPos2D;
        if (@event is InputEventMouse mouseEvent)
        {
            mouseEvent.GlobalPosition = eventPos2D;
        }

        // Calculate the relative event distance.
        if (@event is InputEventMouseMotion motionEvent)
        {
            // If there is not a stored previous position, then we'll assume there is no relative motion.
            if (_lastEventPos2D == Vector2.Zero)
            {
                motionEvent.Relative = Vector2.Zero;
            }
            // If there is a stored previous position, then we'll calculate the relative position by subtracting
            // the previous position from the new position. This will give us the distance the event traveled from prev_pos.
            else
            {
                motionEvent.Relative = eventPos2D - _lastEventPos2D;
                motionEvent.Velocity = motionEvent.Relative / (float)(now - _lastEventTime);
            }
        }

        // Update _lastEventPos2D with the position we just calculated.
        _lastEventPos2D = eventPos2D;

        // Update _lastEventTime to current time.
        _lastEventTime = now;

        // Finally, send the processed input event to the viewport.
        _guiViewport.PushInput(@event);
    }

    
    public override void _PhysicsProcess(double delta)
    {
        if (!_guiCamera.Current && _playerRay.IsColliding() && _playerRay.GetCollider() == this &&
            Input.IsActionJustPressed("inputE"))
        {
            _cameraCooldown.Start();
            ScreenInUse = true;
            Input.MouseMode = Input.MouseModeEnum.Confined;
            _player.Active = false;
            _guiCamera.MakeCurrent();
        }
        
        if (_guiCamera.Current && _cameraCooldown.IsStopped() && Input.IsActionJustPressed("inputE"))
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
            ScreenInUse = false;
            _player.Active = true;
            _playerCamera.MakeCurrent();
        }
    }
    
    
    
    private void UpdateFundsLabel(int value)
    {
        _fundsLabel.Text = "\u20bd" + _gameManager.PlayerCurrentFunds;
    }
}
