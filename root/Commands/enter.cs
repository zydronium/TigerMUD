using System;
using System.Collections.Generic;
using System.Text;
using GameLibrary;

namespace Commands
{

    /// <summary>
    /// Test command
    /// </summary>
    public class Enter : GameLibrary.Command
    {
        public Enter()
        {
            this.Name = "enter";
            this.Words.Add("enter");
            this.Words.Add("in");
            this.Syntax = "enter";
            this.Category = "movement";
        }

        public override bool DoCommand(PlayerCharacter pc, GameLibrary.GameContext gamecontext, string command, string arguments)
        {
            if (pc.Planet.MapTerrain[pc.X, pc.Y].PortalLink != " ")
            {
                pc.MoveToRoom(pc.Planet.MapTerrain[pc.X, pc.Y].PortalLink, gamecontext);

            }
            else
            {
                pc.SendLine("&RError, nothing to enter.");
                
            }

            return false;
        }
    }

}
