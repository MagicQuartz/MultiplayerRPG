using Godot;
using System;
using Godot.Collections;
using System.Collections;

public class Authenticate : Node
{
	NetworkedMultiplayerENet network = new NetworkedMultiplayerENet();
	int port = 1911;
	int max_servers = 10;

	PlayerData playerData;

	public override void _Ready()
	{
		StartServer();
	}

	private void StartServer()
	{
		network.CreateServer(port, max_servers);
		GetTree().NetworkPeer = network;
		GD.Print("Authentication server started");

		network.Connect("peer_connected", this, "_Peer_Connected");
		network.Connect("peer_disconnected", this, "_Peer_Disconnected");
	}

	private void _Peer_Connected(int gateway_id)
	{
		GD.Print("Gateway " + gateway_id.ToString() + " Connected");
	}

	private void _Peer_Disconnected(int gateway_id)
	{
		GD.Print("Gateway " + gateway_id.ToString() + " Disconnected");
	}

	[Remote]
	public void AuthenticatePlayer(String username, String password, int player_id)
	{
		GD.Print("Authentication request received");
		int gateway_id = GetTree().GetRpcSenderId();
		bool result = false;
		GD.Print("Starting authentication");
		playerData = GetNode<PlayerData>("/root/PlayerData");
		//
		if(!usernameExists(username))
			GD.Print("User not recognized");
		else
		{
			Dictionary userData = (Dictionary) playerData.PlayerIDs[username];
			if(userData["Password"].ToString() != password)
				GD.Print("Incorrect password");
			else // Correct password and username
			{
				GD.Print("Successfull authentication");
				result = true;
			}
		}
		/* Replacement above
		if(!playerData.PlayerIDs.Has(username))
			GD.Print("User not recognized");
		else if(!playerData.PlayerIDs[username].Password == password)
			GD.Print("Incorrect password");
		else
		{
			GD.Print("Successfull authentication");
			result = true;
		}
		*/
		GD.Print("Authentication result send to gateway server");
		RpcId(gateway_id, "AuthenticationResults", result, player_id);
	}

	[Remote]
	public void RegisterPlayer(String username, String password, int player_id)
	{
		GD.Print("Register request received");
		int gateway_id = GetTree().GetRpcSenderId();
		bool result = false;
		GD.Print("Starting check");
		playerData = GetNode<PlayerData>("/root/PlayerData");
		//
		if(usernameExists(username))
			GD.Print("User recognized");
		else
		{
			GD.Print("User not recognized, registering...");
			playerData.addUser(username, password);
			result = true;
		}
		GD.Print("Register result send to gateway server");
		RpcId(gateway_id, "RegisterResults", result, player_id);
	}

	private bool usernameExists(String username)
	{
		foreach(String user in playerData.PlayerIDs.Keys)
		{
			if(user.Equals(username))
				return true;
		}
		return false;
	}
}
