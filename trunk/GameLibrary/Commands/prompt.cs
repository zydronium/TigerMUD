using System;
using System.Collections;
using GameLibrary;


/// <summary>
/// Test command
/// </summary>
public class Prompt : GameLibrary.Command
{
    public Prompt()
    {
        this.Name = "prompt";
        this.Words.Add("prompt");
        this.Syntax = "prompt";
        this.Category = "system";
    }

    public override bool DoCommand(PlayerCharacter pc, GameLibrary.GameContext gamecontext, string command, string arguments)
    {
        pc.Send("(" + pc.X + "/" + pc.Y + ") (" + pc.Health + ") (" + gamecontext.Clock.GameTime + ") &yCommand: ");

        return false;
    }
}
