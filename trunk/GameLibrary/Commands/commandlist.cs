using System;
using System.Collections;
using GameLibrary;


/// <summary>
/// Test command
/// </summary>
public class Help : GameLibrary.Command
{
    public Help()
    {
        this.Name = "help";
        this.Words.Add("help");
        this.Words.Add("?");
        this.Syntax = "help";
        this.Category = "system";
    }

    public override bool DoCommand(PlayerCharacter pc, GameLibrary.GameContext gamecontext, string command, string arguments)
    {
        ArrayList list=gamecontext.Commands;
        pc.SendLine("List of commands:");
        foreach (string s in list)
            pc.SendLine(s);

        return false;
    }
}
