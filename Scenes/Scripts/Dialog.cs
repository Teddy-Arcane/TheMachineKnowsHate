using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using Godot.Collections;

public class Dialog : Control
{
	[Export] private float _textSpeed;
	[Export] private string _file;
	[Export] private bool _lastScreen;
	[Export] private string _lastScreenMessage;

	private int _phraseNum;
	private bool _finished;
	private Label _text;
	private Timer _timer;
	private Node2D _indicator;
	private List<DialogItem> _dialog;
	private Game _game;
	private bool _finalChoiceHit = false;
	
	public override void _Ready()
	{
		_text = GetNode<Label>("CanvasLayer/Text");
		_timer = GetNode<Timer>("CanvasLayer/Timer");
		_indicator = GetNode<Node2D>("CanvasLayer/Indicator");
		_game = GetTree().Root.GetNode<Game>("GameWorld");

		_timer.WaitTime = _textSpeed;
		Start();
	}

	public void Start()
	{
		_finished = false;
		
		GetDialog(_file);
		
		NextPhrase();
	}

	public override void _Process(float delta)
	{
		if (_finalChoiceHit)
		{
			if (Input.IsActionJustPressed("jump"))
			{
				_game.NextLevel();
			}
			else if (Input.IsActionPressed("quit_last_screen"))
			{
				GetTree().Quit();
			}
		}
		else
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
			if (_lastScreen)
			{
				_text.Text = _lastScreenMessage;
				_text.VisibleCharacters = _lastScreenMessage.Length;
				_finalChoiceHit = true;

				return;
			}
			else
			{
				_game.NextLevel();
				
				return;
			}
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
