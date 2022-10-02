using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using Godot.Collections;

public class Dialog : Control
{
	[Export] private float _textSpeed;

	private int _phraseNum;
	private bool _finished;
	private Label _text;
	private Timer _timer;
	private Polygon2D _indicator;
	private List<DialogItem> _dialog;
	
	public override void _Ready()
	{
		_text = GetNode<Label>("CanvasLayer/Text");
		_timer = GetNode<Timer>("CanvasLayer/Timer");
		_indicator = GetNode<Polygon2D>("CanvasLayer/Indicator");

		_timer.WaitTime = _textSpeed;

		Start("res://Dialog/dialog1.json");
	}

	public void Start(string dialogPath)
	{
		_finished = false;
		
		GetDialog(dialogPath);
		
		NextPhrase();
	}

	public override void _Process(float delta)
	{
		_indicator.Visible = _finished;
		if (Input.IsActionJustPressed("jump"))
		{
			if (_finished)
			{
				NextPhrase();
			}
			else
			{
				_text.VisibleCharacters = _text.Text.Length;
				_finished = true;
			}
		}
	}

	private void  GetDialog(string dialogPath)
	{
		_dialog = new List<DialogItem>();
		
		var file = new File();
		file.Open(dialogPath, File.ModeFlags.Read);
		string content = file.GetAsText();

		var jsonFile =  JSON.Parse(content).Result;

		var sceneData = jsonFile as Godot.Collections.Array;
		for( int i = 0; i < sceneData.Count; i++ )
		{
			var data = sceneData[i] as Godot.Collections.Dictionary;
			var newItem = new DialogItem()
			{
				Index = data["Index"].ToString(),
				Text = data["Text"].ToString()
			};
			
			_dialog.Add(newItem);
		}
	}

	private async void NextPhrase()
	{
		if (_phraseNum == _dialog.Count)
		{
			QueueFree();
			return;
		}

		_finished = false;

		_text.Text = _dialog[_phraseNum].Text;

		_text.VisibleCharacters = 0;

		while (_text.VisibleCharacters < _text.Text.Length)
		{
			_text.VisibleCharacters++;
			
			_timer.Start();
		
			await ToSignal(_timer, "timeout");
		}

		_finished = true;
		_phraseNum++;
		
		return;
	}
}
public class DialogItem
{
	public string Index { get; set; }
	public string Text { get; set; }
}