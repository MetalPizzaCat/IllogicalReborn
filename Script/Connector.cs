#nullable enable
using Godot;
using System;
using System.Collections.Generic;

public partial class Connector : Node2D
{
	public delegate void SelectedEventHandler(Connector connector);

	public event SelectedEventHandler? OnSelected;

	[Export]
	public bool IsOutput { get; set; } = false;

	private List<Connector> _connections = new List<Connector>();

	public bool CanFitMoreConnections => IsOutput || _connections.Count == 0 || _connections[0] == null;
	public LogicNode? ParentNode { get; set; } = null;

	private void Select()
	{
		OnSelected?.Invoke(this);
	}

	public void Connect(Connector other)
	{
		//Output can have as many connections as needed
		if (IsOutput)
		{
			if (!_connections.Contains(other))
			{
				_connections.Add(other);
			}
		}
		else
		{
			if (_connections.Count == 0)
			{
				_connections.Add(other);
			}
			else
			{
				_connections[0] = other;
			}
		}
	}

	public bool CanConnect(Connector connector)
	{
		return connector != this &&                                         // Can not connect to itself
				connector.IsOutput != IsOutput &&                       // Can not connect to same type of connector
				  ParentNode != null &&                                 // Must have a parent
				  connector.ParentNode != ParentNode &&                 // Can't connect to another connector on the same node
				CanFitMoreConnections;

	}
}
