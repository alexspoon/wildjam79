using System;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class BodyPart : RigidBody3D
{
    //Node references
    public MeshInstance3D BodyPartMesh;
    public GpuParticles3D BloodParticles;
    public CollisionShape3D BodyPartCollision;
    private Label3D _typeLabel;
    private Label3D _valueLabel;
    
    //Generic variables
    public bool Grabbed;
    
    //Growth variables
    public bool Growing;
    public bool FullyGrown;
    public double GrowthPercentage;
    public float GrowthRate;
    
    //Value variables
    [Export] public float BodyPartMaxValue;
    [Export] public float BodyPartMinValue;
    public float BodyPartFinalValue;
    public float BodyPartCurrentValue;
    private float _typeMult;
    
    //Type variables
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
    public BodyPartTypes BodyPartType;
    public Array<BodyPartTypes> AvailableParts;
    public Dictionary BodyPartTypePools = new Dictionary()
    {
        {"level1", new Array<BodyPartTypes> {BodyPartTypes.Finger, BodyPartTypes.Toe, BodyPartTypes.Ear}},
        {"level2", new Array<BodyPartTypes> {BodyPartTypes.Arm, BodyPartTypes.Leg, BodyPartTypes.Spleen, BodyPartTypes.Stomach, BodyPartTypes.Bladder}},
        {"level3", new Array<BodyPartTypes> {BodyPartTypes.Eye, BodyPartTypes.Kidney, BodyPartTypes.Liver}},
        {"level4", new Array<BodyPartTypes> {BodyPartTypes.Torso, BodyPartTypes.Heart, BodyPartTypes.Lung}},
        {"level5", new Array<BodyPartTypes> {BodyPartTypes.Brain}}
    };
    
    //Quality variables
    public enum BodyPartQualities
    {
        Shoddy,
        Mediocre,
        Decent,
        Great,
        Amazing,
        Perfected
    }
    public BodyPartQualities BodyPartQuality;
    public Array<BodyPartQualities> AvailableQualities;
    public Dictionary BodyPartQualityPools = new Dictionary()
    {
        { "level1", new Array<BodyPartQualities> {BodyPartQualities.Shoddy, BodyPartQualities.Mediocre}},
        { "level2", new Array<BodyPartQualities> {BodyPartQualities.Decent}},
        { "level3", new Array<BodyPartQualities> {BodyPartQualities.Great}},
        { "level4", new Array<BodyPartQualities> {BodyPartQualities.Amazing}},
        { "level5", new Array<BodyPartQualities> {BodyPartQualities.Perfected}}
    };
    
    public override void _Ready()
    {
        BodyPartMesh = GetNode<MeshInstance3D>("Mesh");
        BodyPartCollision = GetNode<CollisionShape3D>("Collision");
        BloodParticles =  GetNode<GpuParticles3D>("BloodParticles");
        _typeLabel = GetNode<Label3D>("TypeLabel");
        _valueLabel = GetNode<Label3D>("ValueLabel");
        
        if (BodyPartMesh == null) GD.PrintErr("Bodypart has no mesh");
        if (BodyPartCollision == null) GD.PrintErr("Bodypart has no collision");

        BodyPartType = CalculateBodyPartType();
        _typeMult = CalculateTypeMult(BodyPartType);
        BodyPartQuality = CalculateBodyPartQuality();
        BodyPartMinValue = CalculateMinValue(BodyPartQuality);
        BodyPartMaxValue = CalculateMaxValue(BodyPartQuality);
        BodyPartFinalValue = CalculateFinalValue();
        GrowthRate = 20;
    }
    public override void _PhysicsProcess(double delta)
    {
        if (Grabbed)
        {
            _typeLabel.Text = BodyPartQuality + " " + BodyPartType;
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
            case BodyPartTypes.Finger: return 0.5f;
            case BodyPartTypes.Toe : return 0.5f;
            case BodyPartTypes.Ear : return 0.6f;
            case BodyPartTypes.Arm : return 0.8f;
            case BodyPartTypes.Leg : return 0.8f;
            case BodyPartTypes.Spleen: return 1f;
            case BodyPartTypes.Stomach : return 1f;
            case BodyPartTypes.Bladder: return 1f;
            case BodyPartTypes.Kidney : return 1.2f;
            case BodyPartTypes.Liver : return 1.3f;
            case BodyPartTypes.Eye: return 1.5f;
            case BodyPartTypes.Torso : return 1.5f;
            case BodyPartTypes.Lung : return 1.6f;
            case BodyPartTypes.Heart: return 1.8f;
            case BodyPartTypes.Brain : return 2f;
        }
        return 0;
    }
    private float CalculateMinValue(BodyPartQualities partQuality)
    {
        partQuality = BodyPartQuality;
        switch (partQuality)
        {
            case BodyPartQualities.Shoddy: return 250f;
            case BodyPartQualities.Mediocre: return 500f;
            case BodyPartQualities.Decent: return 750f;
            case BodyPartQualities.Great: return 1000f;
            case BodyPartQualities.Amazing: return 1250f;
            case BodyPartQualities.Perfected: return 2000f;
        }
        return 0;
    }
    private float CalculateMaxValue(BodyPartQualities partQuality)
    {
        partQuality = BodyPartQuality;
        switch (partQuality)
        {
            case BodyPartQualities.Shoddy: return 500f;
            case BodyPartQualities.Mediocre: return 750f;
            case BodyPartQualities.Decent: return 1000f;
            case BodyPartQualities.Great: return 1250f;
            case BodyPartQualities.Amazing: return 1500f;
            case BodyPartQualities.Perfected: return 3000f;
        }
        return 0;
        
    }
    private float CalculateFinalValue()
    {
        var rng = new RandomNumberGenerator();
        BodyPartFinalValue = rng.RandfRange(BodyPartMinValue, BodyPartMaxValue);
        
        BodyPartFinalValue = Mathf.Round(BodyPartFinalValue * _typeMult);
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
    private BodyPartTypes CalculateBodyPartType()
    {
        var level1 = (Array<BodyPartTypes>)BodyPartTypePools["level1"];
        var level2 = (Array<BodyPartTypes>)BodyPartTypePools["level2"];
        var level3 = (Array<BodyPartTypes>)BodyPartTypePools["level3"];
        var level4 = (Array<BodyPartTypes>)BodyPartTypePools["level4"];
        var level5 = (Array<BodyPartTypes>)BodyPartTypePools["level5"];
        AvailableParts = level1 + level2;

        var rnd = new Random();
        var index = rnd.Next(AvailableParts.Count);
        var partType = AvailableParts[index];
        return partType;
    }
    private BodyPartQualities CalculateBodyPartQuality()
    {
        var level1 = (Array<BodyPartQualities>)BodyPartQualityPools["level1"];
        var level2 = (Array<BodyPartQualities>)BodyPartQualityPools["level2"];
        var level3 = (Array<BodyPartQualities>)BodyPartQualityPools["level3"];
        var level4 = (Array<BodyPartQualities>)BodyPartQualityPools["level4"];
        var level5 = (Array<BodyPartQualities>)BodyPartQualityPools["level5"];
        AvailableQualities = level1 + level2 + level3 + level4 + level5;
        
        var rnd = new Random();
        var index = rnd.Next(AvailableQualities.Count);
        var partQuality = AvailableQualities[index];
        return partQuality;
    }
}
