using System;
using Godot;
using Godot.Collections;

public partial class BodyPartVat : StaticBody3D
{
    [Export] private bool _enabled;
    private bool _inUse;
    private Area3D _inputArea;
    private Marker3D _outputMarker;
    private Marker3D _partMarker;
    private Player _player;
    private RayCast3D _playerRay;
    private BodyPart _bodyPart;
    private Array<BodyPart> _overlappingParts = [];
    private Vector3 _outputPosition;
    private Vector3 _partPosition;
    private Label3D _valueLabel;
    private Label3D _growthLabel;
    
    public override void _Ready()
    {
        _inputArea = GetNode<Area3D>("InputArea");
        _outputMarker = GetNode<Marker3D>("OutputMesh/OutputMarker");
        _partMarker = GetNode<Marker3D>("PartMarker");
        _player = GetNode<Player>("../Player");
        _playerRay = _player.GetNode<RayCast3D>("Head/GrabRay");
        _valueLabel = GetNode<Label3D>("ValueLabel");
        _growthLabel = GetNode<Label3D>("GrowthLabel");
        _outputPosition =  _outputMarker.GlobalPosition;
        _partPosition = _partMarker.GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_enabled)
        {
            HandleInput();
            HandleBodyPart();
        }
    }

    private void HandleInput()
    {
        if (_inUse) return;
        
        var inputOverlap = _inputArea.GetOverlappingBodies();

        if (inputOverlap.Count == 0) return;
        
        foreach (var body in inputOverlap)
        {
            if (body is BodyPart)
            {
                _overlappingParts.Add(body as BodyPart);
            }
            else return;
        }
        
        _bodyPart = _overlappingParts[0];
        _overlappingParts.Clear();
    }

    private void HandleBodyPart()
    {
        if (_bodyPart == null) return;

        _inUse = true;
        _bodyPart.FreezeMode = RigidBody3D.FreezeModeEnum.Kinematic;
        _bodyPart.Freeze = true;
        _bodyPart.GlobalPosition = _partPosition;
        _bodyPart.RotateX(0.0025f);
        _bodyPart.RotateY(0.0025f);
        _bodyPart.RotateZ(0.0025f);
        _bodyPart.Growing = true;
        _valueLabel.Text = "\u20bd" + _bodyPart.BodyPartCurrentValue;
        _growthLabel.Text = Mathf.Round(_bodyPart.GrowthPercentage) + "%";

        if (_playerRay.IsColliding() && _playerRay.GetCollider() == this && Input.IsActionJustPressed("inputE"))
        {
            _bodyPart.Hide();
            _valueLabel.Text = null;
            _growthLabel.Text = null;
            _bodyPart.Growing = false;
            OutputPart();
        }
}

    private void OutputPart()
    {
        _bodyPart.GlobalPosition = _outputPosition;
        _bodyPart.Freeze = false;
        _bodyPart.Show();
        _bodyPart.ApplyCentralImpulse(new Vector3(0,0, -5));
        _bodyPart = null;
        _inUse = false;
    }
}
