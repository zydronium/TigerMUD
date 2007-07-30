using System;
using System.Collections;
using GameLibrary;


/// <summary>
/// Test command
/// </summary>
public class Testcommand : GameLibrary.Command
{
    public Testcommand()
    {
        this.Name = "test";
        this.Words.Add("test");
        this.Words.Add("testcommand");
        this.Syntax = "test";
        this.Category = "system";
    }

    public override bool DoCommand(PlayerCharacter pc, GameLibrary.GameContext gamecontext, string command, string arguments)
    {
        pc.Send("This is a command from a pre-compiled assembly.\r\n");

        return false;
    }
}
