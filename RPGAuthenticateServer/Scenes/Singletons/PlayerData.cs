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

    public void saveData()
    {

    }

    public void addUser(String username, String password)
    {
        File data_file = new File();
        data_file.Open("res://Data/UserData.json", File.ModeFlags.Write);
        Dictionary values = new Dictionary();
        values.Add("Password", password);
        //values.Add("XPos", 94.225f); // Moved to server
        //values.Add("YPos", 250.081f); // Moved to server
        PlayerIDs.Add(username, values);
        data_file.StoreLine(JSON.Print(PlayerIDs));
        data_file.Close();
    }
}
