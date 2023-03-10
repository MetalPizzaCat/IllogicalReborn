using Godot;
using System;

/// <summary>
/// Class responsible for handling mouse events that happen on the canvas itself.<para/>
/// This includes mouse position and clicking on the canvas itself.
/// </summary>
public partial class CanvasControl : Control
{
    public Vector2 MousePosition { get; set; } = Vector2.Zero;

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventMouseMotion mouseMotion)
        {
            MousePosition = mouseMotion.Position - Size / 2f;
        }
    }
}
