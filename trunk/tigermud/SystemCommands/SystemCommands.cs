#region TigerMUD License
/*
/-------------------------------------------------------------\
|    _______  _                     __  __  _    _  _____     |
|   |__   __|(_)                   |  \/  || |  | ||  __ \    |
|      | |    _   __ _   ___  _ __ | \  / || |  | || |  | |   |
|      | |   | | / _` | / _ \| '__|| |\/| || |  | || |  | |   |
|      | |   | || (_| ||  __/| |   | |  | || |__| || |__| |   |
|      |_|   |_| \__, | \___||_|   |_|  |_| \____/ |_____/    |
|                 __/ |                                       |
|                |___/                  Copyright (c) 2004    |
\-------------------------------------------------------------/

TigerMUD. A Multi User Dungeon engine.
Copyright (C) 2004 Adam Miller et al.

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 2 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

You can contact the TigerMUD developers at www.tigermud.com or at
http://sourceforge.net/projects/tigermud.

The full licence can be found in <root>/docs/TigerMUD_license.txt
*/
#endregion

using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Web.Mail;

namespace TigerMUD
{

    #region TimerCommands

    /// <summary>
    /// Timer command to update Gametime
    /// </summary>
    public class Command_timer_Gametime : Command
    {
        public Command_timer_Gametime()
        {
            name = "timer_Gametime";
            words = new string[0] { };
            // No help, not user callable.
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            // Increment 12 game seconds for every 1 real second
            Lib.GameTimeAddMinutes(.2);
            // Only save server states every 20 real seconds
            System.Int64 timediff = (Convert.ToInt64(Lib.ServerState["serverticks"].ToString())) - (Convert.ToInt64(Lib.ServerState["lastserverticks"].ToString()));
            if (timediff > 200) Lib.SaveServerState();
            return true;
        }
    }
    /// <summary>
    /// Runs states that do something several times over the course of their duration, like poisons.
    /// </summary>
    public class Command_timer_cyclestates : Command
    {
        public Command_timer_cyclestates()
        {
            name = "timer_cyclestates";
            words = new string[0] { };
            // No help, not user callable.
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Actor user = null;
            try
            {
                ArrayList actors = Lib.actors;
                for (int playercounter = actors.Count - 1; playercounter >= 0; playercounter--)
                {
                    user = (Actor)actors[playercounter];
                    user.CycleStates();
                }
                return true;
            }
            catch (Exception ex)
            {
                Lib.PrintLine(DateTime.Now + " EXCEPTION in " + name + ": " + ex.Message + ex.StackTrace);
                return false;
            }
        }
    }


