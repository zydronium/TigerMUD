using System;
using GameLibrary;

/// <summary>
/// Test command
/// </summary>
public class Exampleparsing : GameLibrary.Command
{
    public Exampleparsing()
    {
        this.Name = "example2";
        this.Words.Add("ex2");
        this.Words.Add("example2");
        this.Syntax = "ex2";
        this.Category = "system";
    }

    public override bool DoCommand(PlayerCharacter pc, GameContext gamecontext, string command, string arguments)
    {
        pc.SendLine("Command word: {0}",command);
        pc.SendLine("Arguments: {0}", arguments); 
        return false;
    }
}
