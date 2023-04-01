using Godot;
using System;
using System.Collections.Generic;

public class Chat : Node
{
    Label label;
    LineEdit textBox;
    Label status;
    Tween[] statusTween = new Tween[2];
    GameServer server;
    Player player;

    String order = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890?!@#$%^&*()-=_+/`'~:;,.<>{} ";
    String scrambled = "";
    Random ran;



    public override void _Ready()
    {
        ran = new Random();
        label = GetNode<Label>("Panel/Label");
        textBox = GetNode<LineEdit>("Message/LineEdit");
        status = GetNode<Label>("Status");
        server = GetNode<GameServer>("/root/GameServer");
        
        player = GetNode<Player>("../../.."); // ../../.. = GetParent().GetParent().GetParent()

        textBox.Connect("text_entered", this, "_TextEntered");
        scrambled = Scramble(order);

        status.Modulate = new Color(1, 1, 1, 0);
        statusTween[0] = status.GetNode<Tween>("TweenOn");
        statusTween[1] = status.GetNode<Tween>("TweenOff");
        statusTween[0].InterpolateProperty(status, "modulate", new Color(1, 1, 1, 0), new Color(1, 1, 1, 1), .5f, Tween.TransitionType.Quad, Tween.EaseType.InOut); // Turn on
        statusTween[1].InterpolateProperty(status, "modulate", new Color(1, 1, 1, 1), new Color(1, 1, 1, 0), .5f, Tween.TransitionType.Quad, Tween.EaseType.InOut); // Turn off
    }

    public override void _Input(InputEvent @event)
    {
        //If player pressed chat button and isn't already in chat
        if(@event.IsActionReleased("chat") && !textBox.HasFocus())
        {
            //If player isn't already stunned (Stunned = Using a ui element)
            if(!player.GuiStun)
            {
                // Open chat
                player.GuiStun = true;
                player.toggleMovemenet(false);
                textBox.Visible = true;
                textBox.GrabFocus();
            }
        }
    }

    public void MsgChat(String username, String encrypted, String scrambled)
    {
        //Check if chat already has 7 lines
        if(label.Text.Split('\n').Length > 7)
        { // Remove first line
            int lineBreakIndex = label.Text.IndexOf('\n') + 1;
            label.Text = label.Text.Substr(lineBreakIndex, label.Text.Length - lineBreakIndex);
        }
        label.Text += username + ": " + Decrypt(encrypted, scrambled) + "\n";
    }


    public String Encrypt(String text, String key)
    {
        String output = "";
        for(int i = 0; i < text.Length; i++)
        {
            int index = order.IndexOf(text[i]);
            if(index != -1)
                output += key[index].ToString();
        }
        return output;
    }

    public String Decrypt(String encrypted, String key)
    {
        String output = "";
        for(int i = 0; i < encrypted.Length; i++)
        {
            int index = key.IndexOf(encrypted[i]);
            if(index != -1)
                output += order[index].ToString();
        }
        return output;
    }

    public String Scramble(String text)
    {
        //Disassembles the string to an array
        Char[] arr = text.ToCharArray();
        
        //Goes through said array and replaces the characters with other random characters in the array
        for(int i = 0; i < arr.Length; i++)
        {
            int replaceIndex = ran.Next(0, arr.Length - 1);
            Char temp = arr[i];
            arr[i] = arr[replaceIndex];
            arr[replaceIndex] = temp;
        }

        String result = new String(arr);
        //GD.Print(result.Length);
        return result;
    }

    public void _TextEntered(String text)
    { // Close chat and send message
            textBox.ReleaseFocus();
            player.GuiStun = false;
            player.toggleMovemenet(true);
            textBox.Visible = false;
            textBox.Text = "";
        if(!text.Equals(""))
        {
            server.SendMessage(Encrypt(text, scrambled), scrambled);
        }
    }

    List<String> queue = new List<String>();
    bool tweening = false;
    public async void statusMessage(String message, String username, float waitTime = 2f)
    {
        if(!username.Equals(player.getUsername()))
        {
            queue.Add(message);
            //If not already tweening
            if(!tweening)
            {
                tweening = true;
                status.Visible = true;
                statusTween[0].Start();
                await ToSignal(statusTween[0], "tween_completed");
                status.Text = queue[queue.Count - 1];
                queue.Remove(message);
                await ToSignal(GetTree().CreateTimer(waitTime), "timeout");
                statusTween[1].Start();
                await ToSignal(statusTween[1], "tween_completed");
                status.Text = "";
                status.Visible = true;
                tweening = false;
            }
            if(queue.Count > 0)
            statusMessage(queue[queue.Count - 1], player.getUsername());
        }
    }
}