    /// <summary>
    /// Increments playtime and regenerates mob and user stats over time.
    /// </summary>
    public class Command_timer_regencycle : Command
    {
        public Command_timer_regencycle()
        {
            name = "timer_regencycle";
            words = new string[0] { };
            // No help, not user callable.
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Actor user = null;
            try
            {
                ArrayList users = Lib.actors;
                for (int playercounter = users.Count - 1; playercounter >= 0; playercounter--)
                {
                    user = (Actor)users[playercounter];

                    // Skip the actors that are in the middle of the login process because
                    // they don't have their properties loaded yet and this
                    // will do strange things.

                    if (user["name"].ToString() != "" && user["name"].ToString() != null)
                    {

                        // Increment each player's play time by 5 minutes
                        //user = (Actor)Lib.GetWorldItems()[playercounter];

                        if (user["type"].ToString() == "user" && Lib.ConvertToBoolean(user["connected"]))
                        {
                            user["played"] = Convert.ToInt64(user["played"]) + 30;  // User's current play session in tenths of a second
                        }

                        // Run any damage/buff over time states
                        user.CycleStates();

                        // Remove any buffs or debuffs that have expired from users and mobs
                        user.ExpireStates();

                        // Reset any stats that are out of whack
                        if (Convert.ToInt32(user["health"]) > Convert.ToInt32(user["healthmax"]))
                        {
                            user["health"] = Convert.ToInt32(user["healthmax"]);
                        }
                        if (Convert.ToInt32(user["stamina"]) > Convert.ToInt32(user["staminamax"]))
                        {
                            user["stamina"] = Convert.ToInt32(user["staminamax"]);
                        }
                        if (Convert.ToInt32(user["mana"]) > Convert.ToInt32(user["manamax"]))
                        {
                            user["mana"] = Convert.ToInt32(user["manamax"]);
                        }




                        // Only do this for living things
                        if (user.Killable)
                        {
                            // Check if a corpse
                            if (Convert.ToInt32(user["health"]) <= 0)
                            {
                                // Do respawn if it's time
                                if (Lib.GetTime() > Convert.ToInt64(user["deathticks"]) + Convert.ToInt64(user["respawntimer"]) * 12)
                                {
                                    user.Respawn();
                                }
                            }
                        }


                        // Don't regen corpse stats
                        if (Convert.ToInt32(user["health"]) > 0)
                        {
                            // Regenerate user stats
                            if (Convert.ToInt32(user["health"]) < Convert.ToInt32(user["healthmax"]))
                            {
                                user["health"] = Convert.ToInt32(user["health"]) + (Convert.ToInt32(user["staminamax"]) / 10);
                                if (Convert.ToInt32(user["health"]) > Convert.ToInt32(user["healthmax"]))
                                {
                                    user["health"] = Convert.ToInt32(user["healthmax"]);
                                }
                            }
                            if (Convert.ToInt32(user["stamina"]) < Convert.ToInt32(user["staminamax"]))
                            {
                                user["stamina"] = Convert.ToInt32(user["stamina"]) + (Convert.ToInt32(user["staminamax"]) / 10);
                                if (Convert.ToInt32(user["stamina"]) > Convert.ToInt32(user["staminamax"]))
                                {
                                    user["stamina"] = Convert.ToInt32(user["staminamax"]);
                                }

                            }
                            if (Convert.ToInt32(user["mana"]) < Convert.ToInt32(user["manamax"]))
                            {
                                user["mana"] = Convert.ToInt32(user["mana"]) + (Convert.ToInt32(user["spirit"]) / 10);
                                if (Convert.ToInt32(user["mana"]) > Convert.ToInt32(user["manamax"]))
                                {
                                    user["mana"] = Convert.ToInt32(user["manamax"]);
                                }
                            }
                            //user.Save();
                        }
                    }
                }
            }
            catch
            { return false; }



            return true;
        }
    }

    /// <summary>
    /// Handle NPC combat
    /// </summary>
    public class Command_timer_npccombat : Command
    {
        public Command_timer_npccombat()
        {
            name = "timer_npccombat";
            words = new string[0] { };
            // No help, not user callable.
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Actor mob = null;
            ArrayList mobs = Lib.actors;
            for (int mobcounter = mobs.Count - 1; mobcounter >= 0; mobcounter--)
            {
                mob = (Actor)mobs[mobcounter];
                mob.Combat();
            }
            return true;
        }
    }

    /// <summary>
    /// Handle player auto-combat
    /// </summary>
    public class Command_timer_playercombat : Command
    {
        public Command_timer_playercombat()
        {
            name = "timer_playercombat";
            words = new string[0] { };
            // No help, not user callable.
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Actor user = null;
            try
            {
                ArrayList actors = Lib.actors;
                for (int usercounter = actors.Count - 1; usercounter >= 0; usercounter--)
                {
                    user = (Actor)actors[usercounter];
                    user.Combat();
                }
                return true;
            }
            catch
            {
                return false;
            }

        }
    }

