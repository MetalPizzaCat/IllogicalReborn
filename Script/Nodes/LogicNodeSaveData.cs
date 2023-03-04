using System;
using Godot;
using System.Collections.Generic;

public class LogicNodeSaveData
{
    public LogicNodeSaveData(int id, Vector2 position, string className, List<int> inputs)
    {
        Id = id;
        Position = position;
        ClassName = className;
        Inputs = inputs;
    }

    public int Id { get; set; } = -1;
    public Vector2 Position { get; set; } = Vector2.Zero;

    public string ClassName { get; set; } = "Lumberfoot";

    public List<int> Inputs { get; set; } = new List<int>();

    public override string ToString()
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(this);
    }
}