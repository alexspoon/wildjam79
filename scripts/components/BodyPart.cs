using Godot;

[GlobalClass]
public partial class BodyPart : RigidBody3D
{
    public MeshInstance3D BodyPartMesh;
    public CollisionShape3D BodyPartCollision;
    public int BodyPartValue = 0;
    
    public override void _Ready()
    {
        BodyPartMesh = GetNode<MeshInstance3D>("Mesh");
        BodyPartCollision = GetNode<CollisionShape3D>("Collision");
        
        if (BodyPartMesh == null) GD.PrintErr("Bodypart has no mesh");
        if (BodyPartCollision == null) GD.PrintErr("Bodypart has no collision");
    }
}
