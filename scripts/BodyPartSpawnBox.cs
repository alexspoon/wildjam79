using Godot;

public partial class BodyPartSpawnBox : StaticBody3D
{
    private Player _player;
    private RayCast3D _playerRay;
    private Marker3D _partMarker;
    [Export] private PackedScene _partScene;
    
    public override void _Ready()
    {
        _player = GetNode<Player>("../Player");
        _playerRay = _player.GetNode<RayCast3D>("Head/GrabRay");
        _partMarker = GetNode<Marker3D>("PartMarker");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_playerRay.IsColliding() && _playerRay.GetCollider() == this && Input.IsActionJustPressed("inputE"))
        {
            var partToSpawn = _partScene.Instantiate() as BodyPart;
            AddSibling(partToSpawn);
            partToSpawn.GlobalPosition = _partMarker.GlobalPosition;
            partToSpawn.ApplyImpulse(new Vector3(0, 10, 5));
            
        }
    }
}
