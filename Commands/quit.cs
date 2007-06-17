using System;
using System.Collections;
using GameLibrary;


/// <summary>
/// Test command
/// </summary>
public class Quit : GameLibrary.Command
{
    public Quit()
    {
        this.Name = "quit";
        this.Words.Add("quit");
        this.Words.Add("q");
        this.Words.Add("logout");
        this.Words.Add("logoff");
        this.Words.Add("exit");
        this.Words.Add("qq");

        this.Syntax = "quit";
        this.Category = "system";
    }

    public override bool DoCommand(PlayerCharacter pc, GameLibrary.GameContext gamecontext, string command, string arguments)
    {
        // fast quit
        if (command == "qq") return true;

        string response=pc.GetResponse("Quit. Are you sure?");
        if (response.ToLower().StartsWith("y"))
        {
            // Disconnect by returning true
            return true;

        }
        
        return false;
    }
}
