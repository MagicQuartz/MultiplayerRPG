using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class GameServer : Node
{
	NetworkedMultiplayerENet network = new NetworkedMultiplayerENet();
	int port = 1909;
	int max_players = 10;

	List<Player> players = new List<Player>();
	List<RPS> minigameInstances = new List<RPS>();

	public override void _Ready()
	{
		startServer();
	}

	public void startServer()
	{
		network.CreateServer(port, max_players);
		GetTree().NetworkPeer = network;
		GD.Print("Server started");

		network.Connect("peer_connected", this, "_Peer_Connected");
		network.Connect("peer_disconnected", this, "_Peer_Disconnected");
	}

	public void _Peer_Connected(int player_id)
	{
		GD.Print("User " + player_id.ToString() + " connected");
	}

	public void _Peer_Disconnected(int player_id)
	{
		GD.Print("User " + player_id.ToString() + " disconnected");
		/*String usrnm = players.Find(plr => plr.id == player_id).username;
		GD.Print(usrnm + " disconnected");
		Rpc("StatusMessage", usrnm + " left the game", usrnm);*/
		RemovePlayer(player_id);
	}

	[Remote]
	public void FetchProperty(String property, ulong requester)
	{
		GD.Print("Got request");
		int player_id = GetTree().GetRpcSenderId();
		ServerData serverData = GetNode<ServerData>("/root/serverData");
		float value = (float) serverData.property_data[property]; //property = skill_name
		GD.Print("Sending " + value.ToString() + " to player " + player_id.ToString());
		RpcId(player_id, "ReturnProperty", property, value, requester);
		GD.Print("Sent successfully!");
	}

	[Remote]
	public void AddPlayer(String username)//, int player_id)
	{
		int player_id = GetTree().GetRpcSenderId();
		PlayerData playerData = GetNode<PlayerData>("/root/PlayerData");
		if(!playerData.userExists(username))
			playerData.addUser(username);

		players.Add(new Player(player_id, username));
		GD.Print("Added " + username + " to list.");
		Rpc("DrawPlayer", username);
	}

	[Remote]
	public void FetchPlayers(/*ulong requester*/)
	{
		int sender_id = GetTree().GetRpcSenderId();
		foreach(Player player in players)
		{
			if(player.id != sender_id)
				RpcId(sender_id, "DrawPlayer", player.username);
		}
	}

	private void RemovePlayer(int player_id)
	{
		Player player = players.Find(plr => plr.id == player_id);
		players.Remove(player);
		SetUserPosition(player.username, player.position);
		Rpc("RemovePlayer", player.username);
	}

	[Remote]
	public void UpdatePlayer(String username, Vector2 position)
	{ // Why, player position, of course!
		Player updatedPlayer = players.FirstOrDefault(player => player.username == username);
		if (updatedPlayer != null)
	   	{
			//GD.Print("Updating " + username + "'s position to " + position.ToString());
			updatedPlayer.position = position;
			RpcUnreliable("UpdatePlayer", username, position);
		}
	}

	[Remote]
	public void SendMessage(String username, String encrypted, String key)
	{
		Rpc("MsgChat", username, encrypted, key);
	}

	/*[Remote]
	public void InitialDataGet(String username) // Moved to AddPlayer
	{
		// Needs to be called on client and returned to it when joining
		PlayerData playerData = GetNode<PlayerData>("/root/PlayerData");
		if(!playerData.userExists(username))
			playerData.addUser(username);
	}*/

	[Remote]
	public void GetUserData(String property)
	{
		int sender_id = GetTree().GetRpcSenderId();
		String username = players.Find(player => player.id == sender_id).username;
		PlayerData playerData = GetNode<PlayerData>("/root/PlayerData");
		/*if(!playerData.userExists(username))
			FirstTimer(username);*/
		
		/*Dictionary userData = (Dictionary) playerData.PlayerIDs[username];
		if(playerData.keyExists(userData, property))
		{
			object value = userData[property];
			// ReturnUserData still needs to be created, wip
			// Could being a dictionary instead of just one value
		}*/
		
		object value = playerData.readData(username, property);
		RpcId(sender_id, "ReturnUserData", property, value);
	}

	[Remote]
	public void GetUserPosition()
	{
		// Add in client
		int sender_id = GetTree().GetRpcSenderId();
		String username = players.Find(player => player.id == sender_id).username;
		PlayerData playerData = GetNode<PlayerData>("/root/PlayerData");

		
		float x = (float) playerData.readData(username, "PosX");
		float y = (float) playerData.readData(username, "PosY");
		Vector2 position = new Vector2(x, y);

		RpcId(sender_id, "ReturnUserPosition", position);
	}

	private void SetUserPosition(String username, Vector2 pos)
	{
		PlayerData playerData = GetNode<PlayerData>("/root/PlayerData");
		
		playerData.saveData(username, "PosX", pos.x);
		playerData.saveData(username, "PosY", pos.y);
	}

	[Remote]
	public void SetUserData(String property, object value)
	{
		int sender_id = GetTree().GetRpcSenderId();
		String username = players.Find(player => player.id == sender_id).username;

		PlayerData playerData = GetNode<PlayerData>("/root/PlayerData");
		playerData.saveData(username, property, value);
	}

	[Remote]
	public void challengeRPS(String challenged)
	{
		int challenger_id = GetTree().GetRpcSenderId();
		String challenger = players.Find(player => player.id == challenger_id).username;
		int challenged_id = players.Find(player => player.username == challenged).id;
		RpcId(challenged_id, "RockPaperScissors", challenger);
	}

	[Remote]
	public void RPSDecide(String challenger_username, bool accepted)
	{
		int challenged_id = GetTree().GetRpcSenderId();
		Player challenged = players.Find(player => player.id == challenged_id);
		Player challenger = players.Find(player => player.username == challenger_username);

		if(accepted)
			minigameInstances.Add(new RPS(challenger, challenged)); // change to under the player
		RpcId(challenger.id, "Results", challenged.username, accepted);
	}

	// Rock paper scissors - actual game
	[Remote]
	public void RPSPlayerChose(int option)
	{// -1 = cancel, 0 = rock, 1 = paper, 2 = scissors
		int chooser_id = GetTree().GetRpcSenderId();
		Player chooser = players.Find(plr => plr.id == chooser_id);
		RPS game = minigameInstances.Find(minigame => minigame.Player1 == chooser || minigame.Player2 == chooser);
		if(game != null)
		{
			Player otherPlayer = null;
			if(chooser == game.Player1)
			{
				otherPlayer = game.Player2;
			} else if(chooser == game.Player2)
			{
				otherPlayer = game.Player1;
			}
			playerChoose(game, chooser, otherPlayer, option);
		}
	}

	private void playerChoose(RPS game, Player chooser, Player otherPlayer, int option)
	{// -1 = cancel, 0 = rock, 1 = paper, 2 = scissors
		if(option == -1) // Cancel
		{
			RpcId(otherPlayer.id, "RPSAction", option);
			minigameInstances.Remove(game);
		} else
		{
			game.choose(chooser, option, GetInstanceId());
		}
	}

	public void ResultRecieved(RPS game, int winningPlayer)
	{
		if(winningPlayer == 0) // no winner - tie
		{
			RpcId(game.Player1.id, "RPSAction", 2);
			RpcId(game.Player2.id, "RPSAction", 2);
		} else
		{ // 0 = lose, 1 = win, 2 = tie
			if(winningPlayer == 1) // Player 1 won
			{
				RpcId(game.Player1.id, "RPSAction", 1);
				RpcId(game.Player2.id, "RPSAction", 0);
			}
			if(winningPlayer == 2) // Player 2 won
			{
				RpcId(game.Player1.id, "RPSAction", 0);
				RpcId(game.Player2.id, "RPSAction", 1);
			}
				
		}
		minigameInstances.Remove(game);
	}
}
