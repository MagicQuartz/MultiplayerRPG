using Godot;
using System;

public class LoginScreen : Control
{
    LineEdit username_input;
    LineEdit userpassword_input;
    Button login_button;
    Button register_button;
    Label error_text;

    String allowed_characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890_";

    public override void _Ready()
    {
        username_input = GetNode<LineEdit>("Panel/Username");
        userpassword_input = GetNode<LineEdit>("Panel/Password");
        login_button = GetNode<Button>("Panel/Login");
        register_button = GetNode<Button>("Panel/Register");
        error_text = GetNode<Label>("Panel/Error");
    }

    
    private void _on_Login_pressed()
    {
        if(username_input.Text.Equals("") || userpassword_input.Text.Equals(""))
        {
            //popup and stop
            GD.Print("Please provide valid userID and password");
            displayError("Please provide valid username and password");
        }
        else
        {
            register_button.Disabled = true;
            login_button.Disabled = true;
            String username = username_input.Text;
            String password = userpassword_input.Text;
            GD.Print("Attempting to login");
            displayError("Attempting to login...", -1f);
            Gateway gateway = GetNode<Gateway>("/root/Gateway");
            gateway.ConnectToServer(username, password);
        }
    }
    
    private void _on_Register_pressed()
    {
        String username = username_input.Text;
        String password = userpassword_input.Text;
        if(username.Length < 2 && password.Length < 6)
        {
            //popup and stop
            GD.Print("Both fields are too short!");
            displayError("Both fields are too short!");
        } else if(username.Length < 2 && password.Length >= 6)
        {
            GD.Print("Username too short...");
            displayError("Username must be longer...");
        } else if(username.Length >= 2 && password.Length < 6)
        {
            GD.Print("Password too short...");
            displayError("Password must be longer...");
        } else if(username.StartsWith("NPC") || username.StartsWith("Player"))
        {
            GD.Print("Inappropriate username...");
            displayError("Username can't start like this!");
        } else
        {
            //Quick character check
            foreach(char letter in username)
            {
                GD.Print(letter);
                if(allowed_characters.Find(letter) == -1)
                {
                    GD.Print("Unvalid character...");
                    displayError("Unvalid character in username...");
                    return;
                }
            }

            register_button.Disabled = true;
            login_button.Disabled = true;
            Gateway gateway = GetNode<Gateway>("/root/Gateway");
            gateway.ConnectToServer(username, password, true);
        }
    }

    public async void displayError(String text, float duration = 3f)
    {
        if(error_text != null)
        {
            error_text.Text = text;
            error_text.Visible = true;
            if(duration != -1f)
            {
                await ToSignal(GetTree().CreateTimer(duration), "timeout");
                error_text.Visible = false;
            }
            
        }
    }
}
