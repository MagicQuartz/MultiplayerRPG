using Godot;
using System;

public class Player : Node
{
    private int Id;
    private String Username;
    private Vector2 Position = Vector2.Zero;
    private RPS RpsInstance; // Rock paper scissors

    public int id
    {
        get { return this.Id; }
        set { this.Id = value; }
    }

    public String username
    {
        get { return this.Username; }
        set { this.Username = value; }
    }
    public Vector2 position
    {
        get { return this.Position; }
        set { this.Position = value; }
    }

    public Player(int id, String username)
    {
        this.Id = id;
        this.username = username;
    }
}
