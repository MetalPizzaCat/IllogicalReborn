#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Connector : Node2D
{
	public delegate void SelectedEventHandler(Connector connector);
	public delegate void ConnectionRemovedEventHandler(Connector self, Connector other);
	public delegate void ConnectionSizeUpdatedEventHandler(Connector self, Connector other, bool compatible);

	public event SelectedEventHandler? OnSelected;
	public event ConnectionRemovedEventHandler? OnConnectionRemoved;
	public event ConnectionSizeUpdatedEventHandler? OnConnectionSizeUpdated;

	[Export]
	public bool IsOutput { get; set; } = false;

	[Export]
	public Label DataSizeLabel { get; set; }

	[Export]
	public Label DebugInfoLabel { get; set; }

	/// <summary>
	/// Id of the connector relative to the parent<para/>
	/// Connectors of output and input types have independent ids
	/// </summary>
	/// <value></value>
	[Export]
	public int Id { get; set; } = 0;

	private List<Connector> _connections = new List<Connector>();

	/// <summary>
	/// All connectors that this connector is connected to.
	/// </summary>
	public List<Connector> Connections => _connections;

	/// <summary>
	/// Get connector that is currently connected.<para/>
	/// Meant for Input type as Output can have multiple connections, so Connections list should be used instead.
	/// </summary>
	/// <value></value>
	public Connector? Connection => _connections.FirstOrDefault();

	public bool CanFitMoreConnections => IsOutput || _connections.Count == 0 || _connections[0] == null;
	public LogicNode? ParentNode { get; set; } = null;

	private bool? _value = null;
	/// <summary>
	/// Current value in this node
	/// </summary>
	/// <value></value>
	public bool? Value
	{
		// because inputs only have to propagate the value that they are connected to
		get => IsOutput ? _value : Connection?.Value;
		set
		{
			_value = value;
			DebugInfoLabel.Text = value?.ToString() ?? "null";
		}
	}

	private bool _hasIncompatibleConnection = false;

	private void Select()
	{
		OnSelected?.Invoke(this);
	}

	/// <summary>
	/// Add connection to other connector, if output new connection will be added<para/>
	/// If input connection at _connections[0] (aka Connection property) will be overridden
	/// </summary>
	/// <param name="other"></param>
	public void ConnectTo(Connector other)
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

	/// <summary>
	/// Attempt to traverse the node tree to check if given connector is present there
	/// </summary>
	/// <param name="source">Connector that is being connected from</param>
	/// <returns></returns>
	private bool CanCreateCyclicConnection(Connector source)
	{
		// if we don't have an output we can't really create a loop can we?
		if(ParentNode.OutputConnector == null)
		{
			return false;
		}
		foreach (Connector conn in ParentNode.OutputConnector.Connections)
		{
			if (conn.ParentNode != null)
			{
				if (source.ParentNode == conn.ParentNode || conn.ParentNode.OutputConnector == null || conn.ParentNode.OutputConnector.CanCreateCyclicConnection(source))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CanConnect(Connector connector)
	{
		if (connector == this  // Can not connect to itself
				|| connector.IsOutput == IsOutput // Can not connect to same type of connector
				|| ParentNode == null // Must have a parent
				|| connector.ParentNode == ParentNode   // Can't connect to another connector on the same node
				|| !CanFitMoreConnections)
		{
			return false;
		}
		// if connecting node is of output type we need to traverse the tree to check 
		// if we are connecting output to an input up the tree which would create cyclic connection
		// but if it's an input there is no need to check
		return connector.IsOutput ? !CanCreateCyclicConnection(connector) : true;
	}

	/// <summary>
	/// Remove connection to other node
	/// </summary>
	/// <param name="connector"></param>
	public void DisconnectFrom(Connector connector)
	{
		_connections.Remove(connector);
		OnConnectionRemoved?.Invoke(this, connector);
	}

	public void ClearConnections()
	{
		foreach (Connector conn in _connections)
		{
			OnConnectionRemoved?.Invoke(this, conn);
		}
		_connections.Clear();
	}
}
