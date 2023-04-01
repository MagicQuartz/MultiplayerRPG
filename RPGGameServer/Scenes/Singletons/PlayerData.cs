using Godot;
using Godot.Collections;
using System;

public class PlayerData : Node
{
    public Dictionary PlayerIDs;
    

    public override void _Ready()
    {
        readData();
    }

    public void readData()
    {
        File data_file = new File();
        data_file.Open("res://Data/UserData.json", File.ModeFlags.Read);
        JSONParseResult data_json = JSON.Parse(data_file.GetAsText());
        //GD.Print(JSON.Print(data_file.GetAsText()));
        data_file.Close();
        PlayerIDs = data_json.Result as Dictionary; //data_json.Result;
    }

    public void saveData(String username, String property, object value)
    {
        File data_file = new File();
        data_file.Open("res://Data/UserData.json", File.ModeFlags.Write);
        Dictionary playerProperties = (Dictionary) PlayerIDs[username];
        if(keyExists(playerProperties, property)) // PlayerIDs is a dictionary, so no worries there.
        {
            playerProperties[property] = value;
        } else
        {
            playerProperties.Add(property, value);    
        }

        
        data_file.StoreLine(JSON.Print(PlayerIDs));
        data_file.Close();
    }

    public object readData(String username, String property)
    {
        Dictionary playerProperties = (Dictionary) PlayerIDs[username];
        if(keyExists(playerProperties, property)) // PlayerIDs is a dictionary, so no worries there.
        {
            return playerProperties[property];
        }
        return null; 
    }

    public void addUser(String username)
    {
        File data_file = new File();
        data_file.Open("res://Data/UserData.json", File.ModeFlags.Write);

        // Data and default values
        Dictionary values = new Dictionary();
        values.Add("PosX", 94.225f); // Moved from authenticator
        values.Add("PosY", 250.081f); // Moved from authenticator
        values.Add("Balance", 250); // Money
        values.Add("Sprite", 0); // Sprite
        PlayerIDs.Add(username, values);
        data_file.StoreLine(JSON.Print(PlayerIDs));
        data_file.Close();
    }

    public bool userExists(String username)
    {
        return keyExists(PlayerIDs, username);
    }

    public bool keyExists(Dictionary d, String key)
	{
		foreach(String property in d.Keys)
		{
			if(property.Equals(key))
				return true;
		}
		return false;
	}
}
