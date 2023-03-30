using System;
using Godot;
using System.Collections.Generic;

/// <summary>
/// Class for storing all data related to the nodes <para/>
/// Used instead of using json convert on node object itself due to how scenes have to be instantiated in godot</para>
/// This also stores all of the info for all of the node types, due to there being a finite and small number of types
/// </summary>
public class LogicNodeSaveData
{
    public LogicNodeSaveData(LogicNode node)
    {
        Id = node.Id;
        Position = node.GlobalPosition;
        ClassName = node.GetType().ToString();
        foreach (Connector conn in node.Inputs)
        {
            if (conn.Connection == null)
            {
                continue;
            }
            if (!Inputs.TryAdd(conn.Connection.ParentNode.Id, new List<int> { conn.Connection.Id }))
            {
                Inputs[conn.Connection.ParentNode.Id].Add(conn.Connection.Id);
            }
        }
    }

    public LogicNodeSaveData() { }

    public LogicNodeSaveData(int id, Vector2 position, string className, Dictionary<int, List<int>> inputs, bool? value = null, OperationNode.OperationType? operationType = null)
    {
        Id = id;
        Position = position;
        ClassName = className;
        Value = value;
        OperationType = operationType;
        Inputs = inputs;
    }

    public int Id { get; set; } = -1;
    public Vector2 Position { get; set; } = Vector2.Zero;

    public string ClassName { get; set; } = "Lumberfoot";

    /// <summary>
    /// Default value of the node <para/>
    /// Intended to store values for const node
    /// </summary>
    public bool? Value { get; set; } = null;

    /// <summary>
    /// Operation type of the operation node
    /// </summary>
    public OperationNode.OperationType? OperationType { get; set; } = null;

    public Dictionary<int, List<int>> Inputs { get; set; } = new();

    public override string ToString()
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(this);
    }
}