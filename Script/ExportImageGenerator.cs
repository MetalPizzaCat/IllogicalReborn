using System;
using Godot;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

public static class ExportImageGenerator
{

    public static string? GetXml(List<LogicNode> nodes, List<ConnectionWire> wires)
    {
        float minX = Math.Abs(Math.Min(nodes.MinBy(x => x.GlobalPosition.X)?.GlobalPosition.X ?? 0, 0)) + 100;
        float minY = Math.Abs(Math.Min(nodes.MinBy(x => x.GlobalPosition.Y)?.GlobalPosition.Y ?? 0, 0)) + 100;
        GD.Print($"MinX {minX} MinY {minY}");
        XElement root = new XElement("svg");
        root.SetAttributeValue("width", (nodes.MaxBy(x => x.GlobalPosition.X)?.GlobalPosition.X ?? 0) - (nodes.MinBy(x => x.GlobalPosition.X)?.GlobalPosition.X ?? 0) + 256);
        root.SetAttributeValue("height", (nodes.MaxBy(x => x.GlobalPosition.Y)?.GlobalPosition.Y ?? 0) - (nodes.MinBy(x => x.GlobalPosition.Y)?.GlobalPosition.Y ?? 0) + 256);
        GD.Print($"MaxX: {(nodes.MaxBy(x => x.GlobalPosition.X)?.GlobalPosition.X ?? 0)} MaxY: {(nodes.MaxBy(x => x.GlobalPosition.Y)?.GlobalPosition.Y ?? 0)}; MinX: {(nodes.MinBy(x => x.GlobalPosition.X)?.GlobalPosition.X ?? 0)} MinY: {(nodes.MinBy(x => x.GlobalPosition.Y)?.GlobalPosition.Y ?? 0)}");
        foreach (LogicNode node in nodes)
        {
            XElement rect = new XElement("rect");
            rect.SetAttributeValue("width", 128);
            rect.SetAttributeValue("height", 128);
            rect.SetAttributeValue("fill", "white");
            rect.SetAttributeValue("stroke-width", 5);
            rect.SetAttributeValue("stroke", "black");
            rect.SetAttributeValue("x", node.GlobalPosition.X + minX - 128 / 2);
            rect.SetAttributeValue("y", node.GlobalPosition.Y + minY - 128 / 2);
            root.Add(rect);
        }
        foreach (ConnectionWire wire in wires)
        {
            XElement line = new XElement("polyline");
            line.SetAttributeValue("stroke-width", 5);
            line.SetAttributeValue("stroke", "black");
            line.SetAttributeValue("fill", "none");
            string points = string.Empty;
            foreach (Vector2 point in wire.Line.Points)
            {
                points += $"{point.X + wire.GlobalPosition.X + minX},{point.Y + wire.GlobalPosition.Y + minY} ";
            }
            line.SetAttributeValue("points", points);
            root.Add(line);
        }
        return root.ToString();
    }
}