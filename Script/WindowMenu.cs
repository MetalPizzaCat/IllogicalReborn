#nullable enable
using Godot;
using System;

public partial class WindowMenu : MenuBar
{
    public delegate void NewFileRequestedEventHandler();

    public event NewFileRequestedEventHandler? OnNewFileRequested;
    protected enum FileMenuOptions
    {
        New = 0,
        Save = 1,
        Load = 3, 
        Exit = 4
    }

    [Export]
    public MainScene? NodeScene { get; set; }

    [Export]
    public FileDialog? SaveFileDialog { get; set; }

    [Export]
    public FileDialog? OpenFileDialog { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void FileMenuPressed(long index)
    {
        GD.Print(index);
        switch ((FileMenuOptions)index)
        {
            case FileMenuOptions.New:
                OnNewFileRequested?.Invoke();
                break;
            case FileMenuOptions.Save:
                SaveFileDialog?.Show();
                break;
            case FileMenuOptions.Load:
                OpenFileDialog?.Show();
                break;
        }
    }

    private void FileToSaveSelected(string path)
    {
        GD.Print($"File to save: {path}");
        NodeScene?.SaveToFile(path);
    }
}



