using System;
using System.Collections;
using GameLibrary;
using System.Collections.Generic;


/// <summary>
/// Test command
/// </summary>
public class ShowRoom : GameLibrary.Command
{
    public ShowRoom()
    {
        this.Name = "showroom";
        this.Words.Add("showroom");
        this.Syntax = "showroom";
        this.Category = "system";
    }

    public override bool DoCommand(PlayerCharacter pc, GameLibrary.GameContext gamecontext, string command, string arguments)
    {
        pc.Send("&W{0}\r\n&n{1}\r\n", pc.Room.NameDisplay, pc.Room.Description);
        pc.Send("\r\nExits:");

        List<string> exits=pc.Room.GetExitList();

        foreach (string exit in exits)
        {
            pc.Send(" &W{0}",exit);
        }
        pc.SendLine("&n");

        return false;
    }

    
}
