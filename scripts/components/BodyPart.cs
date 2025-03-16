using System;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class BodyPart : RigidBody3D
{
    public MeshInstance3D BodyPartMesh;
    public CollisionShape3D BodyPartCollision;
    [Export] public float MaxBodyPartValue;
    [Export] public float MinBodyPartValue;
    public float GrowthRate;
    public bool Grabbed;
    public enum BodyPartTypes
    {
        Leg,
        Arm,
        Finger,
        Toe,
        Torso,
        Heart,
        Lung,
        Kidney,
        Liver,
        Spleen,
        Bladder,
        Brain,
        Stomach,
        Eye,
        Ear
    }

    public Dictionary BodyPartPools = new Dictionary()
    {
        {"level1", new Array<BodyPartTypes> {BodyPartTypes.Finger, BodyPartTypes.Toe, BodyPartTypes.Ear}},
        {"level2", new Array<BodyPartTypes> {BodyPartTypes.Arm, BodyPartTypes.Leg, BodyPartTypes.Spleen, BodyPartTypes.Stomach, BodyPartTypes.Bladder}},
        {"level3", new Array<BodyPartTypes> {BodyPartTypes.Eye, BodyPartTypes.Kidney, BodyPartTypes.Liver}},
        {"level4", new Array<BodyPartTypes> {BodyPartTypes.Torso, BodyPartTypes.Heart, BodyPartTypes.Lung}},
        {"level5", new Array<BodyPartTypes> {BodyPartTypes.Brain}}
    };

    public enum BodyPartQualities
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythical
    }
    public BodyPartTypes BodyPartType;
    public Array<BodyPartTypes> AvailableParts;
    public BodyPartQualities BodyPartQuality;
    public float BodyPartFinalValue;
    public float BodyPartCurrentValue;
    public bool Growing;
    public bool FullyGrown;
    public double GrowthPercentage;
    private float _typeMult;
    private Label3D _typeLabel;
    private Label3D _valueLabel;
    
    public override void _Ready()
    {
        BodyPartMesh = GetNode<MeshInstance3D>("Mesh");
        BodyPartCollision = GetNode<CollisionShape3D>("Collision");
        _typeLabel = GetNode<Label3D>("TypeLabel");
        _valueLabel = GetNode<Label3D>("ValueLabel");
        
        if (BodyPartMesh == null) GD.PrintErr("Bodypart has no mesh");
        if (BodyPartCollision == null) GD.PrintErr("Bodypart has no collision");

        BodyPartType = CalculateBodyPartType();
        _typeMult = CalculateTypeMult(BodyPartType);
        BodyPartFinalValue = CalculateFinalValue();
        GrowthRate = 5;
    }
    public override void _PhysicsProcess(double delta)
    {
        if (Grabbed)
        {
            _typeLabel.Text = BodyPartType.ToString();
            _valueLabel.Text = "\u20bd" + BodyPartCurrentValue;
        }
        if (!Grabbed)
        {
            _typeLabel.Text = null;
            _valueLabel.Text = null;
        }
        
        if (!Growing) return;
        BodyPartCurrentValue = CalculateCurrentValue(BodyPartCurrentValue, BodyPartFinalValue, GrowthRate, GrowthPercentage);
    }

    private float CalculateTypeMult(BodyPartTypes partType)
    {
        partType = BodyPartType;
        switch (partType)
        {
            case BodyPartTypes.Finger: return 0.8f;
            case BodyPartTypes.Toe : return 0.8f;
            case BodyPartTypes.Ear : return 0.8f;
            case BodyPartTypes.Arm : return 1f;
            case BodyPartTypes.Leg : return 1f;
            case BodyPartTypes.Spleen: return 1.2f;
            case BodyPartTypes.Stomach : return 1.2f;
            case BodyPartTypes.Bladder: return 1.2f;
            case BodyPartTypes.Kidney : return 1.3f;
            case BodyPartTypes.Liver : return 1.3f;
            case BodyPartTypes.Eye: return 1.4f;
            case BodyPartTypes.Torso : return 1.5f;
            case BodyPartTypes.Lung : return 1.6f;
            case BodyPartTypes.Heart: return 1.8f;
            case BodyPartTypes.Brain : return 2f;
        }
        return 0;
    }
    
    private float CalculateFinalValue()
    {
        var rng = new RandomNumberGenerator();
        BodyPartFinalValue = rng.RandfRange(MinBodyPartValue, MaxBodyPartValue);
        
        BodyPartFinalValue = Mathf.Round(BodyPartFinalValue);
        return BodyPartFinalValue * _typeMult;
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
    private BodyPartTypes CalculateBodyPartType()
    {
        var level1 = (Array<BodyPartTypes>)BodyPartPools["level1"];
        var level2 = (Array<BodyPartTypes>)BodyPartPools["level2"];
        var level3 = (Array<BodyPartTypes>)BodyPartPools["level3"];
        var level4 = (Array<BodyPartTypes>)BodyPartPools["level4"];
        var level5 = (Array<BodyPartTypes>)BodyPartPools["level5"];
        AvailableParts = level5;

        var rnd = new Random();
        int index = rnd.Next(AvailableParts.Count);
        BodyPartTypes partType = AvailableParts[index];
        return partType;
    }
}
