using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Game heartbeat check
/// </summary>
public class Tick : GameLibrary.Command
{
    public Tick()
    {
        this.Name = "tick";
        this.Words.Add("tick");
        
        this.Syntax = "tick";
        this.Category = "system";
    }

    public override bool DoCommand(GameLibrary.PlayerCharacter pc, GameLibrary.GameContext gamecontext, string command, string arguments)
    {
        try
        {
            Console.WriteLine("Tick");
        }
        catch
        { }
        return false;
    }
}
