using Godot;
using System;

public class Authenticate : Node
{
	NetworkedMultiplayerENet network = new NetworkedMultiplayerENet();
	String ip = "127.0.0.1";
	int port = 1911;

	public override void _Ready()
	{
		ConnectToServer();
	}

	private void ConnectToServer()
	{
		network.CreateClient(ip, port);
		GetTree().NetworkPeer = network;

		network.Connect("connection_failed", this, "_OnConnectionFailed");
		network.Connect("connection_succeeded", this, "_OnConnectionSucceeded");
	}

	private void _OnConnectionFailed()
	{
		GD.Print("Failed to connect to authentication server");
	}

	private void _OnConnectionSucceeded()
	{
		GD.Print("Successfully connected to authentication server");
	}

	public void AuthenticatePlayer(String username, String password, int player_id)
	{
		GD.Print("Sending out authentication request");
		RpcId(1, "AuthenticatePlayer", username, password, player_id);
	}
	
	public void RegisterPlayer(String username, String password, int player_id)
	{
		GD.Print("Checking availability...");
		RpcId(1, "RegisterPlayer", username, password, player_id);
	}

	[Remote]
	public void AuthenticationResults(bool result, int player_id)
	{
		GD.Print("Results received and replying to player login reqest");
		Gateway gateway = GetNode<Gateway>("/root/Gateway");
		gateway.ReturnLoginRequest(result, player_id);
	}

	[Remote]
	public void RegisterResults(bool result, int player_id)
	{
		GD.Print("Results received and replying to player register request");
		Gateway gateway = GetNode<Gateway>("/root/Gateway");
		gateway.ReturnRegisterRequest(result, player_id);
	}
}
