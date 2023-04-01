using Godot;
using System;

public class Gateway : Node
{
    NetworkedMultiplayerENet network = new NetworkedMultiplayerENet();
    MultiplayerAPI gateway_api = new MultiplayerAPI();
    String ip = "127.0.0.1";
    int port = 1910;

    String username = "NaN";
    String password;

    bool connectionAttemptOver = false;

    public override void _Ready()
    {
        return;
    }

    public override void _Process(float delta)
    {
        if(CustomMultiplayer == null)
            return;
        if(!gateway_api.HasNetworkPeer())
            return;
        CustomMultiplayer.Poll();
    }

    public void ConnectToServer(String _username, String _password, bool register = false)
    {
        network = new NetworkedMultiplayerENet();
        gateway_api = new MultiplayerAPI();
        username = _username;
        password = _password;
        network.CreateClient(ip, port);
        CustomMultiplayer = gateway_api;
        CustomMultiplayer.RootNode = this;
        CustomMultiplayer.NetworkPeer = network;

        if(register)
        {
            network.Connect("connection_failed", this, "_OnConnectionRegisterFailed");
            network.Connect("connection_succeeded", this, "_OnConnectionRegisterSucceeded");
        }
        else // login
        {
            network.Connect("connection_failed", this, "_OnConnectionFailed");
            network.Connect("connection_succeeded", this, "_OnConnectionSucceeded");
        }
    }

    private void _OnConnectionFailed()
    {
        GD.Print("Failed to connect to login server");
        GD.Print("Pop-up server offline or something");
        Control loginScreen = GetNode<Control>("/root/Menu/LoginScreen");
        loginScreen.Call("displayError", "Error logging in: The server is probably offline, come back later and try again.");
        loginScreen.GetNode<Button>("Panel/Register").Disabled = false;
        loginScreen.GetNode<Button>("Panel/Login").Disabled = false;
        connectionAttemptOver = true;
    }    

    private void _OnConnectionRegisterFailed()
    {   
        GD.Print("Failed to connect to login server to register");
        GD.Print("Pop-up server offline or something");
        Control loginScreen = GetNode<Control>("/root/Menu/LoginScreen");
        loginScreen.Call("displayError", "Error trying to register: The server is probably offline, come back later and try again.");
        loginScreen.GetNode<Button>("Panel/Register").Disabled = false;
            loginScreen.GetNode<Button>("Panel/Login").Disabled = false;
        connectionAttemptOver = true;
    }

    private void _OnConnectionSucceeded()
    {
        GD.Print("Successfully connected to login server");
        RequestLogin();
        connectionAttemptOver = true;
    }

    private void _OnConnectionRegisterSucceeded()
    {
        GD.Print("Successfully connected to login server to register");
        RequestRegister();
        connectionAttemptOver = true;
    }

    public void RequestLogin()
    {
        GD.Print("Connecting to gateway to request login");
        RpcId(1, "LoginRequest", username, password);
        //username = "";
        password = "";
    }

    public void RequestRegister()
    {
        GD.Print("Connecting to gateway to request register");
        RpcId(1, "RegisterRequest", username, password);
        username = "";
        password = "";
    }

    [Remote]
    public void ReturnLoginRequest(bool results)
    {
        GD.Print("Results received");
        Control loginScreen = GetNode<Control>("/root/Menu/LoginScreen");
        if(results)
        {
            //Successfull login attempt
            GameServer gameServer = GetNode<GameServer>("/root/GameServer");
            gameServer.connectToServer();
            loginScreen.QueueFree();
            GetTree().ChangeScene("res://Scenes/Main.tscn");
            gameServer.FetchPlayers();
        } else
        {
            GD.Print("Please provide correct username and password");
            loginScreen.Call("displayError", "Please provide correct username and password", -1);
            loginScreen.GetNode<Button>("Panel/Register").Disabled = false;
            loginScreen.GetNode<Button>("Panel/Login").Disabled = false;
        }
        network.Disconnect("connection_failed", this, "_OnConnectionFailed");
        network.Disconnect("connection_succeeded", this, "_OnConnectionSucceeded");
    }

    [Remote]
    public void ReturnRegisterRequest(bool results)
    {
        GD.Print("Register results received");
        Control loginScreen = GetNode<Control>("/root/Menu/LoginScreen");
        if(results)
        {
            GD.Print("Success registering!");
            loginScreen.Call("displayError", "Success! Try logging in.", -1);
            loginScreen.GetNode<Button>("Panel/Register").Disabled = false;
            loginScreen.GetNode<Button>("Panel/Login").Disabled = false;
        } else
        {
            GD.Print("Username taken!");
            loginScreen.Call("displayError", "Username taken!", 2f);
            loginScreen.GetNode<Button>("Panel/Register").Disabled = false;
            loginScreen.GetNode<Button>("Panel/Login").Disabled = false;
        }
        network.Disconnect("connection_failed", this, "_OnConnectionRegisterFailed");
        network.Disconnect("connection_succeeded", this, "_OnConnectionRegisterSucceeded");

        //network.CloseConnection();
    }

    public String getUsername()
    {
        return username;
    }
}
