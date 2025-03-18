using Godot;

public partial class GameManager : Node3D
{
    [Signal] public delegate void PartSoldEventHandler(int value);
    
    public float PlayerCurrentFunds;

    public override void _Ready()
    {
        PartSold += OnPartSold;
    }

    private void OnPartSold(int value)
    {
        PlayerCurrentFunds += value;
        GD.Print(PlayerCurrentFunds);
    }
}
