#nullable enable
using Godot;
using System;

public partial class NodeContextMenu : Control
{

	[Export]
	public Label? DisplayNameLabel { get; set; }

	private LogicNode? _currentNode = null;

	[Export]
	public Label? FormulaLabel {get;set;} = null;

	public LogicNode? CurrentNode
	{
		get => _currentNode;
		set
		{
			_currentNode = value;
			if(FormulaLabel != null && value != null)
			{
				FormulaLabel.Text = value.Formula;
			}
		}
	}
}
