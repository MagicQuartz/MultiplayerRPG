using Godot;
using System;

public class Gateway : Node
{
	NetworkedMultiplayerENet network = new NetworkedMultiplayerENet();
	MultiplayerAPI gateway_api = new MultiplayerAPI();
	int port = 1910;
	int max_players = 100;

	public override void _Ready()
	{
		StartServer();
	}

	public override void _Process(float delta)
	{
		if(!CustomMultiplayer.HasNetworkPeer())
			return;
		CustomMultiplayer.Poll();
	}

	private void StartServer()
	{
		network.CreateServer(port, max_players);
		CustomMultiplayer = gateway_api;
		CustomMultiplayer.RootNode = this;
		CustomMultiplayer.NetworkPeer = network;
		GD.Print("Gateway server started");

		network.Connect("peer_connected", this, "_Peer_Connected");
		network.Connect("peer_disconnected", this, "_Peer_Disconnected");
	}

	private void _Peer_Connected(int player_id)
	{
		GD.Print("User " + player_id.ToString() + " Connected");
	}

	private void _Peer_Disconnected(int player_id)
	{
		GD.Print("User " + player_id.ToString() + " Disconnected");
	}

	[Remote]
	public void LoginRequest(String username, String password)
	{
		GD.Print("Login request recieved");
		int player_id = CustomMultiplayer.GetRpcSenderId();
		Authenticate authenticate = GetNode<Authenticate>("/root/Authenticate");
		authenticate.AuthenticatePlayer(username, password, player_id);
	}

	[Remote]
	public void RegisterRequest(String username, String password)
	{
		GD.Print("Register request recieved");
		int player_id = CustomMultiplayer.GetRpcSenderId();
		Authenticate authenticate = GetNode<Authenticate>("/root/Authenticate");
		authenticate.RegisterPlayer(username, password, player_id);
	}

	public void ReturnLoginRequest(bool result, int player_id)
	{
		RpcId(player_id, "ReturnLoginRequest", result);
		network.DisconnectPeer(player_id);
	}

	public void ReturnRegisterRequest(bool result, int player_id)
	{
		RpcId(player_id, "ReturnRegisterRequest", result);
		network.DisconnectPeer(player_id);
	}
}
