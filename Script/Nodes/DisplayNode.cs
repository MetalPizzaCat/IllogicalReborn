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
		DisplayLabel.Text = Execute().ToString();
	}

	public override void _Ready()
	{
		base._Ready();
	}

	public override bool Execute()
	{
		return Inputs.FirstOrDefault()?.Connection?.ParentNode.Execute() ?? false;
	}
}
