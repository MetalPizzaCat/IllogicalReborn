using System;
using Godot;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

public static class ExportImageGenerator
{

    public static int LineWidth = 3;
    public static Vector2 BlockSize = new Vector2(64, 128);
    public static Vector2 CanvasSafeZoneTop = new Vector2(100, 100);
    public static Vector2 CanvasSafeZoneBottom = new Vector2(256, 256);

    public static string? GetXml(List<LogicNode> nodes, List<ConnectionWire> wires)
    {
        float minX = Math.Abs(Math.Min(nodes.MinBy(x => x.GlobalPosition.X)?.GlobalPosition.X ?? 0, 0)) + CanvasSafeZoneTop.X;
        float minY = Math.Abs(Math.Min(nodes.MinBy(x => x.GlobalPosition.Y)?.GlobalPosition.Y ?? 0, 0)) + CanvasSafeZoneTop.Y;
        GD.Print($"MinX {minX} MinY {minY}");
        XElement root = new XElement("svg");
        root.SetAttributeValue("width", (nodes.MaxBy(x => x.GlobalPosition.X)?.GlobalPosition.X ?? 0) - (nodes.MinBy(x => x.GlobalPosition.X)?.GlobalPosition.X ?? 0) + CanvasSafeZoneBottom.X);
        root.SetAttributeValue("height", (nodes.MaxBy(x => x.GlobalPosition.Y)?.GlobalPosition.Y ?? 0) - (nodes.MinBy(x => x.GlobalPosition.Y)?.GlobalPosition.Y ?? 0) + CanvasSafeZoneBottom.Y);
        GD.Print($"MaxX: {(nodes.MaxBy(x => x.GlobalPosition.X)?.GlobalPosition.X ?? 0)} MaxY: {(nodes.MaxBy(x => x.GlobalPosition.Y)?.GlobalPosition.Y ?? 0)}; MinX: {(nodes.MinBy(x => x.GlobalPosition.X)?.GlobalPosition.X ?? 0)} MinY: {(nodes.MinBy(x => x.GlobalPosition.Y)?.GlobalPosition.Y ?? 0)}");
        foreach (LogicNode node in nodes)
        {
            XElement rect = new XElement("rect");
            foreach (Connector conn in node.Inputs)
            {
                Vector2 pos = conn.GlobalPosition + new Vector2(minX, minY);// + new Vector2(32, 64);
                XElement line = new XElement("line");
                line.SetAttributeValue("x1", pos.X - 32);
                line.SetAttributeValue("x2", pos.X + 16);
                line.SetAttributeValue("y1", pos.Y);
                line.SetAttributeValue("y2", pos.Y);
                line.SetAttributeValue("stroke-width", LineWidth);
                line.SetAttributeValue("stroke", "black");
                root.Add(line);
            }

            foreach (Connector conn in node.Outputs)
            {
                Vector2 pos = conn.GlobalPosition + new Vector2(minX, minY);//+ new Vector2(32, 64); ;
                XElement line = new XElement("line");
                line.SetAttributeValue("x1", pos.X - 16);
                line.SetAttributeValue("x2", pos.X + 32);
                line.SetAttributeValue("y1", pos.Y);
                line.SetAttributeValue("y2", pos.Y);
                line.SetAttributeValue("stroke-width", LineWidth);
                line.SetAttributeValue("stroke", "black");
                root.Add(line);
            }

            rect.SetAttributeValue("width", BlockSize.X);
            rect.SetAttributeValue("height", BlockSize.Y);
            rect.SetAttributeValue("fill", "white");
            rect.SetAttributeValue("stroke-width", 5);
            rect.SetAttributeValue("stroke", "black");
            rect.SetAttributeValue("x", node.GlobalPosition.X + minX - BlockSize.X / 2f);
            rect.SetAttributeValue("y", node.GlobalPosition.Y + minY - BlockSize.Y / 2f);
            root.Add(rect);
        }
        foreach (ConnectionWire wire in wires)
        {
            XElement line = new XElement("polyline");
            line.SetAttributeValue("stroke-width", LineWidth);
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