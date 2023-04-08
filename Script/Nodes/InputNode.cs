using System;
using Godot;
using System.Linq;

public partial class InputNode : LogicNode
{
	private char _variableName = 'x';

	[Export]
	public char VariableName
	{
		get => _variableName;
		set
		{
			_variableName = value;
			if (NameLabel != null)
			{
				NameLabel.Text = Symbol;
			}
		}
	}

	public bool Value { get; set; } = false;

	public override string Formula => VariableName.ToString();

	public override string DisplayName => $"Variable";

	public override string Symbol => VariableName.ToString();

	public override void Simulate()
	{
		// there is no point in traversing upwards
		OutputConnector.Value = Value;
	}
}
