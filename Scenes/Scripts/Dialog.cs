using Godot;
using System;
using System.Collections.Generic;

public class Dialog : Control
{
	[Export] private float _textSpeed;

	private int _phraseNum;
	private bool _finished;
	private RichTextLabel _text;
	private RichTextLabel _name;
	private Timer _timer;
	private Polygon2D _indicator;
	private List<DialogItem> _dialog;
	
	public override void _Ready()
	{
		_name = GetNode<RichTextLabel>("Name");
		_text = GetNode<RichTextLabel>("Text");
		_timer = GetNode<Timer>("Timer");
		_timer.WaitTime = _textSpeed;
		_indicator = GetNode<Polygon2D>("Indicator");
	}

	public void Start(string dialogPath)
	{
		_finished = false;
		
		var node = GetParent<CanvasLayer>();
		node.Visible = true;
		
		_dialog = GetDialog(dialogPath);
		
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

	private List<DialogItem>  GetDialog(string dialogPath)
	{
		var file = new File();
		file.Open(dialogPath, File.ModeFlags.Read);
		string content = file.GetAsText();

		var jsonFile =  JSON.Parse(content).Result;
		
		var output =  jsonFile as List<DialogItem>;

		return output;
	}

	private async void NextPhrase()
	{
		if (_phraseNum == _dialog.Count)
		{
			QueueFree();
			return;
		}

		_finished = false;

		_name.Text = _dialog[_phraseNum].Name;
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
	public string Name { get; set; }
	public string Text { get; set; }
}
