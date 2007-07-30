using System;
using System.Collections;
using GameLibrary;


/// <summary>
/// Welcome screen for newly connected clients. 
/// This command is the first ever queued for players so it must exist.
/// </summary>
public class Welcome : GameLibrary.Command
{
    public Welcome()
    {
        this.Name = "Welcome Screen";
        this.Words.Add("welcome");
        this.Syntax = "welcome";
        this.Category = "system";
    }

   

    public override void DoCommand(object stateinfoin)
    {
        StateInfo stateinfo = (StateInfo)stateinfoin;
        PlayerCharacter pc=stateinfo.Pc;
        GameContext gamecontext=stateinfo.GameContext;
        
        pc.SendLine("&RWelcome");
        pc.ConnectionState = GameLibrary.ConnectionState.ConnectedNotWaitingForClient;
    }
}

