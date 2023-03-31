using System;
using Godot;
using System.Linq;

public partial class OutputNode : LogicNode
{
    public override string Formula => InputNodes.FirstOrDefault()?.Formula ?? "INVALID!";
}
