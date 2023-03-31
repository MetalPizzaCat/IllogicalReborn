using System;
using Godot;
using System.Linq;

/// <summary>
/// A simple node that returns whatever value user is set it to return
/// </summary>
public partial class ConstNode : LogicNode
{
	private bool _value = false;

	[Export]
	public bool Value
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

	public override string Formula => _value ? "1" : "0";

	public override LogicNodeSaveData SaveData
	{
		get
		{
			LogicNodeSaveData data = base.SaveData;
			data.Value = Value; 
			return data;
		}
	}

	public override void Simulate()
	{
		// there is no point in traversing upwards
		OutputConnector.Value = Value;
	}

	public override void Load(LogicNodeSaveData data)
	{
		base.Load(data);
		Value = (data.Value ?? false);
	}
}
