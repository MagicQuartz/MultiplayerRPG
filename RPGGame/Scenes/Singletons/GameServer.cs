using Godot;
using Godot.Collections;
using System;
using System.Linq;

public class GameServer : Node
{
    NetworkedMultiplayerENet network = new NetworkedMultiplayerENet();
    String ip = "127.0.0.1";
    int port = 1909;
    [Signal] public delegate void connected();
    bool isConnected = false;

    public Dictionary localData = new Dictionary();

    private PackedScene playerLoad = (PackedScene)GD.Load("res://Objects/Player.tscn");

    public override void _Ready()
    {
        //connectToServer();
        return;
    }

    public override void _Process(float delta)
    {
        if(isConnected && GetNode("/root/Main") != null)
        {
            Player player = GetNode<Player>("/root/Main/YSort/Player");
            RpcUnreliableId(1, "UpdatePlayer", player.getUsername(), player.Position);
        }
    }

    public void connectToServer()
    {
        network.CreateClient(ip, port);
        GetTree().NetworkPeer = network;

        network.Connect("connection_failed", this, "_onConnectionFailed");
        network.Connect("connection_succeeded", this, "_onConnectionSucceeded");
    }

    public void _onConnectionFailed()
    {
        GD.Print("Failed to connect");
    }

    public void _onConnectionSucceeded()
    {
        EmitSignal("connected");
        isConnected = true;
        GD.Print("Successfully connected");

        RpcId(1, "AddPlayer", FetchUsername());//, GetTree().GetNetworkUniqueId());
        RpcId(1, "GetUserPosition");
    }

    public async void FetchProperty(String property, ulong requester)
    {
        if(!isConnected)
            await ToSignal(this, "connected");
        RpcId(1, "FetchProperty", property, requester);
    }

    [Remote]
    public void ReturnProperty(String property, int value, ulong requester)
    {
        GD.InstanceFromId(requester).Call("set" + property, value);
    }

    private String FetchUsername()
    {
        Gateway gateway = GetNode<Gateway>("/root/Gateway");
        return gateway.getUsername();
    }

    public async void FetchPlayers()
    {
        if(!isConnected)
            await ToSignal(this, "connected");
        RpcId(1, "FetchPlayers"/*, GetInstanceId()*/);
    }

    [Remote]
    public void DrawPlayer(String username)
    {
        GD.Print("Got draw attempt for " + username);
        if(!username.Equals(FetchUsername()))
        {
            GD.Print("Got draw player " + username);
            YSort players = GetNode<YSort>("/root/Main/YSort");
            Player playerInstance = (Player) playerLoad.Instance();
            playerInstance.Name = username;
            players.AddChild(playerInstance);
        }
    }

    [Remote]
    public void UpdatePlayer(String username, Vector2 pos) //uses RpcUnreliable
    {
        if(username != FetchUsername())
        {   
            YSort players = GetNode<YSort>("/root/Main/YSort");
            foreach(Node player in players.GetChildren())
            {
                if(!player.Name.Equals("Player") && !player.Name.StartsWith("NPC"))
                {
                    if(player.Call("getUsername").Equals(username))
                    {
                        Player movingPlayer = players.GetNode<Player>(username);
                        //GD.Print("Moving " + username + " to " + pos.ToString());
                        movingPlayer.Position = pos;
                    }
                }
            }
        }
    }

    [Remote]
    public void RemovePlayer(String username)
    {
        YSort players = GetNode<YSort>("/root/Main/YSort");
        foreach(Node player in players.GetChildren())
        {
            if(!player.Name.Equals("Player") && !player.Name.StartsWith("NPC"))
                {
                    if(player.Call("getUsername").Equals(username))
                    {
                        player.QueueFree();
                    }
                }
        }
    }

    //Going to the server
    public void SendMessage(String message, String key)
    {
        RpcId(1, "SendMessage", FetchUsername(), message, key);
    }

    // Coming from the server
    [Remote]
    public void MsgChat(String username, String encrypted, String key)
    {
        Chat chat = GetNode<Chat>("/root/Main/YSort/Player/Camera2D/CanvasLayer/Chat");
        chat.MsgChat(username, encrypted, key);
    }

    /*[Remote]
    public void StatusMessage(String message, String username, float waitTime = 2f)
    {
        Chat chat = GetNode<Chat>("/root/Main/YSort/Player/Camera2D/CanvasLayer/Chat");
        chat.statusMessage(message, username, waitTime);
    }*/

    [Remote]
    public void ReturnUserData(String property, object value)
    {
        if(keyExists(localData, property))
            localData[property] = value;
        else
            localData.Add(property, value);
    }

    [Remote]
    public void ReturnUserPosition(Vector2 position)
    {
        Player player = GetNode<Player>("/root/Main/YSort/Player");
        player.Position = position;
    }

    public bool keyExists(Dictionary d, String key)
	{
		foreach(String property in d.Keys)
		{
			if(property.Equals(key))
				return true;
		}
		return false;
	}


    //Challenging a player to a Rock Paper Scissors game, functions below:

    public void challengeRPS(String challenged)
    {
        RpcId(1, "challengeRPS", challenged);
    }

    public void RPSDecide(String challenger, bool accepted)
    {
        RpcId(1, "RPSDecide", challenger, accepted);
    }

    [Remote]
    public void RockPaperScissors(String challenger)
    { // This is received from the server when another player sends a RPS (Rock Paper Scissors) request
        Player player = GetNode<Player>("/root/Main/YSort/Player");
        player.RockPaperScissors(challenger);
    } 

    [Remote]
    public void Results(String challenged, bool accepted)
    {
        Player player = GetNode<Player>("/root/Main/YSort/Player");
        player.RPSResult(challenged, accepted);
    }


    //The Rock Paper Scissors game itself:

    public void RPSChose(int option)
    {// 0 = rock, 1 = paper, 2 = scissors, -1 = cancel
        RpcId(1, "RPSPlayerChose", option);
        //Needs to be completed on the server side
    }

    [Remote]
    public void RPSAction(int action)
    {// -1 = cancel
        Player player = GetNode<Player>("/root/Main/YSort/Player");
        player.RPSAction(action);
    }
}
