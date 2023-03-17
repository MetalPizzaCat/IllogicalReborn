#nullable enable
using Godot;
using System;

public partial class NodeContextMenu : Control
{
	[Export]
	public OptionButton? SizeOptionButton { get; set; }

	[Export]
	public Label? DisplayNameLabel { get; set; }

	private LogicNode? _currentNode = null;

	public LogicNode? CurrentNode
	{
		get => _currentNode;
		set
		{
			_currentNode = null;
			if (value != null && SizeOptionButton != null)
			{
				SizeOptionButton.Selected = value.DataSize - 1;
				if (DisplayNameLabel != null)
				{
					DisplayNameLabel.Text = value.DisplayName;
				}
			}
			_currentNode = value;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		if (SizeOptionButton == null)
		{
			return;
		}
		for (int i = 1; i <= 32; i++)
		{
			SizeOptionButton.AddItem(i.ToString());
		}
	}

	private void SelectDataSize(long index)
	{
		if(CurrentNode == null)
		{
			return;
		}
		// this works simply  because each option is just DataSize - 1
		CurrentNode.DataSize = (int)(index + 1);
	}
}



