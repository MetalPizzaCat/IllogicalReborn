#nullable enable
using Godot;
using System;

public partial class NodeContextMenu : Control
{

	[Export]
	public Label? DisplayNameLabel { get; set; }

	private LogicNode? _currentNode = null;

	public LogicNode? CurrentNode
	{
		get => _currentNode;
		set
		{
			_currentNode = null;
			_currentNode = value;
		}
	}
}
