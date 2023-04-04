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

	private OperationType _type = OperationType.And;

	[Export]
	public OperationType Operation
	{
		get => _type;
		set
		{
			_type = value;
			if (NameLabel != null)
			{
				NameLabel.Text = Operation.ToString();
			}
		}
	}

	public override LogicNodeSaveData SaveData
	{
		get
		{
			LogicNodeSaveData data = base.SaveData;
			data.OperationType = _type;
			return data;
		}
	}

	public bool? CurrentValue => Inputs.FirstOrDefault()?.Value;

	public override string DisplayName => $"{Operation.ToString()} gate";

	public override string Formula
	{
		get
		{
			string operation = "∧";
			switch (Operation)
			{
				case OperationType.Or:
					operation = "∨";
					break;
				case OperationType.Xor:
					operation = "⊕";
					break;
				case OperationType.Not:
					operation = "¬";
					return $"({operation} {InputNodes.ElementAtOrDefault(0)?.Formula ?? "INVALID!"})";
			}
			return $"({InputNodes.ElementAtOrDefault(0)?.Formula ?? "INVALID!"}) {operation} ({InputNodes.ElementAtOrDefault(1)?.Formula ?? "INVALID!"})";
		}
	}

	public override void Simulate()
	{
		base.Simulate();
		//Not operation can only take one input
		if (Operation == OperationType.Not)
		{
			// Operation node can only have one output, which is the result
			OutputConnector.Value = (CurrentValue == null ? null : (!CurrentValue));
			return;
		}
		// Grab first node instead of picking default value because best default value depends on the operation
		bool? result = CurrentValue;
		if (result == null)
		{
			OutputConnector.Value = null;
			return;
		}
		foreach (Connector con in Inputs.Skip(1))
		{
			bool? connValue = con.Connection?.Value;
			// Option or Err from rust would come in really handy right about now
			if (connValue == null)
			{
				// if ANY node returns null we abort whole chain as it means there is an error
				OutputConnector.Value = null;
				return;
			}
			switch (Operation)
			{
				case OperationType.And:
					result &= connValue.Value;
					break;
				case OperationType.Or:
					result |= connValue.Value;
					break;
				case OperationType.Xor:
					result ^= connValue.Value;
					break;
			}
		}
		OutputConnector.Value = result;
	}

	public override void _Ready()
	{
		base._Ready();
	}

	public override void Load(LogicNodeSaveData data)
	{
		base.Load(data);
		Operation = data.OperationType ?? OperationType.And;
	}
}
