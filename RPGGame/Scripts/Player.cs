using Godot;
using System;
using System.Collections.Generic;

public class Player : KinematicBody2D
{
	[Export] float speed;// = 250.0f;
	[Export] float speedV;
	private String username = "";
	[Export] bool isClient = false;
	
	Vector2 prevPos = Vector2.Zero;
	
	public bool GuiStun = false;
	public override void _Ready()
	{
		GameServer server = GetNode<GameServer>("/root/GameServer");
		server.FetchProperty("Speed", GetInstanceId()); // speed = 
		if(isClient)
		{
			Camera2D camera = GetNode<Camera2D>("Camera2D");
			camera.Current = true;
			Gateway gateway = GetNode<Gateway>("/root/Gateway");
			username = gateway.getUsername();
		} else
		{
			username = this.Name;
			GetNode<CanvasLayer>("Camera2D/CanvasLayer").QueueFree();
		}
		Label nametag = GetNode<Label>("Camera2D/Username");
		nametag.Text = username;
		nametag.Visible = true;
		if(username.ToLower().Equals("nan"))
		{
			Sprite sprite = GetNode<Sprite>("Sprite");
			Texture nanSheet = (Texture)GD.Load("res://Assets/Spritesheets/Custom/NaN Spritesheet.png");
			sprite.Texture = nanSheet;
		}
		base._Ready();
	}

	public override void _PhysicsProcess(float delta)
	{
		AnimationPlayer anim = GetNode<AnimationPlayer>("AnimationPlayer");

		if(isClient)
		{
			Vector2 inputVector = new Vector2(
				Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"),
				Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up")
			);

			Vector2 moveDirection = inputVector.Normalized();
			Vector2 finalMovDirection = speed * moveDirection;
			if(finalMovDirection.x > 0)
				anim.CurrentAnimation = "walk_right";
			else if(finalMovDirection.x < 0)
				anim.CurrentAnimation = "walk_left";
			else if(finalMovDirection.y < 0)
				anim.CurrentAnimation = "walk_up";
			else if(finalMovDirection.y > 0)
				anim.CurrentAnimation = "walk_down";

			if(finalMovDirection == Vector2.Zero)
				if(anim.CurrentAnimation.Equals("walk_up"))
					anim.CurrentAnimation = "idle_up";
				else if(anim.CurrentAnimation.Equals("walk_right"))
					anim.CurrentAnimation = "idle_right";
				else if(anim.CurrentAnimation.Equals("walk_left"))
					anim.CurrentAnimation = "idle_left";
				else if(anim.CurrentAnimation.Equals("walk_down"))
					anim.CurrentAnimation = "idle_down";

			if(Input.IsActionJustPressed("RPS") && currentlyInRange.Count > 0)
				challengeRPS();

			MoveAndSlide(finalMovDirection);
		} else
		{
			Vector2 finalMovDirection = (Position - prevPos).Normalized();
			if(finalMovDirection.x > 0)
				anim.CurrentAnimation = "walk_right";
			else if(finalMovDirection.x < 0)
				anim.CurrentAnimation = "walk_left";
			else if(finalMovDirection.y < 0)
				anim.CurrentAnimation = "walk_up";
			else if(finalMovDirection.y > 0)
				anim.CurrentAnimation = "walk_down";

			if(finalMovDirection == Vector2.Zero)
				if(anim.CurrentAnimation.Equals("walk_up"))
					anim.CurrentAnimation = "idle_up";
				else if(anim.CurrentAnimation.Equals("walk_right"))
					anim.CurrentAnimation = "idle_right";
				else if(anim.CurrentAnimation.Equals("walk_left"))
					anim.CurrentAnimation = "idle_left";
				else if(anim.CurrentAnimation.Equals("walk_down"))
					anim.CurrentAnimation = "idle_down";

			//Put the prevPos initiator at the end
			prevPos = Position;
		}

		base._PhysicsProcess(delta);
	}

	public void toggleMovemenet(bool toggle)
	{
		if(toggle)// Enable movement
			speed = speedV;
		if(!toggle)// Disable movement
			speed = 0;
	}

	public void dialogue(List<String> lines)
	{
		toggleMovemenet(false);
		Control dialogue = GetNode<Control>("Camera2D/CanvasLayer/Dialogue");
		dialogue.Call("startDialogue", lines);
	}

	public void dialogueOver()
	{
		toggleMovemenet(true);
	}

	public void setSpeed(float serverSpeed)
	{
		this.speedV = serverSpeed;
		toggleMovemenet(true);
	}

	public String getUsername()
	{
		return username;
	}

	List<Player> currentlyInRange = new List<Player>();
	public void _BodyEntered(object body)
	{
		if(isClient)
		{
			//GD.Print("Body entered range");
			//GD.Print("Type: " + body.GetType());
			if(body.GetType().ToString().Equals("Player"))
			{
				//GD.Print("Body is player");
				Player plr = (Player) body;
				if(!plr.isClient)
				{
					//GD.Print("Player is not client");
					currentlyInRange.Add(plr);
					Key rpsKey = GetNode<Key>("Camera2D/CanvasLayer/RPSKey");
					rpsKey.setPress("RPS", "R", "Rock Paper Scissors");
					rpsKey.Visible = true;
				}
			}
		}
	}
	
	public void _BodyExited(object body)
	{
		if(isClient)
		{
			if(body.GetType().ToString().Equals("Player"))
			{
				Player plr = (Player) body;
				if(!plr.isClient)
				{
					currentlyInRange.Remove(plr);
					/*GD.PrintRaw("Currently In Range: ");
					foreach(Player pr in currentlyInRange)
						{
							GD.PrintRaw(pr.username + " ");
						}*/
					if(currentlyInRange.Count == 0)
					{
						Key rpsKey = GetNode<Key>("Camera2D/CanvasLayer/RPSKey");
						rpsKey.Visible = false;
					}
				}
			}
		}
	}

	private void challengeRPS()
	{
		Player chosen = currentlyInRange[0];
		GameServer server = GetNode<GameServer>("/root/GameServer");
		server.challengeRPS(chosen.Name);
	}


	public void RockPaperScissors(String challenger)
	{
		RPSpropose popup = GetNode<RPSpropose>("Camera2D/CanvasLayer/RPSpropose");
		popup.Challenge(challenger);
	}

	public void RPSResult(String challenged, bool accepted)
	{
		RPSpropose popup = GetNode<RPSpropose>("Camera2D/CanvasLayer/RPSpropose");
		popup.RPSResult(challenged, accepted);
	}

	public void RPSAction(int action)
	{
		RockPaperScissors rps = GetNode<RockPaperScissors>("Camera2D/CanvasLayer/RockPaperScissors");
		rps.ActionPerformed(action);
	}
}
