using Godot;
using System;

public class RockPaperScissors : Control
{
	private String battling;
	private Panel BattleUI;
	private Panel WaitingUI;
	private Panel CanceledUI;
	private Panel WinLoseUI;

	private AudioStreamPlayer[] sounds = new AudioStreamPlayer[3]; // Win sound

	GameServer server;

	public override void _Ready()
	{
		BattleUI = GetNode<Panel>("Battle");
		WaitingUI = GetNode<Panel>("Waiting");
		CanceledUI = GetNode<Panel>("Canceled");
		WinLoseUI = GetNode<Panel>("WinLose");
		server = GetNode<GameServer>("/root/GameServer");

		sounds[1] = WinLoseUI.GetNode<AudioStreamPlayer>("WinStreamPlayer");
		sounds[2] = WinLoseUI.GetNode<AudioStreamPlayer>("TieStreamPlayer");
		sounds[0] = WinLoseUI.GetNode<AudioStreamPlayer>("LoseStreamPlayer");
	}

	public void battleStart(String username)
	{
		battling = username;
		Label challenger = BattleUI.GetNode<Label>("Challenger");
		challenger.Text = "Opponent: " + username;
		BattleUI.Visible = true;
	}

	public void _RockPressed()
	{
		// Send answer to server
		chose(0);
	}

	public void _PaperPressed()
	{
		chose(1);
	}

	public void _ScissorsPressed()
	{
		// Send answer to server
		chose(2);
	}

	public void _CancelPressed()
	{
		// Send answer to server
		chose(-1);
	}

	public void chose(int option)
	{
		server.RPSChose(option);
		BattleUI.Visible = false;
		if(option != -1)
		{
			WaitingUI.Visible = true;
			GetNode<ThreeDots>("Waiting/Text").Play();
		} else
		{
			WaitingUI.Visible = false;
			CanceledUI.Visible = false;
			battling = null;
		}

	}

	public async void ActionPerformed(int action)
	{// -1 = cancel, 0 = lose, 1 = win, 2 = tie
		BattleUI.Visible = false;
		WaitingUI.Visible = false;
		GetNode<ThreeDots>("Waiting/Text").Stop();
		if(action == -1)
		{
			WinLoseUI.Visible = false;
			CanceledUI.Visible = true;
			await ToSignal(GetTree().CreateTimer(1), "timeout");
			CanceledUI.Visible = false;
			battling = null;
		} else
		{
			Label text = WinLoseUI.GetNode<Label>("Text");
			WaitingUI.Visible = false;
			if(action == 0)
			{
				text.Text = "You lost!";
				WinLoseUI.Visible = true;
				sounds[action].Play();
				await ToSignal(GetTree().CreateTimer(1), "timeout");
				WinLoseUI.Visible = false;
			} else if(action == 1)
			{
				text.Text = "You won!";
				WinLoseUI.Visible = true;
				sounds[action].Play();
				await ToSignal(GetTree().CreateTimer(1), "timeout");
				WinLoseUI.Visible = false;
			} else if(action == 2)
			{
				text.Text = "It's a tie.";
				WinLoseUI.Visible = true;
				sounds[action].Play();
				await ToSignal(GetTree().CreateTimer(1), "timeout");
				WinLoseUI.Visible = false;
			}
		} 
	}
}
