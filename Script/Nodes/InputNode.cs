using System;
using Godot;
using System.Linq;

public partial class InputNode : LogicNode
{
	[Export]
	public char VariableName { get; set; } = 'x';

	public bool Value { get; set; } = false;

	public override string Formula => VariableName.ToString();

	public override string DisplayName => $"Variable";

	public override void Simulate()
	{
		// there is no point in traversing upwards
		OutputConnector.Value = Value;
	}
}
