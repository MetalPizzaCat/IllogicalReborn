using System;
using Godot;
using System.Collections.Generic;

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
            int id = conn.Connection.ParentNode.Id;
            int subId = conn.Connection.Id;
            if (!Inputs.TryAdd(id, new List<int> { subId }))
            {
                Inputs[id].Add(subId);
            }
        }
    }

    public int Id { get; set; } = -1;
    public Vector2 Position { get; set; } = Vector2.Zero;

    public string ClassName { get; set; } = "Lumberfoot";

    public Dictionary<int, List<int>> Inputs { get; set; } = new();

    public override string ToString()
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(this);
    }
}