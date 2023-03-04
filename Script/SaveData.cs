using System;
using Godot;
using System.Collections.Generic;

public class SaveData
{
    public int CurrentId { get; set; } = 0;
    public List<LogicNodeSaveData> Nodes { get; set; } = new List<LogicNodeSaveData>();

    public override string ToString()
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(this);
    }
}