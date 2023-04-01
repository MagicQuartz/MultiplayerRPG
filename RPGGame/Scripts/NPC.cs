using Godot;
using System;
using System.Collections.Generic;

public enum Direction
{
	down,
	up,
	left,
	right
}

public enum ActionState
{
	idle,
	walking
}

public class NPC : KinematicBody2D
{
    [Export] float speed = 250.0f;
    [Export] ActionState state = ActionState.idle;
    [Export] Direction lookDirection = Direction.down;
	[Export] List<String> dialogueText;
	Boolean inRange = false;
	PhysicsBody2D player;

	public override void _Ready()
    {
		AnimationPlayer anim = GetNode<AnimationPlayer>("AnimationPlayer");
		anim.CurrentAnimation = state.ToString() + "_" + lookDirection.ToString();
        base._Ready();
    }

    public override void _PhysicsProcess(float delta)
    {
		AnimationPlayer anim = GetNode<AnimationPlayer>("AnimationPlayer");
		//Vector2 moveDirection = Vector2.Zero;
		//MoveAndSlide(speed * moveDirection);

		if(inRange && Input.IsActionJustReleased("select"))
		{
			if(player != null)
				player.Call("dialogue", dialogueText);
		}
        base._PhysicsProcess(delta);
    }


    public void _BodyEntered(PhysicsBody2D body)
	{
		if(body.Name.Equals("Player"))
		{
			player = body;
			inRange = true;
		}
	}
	
	public void _BodyExited(PhysicsBody2D body)
	{
		if(body.Name.Equals("Player"))
		{
			inRange = false;
		}
	}

}
