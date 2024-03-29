using Godot;
using System;
using System.Collections.Generic;

public partial class MainControl : Control
{
	[Export]
	public VBoxContainer? VariableButtonList { get; set; }

	[Export]
	public PackedScene? VariableButtonPrefab { get; set; }

	[Export]
	public MainScene? MainScene { get; set; }

	private Dictionary<char, VariableValueControl> _buttons = new();

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
				btn.Visible = false;
				btn.OnValueChanged += VariableValueChanged;
				VariableButtonList.AddChild(btn);
				_buttons.Add(name, btn);
			}
		}
	}

	public void DisplayVariableButtons(List<char> variables)
	{
		foreach ((char name, VariableValueControl btn) in _buttons)
		{
			btn.Visible = variables.Contains(name);
		}
	}

	private void VariableValueChanged(char name, bool value)
	{
		MainScene?.SetVariableValue(name, value);
	}
}
