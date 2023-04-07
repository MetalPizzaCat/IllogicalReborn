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
			Visible = value != null;
			if (DisplayNameLabel != null)
			{
				DisplayNameLabel.Text = value?.DisplayName ?? string.Empty;
			}
			if (FormulaLabel != null)
			{
				FormulaLabel.Text = value?.Formula ?? string.Empty;
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