    /// <summary>
    /// Sweep the online users and disconnect idle ones based on timer.
    /// </summary>
    public class Command_timer_idledisconnect : Command
    {
        public Command_timer_idledisconnect()
        {
            name = "timer_idledisconnect";
            words = new string[0] { };
            // No help, not user callable.
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {

            ArrayList users = Lib.actors;
            foreach (Actor tmpuser in users)
            {
                // Only do this for users
                if (tmpuser["type"].ToString() == "user")
                {
                    if (Lib.ConvertToBoolean(tmpuser["connected"]))
                    {
                        // Kick idle users
                        if ((Convert.ToSingle(tmpuser["lastmessageticks"]) + Lib.ServerIdleDisconnect) <= Lib.GetTime())
                        {
                            tmpuser.SendError("You have been idle too long, disconnecting...\r\n");
                            actor.Disco(tmpuser, "idle user function kicked the player");
                        }
                        else if ((Convert.ToSingle(tmpuser["lastmessageticks"]) + Lib.ServerIdleDisconnect - 1800) <= Lib.GetTime())
                        {
                            // Warn idle users at 3 min, 2 min, then disco
                            tmpuser.SendError("You are about to be disconnected for being idle too long.\r\n");
                        }
                    }
                }
            }
            return true;
        }

    }

    /// <summary>
    /// Scheduled command to update the phases of the moon.
    /// </summary>
    public class Command_timer_movesunmoon : Command
    {
        public Command_timer_movesunmoon()
        {
            name = "timer_movesunmoon";
            words = new string[0] { };
            // No help, not user callable.
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            string lastsunquadrant = Lib.ServerState["sunquadrant"].ToString();

            // Put the sun in the sky correctly based on game time
            // This hardcodes sunrise at 6am, and sunset at 6pm
            if (Lib.GameTimeGetHour() >= 6 && Lib.GameTimeGetHour() < 9)
            {
                // day
                Lib.SetServerState("sunquadrant", 0);
            }
            else if (Lib.GameTimeGetHour() >= 9 && Lib.GameTimeGetHour() < 12)
            {
                // day
                Lib.SetServerState("sunquadrant", 1);
            }
            else if (Lib.GameTimeGetHour() >= 12 && Lib.GameTimeGetHour() < 15)
            {
                // day
                Lib.SetServerState("sunquadrant", 2);
            }
            else if (Lib.GameTimeGetHour() >= 15 && Lib.GameTimeGetHour() < 18)
            {
                // day
                Lib.SetServerState("sunquadrant", 3);
            }
            else if (Lib.GameTimeGetHour() >= 18 && Lib.GameTimeGetHour() < 21)
            {
                // night
                Lib.SetServerState("sunquadrant", 4);
            }
            else if (Lib.GameTimeGetHour() >= 21)
            {
                // night
                Lib.SetServerState("sunquadrant", 5);
            }
            else if (Lib.GameTimeGetHour() >= 0 && Lib.GameTimeGetHour() < 3)
            {
                // night
                Lib.SetServerState("sunquadrant", 6);
            }
            else if (Lib.GameTimeGetHour() >= 3 && Lib.GameTimeGetHour() < 6)
            {
                // night
                Lib.SetServerState("sunquadrant", 7);
            }

            if (Lib.SetServerState("sunquadrant") != lastsunquadrant)
            {
                // There was some change in sun quadrant, so check if we need to report sunrise or sunset
                if (Convert.ToInt64(Lib.SetServerState("sunquadrant")) == 0)
                {
                    // Sunrise event
                    Lib.Sayskyevents("Light brightens the sky as the sun rises.");
                }
                if (Convert.ToInt64(Lib.SetServerState("sunquadrant")) == 4)
                {
                    // Sunset event
                    Lib.Sayskyevents("The light fades away as the sun sets.");
                }
            }

            // Check if enough time has passed for the moon to move into a new sky quadrant
            if ((Convert.ToInt64(Lib.SetServerState("moonlastupdate")) + Lib.moonquadrant_ticks) <= Convert.ToInt64(Lib.GetTime()))
            {
                // If so, then move the moon (quadrants are 0-7) (0-3 are visible sky)
                Lib.SetServerState("moonquadrant", Convert.ToInt64(Lib.SetServerState("moonquadrant")) + 1);
                Lib.SetServerState("moonlastupdate", Lib.GetTime());

                // Calculate the moon phase based on new moon and sun position
                long result = Convert.ToInt64(Lib.SetServerState("sunquadrant")) - Convert.ToInt64(Lib.SetServerState("moonquadrant"));

                switch (result)
                {
                    case 0:
                        Lib.SetServerState("moonphase", "new");
                        break;
                    case 1:
                    case -1:
                        Lib.SetServerState("moonphase", "waxing crescent");
                        break;
                    case 2:
                    case -2:
                        Lib.SetServerState("moonphase", "waxing half");
                        break;
                    case 3:
                    case -3:
                        Lib.SetServerState("moonphase", "waxing gibbous");
                        break;
                    case 4:
                    case -4:
                        Lib.SetServerState("moonphase", "full");
                        break;
                    case 5:
                    case -5:
                        Lib.SetServerState("moonphase", "waning gibbous");
                        break;
                    case 6:
                    case -6:
                        Lib.SetServerState("moonphase", "waning half");
                        break;
                    case 7:
                    case -7:
                        Lib.SetServerState("moonphase", "waning crescent");
                        break;
                }

                // Check for moon rise event
                if (Convert.ToInt64(Lib.SetServerState("moonquadrant")) > 7)
                {
                    Lib.SetServerState("moonquadrant", 0);
                    // moonrise event
                    Lib.Sayskyevents("The " + Lib.SetServerState("moonphase") + " moon rises.");
                }

                // Check for moon set event
                if (Convert.ToInt64(Lib.SetServerState("moonquadrant")) == 4)
                {
                    // moonset event
                    Lib.Sayskyevents("The " + Lib.SetServerState("moonphase") + " moon sets.");
                }
            }

            return true;
        }
    }

