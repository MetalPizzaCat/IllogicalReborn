using Godot;
using System;

public partial class VariableValueControl : HBoxContainer
{
	public delegate void ValueChangedEventHandler(char name, bool value);

	public event ValueChangedEventHandler? OnValueChanged;
	private char _variableName = 'x';

	[Export]
	public Label? VariableNameLabel { get; set; } = null;

	[Export]
	public CheckBox? VariableValueBox { get; set; } = null;

	[Export]
	public char VariableName
	{
		get => _variableName;
		set
		{
			_variableName = value;
			if (VariableNameLabel != null)
			{
				VariableNameLabel.Text = value.ToString();
			}
		}
	}

	[Export]
	public bool VariableValue { get; set; } = false;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (VariableValueBox != null)
		{
			VariableValueBox.Toggled += (bool pressed) => { GD.Print($"{VariableName} to {pressed}"); VariableValue = pressed; OnValueChanged?.Invoke(VariableName, pressed); };
		}
	}
}
