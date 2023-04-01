using Godot;
using Godot.Collections;
using System;

public class ServerData : Node
{
    public Dictionary property_data;

    public override void _Ready()
    {
        File property_file = new File();
        property_file.Open("res://Data/SkillData - Sheet1.json", File.ModeFlags.Read);
        JSONParseResult property_json = JSON.Parse(property_file.GetAsText());
        property_file.Close();
        property_data = property_json.Result as Dictionary; //property_json.Result;
    }
}
