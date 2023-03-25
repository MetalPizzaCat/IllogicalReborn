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
	public override void Simulate()
	{
		// there is no point in traversing upwards
		OutputConnector.Value = 0;
	}
	// public override uint? Execute()
	// {
	// 	return 0;
	// }
}
