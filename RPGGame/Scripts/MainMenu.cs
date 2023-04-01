using Godot;
using System;

public class MainMenu : Control
{
    public override void _Ready()
    {
        AudioStreamPlayer audio = GetNode<AudioStreamPlayer>("Panel/AudioStreamPlayer");
        audio.Play();
    }
    
}
