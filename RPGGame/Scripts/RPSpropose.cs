using Godot;
using System;

public class RPSpropose : Control
{
    String currentlyChallenging;

    public void Challenge(String challenger)
    {
        Visible = true;
        currentlyChallenging = challenger;
    }

    public void _Accept()
    {
        GameServer server = GetNode<GameServer>("/root/GameServer");
        server.RPSDecide(currentlyChallenging, true);
        // Show rock paper scissors ui
        GetParent().GetNode<RockPaperScissors>("RockPaperScissors").battleStart(currentlyChallenging);
        // Change in the future ..?
        currentlyChallenging = null;
        Visible = false;

    }

    public void _Decline()
    {
        GameServer server = GetNode<GameServer>("/root/GameServer");
        server.RPSDecide(currentlyChallenging, false);
        currentlyChallenging = null;
        Visible = false;
    }


    public void RPSResult(String challenged, bool accepted)
	{ // The sender receives the results
		Visible = false;
        //GD.Print("Results recieved from " + challenged + ", accepted: " + accepted);
        currentlyChallenging = null;
        if(accepted)
        {
            // Show rock paper scissors ui
            GetParent().GetNode<RockPaperScissors>("RockPaperScissors").battleStart(challenged);
        }
	}
}