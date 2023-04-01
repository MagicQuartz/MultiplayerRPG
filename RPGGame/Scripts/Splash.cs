using Godot;
using System;

public class Splash : Control
{
	TextureRect logo;
	bool canMenu = false;

	public override void _Ready()
	{
		logo = GetNode<TextureRect>("Panel/Logo");
		logoAppear();
	}

	public override void _Process(float delta)
	{
		if(Input.IsActionJustReleased("select") && canMenu)
		{
			PackedScene menu = (PackedScene)ResourceLoader.Load("res://Scenes/Menu.tscn");
			GetTree().ChangeSceneTo(menu);
		}
	}

	public async void logoAppear()
	{
		AudioStreamPlayer audio = GetNode<AudioStreamPlayer>("Panel/AudioStreamPlayer");
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		audio.Play();
		await ToSignal(GetTree().CreateTimer(.2f), "timeout");
		logo.Visible = true;
		canMenu = true;
	}
}
