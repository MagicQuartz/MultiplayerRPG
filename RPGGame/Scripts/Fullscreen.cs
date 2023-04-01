using Godot;
using System;

public class Fullscreen : Node
{
    public override void _Process(float delta)
    {
        if(Input.IsActionJustPressed("toggle_fullscreen"))
			OS.WindowFullscreen = !OS.WindowFullscreen;
    }
}
