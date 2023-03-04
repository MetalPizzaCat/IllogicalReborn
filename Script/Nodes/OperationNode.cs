using System;
using Godot;
using System.Linq;

public partial class OperationNode : LogicNode
{
	public enum OperationType
	{
		And,
		Or,
		Not,
		Xor,
	}

	[Export]
	public OperationType Operation { get; set; } = OperationNode.OperationType.And;

	public override bool Execute()
	{
		//Not operation can only take one input
		if (Operation == OperationType.Not)
		{
			return !(Inputs.FirstOrDefault()?.Connection?.ParentNode.Execute() ?? false);
		}
		// Grab first node instead of picking default value because best default value depends on the operation
		bool result = Inputs.FirstOrDefault()?.Connection?.ParentNode.Execute() ?? false;
		foreach (Connector con in Inputs.Skip(1))
		{
			switch (Operation)
			{
				case OperationType.And:
					result &= con.Connection?.ParentNode.Execute() ?? false;
					break;
				case OperationType.Or:
					result |= con.Connection?.ParentNode.Execute() ?? false;
					break;
				case OperationType.Xor:
					result ^= con.Connection?.ParentNode.Execute() ?? false;
					break;
			}

		}
		return result;
	}

	public override void _Ready()
	{
		base._Ready();
	}
}
