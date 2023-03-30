using System;
using Godot;
using System.Linq;

public partial class DisplayNode : LogicNode
{
    [Export]
    public Label DisplayLabel { get; set; }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Display();
    }

    public void Display()
    {
        UInt32? data = Inputs.FirstOrDefault()?.Value;
        DisplayLabel.Text = data?.ToString() ?? "Invalid";
    }

    public override void Simulate()
    {
        base.Simulate();
        OutputConnector.Value = Inputs.FirstOrDefault()?.Value;
    }

    public override void _Ready()
    {
        base._Ready();
    }
}
