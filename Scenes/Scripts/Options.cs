using Godot;

public class Options : Control
{
	private Settings _settings;
	private CheckBox _fullscreen;
	private CheckBox _vsync;
	private Button _ok;

	public override void _Ready()
	{
		_fullscreen = GetNode<CheckBox>("CanvasLayer/ColorRect/Fullscreen");
		_vsync = GetNode<CheckBox>("CanvasLayer/ColorRect/Vsync");
		_ok = GetNode<Button>("CanvasLayer/ColorRect/OK");
		_settings = GetNode<Settings>("/root/Settings");

		_ok.GrabFocus();
		
		SetToggles();
	}

	private void SetToggles()
	{
		if (OS.WindowFullscreen == true)
		{
			_fullscreen.SetPressedNoSignal(true);
		}
		if (OS.VsyncEnabled == true)
		{
			_vsync.SetPressedNoSignal(true);
		}
	}

	private void _on_Fullscreen_toggled(bool button_pressed)
	{
		if (OS.WindowFullscreen == true)
		{
			OS.WindowFullscreen = false;
		}
		else
		{
			OS.WindowFullscreen = true;
		}
	}

	private void _on_Vsync_toggled(bool button_pressed)
	{
		if (OS.VsyncEnabled == true)
		{
			OS.VsyncEnabled = false;
		}
		else
		{
			OS.VsyncEnabled = true;
		}
	}

	private void _on_OK_pressed()
	{
		var fullscreen = OS.WindowFullscreen;
		var vsync = OS.VsyncEnabled;

		_settings.SaveConfig(fullscreen, vsync);

		GetTree().ChangeScene("res://Scenes/MainMenu.tscn");
	}
}
