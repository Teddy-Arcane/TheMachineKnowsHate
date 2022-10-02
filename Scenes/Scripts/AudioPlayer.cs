using Godot;
using System;

public class AudioPlayer : Node2D
{
	public void Play(string name)
	{
		GetNode<AudioStreamPlayer>(name).Play();
	}
}
