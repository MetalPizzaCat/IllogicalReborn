using Godot;
using System;
using System.Collections.Generic;


public partial class LogicNode : Node2D
{
	public delegate void ConnectorSelectedEventHandler(Connector connector);
	public delegate void NodeSelectedEventHandler(LogicNode node);
	public delegate void NodeDeselectedEventHandler(LogicNode node);
	public delegate void DeleteNodeEventHandler(LogicNode node);

	// Events can be nullable because they are assigned from code
#nullable enable
	public event ConnectorSelectedEventHandler? OnConnectorSelected;
	public event NodeSelectedEventHandler? OnNodeSelected;

	public event NodeDeselectedEventHandler? OnNodeDeselected;
	public event DeleteNodeEventHandler? OnNodeDeleted;

	[Export]
	public PackedScene? ConnectorPrefab { get; set; } = null;

	[Export]
	public Node2D? InputNodeParent { get; set; } = null;

	[Export]
	public PopupMenu? ContextMenu { get; set; } = null;

	[Export]
	public Label? DebugInfoLabel { get; set; } = null;
#nullable disable

	[Export]
	public float GridSize = 16;
	[Export]
	public int InputSize { get; set; } = 2;

	public bool IsGrabbed { get; set; } = false;

	[Export]
	public Connector OutputConnector { get; set; } = null;

	public List<Connector> Inputs { get; set; } = new List<Connector>();

	public int Id { get; set; } = -1;

	public override void _Ready()
	{
		OutputConnector.OnSelected += (Connector connector) => OnConnectorSelected?.Invoke(connector);
		OutputConnector.ParentNode = this;
		if (ConnectorPrefab == null || InputNodeParent == null)
		{
			return;
		}
		for (int i = 0; i < InputSize; i++)
		{
			Connector con = ConnectorPrefab.Instantiate<Connector>();
			InputNodeParent.AddChild(con);
			Inputs.Add(con);
			con.Position = new Vector2(0, i * 48);
			con.OnSelected += (Connector connector) => OnConnectorSelected?.Invoke(connector);
			con.ParentNode = this;
		}
	}

	//TODO: move out of this class and into the canvas to make it easier to scale with camera
	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		// if (@event is InputEventMouseMotion motionEvent && IsGrabbed)
		// {
		// 	GlobalPosition = motionEvent.GlobalPosition.Snapped(new Vector2(GridSize, GridSize));
		// }
	}

	public void MoveTo(Vector2 location)
	{
		Position = location.Snapped(new Vector2(GridSize, GridSize));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if (DebugInfoLabel != null)
		{
			DebugInfoLabel.Text = Position.ToString();
		}
	}

	public virtual bool Execute()
	{
		return false;
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
					ContextMenu.Position = new Vector2I((int)GlobalPosition.X, (int)GlobalPosition.Y);
					break;
			}
		}
	}


	private void OnContextMenuPressed(long index)
	{
		if (index == 0)
		{
			OnNodeDeleted?.Invoke(this);
			foreach (Connector input in Inputs)
			{
				input.Connection?.DisconnectFrom(input);
				input.DisconnectFrom(input.Connection);
			}
			foreach (Connector output in OutputConnector.Connections)
			{
				output.DisconnectFrom(OutputConnector);
				OutputConnector.DisconnectFrom(output);
			}
		}
	}
}
