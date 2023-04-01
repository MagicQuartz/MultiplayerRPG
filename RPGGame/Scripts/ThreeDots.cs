using Godot;
using System;

public class ThreeDots : Label
{
    [Export] public String prefix = "";
    [Export] public String suffix = "";

    bool playing = false;

    public async void Play()
    {
        playing = true;
        String dots = ".";
        while(playing)
        {
            if(Visible)
            {
                await ToSignal(GetTree().CreateTimer(.4f), "timeout");
                Text = prefix + dots + suffix;
                dots = dots + ".";
                if(dots.Length > 3)
                 dots = ".";
            }
        }
    }

    public void Stop()
    {
        playing = false;
        Text = prefix + "..." + suffix;
    }
}
