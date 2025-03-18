using Godot;


public partial class Main : Node3D
{
    private SubViewport _screenViewport;
    private SellScreen _sellScreen;

    public override void _Ready()
    {
        _sellScreen = GetNode<SellScreen>("SellScreen");
        _screenViewport = _sellScreen.GetNode<SubViewport>("GUIViewport");
    }
}
