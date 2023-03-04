#nullable enable
using Godot;
using System;

public partial class Connector : Node2D
{
	public delegate void SelectedEventHandler(Connector connector);

	public event SelectedEventHandler? OnSelected;

	public Connector? Destination { get; set; } = null;

	public LogicNode? ParentNode { get; set; } = null;

	private void Select()
	{
	   OnSelected?.Invoke(this);
	}
}
