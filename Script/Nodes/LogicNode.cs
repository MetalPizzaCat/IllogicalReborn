#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class LogicNode : Node2D
{
	public delegate void ConnectorSelectedEventHandler(Connector connector);
	public delegate void NodeSelectedEventHandler(LogicNode node);
	public delegate void NodeDeselectedEventHandler(LogicNode node);
	public delegate void DeleteNodeEventHandler(LogicNode node);

	public delegate void ConnectionSizeUpdatedEventHandler(Connector self, Connector other, bool compatible);

	// Events can be nullable because they are assigned from code
	public event ConnectorSelectedEventHandler? OnConnectorSelected;
	public event NodeSelectedEventHandler? OnNodeSelected;

	public event NodeDeselectedEventHandler? OnNodeDeselected;
	public event DeleteNodeEventHandler? OnNodeDeleted;

	public event ConnectionSizeUpdatedEventHandler? OnConnectionSizeUpdated;


	[Export]
	public PackedScene? ConnectorPrefab { get; set; } = null;

	[Export]
	public Node2D? InputNodeParent { get; set; } = null;

	[Export]
	public Node2D? OutputNodeParent { get; set; } = null;

	[Export]
	public PopupMenu? ContextMenu { get; set; } = null;

	[Export]
	public Label? DebugInfoLabel { get; set; } = null;

	[Export]
	public Label? NameLabel { get; set; } = null;

	[Export]
	public float GridSize = 16;
	[Export]
	public int InputSize { get; set; } = 2;
	[Export]
	public int OutputSize { get; set; } = 1;

	private int _dataSize = 1;
	public UInt32 DataMask { get; set; } = 1;

	[Export]
	public int DataSize
	{
		get => _dataSize;
		set
		{
			_dataSize = value;
			DataMask = 0;
			for (int i = 0; i < value; i++)
			{
				DataMask |= (1u << i);
			}
			NotifyConnectorsAboutSizeChange();
		}
	}

	public virtual string DisplayName => "Invalid node";

	public bool IsGrabbed { get; set; } = false;

	public List<Connector> Inputs { get; set; } = new List<Connector>();

	public List<Connector> Outputs { get; set; } = new List<Connector>();

	public Connector? OutputConnector => Outputs.FirstOrDefault();

	public int Id { get; set; } = -1;

	protected bool Simulated = false;

	public List<LogicNode> InputNodes
	{
		get
		{
			List<LogicNode> nodes = new List<LogicNode>();
			foreach (Connector conn in Inputs)
			{
				if (conn.Connection?.ParentNode != null)
				{
					nodes.Add(conn.Connection?.ParentNode);
				}
			}
			return nodes;
		}
	}

	public List<LogicNode> OutputNodes
	{
		get
		{
			List<LogicNode> nodes = new List<LogicNode>();
			foreach (Connector conn in Outputs)
			{
				if (conn.Connection?.ParentNode != null)
				{
					nodes.Add(conn.Connection?.ParentNode);
				}
			}
			return nodes;
		}
	}

	public override void _Ready()
	{
		if (ConnectorPrefab == null || InputNodeParent == null)
		{
			return;
		}
		for (int i = 0; i < OutputSize; i++)
		{
			Connector con = ConnectorPrefab.Instantiate<Connector>();
			OutputNodeParent.AddChild(con);
			Outputs.Add(con);
			con.IsOutput = true;
			con.Position = new Vector2(0, i * 48);
			con.OnSelected += (Connector connector) => OnConnectorSelected?.Invoke(connector);
			con.OnConnectionSizeUpdated += (Connector source, Connector destination, bool compatible) => OnConnectionSizeUpdated?.Invoke(source, destination, compatible);
			con.ParentNode = this;
		}

		for (int i = 0; i < InputSize; i++)
		{
			Connector con = ConnectorPrefab.Instantiate<Connector>();
			InputNodeParent.AddChild(con);
			Inputs.Add(con);
			con.Position = new Vector2(0, i * 48);
			con.OnSelected += (Connector connector) => OnConnectorSelected?.Invoke(connector);
			con.OnConnectionSizeUpdated += (Connector source, Connector destination, bool compatible) => OnConnectionSizeUpdated?.Invoke(source, destination, compatible);
			con.ParentNode = this;
		}
	}

	/// <summary>
	/// Makes every connector object that is child to this node check if data sizes are valid and raise events accordingly
	/// </summary>
	public void NotifyConnectorsAboutSizeChange()
	{
		GD.Print($"{DisplayName} changed size to {DataSize}");
		foreach (Connector connector in Inputs)
		{
			connector.NotifyConnectedNodesAboutSizeChange();
		}
		OutputConnector?.NotifyConnectedNodesAboutSizeChange();
	}

	/// <summary>
	/// Set new location for the node while respecting grid size preferences
	/// </summary>
	/// <param name="location"></param>
	public void MoveTo(Vector2 location)
	{
		Position = location.Snapped(new Vector2(GridSize, GridSize));
	}

	/// <summary>
	/// Simulate this node and write the results into connectors <para/>
	/// If any of the input nodes are not simulated node will request it to be simulated
	/// </summary>
	public virtual void Simulate()
	{
		if (Simulated)
		{
			return;
		}
		foreach (LogicNode node in InputNodes)
		{
			node.Simulate();
		}
	}

	private void Grab()
	{
		IsGrabbed = true;
		OnNodeSelected?.Invoke(this);
	}

	private void Release()
	{
		IsGrabbed = false;
		OnNodeDeselected?.Invoke(this);
	}

	private void OnButtonGuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mousePress && mousePress.Pressed)
		{
			switch (mousePress.ButtonIndex)
			{
				case MouseButton.Right:
					//ContextMenu?.Show();
					ContextMenu.Popup();
					ContextMenu.Position = new Vector2I((int)mousePress.GlobalPosition.X, ((int)mousePress.GlobalPosition.Y));
					break;
			}
		}
	}

	private void OnContextMenuPressed(long index)
	{
		if (index == 0)
		{
			foreach (Connector input in Inputs)
			{
				input.Connection?.DisconnectFrom(input);
				input.DisconnectFrom(input.Connection);
			}
			foreach (Connector output in OutputConnector.Connections)
			{
				output.DisconnectFrom(OutputConnector);
			}
			OutputConnector.ClearConnections();
			OnNodeDeleted?.Invoke(this);
			QueueFree();
		}
	}
}
