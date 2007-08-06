using System;
using System.Collections;
using GameLibrary;


/// <summary>
/// Test command
/// </summary>
public class Give : GameLibrary.Command
{
    public Give()
    {
        this.Name = "give";
        this.Words.Add("give");
        this.Syntax = "give";
        this.Category = "items";
    }

    public override bool DoCommand(PlayerCharacter pc, GameLibrary.GameContext gamecontext, string command, string arguments)
    {
        // figure out what is being given

        // parse arguments to remove "to" and find target

        // search for target in players
        // GetPlayer("shortname");
        // if we get a hit, then run GiveToPlayer(item,player);

        // search for target in mobs
        // GetMob("shortname");
        // if we get a hit, then run GiveToMob(item,mob);

        // if no hits, then say "cannot find so and so"
              

        return false;
    }
}
