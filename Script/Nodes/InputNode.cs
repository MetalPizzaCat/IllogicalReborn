using System;
using Godot;
using System.Linq;

public partial class InputNode : LogicNode
{
    [Export]
    public string VariableName { get; set; } = "x";

    public override string Formula => VariableName;
}
