using System;
using Godot;
using System.Collections.Generic;

public class LogicOperationNodeSaveData : LogicNodeSaveData
{
    public LogicOperationNodeSaveData(LogicNode node) : base(node)
    {
        if (node is OperationNode operation)
        {
            Operation = operation.Operation;
        }
    }

    public OperationNode.OperationType Operation { get; set; }
}