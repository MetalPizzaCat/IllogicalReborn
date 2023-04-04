using Godot;
using System;

public partial class MainControl : Control
{
	[Export]
	public VBoxContainer? VariableButtonList { get; set; }

	[Export]
	public PackedScene? VariableButtonPrefab { get; set; }

	[Export]
	public MainScene? MainScene { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (VariableButtonList != null && VariableButtonPrefab != null)
		{
			for (char name = 'a'; name <= 'z'; name++)
			{
				VariableValueControl? btn = VariableButtonPrefab.InstantiateOrNull<VariableValueControl>();
				btn.VariableName = name;
				btn.VariableValue = false;
				btn.OnValueChanged += VariableValueChanged;
				VariableButtonList.AddChild(btn);
			}
		}
	}

	private void VariableValueChanged(char name, bool value)
	{
		MainScene?.SetVariableValue(name, value);
	}
}
