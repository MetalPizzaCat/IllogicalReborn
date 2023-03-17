#nullable enable
using Godot;
using System;

public partial class ConnectionWire : Node2D
{
	[Export]
	public Line2D? Line { get; set; }

	[Export]
	public Color ValidConnectionColor { get; set; } = Godot.Colors.White;

	[Export]
	public Color InvalidConnectionColor { get; set; } = Godot.Colors.Red;

	private Connector? _source;

	private Connector? _destination;

	public Connector? Source
	{
		get => _source;
		set
		{
			_source = value;
			if (Line == null || _source == null)
			{
				return;
			}
			Line.SetPointPosition(0, _source.GlobalPosition);
		}
	}

	public Connector? Destination
	{
		get => _destination;
		set
		{
			_destination = value;
			if (Line == null || _destination == null)
			{
				return;
			}
			Line.SetPointPosition(1, _destination.GlobalPosition);
		}
	}

	private bool _isDisplayingValidConnection = true;

	public bool IsDisplayingValidConnection
	{
		get => _isDisplayingValidConnection;
		set
		{
			_isDisplayingValidConnection = value;
			if (Line != null)
			{
				Line.DefaultColor = value ? ValidConnectionColor : InvalidConnectionColor;
			}
		}
	}

	public override void _Ready()
	{
		base._Ready();
		if (Line != null)
		{
			Line.Points = new Vector2[2];
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (Destination != null && Source != null)
		{
			Line.SetPointPosition(0, _source.GlobalPosition);
			Line.SetPointPosition(1, _destination.GlobalPosition);
		}
	}
}
