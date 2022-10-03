using Godot;

public class IntroVideo : Control
{
	private void _on_VideoPlayer_finished()
	{
		GetTree().ChangeScene("res://Scenes/Warning.tscn");
	}

	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("skip_video"))
		{
			GetTree().ChangeScene("res://Scenes/Warning.tscn");
		}
	}
}
