#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public partial class MainScene : Node
{
    [Export]
    public string AppTitle { get; set; } = "Illogical Reborn";

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

    public List<ConnectionWire> Wires { get; set; } = new List<ConnectionWire>();

    private Connector? _currentlySelectedConnector = null;

    private int _currentNodeId = 0;

    private string? _currentPath = null;

    public string? CurrentPath
    {
        get => _currentPath;
        set
        {
            _currentPath = value;
            GetWindow().Title = value == null ? $"{AppTitle}" : $"{AppTitle}: {value}";
        }
    }

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
            ConnectionLinePreview?.SetPointPosition(1, motionEvent.GlobalPosition);
        }
    }

    public override void _Ready()
    {
        base._Ready();
        ConnectionLinePreview.Points = new Vector2[2];
        ConnectionLinePreview.Visible = false;
        GetWindow().Title = AppTitle;
    }

    public void SaveToFile(string path)
    {
        CurrentPath = path;
        SaveData data = new SaveData() { CurrentId = _currentNodeId };
        foreach (LogicNode node in LogicComponents)
        {
            data.Nodes.Add(new LogicNodeSaveData(node.Id, node.GlobalPosition, node.GetType().Name, new List<int>()));
        }
        File.WriteAllText(path, data.ToString());
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
            /*
			if has connections:
				if is input:
					delete connection
					grab destination as source
				else:
					ignore
			if not:
				grab source
			*/
            if (!connector.IsOutput && connector.Connection != null)
            {
                GD.Print("Already has connection");
                ConnectionWire? wire = Wires.FirstOrDefault(p => p.Source == connector.Connection && p.Destination == connector);
                GD.Print(wire == null ? "and found wire" : "but didn't find wire");
                if (wire != null)
                {
                    wire.QueueFree();
                    Wires.Remove(wire);

                    _currentlySelectedConnector = connector.Connection;
                    ConnectionLinePreview.Visible = true;
                    ConnectionLinePreview.SetPointPosition(0, connector.Connection.GlobalPosition);
                    //disconnect other node from this
                    connector.Connection.DisconnectFrom(connector);
                    // disconect this node from other
                    connector.DisconnectFrom(connector.Connection);
                    //this is getting confusing 
                    return;
                }
            }
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
            Wires.Add(wire);
            //ensure that source is always the input and destination is always output
            wire.Source = !connector.IsOutput ? _currentlySelectedConnector : connector;
            wire.Destination = connector.IsOutput ? _currentlySelectedConnector : connector;
            _currentlySelectedConnector.ConnectTo(connector);
            connector.ConnectTo(_currentlySelectedConnector);
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
        node.Id = _currentNodeId++;
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
