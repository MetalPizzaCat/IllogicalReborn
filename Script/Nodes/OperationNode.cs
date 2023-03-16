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

	public UInt32 CurrentValue => Inputs.FirstOrDefault()?.Connection?.ParentNode.Execute() ?? 0;

	public override UInt32 Execute()
	{
		//Not operation can only take one input
		if (Operation == OperationType.Not)
		{
			return (~CurrentValue) & (DataMask);
		}
		// Grab first node instead of picking default value because best default value depends on the operation
		UInt32 result = CurrentValue;
		foreach (Connector con in Inputs.Skip(1))
		{
			switch (Operation)
			{
				case OperationType.And:
					result &= con.Connection?.ParentNode.Execute() ?? 0;
					break;
				case OperationType.Or:
					result |= con.Connection?.ParentNode.Execute() ?? 0;
					break;
				case OperationType.Xor:
					result ^= con.Connection?.ParentNode.Execute() ?? 0;
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
