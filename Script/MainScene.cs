#nullable enable
using Godot;
using System;

public partial class MainScene : Node
{
	[Export]
	public Camera2D? CurrentCamera { get; set; } = null;

	[Export]
	public float ZoomStep { get; set; } = 0.25f;

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
	}
}
