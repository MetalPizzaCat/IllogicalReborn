#nullable enable
using Godot;
using System;
using System.Collections.Generic;

public partial class MainScene : Node
{
	[Export]
	public Camera2D? CurrentCamera { get; set; } = null;

	[Export]
	public Line2D? ConnectionLinePreview { get; set; } = null;


	[Export]
	public PackedScene? ConnectorPrefab { get; set; } = null;

	[Export]
	public float ZoomStep { get; set; } = 0.25f;

	[Export]
	public Node2D? NodeSpawnLocation { get; set; }

	[ExportCategory("Nodes")]
	[Export]
	public PackedScene? OperationNodePrefab { get; set; } = null;

	[Export]
	public PackedScene? OperationNotNodePrefab { get; set; } = null;

	[Export]
	public PackedScene? DisplayNodePrefab { get; set; } = null;


	public List<LogicNode> LogicComponents { get; set; } = new List<LogicNode>();

	public List<Connector> Connectors { get; set; } = new List<Connector>();

	private Connector? _currentlySelectedConnector = null;

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		if (CurrentCamera != null)
		{
			if (Input.IsActionJustPressed("zoom_in"))
			{
				CurrentCamera.Zoom += new Vector2(ZoomStep, ZoomStep);
			}
			if (Input.IsActionJustPressed("zoom_out"))
			{
				CurrentCamera.Zoom -= new Vector2(ZoomStep, ZoomStep);
			}
		}
		if (Input.IsActionJustPressed("Cancel"))
		{
			CancelConnection();
		}
		if (@event is InputEventMouseMotion motionEvent)
		{
			ConnectionLinePreview.SetPointPosition(1, motionEvent.GlobalPosition);
		}
	}

	public override void _Ready()
	{
		base._Ready();
		ConnectionLinePreview.Points = new Vector2[2];
		ConnectionLinePreview.Visible = false;
	}

	private void CancelConnection()
	{
		_currentlySelectedConnector = null;
		ConnectionLinePreview.Visible = false;
	}

	private void SelectConnector(Connector connector)
	{
		if (_currentlySelectedConnector == null)
		{
			_currentlySelectedConnector = connector;
			ConnectionLinePreview.Visible = true;
			ConnectionLinePreview.SetPointPosition(0, connector.GlobalPosition);
		}
		else
		{
			if (ConnectorPrefab == null || !connector.CanConnect(_currentlySelectedConnector))
			{
				GD.PrintErr("Can't connect two nodes");
				return;
			}
			ConnectionWire wire = ConnectorPrefab.Instantiate<ConnectionWire>();
			AddChild(wire);
			wire.Source = connector.IsOutput ? _currentlySelectedConnector : connector;
			wire.Destination = !connector.IsOutput ? _currentlySelectedConnector : connector;
			_currentlySelectedConnector.Connect(connector);
			connector.Connect(_currentlySelectedConnector);
			CancelConnection();
		}

	}

	private T? AddLogicNode<T>(PackedScene? prefab) where T : LogicNode
	{
		if (prefab == null)
		{
			return null;
		}
		T node = prefab.Instantiate<T>();
		AddChild(node);
		LogicComponents.Add(node);
		node.OnConnectorSelected += SelectConnector;
		node.Position = NodeSpawnLocation?.Position ?? new Vector2(50, 50);
		return node;
	}

	private void AddLogicNodeAnd()
	{
		AddLogicNode<OperationNode>(OperationNodePrefab);
	}

	private void AddDisplayNode()
	{
		AddLogicNode<DisplayNode>(DisplayNodePrefab);
	}

	private void AddLogicNodeNot()
	{
		AddLogicNode<OperationNode>(OperationNotNodePrefab);
	}
}
