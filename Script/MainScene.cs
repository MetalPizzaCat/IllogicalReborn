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
    public CanvasControl? CanvasControl { get; set; } = null;

    [Export]
    public Camera2D? CurrentCamera { get; set; } = null;

    [Export]
    public Line2D? ConnectionLinePreview { get; set; } = null;

    [Export]
    public Label? DebugInfoLabel { get; set; } = null;


    [Export]
    public PackedScene? ConnectorPrefab { get; set; } = null;

    [Export]
    public float ZoomStep { get; set; } = 0.25f;

    [Export]
    public Node2D? NodeSpawnLocation { get; set; }

    [Export]
    public NodeContextMenu? ContextMenu { get; set; } = null;

    [ExportCategory("Nodes")]
    [Export]
    public PackedScene? OperationNodePrefab { get; set; } = null;

    [Export]
    public PackedScene? OperationNotNodePrefab { get; set; } = null;

    [Export]
    public PackedScene? DisplayNodePrefab { get; set; } = null;

    [Export]
    public PackedScene? ConstNodePrefab { get; set; } = null;


    /// <summary>
    /// All of the logic nodes of the scheme
    /// </summary>
    /// <typeparam name="LogicNode"></typeparam>
    /// <returns></returns>
    public List<LogicNode> LogicComponents { get; set; } = new List<LogicNode>();

    /// <summary>
    /// All of the wires that scheme has.<para/>
    /// Wires are visual only object and don't have any effect on logic
    /// </summary>
    /// <typeparam name="ConnectionWire"></typeparam>
    /// <returns></returns>
    public List<ConnectionWire> Wires { get; set; } = new List<ConnectionWire>();

    /// <summary>
    /// Current mouse position with 0,0 being the center of the screen
    /// </summary>
    public Vector2 MousePosition => CanvasControl?.MousePosition ?? Vector2.Zero;

    /// <summary>
    /// Current camera offset, used to mimic canvas movement
    /// </summary>
    public Vector2 CameraOffset => CurrentCamera?.Position ?? Vector2.Zero;

    public Vector2 SpawnPosition => NodeSpawnLocation?.Position ?? Vector2.Zero;

    /// <summary>
    /// Current camera zoom or (1,1) if not camera is present
    /// </summary>
    public Vector2 CurrentZoom => CurrentCamera?.Zoom ?? Vector2.One;

    /// <summary>
    /// Current mouse location with offset, zoom and viewport adjustments applied
    /// </summary>
    /// <returns></returns>
    public Vector2 CurrentPointerPosition => (MousePosition / CurrentZoom + CameraOffset);

    /// <summary>
    ///  Current selector object that is being connected from
    /// </summary>
    private Connector? _currentlySelectedConnector = null;

    /// <summary>
    /// Current node that is being moved
    /// </summary>
    private LogicNode? _currentlySelectedNode = null;

    /// <summary>
    /// Same as _currentlySelectedNode but does not get cleared when user releases the node<para/>
    /// Intended for use with context menus
    /// </summary>
    private LogicNode? _currentlyEditedNode = null;

    private int _currentNodeId = 0;

    private string? _currentPath = null;


    /// <summary>
    /// Path to the file where any quick or auto save will write to
    /// </summary>
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
            ConnectionLinePreview?.SetPointPosition(1, CurrentPointerPosition);
            if (Input.IsActionPressed("move_canvas"))
            {
                CurrentCamera.Position -= motionEvent.Relative / CurrentZoom;
                if (_currentlySelectedConnector != null)
                {
                    ConnectionLinePreview.SetPointPosition(0, _currentlySelectedConnector.GlobalPosition);
                }

            }
            if (_currentlySelectedNode != null)
            {
                _currentlySelectedNode.MoveTo(CurrentPointerPosition);
            }
            if (DebugInfoLabel != null)
            {
                DebugInfoLabel.Text = $"{MousePosition} \n {CameraOffset} \n {CurrentPointerPosition}";
            }
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
                    // disconnect this node from other
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

    public void OnNodeSelected(LogicNode node)
    {
        _currentlySelectedNode = node;
        if (ContextMenu != null)
        {
            ContextMenu.CurrentNode = node;
        }
    }

    public void OnNodeDeselected(LogicNode node)
    {
        _currentlySelectedNode = null;
    }

    public void OnConnectorsSizeUpdate(Connector source, Connector destination, bool compatible)
    {
        ConnectionWire? wire = Wires.FirstOrDefault(p => p.Source == source && p.Destination == destination || p.Source == destination && p.Destination == source);
        if (wire == null)
        {
            GD.PrintErr("Attempted to display connection error but nodes have no wire connecting them");
            return;
        }
        wire.IsDisplayingValidConnection = compatible;
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
        node.Position = SpawnPosition + CameraOffset;
        node.OnNodeSelected += OnNodeSelected;
        node.OnNodeDeselected += OnNodeDeselected;
        node.OnConnectionSizeUpdated += OnConnectorsSizeUpdate;
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

    private void AddLogicNodeOr()
    {
        AddLogicNode<OperationNode>(OperationNodePrefab).Operation = OperationNode.OperationType.Or;
    }


    private void AddLogicNodeXor()
    {
        AddLogicNode<OperationNode>(OperationNodePrefab).Operation = OperationNode.OperationType.Xor;
    }

    private void AddLogicNodeConst()
    {
        AddLogicNode<ConstNode>(ConstNodePrefab);
    }
}
