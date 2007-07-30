using System;
using System.Collections;
using System.Collections.Generic;
using GameLibrary;


/// <summary>
/// Test command
/// </summary>
public class Who : GameLibrary.Command
{
    public Who()
    {
        this.Name = "who";
        this.Words.Add("who");
        
        this.Syntax = "who";
        this.Category = "system";
    }

    public override bool DoCommand(PlayerCharacter pc, GameLibrary.GameContext gamecontext, string command, string arguments)
    {
        pc.SendLine("Players:");
        List<PlayerCharacter> players=gamecontext.Players;
        foreach (PlayerCharacter player in players)
        {
            pc.SendLine(player.NameFirst);
        }
        pc.SendLine("{0} Player(s).", players.Count);
        


        return false;
    }
}
