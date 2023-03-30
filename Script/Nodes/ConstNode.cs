using System;
using Godot;
using System.Linq;

/// <summary>
/// A simple node that returns whatever value user is set it to return
/// </summary>
public partial class ConstNode : LogicNode
{
    private UInt32 _value = 0;

    [Export]
    public UInt32 Value
    {
        get => _value;
        set
        {
            _value = value;
            if (NameLabel != null)
            {
                NameLabel.Text = _value.ToString();
            }
        }
    }

    public override LogicNodeSaveData SaveData
    {
        get
        {
            LogicNodeSaveData data = base.SaveData;
            data.Value = Value == 0; // TODO: replace value type with bool to revert back to original idea
            return data;
        }
    }

    public override void Simulate()
    {
        // there is no point in traversing upwards
        OutputConnector.Value = 0;
    }

    public override void Load(LogicNodeSaveData data)
    {
        base.Load(data);
        Value = (data.Value ?? false) ? 1u : 0u;
    }
    // public override uint? Execute()
    // {
    // 	return 0;
    // }
}
