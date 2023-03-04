using Godot;
using System;

public partial class LogicNode : Node2D
{
	public delegate void ConnectorSelectedEventHandler(Connector connector);
	public delegate void NodeSelectedEventHandler(LogicNode node);

	// Events can be nullable because they are assigned from code
#nullable enable
	public event ConnectorSelectedEventHandler? OnConnectorSelected;
	public event NodeSelectedEventHandler? OnNodeSelected;
#nullable disable

	[Export]
	public float GridSize = 16;
	public bool IsGrabbed { get; set; } = false;

	[Export]
	public Connector OutputConnector { get; set; } = null;

	[Export]
	public PackedScene ConnectionWirePrefab { get; set; } = null;

	public override void _Ready()
	{
		OutputConnector.OnSelected += (Connector connector) => OnConnectorSelected?.Invoke(connector); 
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		if (@event is InputEventMouseMotion motionEvent && IsGrabbed)
		{
			Position = motionEvent.Position.Snapped(new Vector2(GridSize, GridSize));
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public virtual bool Execute()
	{
		return false;
	}

	private void Grab()
	{
		IsGrabbed = true;
	}

	private void Release()
	{
		IsGrabbed = false;
	}
}
