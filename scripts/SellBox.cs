using Godot;
using Godot.Collections;

public partial class SellBox : StaticBody3D
{
    private Area3D _sellArea;
    private Timer _sellTimer;
    private Label3D _moneyLabel;
    private int _currentMoney;

    public override void _Ready()
    {
        _sellArea = GetNode<Area3D>("SellArea");
        _sellTimer = GetNode<Timer>("SellTimer");
        _moneyLabel = GetNode<Label3D>("Desk/SellScreen/MoneyLabel");
        _moneyLabel.Text = "\u20bd" + 0;
        _sellArea.BodyEntered += HandleSelling;
    }
    
    private void HandleSelling(Node3D i)
    {
        var sellOverlap = _sellArea.GetOverlappingBodies();

        if (sellOverlap.Count == 0) return;
        
        foreach (var body in sellOverlap)
        {
            if (body is BodyPart)
            {
                var bodyPart = body as BodyPart;
                _currentMoney += bodyPart.BodyPartValue;
                _moneyLabel.Text = "\u20bd"  + _currentMoney;
                body.QueueFree();
            }
            else return;
        }
        
        
        
    }
}
