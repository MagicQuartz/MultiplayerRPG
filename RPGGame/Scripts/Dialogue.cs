using Godot;
using System;
using System.Collections.Generic;

public class Dialogue : Control
{
    [Export] float waitTime = .05f;
    [Signal] public delegate void select();
    Player player;

    public override void _Ready()
    {
        player = GetNode<Player>("../../.."); // ../../.. = GetParent().GetParent().GetParent()
        base._Ready();
    }

    public override void _PhysicsProcess(float delta)
    {
        if(Input.IsActionJustReleased("select") && Visible)
            EmitSignal("select");
        base._PhysicsProcess(delta);
    }

    public async void startDialogue(List<String> lines)
    {
        if(!player.GuiStun)
        {
            player.GuiStun = true;
            Visible = true;
            Label label = GetNode<Label>("Panel/Label");
            for(int i = 0; i < lines.Count; i++) // j refers to the line number, List Count == Array Length
            {
                for(int j = 0; j <= lines[i].Length; j++)// j refers to the current letter in the line/sentence
                {
                    label.Text = lines[i].Substring(0, j);
                    await ToSignal(GetTree().CreateTimer(waitTime), "timeout");
                }
                await ToSignal(this, "select");
            }
            Visible = false;
            player.dialogueOver();
            player.GuiStun = false;
        }
    }
}
