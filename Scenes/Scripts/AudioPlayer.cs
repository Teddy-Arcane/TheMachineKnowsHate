using Godot;
using System;
using System.Linq;

public class AudioPlayer : Node2D
{
	public void Play(string name)
	{
		var audio = GetNode<AudioStreamPlayer>(name);
		if(!audio.Playing)
			audio.Play();
	}

	public void Stop(string name)
	{
		GetNode<AudioStreamPlayer>(name).Stop();
	}

	public void ToggleRun(bool run)
	{
		var tiptap = GetNode<AudioStreamPlayer>("Run");
		if (tiptap.Playing && !run)
		{
			tiptap.Playing = false;
		}
		else if (!tiptap.Playing && run)
		{
			tiptap.Playing = true;
		}
	}
}
