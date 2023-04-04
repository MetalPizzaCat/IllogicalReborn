#nullable enable
using Godot;
using System;

public partial class NodeContextMenu : Control
{

	[Export]
	public Label? DisplayNameLabel { get; set; }

	private LogicNode? _currentNode = null;

	[Export]
	public Label? FormulaLabel { get; set; } = null;

	[Export]
	public OptionButton? VariableNameOptionButton { get; set; } = null;

	[Export]
	public Control? VariableNameControlNode { get; set; } = null;

	public LogicNode? CurrentNode
	{
		get => _currentNode;
		set
		{
			_currentNode = value;
			if (FormulaLabel != null && value != null)
			{
				FormulaLabel.Text = value.Formula;
			}
			if (VariableNameControlNode != null && VariableNameOptionButton != null && value is InputNode input)
			{
				VariableNameControlNode.Visible = VariableNameControlNode != null;
				VariableNameOptionButton.Selected = input.VariableName - 'a';
			}
		}
	}

	public override void _Ready()
	{
		base._Ready();
		if (VariableNameOptionButton != null)
		{
			for (char name = 'a'; name <= 'z'; name++)
			{
				VariableNameOptionButton.AddItem(name.ToString());
			}
		}
	}

	private void VariableNameSelected(long index)
	{
		if (_currentNode is InputNode input)
		{
			input.VariableName = ((char)((char)index + 'a'));
		}
	}
}



