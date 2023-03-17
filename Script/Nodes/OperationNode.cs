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
    public UInt32? CurrentValue => Inputs.FirstOrDefault()?.Connection?.ParentNode.Execute();

    public override string DisplayName => $"{Operation.ToString()} gate";

    public override UInt32? Execute()
    {
        //Not operation can only take one input
        if (Operation == OperationType.Not)
        {
            return CurrentValue == null ? null : (~CurrentValue) & (DataMask);
        }
        // Grab first node instead of picking default value because best default value depends on the operation
        UInt32? result = CurrentValue;
        if (result == null)
        {
            return null;
        }
        foreach (Connector con in Inputs.Skip(1))
        {
            UInt32? connValue = con.Connection?.ParentNode.Execute();
            // Option or Err from rust would come in really handy right about now
            if (connValue == null || con.Connection?.ParentNode.DataSize != DataSize)
            {
                // if ANY node returns null we abort whole chain as it means there is an error
                return null;
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
        return result;
    }

    public override void _Ready()
    {
        base._Ready();
    }
}
