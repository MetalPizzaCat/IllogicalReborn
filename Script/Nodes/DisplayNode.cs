using System;
using Godot;
using System.Linq;

public partial class DisplayNode : LogicNode
{
	[Export]
	public Label DisplayLabel { get; set; }

	public override void _Process(double delta)
	{
		base._Process(delta);
		UInt32? data = Execute();
		DisplayLabel.Text = data?.ToString() ?? "Invalid";
	}

	public override void _Ready()
	{
		base._Ready();
	}

	public override UInt32? Execute()
	{
		return Inputs.FirstOrDefault()?.Connection?.ParentNode.Execute();
	}
}
