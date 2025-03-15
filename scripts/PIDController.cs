using Godot;

public partial class PIDController : Node
{
    //PID variables
    [Export] public float ProportionalGain = 1;
    [Export] public float IntegralGain;
    [Export] public float DerivativeGain;
    public float OutputMin = -1000;
    public float OutputMax = 1000;
    public float IntegralSaturationX;
    public float IntegralSaturationY;
    public float IntegralSaturationZ;
    public float IntegrationStoredX;
    public float IntegrationStoredY;
    public float IntegrationStoredZ;
    public float ErrorLastX;
    public float ErrorLastY;
    public float ErrorLastZ;
    public float ValueLastX;
    public float ValueLastY;
    public float ValueLastZ;
    
    public float UpdatePIDX(float currentValue, float targetValue, float deltaTime)
    {
        //Calculate error value
        float error = targetValue - currentValue;

        //Calculate proportional term
        float p = ProportionalGain * error;

        //Calculate integral term
        IntegrationStoredX = Mathf.Clamp(IntegrationStoredX + (error * deltaTime), -IntegralSaturationX, IntegralSaturationX);
        float i = IntegralGain * IntegrationStoredX;

        //Calculate the change rate of error
        float errorRateOfChange = (error - ErrorLastX) / deltaTime;
        ErrorLastX = error;

        //Calculate the change rate of value
        float valueRateOfChange = (currentValue - ValueLastX) / deltaTime;
        ValueLastX = currentValue;

        //Calculate derivative term
        float d = DerivativeGain * valueRateOfChange;

        //Calculate result
        float resultX = p + i + d;

        //Return result
        return resultX;
    }
    public float UpdatePIDY(float currentValue, float targetValue, float deltaTime)
    {
        //Calculate error value
        float error = targetValue - currentValue;

        //Calculate proportional term
        float p = ProportionalGain * error;

        //Calculate integral term
        IntegrationStoredY = Mathf.Clamp(IntegrationStoredY + (error * deltaTime), -IntegralSaturationY, IntegralSaturationY);
        float i = IntegralGain * IntegrationStoredY;

        //Calculate the change rate of error
        float errorRateOfChange = (error - ErrorLastY) / deltaTime;
        ErrorLastY = error;

        //Calculate the change rate of value
        float valueRateOfChange = (currentValue - ValueLastY) / deltaTime;
        ValueLastY = currentValue;

        //Calculate derivative term
        float d = DerivativeGain * valueRateOfChange;

        //Calculate result
        float resultY = p + i + d;

        //Return result
        return resultY;
    }
    public float UpdatePIDZ(float currentValue, float targetValue, float deltaTime)
    {
        //Calculate error value
        float error = targetValue - currentValue;

        //Calculate proportional term
        float p = ProportionalGain * error;

        //Calculate integral term
        IntegrationStoredZ = Mathf.Clamp(IntegrationStoredZ + (error * deltaTime), -IntegralSaturationZ, IntegralSaturationZ);
        float i = IntegralGain * IntegrationStoredZ;

        //Calculate the change rate of error
        float errorRateOfChange = (error - ErrorLastZ) / deltaTime;
        ErrorLastZ = error;

        //Calculate the change rate of value
        float valueRateOfChange = (currentValue - ValueLastZ) / deltaTime;
        ValueLastZ = currentValue;

        //Calculate derivative term
        float d = DerivativeGain * valueRateOfChange;

        //Calculate result
        float resultZ = p + i + d;

        //Return result
        return resultZ;
    }
}
