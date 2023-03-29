using System;
using Godot;
using System.Collections.Generic;

public class SaveData
{
    public int CurrentId { get; set; } = 0;
    public List<LogicNodeSaveData> OperationNodes { get; set; } = new();

    public List<LogicNodeSaveData> ConstNodes { get; set; } = new();

    public List<LogicNodeSaveData> GenericNodes { get; set; } = new();

    public override string ToString()
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(this);
    }
}