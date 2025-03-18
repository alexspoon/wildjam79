using Godot;

public partial class SellBox : StaticBody3D
{
    private Area3D _sellArea;
    private Timer _sellTimer;
    private GameManager _gameManager;

    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/GameManager");
        _sellArea = GetNode<Area3D>("SellArea");
        _sellTimer = GetNode<Timer>("SellTimer");
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
                var value = bodyPart.BodyPartCurrentValue;
                _gameManager.EmitSignal(nameof(GameManager.PartSold), value);
                body.QueueFree();
            }
            else return;
        }
        
        
        
    }
}
