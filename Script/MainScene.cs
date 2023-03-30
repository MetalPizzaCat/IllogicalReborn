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
	public ConnectionLine? ConnectionLinePreview { get; set; } = null;

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
	public WindowMenu? Menu { get; set; } = null;

	[Export]
	public PackedScene? OperationNodePrefab { get; set; } = null;

	[Export]
	public PackedScene? OperationNotNodePrefab { get; set; } = null;

	[Export]
	public PackedScene? DisplayNodePrefab { get; set; } = null;

	[Export]
	public PackedScene? ConstNodePrefab { get; set; } = null;

	[Export]
	public ColorRect? SelectionBox { get; set; } = null;

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
	/// Location from which selection started
	/// </summary>
	private Vector2 _currentSelectionBoxStart = Vector2.Zero;

	/// <summary>
	///  Current selector object that is being connected from
	/// </summary>
	private Connector? _currentlySelectedConnector = null;

	/// <summary>
	/// Current node that is being moved
	/// </summary>
	private LogicNode? _currentlySelectedNode = null;

	/// <summary>
	/// All nodes selected via selection box
	/// </summary>
	private List<LogicNode> _currentGroupSelection = new List<LogicNode>();

	/// <summary>
	/// Same as _currentlySelectedNode but does not get cleared when user releases the node<para/>
	/// Intended for use with context menus
	/// </summary>
	private LogicNode? _currentlyEditedNode = null;

	private int _currentNodeId = 0;

	private string? _currentPath = null;

	private bool _isSelecting = false;

	public bool IsSelecting
	{
		get => _isSelecting;
		set
		{
			_isSelecting = value;
			if (SelectionBox != null)
			{
				SelectionBox.Visible = value;
			}
		}
	}


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
			ConnectionLinePreview.End = CurrentPointerPosition;
			if (Input.IsActionPressed("move_canvas"))
			{
				CurrentCamera.Position -= motionEvent.Relative / CurrentZoom;
				if (_currentlySelectedConnector != null)
				{
					ConnectionLinePreview.Start = (_currentlySelectedConnector.GlobalPosition);
				}
			}
			if (_currentlySelectedNode != null)
			{
				MoveNodes();
			}
			else if (SelectionBox != null && _isSelecting)
			{
				SelectNodes();
			}
			if (DebugInfoLabel != null)
			{
				DebugInfoLabel.Text = $"{MousePosition} \n {CameraOffset} \n {CurrentPointerPosition}";
			}
		}
	}

	public void MoveNodes()
	{
		if (_currentlySelectedNode == null)
		{
			return;
		}
		// solution for movement: calculate distance from node before move and use that 
		foreach (LogicNode node in _currentGroupSelection.Where(p => p != _currentlySelectedNode))
		{
			Vector2 offset = node.GlobalPosition - _currentlySelectedNode.GlobalPosition;
			node.MoveTo(CurrentPointerPosition + offset);
		}
		_currentlySelectedNode.MoveTo(CurrentPointerPosition);
	}

	public void SelectNodes()
	{
		Vector2 min = new Vector2
		(
			Math.Min(_currentSelectionBoxStart.X, CurrentPointerPosition.X),
			Math.Min(_currentSelectionBoxStart.Y, CurrentPointerPosition.Y)
		);
		Vector2 max = new Vector2
		(
			Math.Max(_currentSelectionBoxStart.X, CurrentPointerPosition.X),
			Math.Max(_currentSelectionBoxStart.Y, CurrentPointerPosition.Y)
		);
		SelectionBox.GlobalPosition = min;
		SelectionBox.Size = max - min;
		foreach (LogicNode node in _currentGroupSelection)
		{
			node.IsSelected = false;
		}
		_currentGroupSelection = CurrentGroupSelection;
		foreach (LogicNode node in _currentGroupSelection)
		{
			node.IsSelected = true;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		ConnectionLinePreview.Visible = false;
		GetWindow().Title = AppTitle;

		CanvasControl.OnSelectionBegun += (Vector2 loc) => { _currentSelectionBoxStart = CurrentPointerPosition; IsSelecting = true; };
		CanvasControl.OnSelectionEnded += FinishSelection;

		if (Menu != null)
		{
			Menu.OnNewFileRequested += NewFile;

		}
	}

	public void FinishSelection()
	{
		foreach (LogicNode node in _currentGroupSelection)
		{
			node.IsSelected = false;
		}
		_currentGroupSelection = CurrentGroupSelection;
		foreach (LogicNode node in _currentGroupSelection)
		{
			node.IsSelected = true;
		}
		IsSelecting = false;
		SelectionBox.Size = Vector2.Zero;
	}

	/// <summary>
	/// Get nodes that were covered by the selection box
	/// </summary>
	/// <returns></returns>
	public List<LogicNode> CurrentGroupSelection => SelectionBox == null ? new() : LogicComponents.Where(p => p.Position + new Vector2(16, 16) <= (SelectionBox.GlobalPosition + SelectionBox.Size) && p.Position > SelectionBox.GlobalPosition).ToList();

	/// <summary>
	/// Removes all nodes and connection wires from canvas
	/// </summary>
	private void ClearCanvas()
	{
		foreach (ConnectionWire wire in Wires)
		{
			wire.QueueFree();
		}
		foreach (LogicNode node in LogicComponents)
		{
			node.QueueFree();
		}
		LogicComponents.Clear();
		Wires.Clear();
		_currentGroupSelection.Clear();
		_currentlySelectedConnector = null;
		_currentlyEditedNode = null;
		_currentlySelectedNode = null;
		_currentlySelectedConnector = null;
	}


	/// <summary>
	/// Saves all of the project related info into a file 
	/// </summary>
	/// <param name="path">File to save to</param>
	public void SaveToFile(string path)
	{
		CurrentPath = path;
		SaveData data = new SaveData() { CurrentId = _currentNodeId };
		ILookup<Type, LogicNode>? lookup = LogicComponents.ToLookup(p => p.GetType());
		foreach (IGrouping<Type, LogicNode>? nodes in lookup)
		{
			if (nodes == null)
			{
				continue;
			}
			List<LogicNodeSaveData> group = new();
			foreach (LogicNode node in nodes)
			{
				group.Add(node.SaveData);
			}
			data.Nodes.Add(nodes.Key.ToString(), group);
		}
		File.WriteAllText(path, data.ToString());
	}

	public void NewFile()
	{
		ClearCanvas();
	}

	/// <summary>
	/// Create nodes based on the information from data and initiate their values
	/// </summary>
	/// <param name="data">Data to load from</param>
	private void LoadNodes(SaveData data)
	{
		foreach ((string type, List<LogicNodeSaveData> nodes) in data.Nodes)
		{
			foreach (LogicNodeSaveData nodeData in nodes)
			{
				if (type == typeof(OperationNode).ToString())
				{
					if (nodeData.OperationType == null)
					{
						continue;
					}
					PackedScene? scene = nodeData.OperationType == OperationNode.OperationType.Not ? OperationNotNodePrefab : OperationNodePrefab;
					OperationNode? node = AddLogicNode<OperationNode>(scene);
					if (node == null)
					{
						continue;
					}
					node.Load(nodeData);
				}
				else if (type == typeof(ConstNode).ToString())
				{
					ConstNode? node = AddLogicNode<ConstNode>(ConstNodePrefab);
					if (node == null)
					{
						continue;
					}
					node.Load(nodeData);
				}
				else if (type == typeof(DisplayNode).ToString())
				{
					DisplayNode? node = AddLogicNode<DisplayNode>(DisplayNodePrefab);
					if (node == null)
					{
						continue;
					}
					node.Load(nodeData);
				}
			}
		}
	}


	/// <summary>
	/// Load connections for nodes, this will both connect nodes and create wires
	/// </summary>
	/// <param name="data">Data to load from</param>
	private void LoadConnections(SaveData data)
	{
		if (ConnectorPrefab == null)
		{
			return;
		}
		// we don't care about what type that node has we only need data itself
		foreach (LogicNodeSaveData nodeData in data.Nodes.Values.SelectMany(x => x).ToList())
		{
			LogicNode? node = LogicComponents.FirstOrDefault(p => p.Id == nodeData.Id);
			if (node == null)
			{
				GD.PrintErr($"Attempted to access node with id {nodeData.Id} to start connection but node not found");
				continue;
			}
			foreach (LogicInputSaveData connectionData in nodeData.Inputs)
			{
				LogicNode? destination = LogicComponents.FirstOrDefault(p => p.Id == connectionData.DestinationNodeId);
				if (destination == null)
				{
					GD.PrintErr($"Attempted to access node with id {connectionData.DestinationNodeId} to finish connection but node not found");
					continue;
				}
				Connector source = node.Inputs[connectionData.SourceConnectorId];
				Connector dest = destination.Outputs[connectionData.DestinationConnectorId];
				if (!source.CanConnect(dest))
				{
					GD.PrintErr("Can't connect two nodes");
					continue;
				}
				ConnectionWire wire = ConnectorPrefab.Instantiate<ConnectionWire>();
				AddChild(wire);
				Wires.Add(wire);
				// unlike when we do this for user doing the connection we CAN know which one is output 
				// and which one is input
				// because save system only records from the perspective of the input
				wire.Source = dest;
				wire.Destination = source;
				source.ConnectTo(dest);
				dest.ConnectTo(source);
			}
		}
	}

	public void LoadFromFile(string path)
	{
		try
		{
			NewFile();
			string saveFile = File.ReadAllText(path);
			GD.Print(saveFile);
			SaveData? data = Newtonsoft.Json.JsonConvert.DeserializeObject<SaveData>(saveFile);

			if (data == null)
			{
				GD.PrintErr("Unable to load save data");
				return;
			}
			LoadNodes(data);
			LoadConnections(data);
		}
		catch (FileNotFoundException e)
		{
			GD.PrintErr($"Unable to find file at {path}. Error: {e.Message}");
		}
		catch (DirectoryNotFoundException e)
		{
			GD.PrintErr($"Unable to find file at {path}. Error: {e.Message}");
		}
		catch (IOException e)
		{
			GD.PrintErr($"Unable to read file at {path}. Error: {e.Message}");
		}
		catch (Newtonsoft.Json.JsonSerializationException e)
		{
			GD.PrintErr($"Unable to parse file at {path}. Error: {e.Message}");
		}
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
					ConnectionLinePreview.Start = connector.Connection.GlobalPosition;
					//disconnect other node from this
					connector.Connection.DisconnectFrom(connector);
					// disconnect this node from other
					connector.DisconnectFrom(connector.Connection);
					//this is getting confusing 
					Simulate();
					return;
				}
			}
			_currentlySelectedConnector = connector;
			ConnectionLinePreview.Visible = true;
			ConnectionLinePreview.Start = connector.GlobalPosition;
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
		Simulate();

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
		Simulate();
	}

	private void DeleteNode(LogicNode node)
	{
		foreach (Connector conn in node.Inputs)
		{
			Wires.Where(p => p.Source == conn || p.Destination == conn).ToList().ForEach(p => p.QueueFree());
			Wires.RemoveAll(p => p.Source == conn || p.Destination == conn);
		}
		Wires.Where(p => p.Source == node.OutputConnector || p.Destination == node.OutputConnector).ToList().ForEach(p => p.QueueFree());
		Wires.RemoveAll(p => p.Source == node.OutputConnector || p.Destination == node.OutputConnector);
		Simulate();
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
		node.OnNodeDeleted += DeleteNode;
		return node;
	}

	private void AddLogicNodeAnd()
	{
		AddLogicNode<OperationNode>(OperationNodePrefab);
		Simulate();
	}

	private void AddDisplayNode()
	{
		AddLogicNode<DisplayNode>(DisplayNodePrefab);
		Simulate();
	}

	private void AddLogicNodeNot()
	{
		AddLogicNode<OperationNode>(OperationNotNodePrefab);
		Simulate();
	}

	private void AddLogicNodeOr()
	{
		AddLogicNode<OperationNode>(OperationNodePrefab).Operation = OperationNode.OperationType.Or;
		Simulate();
	}


	private void AddLogicNodeXor()
	{
		AddLogicNode<OperationNode>(OperationNodePrefab).Operation = OperationNode.OperationType.Xor;
		Simulate();
	}

	private void AddLogicNodeConst()
	{
		AddLogicNode<ConstNode>(ConstNodePrefab);
		Simulate();
	}

	private void Simulate()
	{
		List<LogicNode> endNodes = new List<LogicNode>();
		// nodes without output connections are best start because there is nothing else to simulate after them
		// picking only them and traversing upwards means less tree traversals
		foreach (LogicNode node in LogicComponents)
		{
			if (node.OutputNodes.Count == 0)
			{
				endNodes.Add(node);
			}
		}
		GD.Print($"Ahooga: {endNodes.Count}");
		// make every node calculate values
		foreach (LogicNode node in endNodes)
		{
			node.Simulate();
		}
	}
}
