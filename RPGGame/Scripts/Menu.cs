using Godot;
using System;

public class Menu : Control
{
    [Export] int selected = 0;
    Label[] labels;
    Player player;

    public override void _Ready()
    {
        Panel panel = GetNode<Panel>("Panel");
        labels = new Label[] {
            panel.GetNode<Label>("Inventory"),
            panel.GetNode<Label>("Party"),
            panel.GetNode<Label>("Settings"),
            panel.GetNode<Label>("Quit")

        };
        
        player = GetNode<Player>("../../.."); // ../../.. = GetParent().GetParent().GetParent()
        base._Ready();
    }

    public override void _PhysicsProcess(float delta)
    {
        if(Input.IsActionJustPressed("start") && !Visible)
        {
            if(!player.GuiStun)
                toggleMenu(true);
        }
        if(Input.IsActionJustPressed("back") && Visible)
        {
            toggleMenu(false);
        }

        if(Visible)
        {
            int select = selected;
            if(Input.IsActionJustPressed("move_down"))
            {
                select += 1;
                if(select > 3)
                    select = 0;
                setStar(select);
            }
            if(Input.IsActionJustPressed("move_up"))
            {
                select -= 1;
                if(select < 0)
                    select = 3;
                setStar(select);
            }
            if(Input.IsActionJustPressed("select"))
            {
                if(labels[selected].Name.Equals("Quit"))
                    Quit();
            }
        }

        base._PhysicsProcess(delta);
    }

    public void toggleMenu(bool toggle)
    {
        player.GuiStun = toggle;
        Visible = toggle; //When the menu turns visible, the player can no longer move and vice versa
        player.toggleMovemenet(!toggle);
        if(!toggle)
        {
            setStar(0);
        }
    }

    private void setStar(int select)
    {
        if(labels != null)
        {
            labels[selected].GetNode<TextureRect>("Star").Visible = false;
            labels[select].GetNode<TextureRect>("Star").Visible = true;
            selected = select;
        }
    }

    private void Quit()
    {
        GetTree().Quit();
    }
}
