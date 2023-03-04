using Godot;
using System;
using System.Collections.Generic;


public partial class LogicNode : Node2D
{
    public delegate void ConnectorSelectedEventHandler(Connector connector);
    public delegate void NodeSelectedEventHandler(LogicNode node);

    // Events can be nullable because they are assigned from code
#nullable enable
    public event ConnectorSelectedEventHandler? OnConnectorSelected;
    public event NodeSelectedEventHandler? OnNodeSelected;

    [Export]
    public PackedScene? ConnectorPrefab { get; set; } = null;

    [Export]
    public Node2D? InputNodeParent { get; set; } = null;
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
