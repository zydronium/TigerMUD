using System;
using GameLibrary;

/// <summary>
/// Test command
/// </summary>
public class Examplecommand : GameLibrary.Command
{
    public Examplecommand()
    {
        this.Name = "test";
        this.Words.Add("example");
        this.Words.Add("test");
        this.Words.Add("test2");
        this.Syntax = "test";
        this.Category = "system";
    }

    public override bool DoCommand(PlayerCharacter pc, GameContext gamecontext, string command, string arguments)
    {
        
        pc.Send("It worked!!!\r\n");
        return false;
    }
}
