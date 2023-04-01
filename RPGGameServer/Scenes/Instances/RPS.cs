using Godot;
using System;

public class RPS : Node
{
    private Player player1;
    private int selection1 = -2;
    private Player player2;
    private int selection2 = -2;

    // needs modifications

    public RPS(Player plr1, Player plr2)
    {
        this.player1 = plr1;
        this.player2 = plr2;
    }

    public Player Player1
    {
        get { return this.player1; }
    }

    public Player Player2
    {
        get { return this.player2; }
    }

    public int Selection1
    {
        get { return this.selection1; }
        set { this.selection1 = value; }
    }

    public int Selection2
    {
        get { return this.selection2; }
        set { this.selection2 = value; }
    }

    public void choose(Player player, int chose, ulong requester)
    {
        if(player1.username.Equals(player.username))
            selection1 = chose;
        else if(player2.username.Equals(player.username))
            selection2 = chose;

        if(selection1 != -2 && selection2 != -2) // -2 meaning the player hasn't chosen yet
            calculateResults(requester);
    }

    private void calculateResults(ulong requester)
    {
        if (selection1 == selection2)
            {
                //Tie
                GD.InstanceFromId(requester).Call("ResultRecieved", this, 0);
            }
            else if ((selection1 == 0 && selection2 == 2) || (selection1 == 1 && selection2 == 0) || (selection1 == 2 && selection2 == 1))
            {
                //Player 1 wins
                GD.InstanceFromId(requester).Call("ResultRecieved", this, 1);
            }
            else
            {
                //Player 2 wins
                GD.InstanceFromId(requester).Call("ResultRecieved", this, 2);
            }
    }
}
