using Godot;
using System;
using System.Linq;

public class AudioPlayer : Node2D
{
	public void Play(string name)
	{
		var players = GetChildren().OfType<AudioStreamPlayer>();
		foreach (var player in players)
		{
			if (player.Playing)
				player.Stop();
		}
		
		GetNode<AudioStreamPlayer>(name).Play();
	}
}
