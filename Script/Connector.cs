#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Connector : Node2D
{
    public delegate void SelectedEventHandler(Connector connector);

    public event SelectedEventHandler? OnSelected;

    [Export]
    public bool IsOutput { get; set; } = false;

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

    public bool CanConnect(Connector connector)
    {
        return connector != this &&                                     // Can not connect to itself
                connector.IsOutput != IsOutput &&                       // Can not connect to same type of connector
                  ParentNode != null &&                                 // Must have a parent
                  connector.ParentNode != ParentNode &&                 // Can't connect to another connector on the same node
                CanFitMoreConnections;

    }

	/// <summary>
	/// Remove connection to other node
	/// </summary>
	/// <param name="connector"></param>
    public void DisconnectFrom(Connector connector)
    {
        _connections.Remove(connector);
    }
}