    public class Command_timer_growplants : Command
    {
        public Command_timer_growplants()
        {
            name = "timer_growplants";
            words = new string[0] { };
            //No help, not user callable.
        }
        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            return true;
        }
    }

    /// <summary>
    /// Moves mobs 
    /// </summary>
    public class Command_timer_movemobs : Command
    {
        public Command_timer_movemobs()
        {
            name = "timer_movemobs";
            words = new string[0] { };
            // No help, not user callable.
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Randomizer r = new Randomizer();
            lock (Lib.GetWorldItems().SyncRoot)
            {
                ArrayList mobs = Lib.actors;
                foreach (Actor mob in mobs)
                {
                    // mob["mobile"]==0 means the mob never moves.
                    // mob["mobile"]==1 means the mob always moves.
                    // Anything higher than 1 means the mob has 1 chance in mob["mobile"] to move.
                    // The higher the number, the less chance the mob will move on a given round.
                    // For example, Mob["mobile"]==3 means the mob has a 33% chance (1 in 3) to move that round.
                    // Mob["mobile"]==4 means the mob has a 25% chance (1 in 4) to move, and so on.
                    
                    if (mob["name"].ToString().StartsWith("Corpse of")) continue;
                    // Also don't move mob if he is in combat.
                    if (mob["mobile"] != null)
                    {
                        //Console.WriteLine(mob["name"].ToString() + " " + Convert.ToInt32(mob["mobile"]) + " " + mob.hatetable.Count);
                        if (Convert.ToInt32(mob["mobile"]) > 0 && mob.hatetable.Count < 1)
                        {
                            // First, decide if the mob is going to make a move this round.
                            // Chance to move is 1 in (mob["mobile"]), and we make the move when we roll a 1.
                            int move = r.getrandomnumber(1, Convert.ToInt32(mob["mobile"]));
                            // If we roll a 1, then do the move.
                            if (move == 1)
                            {
                                bool hasmoved = false;
                                Actor room = mob.GetRoom();
                                int exitnumber = 0;
                                // All the possible movement directions are 1-10 (like the spokes of a wheel). 
                                // 1=north, 2=northeast ... 8=northwest, 9=up, 10=down.
                                // Mobs are not coded to make any moves using the (in) and (out) directions.
                                // Cycle through all the available exits, counting each one, then take exit number(useexitnumber).
                                // Don't count any exits that are unavailable.
                                int useexitnumber = r.getrandomnumber(1, 10);
                                while (hasmoved == false)
                                {
                                    if (room["xnorth"] != null)
                                    {
                                        // Only consider this exit direction if it is available in this room.
                                        if (room["xnorth"].ToString() != "")
                                        {
                                            // Count this exit.
                                            exitnumber++;
                                            // Use this exit if it matches our useexitnumber count.
                                            if (useexitnumber == exitnumber)
                                            {
                                                // Make the mob move in the direction.
                                                mob.MoveToRoom(room["xnorth"].ToString(), "north", false);
                                                // Set the move flag to ensure we only move the mob once.
                                                hasmoved = true;
                                            }
                                        }
                                    }
                                    if (room["xnortheast"] != null)
                                    {
                                        // Only consider this exit direction if it is available in this room.
                                        if (room["xnortheast"].ToString() != "")
                                        {
                                            // Count this exit.
                                            exitnumber++;
                                            // Use this exit if it matches our useexitnumber count.
                                            if (useexitnumber == exitnumber)
                                            {
                                                // Make the mob move in the direction.
                                                mob.MoveToRoom(room["xnortheast"].ToString(), "northeast", false);
                                                // Set the move flag to ensure we only move the mob once.
                                                hasmoved = true;
                                            }
                                        }
                                    }
                                    if (room["xeast"] != null)
                                    {
                                        // Only consider this exit direction if it is available in this room.
                                        if (room["xeast"].ToString() != "")
                                        {
                                            // Count this exit.
                                            exitnumber++;
                                            // Use this exit if it matches our useexitnumber count.
                                            if (useexitnumber == exitnumber)
                                            {
                                                // Make the mob move in the direction.
                                                mob.MoveToRoom(room["xeast"].ToString(), "east", false);
                                                // Set the move flag to ensure we only move the mob once.
                                                hasmoved = true;
                                            }
                                        }
                                    }
                                    if (room["xsoutheast"] != null)
                                    {
                                        // Only consider this exit direction if it is available in this room.
                                        if (room["xsoutheast"].ToString() != "")
                                        {
                                            // Count this exit.
                                            exitnumber++;
                                            // Use this exit if it matches our useexitnumber count.
                                            if (useexitnumber == exitnumber)
                                            {
                                                // Make the mob move in the direction.
                                                mob.MoveToRoom(room["xsoutheast"].ToString(), "southeast", false);
                                                // Set the move flag to ensure we only move the mob once.
                                                hasmoved = true;
                                            }
                                        }
                                    }
                                    if (room["xsouth"] != null)
                                    {
                                        // Only consider this exit direction if it is available in this room.
                                        if (room["xsouth"].ToString() != "")
                                        {
                                            // Count this exit.
                                            exitnumber++;
                                            // Use this exit if it matches our useexitnumber count.
                                            if (useexitnumber == exitnumber)
                                            {
                                                // Make the mob move in the direction.
                                                mob.MoveToRoom(room["xsouth"].ToString(), "south", false);
                                                // Set the move flag to ensure we only move the mob once.
                                                hasmoved = true;
                                            }
                                        }
                                    }
                                    if (room["xsouthwest"] != null)
                                    {
                                        // Only consider this exit direction if it is available in this room.
                                        if (room["xsouthwest"].ToString() != "")
                                        {
                                            // Count this exit.
                                            exitnumber++;
                                            // Use this exit if it matches our useexitnumber count.
                                            if (useexitnumber == exitnumber)
                                            {
                                                // Make the mob move in the direction.
                                                mob.MoveToRoom(room["xsouthwest"].ToString(), "southwest", false);
                                                // Set the move flag to ensure we only move the mob once.
                                                hasmoved = true;
                                            }
                                        }
                                    }
                                    if (room["xwest"] != null)
                                    {
                                        // Only consider this exit direction if it is available in this room.
                                        if (room["xwest"].ToString() != "")
                                        {
                                            // Count this exit.
                                            exitnumber++;
                                            // Use this exit if it matches our useexitnumber count.
                                            if (useexitnumber == exitnumber)
                                            {
                                                // Make the mob move in the direction.
                                                mob.MoveToRoom(room["xwest"].ToString(), "west", false);
                                                // Set the move flag to ensure we only move the mob once.
                                                hasmoved = true;
                                            }
                                        }
                                    }
                                    if (room["xnorthwest"] != null)
                                    {
                                        // Only consider this exit direction if it is available in this room.
                                        if (room["xnorthwest"].ToString() != "")
                                        {
                                            // Count this exit.
                                            exitnumber++;
                                            // Use this exit if it matches our useexitnumber count.
                                            if (useexitnumber == exitnumber)
                                            {
                                                // Make the mob move in the direction.
                                                mob.MoveToRoom(room["xnorthwest"].ToString(), "northwest", false);
                                                // Set the move flag to ensure we only move the mob once.
                                                hasmoved = true;
                                            }
                                        }
                                    }
                                    if (room["xup"] != null)
                                    {
                                        // Only consider this exit direction if it is available in this room.
                                        if (room["xup"].ToString() != "")
                                        {
                                            // Count this exit.
                                            exitnumber++;
                                            // Use this exit if it matches our useexitnumber count.
                                            if (useexitnumber == exitnumber)
                                            {
                                                // Make the mob move in the direction.
                                                mob.MoveToRoom(room["xup"].ToString(), "up", false);
                                                // Set the move flag to ensure we only move the mob once.
                                                hasmoved = true;
                                            }
                                        }
                                    }
                                    if (room["xdown"] != null)
                                    {
                                        // Only consider this exit direction if it is available in this room.
                                        if (room["xdown"].ToString() != "")
                                        {
                                            // Count this exit.
                                            exitnumber++;
                                            // Use this exit if it matches our useexitnumber count.
                                            if (useexitnumber == exitnumber)
                                            {
                                                // Make the mob move in the direction.
                                                mob.MoveToRoom(room["xdown"].ToString(), "down", false);
                                                // Set the move flag to ensure we only move the mob once.
                                                hasmoved = true;
                                            }
                                        }
                                    }
                                    if (room["xin"] != null)
                                    {
                                        // Only consider this exit direction if it is available in this room.
                                        if (room["xin"].ToString() != "")
                                        {
                                            // Count this exit.
                                            exitnumber++;
                                            // Use this exit if it matches our useexitnumber count.
                                            if (useexitnumber == exitnumber)
                                            {
                                                // Make the mob move in the direction.
                                                mob.MoveToRoom(room["xin"].ToString(), "inside", false);
                                                // Set the move flag to ensure we only move the mob once.
                                                hasmoved = true;
                                            }
                                        }
                                    }
                                    if (room["xout"] != null)
                                    {
                                        // Only consider this exit direction if it is available in this room.
                                        if (room["xout"].ToString() != "")
                                        {
                                            // Count this exit.
                                            exitnumber++;
                                            // Use this exit if it matches our useexitnumber count.
                                            if (useexitnumber == exitnumber)
                                            {
                                                // Make the mob move in the direction.
                                                mob.MoveToRoom(room["xout"].ToString(), "outside", false);
                                                // Set the move flag to ensure we only move the mob once.
                                                hasmoved = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
    }

    #endregion TimerCommands
}
