using System;
using Godot;
using System.Collections.Generic;
using System.Linq;

public class SaveData
{
    public int CurrentId { get; set; } = 0;
    public Dictionary<string, List<LogicNodeSaveData>> Nodes { get; set; } = new();

    public override string ToString()
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(this);
    }
}