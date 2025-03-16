using Godot;

[GlobalClass]
public partial class BodyPart : RigidBody3D
{
    public MeshInstance3D BodyPartMesh;
    public CollisionShape3D BodyPartCollision;
    [Export] public float MaxBodyPartValue;
    [Export] public float MinBodyPartValue;
    [Export] public float  GrowthRate;
    public float BodyPartFinalValue;
    public float BodyPartCurrentValue;
    public bool Growing;
    public double GrowthPercentage;
    
    public override void _Ready()
    {
        BodyPartMesh = GetNode<MeshInstance3D>("Mesh");
        BodyPartCollision = GetNode<CollisionShape3D>("Collision");
        
        if (BodyPartMesh == null) GD.PrintErr("Bodypart has no mesh");
        if (BodyPartCollision == null) GD.PrintErr("Bodypart has no collision");

        BodyPartFinalValue = CalculateFinalValue();
        GD.Print("Final value: " + BodyPartFinalValue);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!Growing) return;
        
        BodyPartCurrentValue = CalculateCurrentValue(BodyPartCurrentValue, BodyPartFinalValue, GrowthRate, GrowthPercentage);
    }
    
    private float CalculateFinalValue()
    {
        var rng = new RandomNumberGenerator();
        BodyPartFinalValue = rng.RandfRange(MinBodyPartValue, MaxBodyPartValue);
        BodyPartFinalValue = Mathf.Round(BodyPartFinalValue);
        return BodyPartFinalValue;
    }
    
    private float CalculateCurrentValue(float currentValue, float finalValue, float growthRate, double growthPercentage)
    {
        currentValue = Mathf.Min(currentValue, finalValue);
        growthPercentage = Mathf.InverseLerp(0, finalValue, currentValue) * 100;

        currentValue += growthRate * 1;
        
        GrowthPercentage = growthPercentage;
        currentValue = Mathf.Round(currentValue);
        if (currentValue > finalValue) currentValue = finalValue;
        return currentValue;
    }
}
