using Godot;
using System;

public class Settings : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var file = new File();
		if (!file.FileExists("user://settings.cfg"))
			SaveConfig(true, true);

		ApplySettings();
	}

	public void SaveConfig(bool fullscreen, bool vsync)
	{
		var config = new ConfigFile();

		// Store some values.
		config.SetValue("Video", "fullscreen", fullscreen);
		config.SetValue("Video", "vsync", vsync);

		// Save it to a file (overwrite if already exists).
		config.Save("user://settings.cfg");
	}

	private void ApplySettings()
	{
		var data = new Godot.Collections.Dictionary();
		var config = new ConfigFile();

		// Load data from a file.
		Error err = config.Load("user://settings.cfg");

		// If the file didn't load, ignore it.
		if (err != Error.Ok)
		{
			return;
		}

		// Fetch the data for each section.
		var fullscreen = (bool)config.GetValue("Video", "fullscreen");
		var vsync = (bool)config.GetValue("Video", "vsync");

		SetScreen(fullscreen, vsync);
	}

	private void SetScreen(bool fullscreen, bool vsync)
	{
		OS.WindowFullscreen = fullscreen;
		OS.VsyncEnabled = vsync;
	}
}
