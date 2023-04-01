using Godot;
using System;

public class Key : Control
{
    String action;
    
    Texture keyDown = (Texture)GD.Load("res://Assets/Images/key_down.png");
    Texture keyUp = (Texture)GD.Load("res://Assets/Images/key_up.png");
    TextureRect rect;
    Label keyName;
    Label actionName;

    public override void _Ready()
    {
        rect = GetNode<TextureRect>("TextureRect");
        keyName = GetNode<Label>("KeyName");
        actionName = GetNode<Label>("ActionName");
    }

    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventKey)
        {
            if(action != null && rect != null)
            {
                if(@event.IsActionPressed(action))
                {
			        rect.Texture = keyDown;
                    keyName.RectPosition += new Vector2(0, 2);
                }
                else if(@event.IsActionReleased(action))
                {
			        rect.Texture = keyUp;
                    keyName.RectPosition -= new Vector2(0, 2);
                }
            }
        }

    }

    public void setPress(String action, String keyText, String actionText)
    {
        if(keyName != null && actionName != null)
        {
            this.action = action;
            keyName.Text = keyText;
            actionName.Text = actionText;
        }
    }
}
