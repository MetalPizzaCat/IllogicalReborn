#nullable enable
using Godot;
using System;

/// <summary>
/// Class responsible for handling mouse events that happen on the canvas itself.<para/>
/// This includes mouse position and clicking on the canvas itself.
/// </summary>
public partial class CanvasControl : Control
{
    public delegate void BeginSelectionEventHandler(Vector2 location);
    public delegate void EndSelectionEventHandler();

    public event BeginSelectionEventHandler? OnSelectionBegun;
    public event EndSelectionEventHandler? OnSelectionEnded;
    public Vector2 MousePosition { get; set; } = Vector2.Zero;

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventMouseMotion mouseMotion)
        {
            MousePosition = mouseMotion.Position - Size / 2f;
        }

    }

    public override void _GuiInput(InputEvent @event)
    {
        base._GuiInput(@event);
        if (@event is InputEventMouseButton && Input.IsActionJustPressed("pointer_press"))
        {
            GD.Print("Start");
            OnSelectionBegun?.Invoke(MousePosition);
        }
        else if (@event is InputEventMouseButton && Input.IsActionJustReleased("pointer_press"))
        {
            OnSelectionEnded?.Invoke();
            GD.Print("end");
        }
    }
}
