#nullable enable
using Godot;
using System;
using System.Linq;

/// <summary>
/// Line that only uses 90 degree turns
/// </summary>
public partial class ConnectionLine : Line2D
{
	[Export]
	public Vector2 Start
	{
		get => Points.FirstOrDefault();
		set
		{
			SetPointPosition(0, value);
			SetPointPosition(1, new Vector2((Points[3].X - Points[0].X) / 2f + Points[0].X, Points[0].Y));
			SetPointPosition(2, new Vector2((Points[3].X - Points[0].X) / 2f + Points[0].X, Points[3].Y));
		}
	}

	[Export]
	public Vector2 End
	{
		get => Points.LastOrDefault();
		set
		{
			SetPointPosition(3, value);
			SetPointPosition(1, new Vector2((Points[3].X - Points[0].X) / 2f + Points[0].X, Points[0].Y));
			SetPointPosition(2, new Vector2((Points[3].X - Points[0].X) / 2f + Points[0].X, Points[3].Y));
		}
	}
}
