#nullable enable
using Godot;
using System;

public partial class ConnectionWire : Node2D
{
	[Export]
	public Line2D? Line { get; set; }

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

	public override void _Ready()
	{
		base._Ready();
		if (Line != null)
		{
			Line.Points = new Vector2[2];
		}
	}
}
