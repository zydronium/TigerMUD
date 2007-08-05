using System;
using System.Collections;
using GameLibrary;


/// <summary>
/// Test command
/// </summary>
public class Look : GameLibrary.Command
{
    public Look()
    {
        this.Name = "look";
        this.Words.Add("look");
        this.Words.Add("l");
        this.Syntax = "look";
        this.Category = "system";
    }

    public override bool DoCommand(PlayerCharacter pc, GameLibrary.GameContext gamecontext, string command, string arguments)
    {

        Command tempcommand = gamecontext.GetCommand("showroom");
        tempcommand.DoCommand(pc, gamecontext, "showroom", null);
        
        return false;
    }
}
