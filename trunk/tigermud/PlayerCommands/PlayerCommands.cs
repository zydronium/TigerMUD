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
using System.Text;
using System.IO;


namespace TigerMUD
{

    /// <summary>
    /// Say command for player.
    /// </summary>
    public class Command_defaultsay : Command, ICommand
    {

        public Command_defaultsay()
        {
            name = "command_defaultsay";
            words = new string[1] { "'" };
            help.Command = "'";
            help.Summary = "sends a message to all the awake players in your current room using your default speech mode.";
            help.Syntax = "' <text>";
            help.Examples = new string[1];
            help.Examples[0] = "' Hello, how are you?";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            string verb = "";
            if (arguments.Length < 1)
            {
                actor.SendError("You must specify text to say.\r\n");
                return false;
            }

            switch (actor["defaultsay"].ToString())
            {
                case "say":
                    verb = "say";
                    break;
                case "yell":
                    verb = "yell";
                    break;
                case "shout":
                    verb = "shout";
                    break;
                case "whisper":
                    verb = "whisper";
                    break;
                case "lecture":
                    verb = "lecture";
                    break;
                case "think":
                    verb = "think";
                    break;
                default:
                    verb = "say";
                    break;
            }

            actor.Send("You " + verb + ", \"" + arguments + "\"\r\n");
            actor.Sayinroom(actor["shortnameupper"] + " " + verb + "s, '" + arguments + "'");
            return true;
        }

    }

    /// <summary>
    /// Reset all game items.
    /// </summary>
    public class Command_reset : Command, ICommand
    {





        public Command_reset()
        {
            name = "command_reset";
            words = new string[1] { "reset" };
            help.Command = "reset";
            help.Summary = "Moves all of a specified type back to their original locations.";
            help.Syntax = "reset";
            help.Examples = new string[3];
            help.Examples[0] = "reset items";
            help.Examples[1] = "reset mobs";
            help.Examples[2] = "reset actors";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            ArrayList words = Lib.GetWords(arguments);
            if (words.Count != 1)
            {
                actor.SendError("You must specify a valid actor type. Check your syntax.\r\n");
                return false;
            }
            // Strip s off the end of the word. For example, turn items into item, etc.
            try
            {
                words[0] = words[0].ToString().Substring(0, words[0].ToString().Length - 1);
                // Don't let this happen for rooms
                if (words[0].ToString() == "room")
                {
                    actor.SendError("Invalid input. Check your syntax.\r\n");
                    return false;
                }
            }
            catch
            {
                actor.SendError("Invalid input. Check your syntax.\r\n");
                return false;
            }


            lock (Lib.actors.SyncRoot)
            {
                ArrayList items = new ArrayList();
                items = Lib.GetAllOfType(words[0].ToString());
                foreach (Actor item in items)
                {
                    item.MoveToRoom(Lib.GetSpecialLocation("new user spawn", "")["id"].ToString(), "nowhere", false);
                }

            }
            return true;
        }

    }

    /// <summary>
    /// Speechmode command for player.
    /// </summary>
    public class Command_setdefaultsay : Command, ICommand
    {


        public Command_setdefaultsay()
        {
            name = "command_setdefaultsay";
            words = new string[1] { "defaultsay" };
            help.Command = "defaultsay";
            help.Summary = "sets your speech mode when speaking to other players.";
            help.Syntax = "defaultsay <mode>";
            help.Examples = new string[1];
            help.Examples[0] = "defaultsay shout";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            string verb = "";

            // Set the mode if the user specified a parameter
            if (arguments != "")
            {
                try
                {
                    arguments = arguments.Trim();
                    actor["defaultsay"] = arguments;
                }
                catch
                {
                    actor.SendError("An error occurred setting your speech mode. Returning to 'say' as the default.\r\n");
                    actor["defaultsay"] = "say";
                    return false;
                }
            }

            // Report to the user what the speech mode is
            switch (actor["defaultsay"].ToString())
            {
                case "say":
                    verb = "say";
                    break;
                case "yell":
                    verb = "yell";
                    break;
                case "shout":
                    verb = "shout";
                    break;
                case "whisper":
                    verb = "whisper";
                    break;
                case "lecture":
                    verb = "lecture";
                    break;
                case "think":
                    verb = "think";
                    break;
                default:
                    verb = "say";
                    break;
            }

            actor.Send("Your speech mode is '" + verb + "'." + "\"\r\n");
            return true;
        }

    }


    /// <summary>
    /// Say command for player.
    /// </summary>
    public class Command_say : Command, ICommand
    {




        public Command_say()
        {
            name = "command_say";
            words = new string[1] { "say" };
            help.Command = "say";
            help.Summary = "sends a message to all the awake players in your current room.";
            help.Syntax = "say <text>";
            help.Examples = new string[1];
            help.Examples[0] = "say Hello, how are you?";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            if (arguments.Length < 1)
            {
                actor.SendError("You must specify text to say.\r\n");
                return false;
            }
            actor.Send("You say, \"" + arguments + "\"\r\n");
            actor.Sayinroom(actor["shortnameupper"] + " says, '" + arguments + "'");
            return true;
        }

    }

    /// <summary>
    /// Say command for player.
    /// </summary>
    public class Command_compile : Command, ICommand
    {




        public Command_compile()
        {
            name = "command_compile";
            words = new string[1] { "compile" };
            help.Command = "compile";
            help.Summary = "compiles a source file and loads commands found there.";
            help.Syntax = "compile <filename>";
            help.Examples = new string[1];
            help.Examples[0] = "compile test.tmc.cs";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Object[] Objects = new Object[Lib.MaxGameCommands];
            string errors = "";
            ArrayList words = Lib.GetWords(arguments);
            if (words.Count != 1)
            {
                actor.SendError("You must specify only one source file to compile. Check your syntax.\r\n");
                return false;
            }
            string filename = words[0].ToString();
            actor.SendAlertGood("Attempting to compile file: " + filename + "\r\n");
            try
            {
                ////Load Commands from precompiled DLLs
                ////DirectoryInfo toplevel = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory);
                ////toplevel = toplevel.Parent;
                //ArrayList files = Lib.GetFilesRecursive(Lib.PathtoRoot, "*commands.dll");
                //if (files.Count > 0)
                //{
                //    foreach (string file in files)
                //    {
                //        if (file.ToLower().EndsWith(filename.ToLower()))
                //        {
                //            actor.SendAlertGood("Found file " + filename + "\r\n" );
                //            Lib.LoadPlugin(file);
                //            actor.SendAlertGood("Loaded precompiled DLL named " + file + "\r\n" );
                //        }
                //    }
                //}
                ArrayList files = Lib.GetFilesRecursive(Lib.PathtoRootScriptsandPlugins, "*.tmc.cs");

                if (files.Count > 0)
                {
                    foreach (string file in files)
                    {
                        if (file.ToLower().EndsWith(filename.ToLower()))
                        {
                            actor.SendAlertGood("Found file " + filename + "\r\n");
                            try
                            {
                                Objects = Lib.Compiler.GetObjectsFromFile(file, out errors);
                                if (errors != "")
                                {
                                    actor.SendError("Compile failed with error: " + errors + "\r\n");
                                }
                            }
                            catch (Exception ex)
                            {
                                actor.SendError("Compile failed with error: " + ex.Message + ex.StackTrace + "\r\n");
                                return false;
                            }
                            if (Objects != null)
                            {
                                foreach (Object Command in Objects)
                                {
                                    // Get rid of any already existing versions of this command
                                    Lib.DeleteCommandWord((TigerMUD.Command)Command);
                                    Lib.AddCommand((TigerMUD.Command)Command);
                                    actor.SendAlertGood("Successfully added command " + ((TigerMUD.Command)Command).Name + " from file: " + file + "\r\n");

                                }
                                actor.SendAlertGood("\r\nIf these are brand new commands, then use 'enablecommand' to make them player-accessible. If these are newly-compiled versions of existing commands, then you can use 'enablecommand' to set a new security level, or just use the commands as-is.\r\n");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                actor.SendError("EXCEPTION in compile command: " + filename + ".\r\n Error was:\r\n" + ex.Message + ex.StackTrace + "\r\n");
                return false;
            }

            return true;
        }

    }


    /// <summary>
    /// Displays the creator/builder credits for the current room.
    /// </summary>
    public class Command_credits : Command, ICommand
    {




        public Command_credits()
        {
            name = "command_credits";
            words = new string[1] { "credits" };
            help.Command = "credits";
            help.Summary = "displays the creator/builder credits for your current room.";
            help.Syntax = "credits";
            help.Examples = new string[1];
            help.Examples[0] = "credits";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            actor.Send(actor["colorexits"] + "Credits for this room:\r\n");
            Actor room = Lib.GetByID(actor["container"].ToString());
            actor.Send(room["credits"].ToString() + "\r\n\r\n");
            actor.Send(actor["colorexits"] + "TigerMUD credits\r\n");
            actor.Send(Lib.Ansifboldyellow + "Developers: " + Lib.Ansifwhite + Lib.Creditsdev + "\r\n");
            actor.Send(Lib.Ansifboldyellow + "Special Thanks: " + Lib.Ansifwhite + Lib.Creditsother + "\r\n");
            return true;
        }

    }


    /// <summary>
    /// Clears the player's target.
    /// </summary>
    public class Command_notarget : Command, ICommand
    {




        public Command_notarget()
        {
            name = "command_notarget";
            words = new string[2] { "notarget", "nt" };
            help.Command = "notarget";
            help.Summary = "clears your combat target and stops combat.";
            help.Syntax = "notarget";
            help.Examples = new string[2];
            help.Examples[0] = "notarget";
            help.Examples[1] = "nt";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            if (actor["target"].ToString() == "")
            {
                actor.SendError("You have no target to clear.\r\n");
                return true;
            }
            else
            {
                actor["target"] = "";
                actor.Send("Target cleared.\r\n");
                return true;
            }
        }

    }

    /// <summary>
    /// Picks a plant.
    /// </summary>
    public class Command_pick : Command, ICommand
    {




        public Command_pick()
        {
            name = "command_pick";
            words = new string[1] { "pick" };
            help.Command = "pick";
            help.Summary = "picks a plant in a room.";
            help.Syntax = "pick <plant name>";
            help.Examples = new string[5];
            help.Examples[0] = "pick strawberry";
            help.Examples[1] = "pick a strawberry";
            help.Examples[2] = "pick 2strawberry";
            help.Examples[3] = "pick red strawberry";
            help.Examples[4] = "pick a red strawberry";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            //string plantname;
            ArrayList words = Lib.GetWords(arguments);

            //Break the list of words down to a useable level
            //remove the "a" if used 
            //if there is a blue strawberry and a red strawberry make sure we get the right one
            //does the adjective really serve a purpose (red, blue) get red strawberry (if all are red, etc)
            //get the numbered strawberry that the user wants. 2strawberry
            return true;
        }

    }


    /// <summary>
    /// Casts a spell.
    /// </summary>
    public class Command_cast : Command, ICommand
    {




        public Command_cast()
        {
            name = "command_cast";
            words = new string[3] { "cast", "c", "spell" };
            help.Command = "cast";
            help.Summary = "casts a spell on your target.";
            help.Syntax = "cast <spell name>";
            help.Examples = new string[2];
            help.Examples[0] = "cast heal1";
            help.Examples[1] = "cast Inferior Heal";
            help.Description = "Spells always cast on your current target.";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            string spellname;
            ArrayList words = Lib.GetWords(arguments);

            // did they specify a spell name at all?
            if (arguments.Length < 1)
            {
                actor.SendError("You must specify a spell to cast.\r\n");
                return false;
            }

            // Get the spell's name from the user input
            if (words.Count > 1)
            {
                spellname = arguments;
            }
            else
            {
                spellname = words[0].ToString();
            }

            // Get the spell using the name provided
            Spell spell = actor.GetSpellByName(spellname);
            // Error out if the spell doesn't exist
            if (spell == null)
            {
                actor.SendError("There is no spell named '" + spellname + "'.\r\n");
                return false;
            }

            // can the user cast the spell?
            if (!spell.CanCast(actor))
            {
                return false;
            }

            // get target
            Actor targetuser = null;


            // What type of target is this?
            targetuser = Lib.GetByID(actor["target"].ToString());

            if (targetuser == null)
            {
                actor.SendError("You don't have a target selected.\r\n");
                // Clear target
                actor["target"] = "";
                actor["targettype"] = "";
                return false;
            }

            // Ensure target is in the room
            if (targetuser["container"].ToString() != actor["container"].ToString())
            {
                actor.SendError("Target \"" + targetuser.states["name"].ToString() + "\" is an invalid target. \r\n");
                return false;
            }

            //if(targetuser[""])

            // Ensure target is in the room
            if (targetuser["container"].ToString() != actor["container"].ToString())
            {
                actor.SendError("Target \"" + targetuser.states["name"].ToString() + "\" is an invalid target. \r\n");
                return false;
            }
            spell.BeginCast(actor);

            //spell.Cast(user, targetuser);
            return true;
        }

    }


    /// <summary>
    /// Clears the player's target and stops all combat.
    /// </summary>
    public class Command_combatstop : Command, ICommand
    {




        public Command_combatstop()
        {
            name = "command_combatstop";
            words = new string[4] { "combatstop", "cs", "cleartarget", "ct" };
            help.Command = "combatstop";
            help.Summary = "clears your target and stops all combat.";
            help.Syntax = "combatstop";
            help.Examples = new string[4];
            help.Examples[0] = "combatstop";
            help.Examples[1] = "cs";
            help.Examples[2] = "cleartarget";
            help.Examples[3] = "ct";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            actor.CombatStop();
            actor.Send("You clear your target.\r\n");
            return true;
        }

    }


    /// <summary>
    /// Shows all the spells the player knows.
    /// </summary>
    public class Command_spells : Command, ICommand
    {




        public Command_spells()
        {
            name = "command_spells";
            words = new string[1] { "spells" };
            help.Command = "spells";
            help.Summary = "displays the spells you know.";
            help.Syntax = "spells";
            help.Examples = new string[1];
            help.Examples[0] = "spells";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            actor.Showspells();
            return true;
        }

    }

    /// <summary>
    /// Shows all the skills the player knows.
    /// </summary>
    public class Command_skills : Command, ICommand
    {




        public Command_skills()
        {
            name = "command_skills";
            words = new string[1] { "skills" };
            help.Command = "skills";
            help.Summary = "displays the skills you know.";
            help.Syntax = "skills";
            help.Examples = new string[1];
            help.Examples[0] = "skills";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            actor.Showskills();
            return true;
        }

    }




    /// <summary>
    /// Sets someone as your combat target.
    /// </summary>
    public class Command_target : Command, ICommand
    {




        public Command_target()
        {
            name = "command_target";
            words = new string[2] { "target", "ta" };
            help.Command = "target";
            help.Summary = "sets something as your target, but does not attack it.";
            help.Syntax = "target [object name/object id]";
            help.Examples = new string[5];
            help.Examples[0] = "target Bob";
            help.Examples[1] = "ta Bob";
            help.Examples[2] = "ta bag";
            help.Examples[3] = "ta 3mushroom";
            help.Examples[3] = "ta 12557331";
            help.Examples[4] = "\"ta clear\" clears target.";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Actor tmpuser = null;
            int itemnumericprefix = 0;
            int itemquantity = 0;
            bool all = false;
            Actor room = null;

            if (arguments.Length < 1)
            {
                if (actor["target"].ToString() == "")
                {
                    actor.SendError("You have no target.\r\n");
                    return true;
                }

                if (actor["target"].ToString() != "")
                {
                    tmpuser = Lib.GetByID(actor["target"].ToString());
                    // Don't reset target if they are a builder. They are allowed to target objects that
                    // are not in the same room for building purposes.
                    if (tmpuser == null || (tmpuser["container"].ToString() != actor["container"].ToString() && Convert.ToInt32(actor["accesslevel"]) < (int)AccessLevel.Builder))
                    {
                        actor.SendError("Your target is no longer valid. Resetting to nothing.\r\n");
                        actor.ClearTarget();
                    }
                    else
                    {
                        actor["target"] = tmpuser["id"];
                        actor["targettype"] = tmpuser["type"].ToString();
                        actor.Send("Your target is a " + tmpuser["type"].ToString() + " named '" + tmpuser["name"] + "'.\r\n");
                    }
                }
                return true;
            }

            // specified an object to target
            // try to find it by name first



            ArrayList words = Lib.GetWords(arguments);

            // Determine the target item to get and get item count if there is one.
            string itemname = Lib.GetItemNamePrefixAndQuantity(words, ref itemnumericprefix, ref itemquantity, ref all);

            if (itemname == null)
            {
                actor.SendError("Couldn't determine what item you are referring to.\r\n");
                return false;
            }
            if (itemname == "")
            {
                actor.SendError("Couldn't determine what item you are referring to.\r\n");
                return false;
            }
            if (itemname == "clear")
            {
                actor.CombatStop();
                actor.Send("You clear your target.\r\n");
                return true;
            }

            if (itemname.ToLower() == "me" || itemname.ToLower() == "self")
            {
                actor["target"] = actor["id"].ToString();
                actor["targettype"] = actor["type"].ToString();
                actor.Send("You target yourself.\r\n");
                return true;
            }



            Actor item = null;
            // Get current room
            room = actor.GetContainer();
            if (room != null)
            {


                // Check for the target item in the room.
                item = room.GetItemByName(itemname, itemnumericprefix, itemquantity);
                if (item == null)
                {
                    // Check inventory
                    item = actor.GetItemByName(itemname, itemnumericprefix, itemquantity);
                }
                if (item != null)
                {
                    // Found it
                    // Only builders can target something not in the room or someone not logged in
                    //if (Lib.ConvertToBoolean(item["connected"]) == true || Convert.ToInt32(actor["accesslevel"]) >= (int)AccessLevel.Builder)
                    //{
                    //    actor["target"] = item["id"];
                    //    actor["targettype"] = item["type"].ToString();
                    //    actor.Send("Your target is now a " + item["type"].ToString() + " named '" + item["name"] + "'.\r\n");
                    //    return true;
                    //}
                    actor["target"] = item["id"];
                    actor["targettype"] = item["type"].ToString();
                    actor.Send("Your target is now a " + item["type"].ToString() + " named '" + item["name"] + "'.\r\n");
                    return true;
                }
                else
                {
                    // Check if the target IS the room/container
                    if (room["name"].ToString().ToLower() == itemname.ToLower() || room["id"].ToString() == itemname || room["name"].ToString().ToLower().StartsWith(itemname.ToLower()))
                    {
                        // Found it
                        actor["target"] = room["id"];
                        actor["targettype"] = room["type"].ToString();
                        actor.Send("Your target is now a " + room["type"].ToString() + " named '" + room["name"] + "'.\r\n");
                        return true;
                    }
                    // Builders can specify any item by name and get a hit in the whole world
                    if (Convert.ToInt32(actor["accesslevel"]) >= (int)AccessLevel.Builder)
                    {
                        // Check the rest of the world rooms by this name 
                        foreach (Actor tmproom in Lib.GetWorldItems())
                        {
                            if (tmproom["name"].ToString().ToLower() == itemname.ToLower() || tmproom["id"].ToString() == itemname || tmproom["name"].ToString().ToLower().StartsWith(itemname.ToLower()))
                            {
                                // Found it
                                actor["target"] = tmproom["id"];
                                actor["targettype"] = tmproom["type"].ToString();
                                actor.Send("Your target is now a " + tmproom["type"].ToString() + " named '" + tmproom["name"] + "'.\r\n");
                                return true;
                            }
                        }
                    }
                }
            }

            // Player may have given id instead of name
            item = room.GetItemById(itemname);
            if (item == null)
            {
                // Check inventory
                item = actor.GetItemByName(itemname, itemnumericprefix, itemquantity);
            }
            else
            {
                // Found it
                // Only builders can target something not in the room or someone not logged in
                if (Lib.ConvertToBoolean(item["connected"]) == true || Convert.ToInt32(actor["accesslevel"]) >= (int)AccessLevel.Builder)
                {
                    actor["target"] = item["id"];
                    actor["targettype"] = item["type"].ToString();
                    actor.Send("Your target is now a " + item["type"].ToString() + " named '" + item["name"] + "'.\r\n");
                    return true;
                }
            }

            // Check outside the room
            // Only builders can target something not in the room or someone not logged in
            if (Convert.ToInt32(actor["accesslevel"]) >= (int)AccessLevel.Builder)
            {
                foreach (Actor tmpitem in Lib.GetWorldItems())
                {
                    if (tmpitem["id"].ToString() == itemname)
                    {
                        actor["target"] = tmpitem["id"];
                        actor["targettype"] = tmpitem["type"].ToString();
                        actor.Send("Your target is now a " + tmpitem["type"].ToString() + " named '" + tmpitem["name"] + "'.\r\n");
                        return true;

                    }
                }
            }


            actor.SendError("The object you specified could not be found.\r\n");
            return false;
        }

    }



    /// <summary>
    /// yell command for player.
    /// </summary>
    public class Command_yell : Command, ICommand
    {

        public Command_yell()
        {
            name = "command_yell";
            words = new string[1] { "yell" };
            help.Command = "yell";
            help.Summary = "sends a message to all the awake players in your current room.";
            help.Syntax = "yell <text>";
            help.Examples = new string[1];
            help.Examples[0] = "yell Hey, I'm over here!";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            if (arguments.Length < 1)
            {
                actor.SendError("You must specify text to say.\r\n");
                return false;
            }
            actor.Send("You yell, \"" + arguments + "\"\r\n");
            actor.Sayinroom(actor["shortnameupper"] + " yells, '" + arguments + "'");
            return true;
        }

    }

    /// <summary>
    /// shout command for player.
    /// </summary>
    public class Command_shout : Command, ICommand
    {




        public Command_shout()
        {
            name = "command_shout";
            words = new string[1] { "shout" };
            help.Command = "shout";
            help.Summary = "sends a message to all the awake players up to a number of rooms away from you.";
            help.Syntax = "shout <text>";
            help.Examples = new string[1];
            help.Examples[0] = "shout Somebody help!";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            if (arguments.Length < 1)
            {
                actor.SendError("You must specify text to say.\r\n");
                return false;
            }
            actor.Send("You shout, \"" + arguments + "\"\r\n");
            //Maybe change this to "someone shouts", if we don't want people to know who's shouting.
            //Can modify it so people on the current room know who's shouting, but everyone else 
            //gets someone.
            actor.SayinRadius(actor["shortnameupper"] + " shouts, '" + arguments + "'", Lib.ShoutRadius);
            return true;
        }

    }

    ///// <summary>
    ///// whisper command for player.
    ///// </summary>
    //public class Command_whisper : Command,ICommand
    //{
    //    public Command_whisper()
    //    {
    //        name = "command_whisper";
    //        words = new string[1] { "whisper" };
    //        help.Command = "whisper";
    //        help.Summary = "sends a message to all the awake players in your current room.";
    //        help.Syntax = "whisper <text>";
    //        help.Examples = new string[1];
    //        help.Examples[0] = "whisper Want to sneak off and have some fun?";

    //    }

    //    public override bool DoCommand(Actor actor, string command, string arguments)
    //    {
    //        if (arguments.Length < 1)
    //        {
    //            actor.SendError("You must specify text to say.\r\n");
    //            return false;
    //        }
    //        actor.Send("You whisper, \"" + arguments + "\"\r\n");
    //        actor.Sayinroom(actor["shortnameupper"] + " whispers, '" + arguments + "'");
    //        return true;
    //    }
    //}

    /// <summary>
    /// lecture command for player.
    /// </summary>
    public class Command_lecture : Command, ICommand
    {

        public Command_lecture()
        {
            name = "command_lecture";
            words = new string[1] { "lecture" };
            help.Command = "lecture";
            help.Summary = "sends a message to all the awake players in your current room.";
            help.Syntax = "lecture <text>";
            help.Examples = new string[1];
            help.Examples[0] = "lecture You shouldn't do that.";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            if (arguments.Length < 1)
            {
                actor.SendError("You must specify text to say.\r\n");
                return false;
            }
            actor.Send("You lecture, \"" + arguments + "\"\r\n");
            actor.Sayinroom(actor["shortnameupper"] + " lectures, '" + arguments + "'");
            return true;
        }

    }

    /// <summary>
    /// Starts combat with the specified target.
    /// </summary>
    public class Command_kill : Command, ICommand
    {




        public Command_kill()
        {
            name = "command_kill";
            words = new string[2] { "kill", "attack" };
            help.Command = "kill";
            help.Summary = "Attacks the specified user or mob.";
            help.Syntax = "kill <name>";
            help.Examples = new string[2];
            help.Examples[0] = "kill marsha";
            help.Examples[1] = "attack marsha";


        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            ArrayList words = Lib.GetWords(arguments);
            string targetactorname = "";
            // Is user alive?
            if (Convert.ToInt32(actor["health"]) < 1)
            {
                actor.SendError("You cannot attack while dead!");
                return false;
            }
            // No target specified?
            if (arguments.Length < 1)
            {
                actor.SendError("You must specify something to attack.\r\n");
                return false;
            }

            // User specified a long string
            if (arguments.Length > 1)
            {
                targetactorname = arguments;
            }

            if (arguments.Length == 1)
            {
                targetactorname = (string)words[0];
            }

            // Ensure they have an equipped weapon
            if (actor.GetWeaponPrimary() == null && actor.GetWeaponSecondary() == null)
            {
                actor.SendError("You have no weapon to attack with!\r\n");
                return false;
            }

            // Call the target command for code reuse
            bool result = actor.RunCommand("target", targetactorname);
            if (!result)
                return false;


            // Enemy should now be the user's target
            Actor targetactor = Lib.GetByID(actor["target"].ToString());


            // Can't attack yourself
            if (targetactor["id"].ToString() == actor["id"].ToString())
            {
                actor.SendError("You cannot attack yourself!\r\n");
                return false;
            }

            actor["target"] = targetactor["id"];

            if (targetactor["type"].ToString() == "user")
            {
                actor["targettype"] = "user";
            }
            else if (targetactor["type"].ToString() == "mob")
            {
                actor["targettype"] = "mob";
                // Add player to hate list
                Actor.HateTarget hateplayer;
                hateplayer.actor = actor;
                hateplayer.hate = 1;
                targetactor.hatetable.Add(hateplayer);
            }
            else if (targetactor["type"].ToString() == "item")
            {
                actor.SendError("Your target is an item, which you cannot attack.\r\n");
                return false;
            }
            // Flag player as being in combat
            actor["combatactive"] = true;
            actor.Send("Attacking " + targetactor["name"] + "...\r\n");
            if (targetactor["type"].ToString() == "user")
            {
                actor.Sayinroom(actor["shortnameupper"] + " attacks " + targetactor["name"] + "!", (Actor)targetactor);
            }
            else
                actor.Sayinroom(actor["shortnameupper"] + " attacks " + targetactor["name"] + "!");
            targetactor.SendError(actor["name"] + " attacks you!\r\n");

            return true;
        }

    }

    /// <summary>
    /// Send a system message to all users of the MUD
    /// </summary>
    public class Command_sysmessage : Command, ICommand
    {




        public Command_sysmessage()
        {
            name = "command_sysmessage";
            words = new string[1] { "sysmessage" };
            help.Command = "sysmessage";
            help.Summary = "Sends a system message to all users in the world.";
            help.Syntax = "sysmessage <message>";
            help.Examples = new string[1];
            help.Examples[0] = "sysmessage Server is coming down in 5 minutes!";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            if (arguments.Length > 0)
            {
                actor.Send("You send the system message: " + arguments + "\r\n");
                actor.Sayinworld("\r\n\r\n*** SYSTEM MESSAGE: " + arguments + "\r\n");
                Lib.AddServerLogEntry(actor, "SYSMESSAGE: " + arguments);
            }
            else
            {
                actor.SendError("You must specify some text to send.\r\n");
            }
            return true;
        }

    }

    /// <summary>
    /// Send a system message to all users of the MUD
    /// </summary>
    public class Command_announce : Command, ICommand
    {




        public Command_announce()
        {
            name = "command_announce";
            words = new string[1] { "announce" };
            help.Command = "announce";
            help.Summary = "Sends an announcement message to all users in the world.";
            help.Syntax = "announce <message>";
            help.Examples = new string[1];
            help.Examples[0] = "announce Please welcome our new Admin John!";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            if (arguments.Length > 0)
            {
                actor.Send("You announce: " + arguments + "\r\n");
                actor.Sayinworld("*** Announcement from " + actor["name"] + ": " + arguments);
                Lib.AddServerLogEntry(actor, "ANNOUNCE: " + arguments);
            }
            else
            {
                actor.SendError("You must specify some text to send.\r\n");
            }
            return true;
        }

    }


    /// <summary>
    /// think command for player.
    /// </summary>
    public class Command_think : Command, ICommand
    {




        public Command_think()
        {
            name = "command_think";
            words = new string[1] { "think" };
            help.Command = "think";
            help.Summary = "sends a message to all the awake players in your current room.";
            help.Syntax = "think <text>";
            help.Examples = new string[1];
            help.Examples[0] = "think That was a bad idea.";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            if (arguments.Length < 1)
            {
                actor.SendError("You must specify text to say.\r\n");
                return false;
            }
            actor.Send("You think, \"" + arguments + "\"\r\n");
            actor.Sayinroom(actor["shortnameupper"] + " thinks, '" + arguments + "'");
            return true;
        }

    }


    /// <summary>
    /// Kudos command to give kudos to others and to check current reputation.
    /// </summary>
    /// 
    public class Command_kudos : Command, ICommand
    {




        public Command_kudos()
        {
            name = "command_kudos";
            words = new string[1] { "kudos" };
            help.Command = "kudos";
            help.Summary = "This command lets you give kudos points to others or check your current reputation.";
            help.Syntax = "kudos give <playername> <amount>";
            help.Examples = new string[2];
            help.Examples[0] = "kudos";
            help.Examples[1] = "kudos give bob 10";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            ArrayList words = Lib.GetWords(arguments);
            // With no arguments, just tell how many kudos points are available

            if (words.Count == 0)
            {
                actor.Send("Kudos available: " + actor["kudostogive"] + "\r\nReputation: " + actor["reputation"] + "\r\n");
            }
            // ensure 3 arguments
            else if (words.Count == 3)
            {
                // Ensure last argument is numeric
                if (!Lib.IsNumeric((string)words[2]))
                {
                    actor.Send("You must specify the number of kudos to give.\r\n");
                    return false;
                }
                // ensure you have that many kudos points to give
                if (Convert.ToInt32(words[2]) > Convert.ToInt32(actor["kudostogive"]))
                {
                    actor.SendError("You do not have that many kudos to give.\r\n");
                    return false;

                }
                // must give more than 1 kudos
                if (Convert.ToInt32(words[2]) == 0)
                {
                    actor.SendError("You cannot give less than one kudos.\r\n");
                    return false;

                }
                // Ensure target player is valid and logged in

                for (int i = Lib.GetWorldItems().Count - 1; i >= 0; i--)
                {
                    Actor tmpactor = (Actor)Lib.GetWorldItems()[i];
                    if (tmpactor["shortname"].ToString() == (string)words[1] || tmpactor["name"].ToString() == (string)words[1])
                    {
                        // make sure they don't give kudos to themselves
                        if (tmpactor["shortname"] != actor["shortname"])
                        {
                            // found user so do the transaction
                            tmpactor.Kudosadd(Convert.ToInt32(words[2]));
                            actor.Kudosremove(Convert.ToInt32(words[2]));
                            actor.Send("You gave " + words[2] + " kudos to " + tmpactor["name"] + ".\r\n");
                            tmpactor.Send(tmpactor["name"] + " gave you " + words[2] + " kudos!!!\r\n");
                            return true;
                        }
                        else
                        {
                            actor.SendError("You cannot give kudos to yourself.\r\n");
                            return false;
                        }
                    }
                }
                actor.SendError("There is no one available by the name " + (string)words[3] + ".\r\n");

            }
            else
            {
                actor.SendError("Incorrect number of parameters for this command.\r\n");
                return false;
            }

            return true;
        }

    }

    /// <summary>
    /// Emote command for player.
    /// </summary>
    public class Command_emote : Command, ICommand
    {




        public Command_emote()
        {
            name = "command_emote";
            words = new string[3] { "emote", "em", "!" };
            help.Command = "emote, em or !";
            help.Summary = "This command is a way for you to show your expressions or actions.";
            help.Syntax = "emote <text>";
            help.Examples = new string[2];
            help.Examples[0] = "emote dances around the room.";
            help.Examples[1] = "! hops about madly";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            // Catch when someone sends the emote command with nothing after it
            if (arguments.Length < 1)
            {
                actor.SendError("You must specify text to emote.\r\n");
                return false;
            }

            actor.Send("** " + actor.GetNameUpper() + " " + arguments + " **" + "\r\n");
            actor.Sayinroom("** " + actor.GetNameUpper() + " " + arguments + " **");

            return true;
        }

    }

    /// <summary>
    /// Tell command for player.
    /// </summary>
    public class Command_tell : Command, ICommand
    {




        public Command_tell()
        {
            name = "command_tell";
            words = new string[2] { "tell", "t" };
            help.Command = "tell or t";
            help.Summary = "Sends a message to a specific person.";
            help.Syntax = "tell <player> <text>";
            help.Examples = new string[1];
            help.Examples[0] = "tell bob Hello there!";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            bool chk = false;
            if (arguments.Length < 1)
            {
                actor.SendError("You must specify a player name and a message to tell.\r\n");
                return false;
            }
            ArrayList words = Lib.GetWords(arguments);
            if (words.Count < 2)
            {
                actor.SendError("You must specify a player name and a message to tell.\r\n");
                return true;
            }
            // user to tell is the second word in arraylist
            string tellactor = (string)words[0];
            string tellmsg = arguments.Substring(tellactor.Length + 1);
            // Find the target user

            for (int i = Lib.GetWorldItems().Count - 1; i >= 0; i--)
            {
                Actor tmpactor = (Actor)Lib.GetWorldItems()[i];
                if (tmpactor["shortname"].ToString() == tellactor && tmpactor.UserSocket.Connected)
                {
                    if (tmpactor["shortname"] != actor["shortname"])
                    {
                        chk = true;
                        actor.Send("You tell " + tmpactor.GetNameUpper() + ", '" + tellmsg + "'" + "\r\n");
                        tmpactor.Send(actor.GetNameUpper() + " tells you, '" + tellmsg + "'" + "\r\n");
                    }
                    else
                    {
                        actor.SendError("Talking to yourself?" + "\r\n");
                        chk = true;
                    }
                }
            }
            // Didn't find user to speak to
            if (chk == false)
            {
                actor.SendError("Cannot find user '" + tellactor + "'" + "\r\n");
            }

            return true;
        }

    }

    /// <summary>
    /// Whsiper Command starts here - Kedearian
    /// </summary>
    public class Command_whipser : Command, ICommand
    {




        public Command_whipser()
        {
            name = "command_whisper";
            words = new string[1] { "whisper" };
            help.Command = "whisper";
            help.Summary = "Sends a message to a specific person.";
            help.Syntax = "whisper <player> <text>";
            help.Examples = new string[1];
            help.Examples[0] = "whisper susan Hello.";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            bool chk = false;
            if (arguments.Length < 1)
            {
                actor.SendError("You must specify a player name and a message to whisper.\r\n");
                return false;
            }
            ArrayList words = Lib.GetWords(arguments);
            if (words.Count < 2)
            {
                actor.SendError("You must specify a player name and a message to whisper.\r\n");
                return true;
            }
            string tellactor = (string)words[0];
            string tellmsg = arguments.Substring(tellactor.Length + 1);
            Actor room = actor.GetContainer();

            for (int i = room.GetContents().Count - 1; i >= 0; i--)
            //for (int i = Lib.GetWorldItems().Count - 1; i >= 0; i--)
            {
                Actor tmpactor = (Actor)room.GetContents()[i];
                if (tmpactor["shortname"].ToString() == tellactor && tmpactor.UserSocket.Connected)
                {
                    if (tmpactor["shortname"] != actor["shortname"])
                    {
                        chk = true;
                        actor.Send("You whipser, '" + tellmsg + "' to " + tmpactor.GetNameUpper() + "\r\n");
                        tmpactor.Send(actor.GetNameUpper() + " whispers to you, '" + tellmsg + "'" + "\r\n");
                    }
                    else
                    {
                        actor.SendError("Whispering to yourself is a bit silly, isn't it?" + "\r\n");
                        chk = true;
                    }
                }
            }
            // Didn't find user to speak to
            if (chk == false)
            {
                actor.SendError("You look around but dont see '" + tellactor + "' anwhere around here." + "\r\n");
            }

            return true;
        }


    }



    /// <summary>
    /// Look command for player.
    /// </summary>
    public class Command_look : Command, ICommand
    {




        public Command_look()
        {
            name = "command_look";
            words = new string[5] { "look", "l", "lo", "examine", "exa" };
            help.Command = "look or l";
            help.Summary = "Look at room or at something.";
            help.Syntax = "look [in] [object|character]";
            help.Examples = new string[4];
            help.Examples[0] = "look";
            help.Examples[1] = "look bag";
            help.Examples[2] = "look in bag";
            help.Examples[3] = "look bob";

        }


        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            if (arguments.Length < 1)
            {
                actor.Showroom();
                return true;
            }

            Actor item = Lib.GetActorFromArguments(actor, arguments);
            if (item == null)
            {
                return false;
            }

            // Let target user know they are being looked at
            // As long as you're not looking at yourself
            if (actor != item)
            {
                item.Send(item["colormessages"] + actor.GetNameUpper() + " is looking at you.\r\n" + item["colorcommandtext"]);
            }

            actor.Send(item.GetNameFullUpper() + "\r\n" + item["descr"] + "\r\n");

            // Show contents if they exist
            if (item.GetContents().Count > 0)
            {
                item.ShowContents(actor);
            }

            // If item is living, then show reputation, health, etc.
            if (item.Killable)
            {
                if (item["type"].ToString().Equals("user"))
                {
                    actor.Send("This person's reputation score is " + item["reputation"] + ".\r\n");
                }

                if (Convert.ToInt32(item["health"]) <= 0)
                {
                    actor.Send(item.GetNameUpper().Replace("Corpse of ", "") + ". This person is dead.\r\n");
                }
                else if (Convert.ToInt32(item["health"]) < Convert.ToInt32(item["healthmax"]))
                {
                    actor.Send("This person is injured, but you cannot tell to what extent.\r\n");
                }
                else if (Convert.ToInt32(item["health"]) >= Convert.ToInt32(item["healthmax"]))
                {
                    actor.Send("This person is perfectly healthy.\r\n");
                }
            }
            return true;
        }

    }

    /// <summary>
    /// Movement command for player.
    /// </summary>
    public class Command_move : Command, ICommand
    {




        public Command_move()
        {
            name = "command_move";
            words = new string[22] { "north", "n", "northeast", "ne",
									   "east", "e", "southeast", "se", "south", "s",
									   "southwest", "sw", "west", "w", "northwest", "nw",
									   "up", "u", "down", "d", "in", "out" };
            help.Command = "<direction>";
            help.Summary = "Move in a particular direction.";
            help.Syntax = "<direction>";
            help.Examples = words;

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            string movedir = ""; // String of your move direction
            string destroomid = ""; // Chk if there is actually a room where you want to go  
            Actor destination = new Actor();

            Actor room = Lib.GetByID(actor["container"].ToString());
            bool dig = false;
            string newRoomID = "";

            if ((string)actor["digging"] == "true")
                dig = true;

            switch (command)
            {
                case "north":
                    movedir = "to the north";
                    if (room["xnorth"] != null)
                        destroomid = room["xnorth"].ToString();
                    break;
                case "n":
                    movedir = "to the north";
                    if (room["xnorth"] != null)
                        destroomid = room["xnorth"].ToString();
                    break;
                case "northeast":
                    movedir = "to the northeast";
                    if (room["xnortheast"] != null)
                        destroomid = room["xnortheast"].ToString();
                    break;
                case "ne":
                    movedir = "to the northeast";
                    if (room["xnortheast"] != null)
                        destroomid = room["xnortheast"].ToString();
                    break;
                case "east":
                    movedir = "to the east";
                    if (room["xeast"] != null)
                        destroomid = room["xeast"].ToString();
                    break;
                case "e":
                    movedir = "to the east";
                    if (room["xeast"] != null)
                        destroomid = room["xeast"].ToString();
                    break;
                case "southeast":
                    movedir = "to the southeast";
                    if (room["xsoutheast"] != null)
                        destroomid = room["xsoutheast"].ToString();
                    break;
                case "se":
                    movedir = "to the southeast";
                    if (room["xsoutheast"] != null)
                        destroomid = room["xsoutheast"].ToString();
                    break;
                case "south":
                    movedir = "to the south";
                    if (room["xsouth"] != null)
                        destroomid = room["xsouth"].ToString();
                    break;
                case "s":
                    movedir = "to the south";
                    if (room["xsouth"] != null)
                        destroomid = room["xsouth"].ToString();
                    break;
                case "southwest":
                    movedir = "to the southwest";
                    if (room["xsouthwest"] != null)
                        destroomid = room["xsouthwest"].ToString();
                    break;
                case "sw":
                    movedir = "to the southwest";
                    if (room["xsouthwest"] != null)
                        destroomid = room["xsouthwest"].ToString();
                    break;
                case "west":
                    movedir = "to the west";
                    if (room["xwest"] != null)
                        destroomid = room["xwest"].ToString();
                    break;
                case "w":
                    movedir = "to the west";
                    if (room["xwest"] != null)
                        destroomid = room["xwest"].ToString();
                    break;
                case "northwest":
                    movedir = "to the northwest";
                    if (room["xnorthwest"] != null)
                        destroomid = room["xnorthwest"].ToString();
                    break;
                case "nw":
                    movedir = "to the northwest";
                    if (room["xnorthwest"] != null)
                        destroomid = room["xnorthwest"].ToString();
                    break;
                case "up":
                    movedir = "upwards";
                    if (room["xup"] != null)
                        destroomid = room["xup"].ToString();
                    break;
                case "u":
                    movedir = "upwards";
                    if (room["xup"] != null)
                        destroomid = room["xup"].ToString();
                    break;
                case "down":
                    movedir = "downwards";
                    if (room["xdown"] != null)
                        destroomid = room["xdown"].ToString();
                    break;
                case "d":
                    movedir = "downwards";
                    if (room["xdown"] != null)
                        destroomid = room["xdown"].ToString();
                    break;
                case "in":
                    movedir = "inside";
                    if (room["xin"] != null)
                        destroomid = room["xin"].ToString();
                    break;
                case "out":
                    movedir = "outside";
                    if (room["xout"] != null)
                        destroomid = room["xout"].ToString();
                    break;
                case "o":
                    movedir = "outside";
                    if (room["xout"] != null)
                        destroomid = room["xout"].ToString();
                    break;
            }


            if (dig)
            {
                destination = Lib.GetByID(destroomid);
                if (destination == null)
                {
                    newRoomID = DigRoom(actor, room, command);
                    destination = Lib.GetByID(newRoomID);
                }
                else
                {
                    actor.SendError("You cannot dig that way." + "\r\n");
                    return false;
                }
            }
            else
            {
                // Protect against invalid destinations
                destination = Lib.GetByID(destroomid);
                if (destination == null)
                {
                    actor.SendError("You cannot go that way." + "\r\n");
                    return false;
                }
            }


            actor.Sayinroom(actor.GetNameUpper() + " left, going " + movedir + ".");
            //actor["container"]=destination["id"];
            //actor["containertype"]=destination["type"].ToString();
            // Remove user from current room
            room.Removeitem(actor);
            // Add them to the new room
            destination.Additem(actor);
            // Send 'he arrived' messages
            actor.Sayinroom(actor.GetNameUpper() + " arrived from " + actor.GetOppositeDirection(movedir) + ".");
            // Display room
            actor.Showroom();
            return true;
        }

        /// <summary>
        /// handles calling functions to create the new room, set its exit to starting room, 
        /// and updating current room's exit to new room.
        /// Also sets new room as user's target for easy modifing.
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="room"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public string DigRoom(Actor actor, Actor room, string direction)
        {
            string newRoomID = "";
            string backdir = GetOppositeDirection(direction);
            newRoomID = CreateNewRoom(room, backdir);
            actor["target"] = newRoomID;
            actor["targettype"] = "room";
            SetCurrentRoomNewExit(room, newRoomID, direction);
            return newRoomID;
        }

        /// <summary>
        /// Takes a direction and returns the opposite direction.  Used to map new room's exit to the opposite of 
        /// which the use came from (if digging south, set new room's north exit to start room)
        /// </summary>
        /// <param name="direction"></param>
        /// <returns>opposite of direction</returns>
        public string GetOppositeDirection(string direction)
        {
            string backdir = "";
            switch (direction)
            {
                case "north":
                    backdir = "s";
                    break;
                case "n":
                    backdir = "s";
                    break;
                case "northeast":
                    backdir = "sw";
                    break;
                case "ne":
                    backdir = "sw";
                    break;
                case "east":
                    backdir = "w";
                    break;
                case "e":
                    backdir = "w";
                    break;
                case "southeast":
                    backdir = "nw";
                    break;
                case "se":
                    backdir = "nw";
                    break;
                case "south":
                    backdir = "n";
                    break;
                case "s":
                    backdir = "n";
                    break;
                case "southwest":
                    backdir = "ne";
                    break;
                case "sw":
                    backdir = "ne";
                    break;
                case "west":
                    backdir = "e";
                    break;
                case "w":
                    backdir = "e";
                    break;
                case "northwest":
                    backdir = "se";
                    break;
                case "nw":
                    backdir = "se";
                    break;
                case "up":
                    backdir = "d";
                    break;
                case "u":
                    backdir = "d";
                    break;
                case "down":
                    backdir = "u";
                    break;
                case "d":
                    backdir = "u";
                    break;
                case "in":
                    backdir = "out";
                    break;
                case "out":
                    backdir = "in";
                    break;
                case "o":
                    backdir = "in";
                    break;
            }
            return backdir;
        }


        /// <summary>
        /// Takes a room, direction for exit, and roomid for exit and sets them
        /// </summary>
        /// <param name="room"></param>
        /// <param name="direction"></param>
        /// <param name="toRoomId"></param>
        /// <returns>true</returns>
        public bool setRoomExit(Actor room, string direction, string toRoomId)
        {
            switch (direction)
            {
                case "north":
                case "n":
                    room["xnorth"] = toRoomId;
                    break;
                case "south":
                case "s":
                    room["xsouth"] = toRoomId;
                    break;
                case "west":
                case "w":
                    room["xwest"] = toRoomId;
                    break;
                case "east":
                case "e":
                    room["xeast"] = toRoomId;
                    break;
                case "northwest":
                case "nw":
                    room["xnorthwest"] = toRoomId;
                    break;
                case "northeast":
                case "ne":
                    room["xnortheast"] = toRoomId;
                    break;
                case "southwest":
                case "sw":
                    room["xsouthwest"] = toRoomId;
                    break;
                case "southeast":
                case "se":
                    room["xsoutheast"] = toRoomId;
                    break;
                case "in":
                    room["xin"] = toRoomId;
                    break;
                case "o":
                case "out":
                    room["xout"] = toRoomId;
                    break;
                case "up":
                case "u":
                    room["xup"] = toRoomId;
                    break;
                case "down":
                case "d":
                    room["xdown"] = toRoomId;
                    break;
            }
            return true;
        }


        /// <summary>
        /// creates a new room, sets its zone and subtype to start room's zone and subtype,
        /// adds it to lib.items
        /// sets name and short name to avoid modify list error
        /// saves room
        /// </summary>
        /// <param name="room"></param>
        /// <param name="backdir"></param>
        /// <returns>new room's id</returns>
        public string CreateNewRoom(Actor room, string backdir)
        {
            Actor newroom = new Actor();
            newroom["type"] = "room";
            string roomid = room["id"].ToString();

            Lib.GetWorldItems().Add(newroom);

            setRoomExit(newroom, backdir, roomid);
            newroom["subtype"] = room["subtype"];
            newroom["zone"] = room["zone"];
            newroom["name"] = "Blank Room";
            newroom["shortname"] = "room";
            //newroom.LastLoginDate = room.LastLoginDate;
            newroom.Save();
            return newroom["id"].ToString();
        }


        /// <summary>
        /// Takes starting room and adds newly dug room to exit
        /// </summary>
        /// <param name="room"></param>
        /// <param name="newRoomID"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool SetCurrentRoomNewExit(Actor room, string newRoomID, string direction)
        {
            string roomid = newRoomID;
            setRoomExit(room, direction, newRoomID);
            room.Save();
            return true;
        }
    }

    /// <summary>
    /// Inventory command for player.
    /// </summary>
    public class Command_inventory : Command, ICommand
    {

        public Command_inventory()
        {
            name = "command_inventory";
            words = new string[3] { "inventory", "inv", "i" };
            help.Command = "inventory";
            help.Summary = "Displays the items that you are carrying on you.";
            help.Syntax = "inventory";
            help.Examples = new string[1];
            help.Examples[0] = "inventory";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            bool chk = false;
            string txt;

            actor.Send("You are carrying: " + "\r\n");
            foreach (Actor item in actor.GetContents())
            {
                if (item != null)
                {
                    chk = true;
                    if (Convert.ToInt32(item["quantity"]) > 1)
                    {
                        txt = item["quantity"] + " " + item["name"] + "s";
                    }
                    else
                    {
                        txt = item.GetNamePrefixUpper() + " " + item["name"];
                    }
                    if (Lib.ConvertToBoolean(item["equipped"]) == true)
                    {
                        txt = txt + " (Equipped)";
                    }
                    actor.Send(txt + "\r\n");
                }
            }
            if (chk == false)
            {
                actor.Send("Nothing" + "\r\n");
            }

            return true;
        }
    }

    /// <summary>
    /// Inventory command for player.
    /// </summary>
    public class Command_market : Command, ICommand
    {

        public Command_market()
        {
            name = "command_market";
            words = new string[2] { "market", "mar" };
            help.Command = "market";
            help.Summary = "Displays the market welcome screen to buy and sell items.";
            help.Syntax = "market";
            help.Examples = new string[1];
            help.Examples[0] = "market";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {

            // Make sure a marketer is in the same room
            if (!Market.CheckForMarketer(actor))
            {
                actor.SendError("There is no marketer in this room.\r\n");
                return false;
            }

            Lib.ShowMarket(actor);
            return true;
        }

    }

    /// <summary>
    /// Get command for player.
    /// </summary>
    public class Command_get : Command, ICommand
    {




        public Command_get()
        {
            name = "command_get";
            words = new string[1] { "get" };
            help.Command = "get\r\n";
            help.Summary = "Gets an object or objects from the ground or from containers.";
            help.Syntax = "get [#|all] [#][item] [from <containername>]";
            help.Examples = new string[7];
            help.Examples[0] = "get sword";
            help.Examples[1] = "get 2sword";
            help.Examples[2] = "get 20 coin";
            help.Examples[3] = "get all coin";
            help.Examples[4] = "get all";
            help.Examples[5] = "get 20 coins from backpack";
            help.Examples[6] = "get all from backpack";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Actor room = actor.GetContainer();
            string containername = null;
            Actor item = null;
            Actor container = null;
            string itemname = null;
            int containernumericprefix = 0; // Numeric portion of the text converted to integer
            int itemnumericprefix = 0;
            int itemquantity = 0;
            bool all = false;

            // Populate words array with the command words
            ArrayList words = Lib.GetWords(arguments);
            if (words.Count < 1)
            {
                actor.SendError("You must specify something to get.\r\n");
                return false;
            }
            if (words.Count == 1 && (string)words[0] == "all")
            {
                foreach (Actor scanitem in room.GetContents())
                {
                    actor.TakeItem(scanitem, 0, room);
                }
                return true;
            }
            // Determine the target item to get and get item count if there is one.
            itemname = Lib.GetItemNamePrefixAndQuantity(words, ref itemnumericprefix, ref itemquantity, ref all);
            if (itemname == null)
            {
                actor.SendError("Couldn't determine what item you are referring to.\r\n");
                return false;
            }
            if (itemname == "")
            {
                actor.SendError("Couldn't determine what item you are referring to.\r\n");
                return false;
            }
            // Get the container name from the word array if there is one.
            containername = Lib.GetContainerName(words);

            // Code for when container is involved
            if (containername != null)
            {
                containername = Lib.SplitItemNameFromNumericPrefix(containername, ref containernumericprefix);
                container = actor.GetItemByName(containername, containernumericprefix);
                if (container == null)
                {
                    container = room.GetItemByName(containername, containernumericprefix);
                }
                if (container == null)
                {
                    actor.SendError("You don't have a container named '" + containername + "'.\r\n");
                    return false;
                }
                // Cannot just take items from users and mobs, that would be a steal command or something like it
                if (container["type"].ToString() == "user" || container["type"].ToString() == "mob")
                {
                    actor.SendError("You cannot just take things from " + container["name"] + ".\r\n");
                    return false;
                }
                item = container.GetItemByName(itemname, itemnumericprefix, itemquantity);
                if (item == null)
                {
                    actor.SendError("The item is not in the container you specified.\r\n");
                    return false;
                }

                room = container;
            }


            // Code for when no container is involved.
            // Check for the target item in the room.
            item = room.GetItemByName(itemname, itemnumericprefix, itemquantity);
            if (item == null)
            {
                actor.SendError("The item you specified is not in the room.\r\n");
                return false;
            }
            // first of all, you cannot do this to other players no matter what
            if (item["type"].ToString() == "user" || item["type"].ToString() == "room" || item["type"].ToString() == "mob")
            {
                actor.SendError("You cannot do that.\r\n");
                return false;
            }
            // All of something specific, rather than just 'all' of everything
            if (all)
            {
                foreach (Actor scanitem in room.GetContents())
                {
                    if (scanitem["subtype"] == item["subtype"]) actor.TakeItem(item, 0, room);
                }
                return true;
            }
            else
            {
                actor.TakeItem(item, itemquantity, room);
                return true;
            }
        }

    }

    /// <summary>
    /// Merges multiple stacked items into one stack.
    /// </summary>
    public class Command_merge : Command, ICommand
    {




        public Command_merge()
        {
            name = "command_merge";
            words = new string[1] { "merge" };
            help.Command = "merge";
            help.Summary = "Merges multiple stacked items into one stack.";
            help.Syntax = "merge <item name>";
            help.Examples = new string[1];
            help.Examples[0] = "merge coins";


        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            int itemnumericprefix = 0;
            int itemquantity = 0;
            bool all = false;

            // Populate words array with the command words
            ArrayList words = Lib.GetWords(arguments);

            if (words.Count < 1)
            {
                actor.SendError("You must specify something to merge.\r\n");
                return false;
            }

            // Determine the target item to get and get item count if there is one.
            string itemname = Lib.GetItemNamePrefixAndQuantity(words, ref itemnumericprefix, ref itemquantity, ref all);
            if (itemname == null)
            {
                actor.SendError("Couldn't determine what item you are referring to.\r\n");
                return false;
            }
            if (itemname == "")
            {
                actor.SendError("Couldn't determine what item you are referring to.\r\n");
                return false;
            }

            Actor item = actor.GetItemByName(itemname);
            if (item == null)
            {
                actor.SendError("You don't have any of that item in your top level inventory.\r\n");
                return false;
            }

            // You can only merge stackable items
            if (!item["subtype"].ToString().StartsWith("stack"))
            {
                actor.SendError("That is not a stackable item, thus it cannot be merged.\r\n");
                return false;
            }


            ArrayList mergeitems = new ArrayList();

            // Find all of that type of item in player inventory and add to arraylist
            foreach (Actor tmpitem in actor.GetContents())
            {
                if (item["subtype"] == tmpitem["subtype"])
                {
                    mergeitems.Add(tmpitem);
                }
            }

            if (mergeitems.Count < 2)
            {
                actor.SendError("There was only one of that type of item, so no merge occurred.\r\n");
                return false;
            }

            // Find the first item
            Actor firstitem = (Actor)mergeitems[0];


            // Now merge them all together
            foreach (Actor tmpitem2 in mergeitems)
            {
                // Don't add first item to itself
                if (tmpitem2 != firstitem)
                {
                    actor.Send("Merged " + tmpitem2["quantity"] + " into an existing stack, thus creating a new stack of " + (Convert.ToInt32(firstitem["quantity"]) + Convert.ToInt32(tmpitem2["quantity"])) + ".\r\n");
                    // Add all the other item quantities to the first item, then destroy them.
                    firstitem["quantity"] = Convert.ToInt32(firstitem["quantity"]) + Convert.ToInt32(tmpitem2["quantity"]);
                    tmpitem2.Destroy();
                }
            }
            return true;
        }

    }

    /// <summary>
    /// Drop command for player.
    /// </summary>
    public class Command_drop : Command, ICommand
    {




        public Command_drop()
        {
            name = "command_drop";
            words = new string[2] { "drop", "put" };
            help.Command = "drop";
            help.Summary = "Drops an object, or currency, on the ground.";
            help.Syntax = "drop/put [#|all] [#][item] into/in [container]";
            help.Examples = new string[9];
            help.Examples[0] = "drop sword";
            help.Examples[1] = "drop sword from bag";
            help.Examples[2] = "drop 2sword";
            help.Examples[3] = "drop 2sword from bag";
            help.Examples[4] = "drop 20 coin";
            help.Examples[5] = "drop 20 coin from bag";
            help.Examples[6] = "drop all coin";
            help.Examples[7] = "drop all coin from bag";
            help.Examples[8] = "drop all";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Actor newstack = new Actor();

            Actor room = Lib.GetByID(actor["container"].ToString());
            string containername = null;
            Actor item = null;
            Actor container = null;
            string itemname = null;
            int containernumericprefix = 0; // Numeric portion of the text converted to integer
            int itemnumericprefix = 0;
            int itemquantity = 0;
            bool all = false;
            // Populate words array with the command words
            ArrayList words = Lib.GetWords(arguments);
            if (words.Count < 1)
            {
                actor.SendError("You must specify something to drop.\r\n");
                return false;
            }
            if (words.Count == 1 && (string)words[0] == "all")
            {
                foreach (Actor scanitem in actor.GetContents())
                {
                    actor.PutItem(scanitem, 0, room);
                }
                return true;
            }

            // Determine the target item to get and get item count if there is one.
            itemname = Lib.GetItemNamePrefixAndQuantity(words, ref itemnumericprefix, ref itemquantity, ref all);

            if (itemname == null)
            {
                actor.SendError("Couldn't determine what item you are referring to.\r\n");
                return false;
            }
            if (itemname == "")
            {
                actor.SendError("Couldn't determine what item you are referring to.\r\n");
                return false;
            }

            // Get the container name from the word array if there is one.
            containername = Lib.GetContainerName(words);

            // Code for when container is involved
            if (containername != null)
            {
                containername = Lib.SplitItemNameFromNumericPrefix(containername, ref containernumericprefix);
                container = actor.GetItemByName(containername, containernumericprefix);
                if (container == null)
                {
                    container = room.GetItemByName(containername, containernumericprefix);
                }
                if (container == null)
                {
                    actor.SendError("You don't have a container named '" + containername + "'.\r\n");
                    return false;
                }
                // Cannot drop items into users and mobs
                if (container["type"].ToString() == "user" || container["type"].ToString() == "mob")
                {
                    actor.SendError("You cannot put anything into " + container["name"] + ".\r\n");
                    return false;
                }

                item = actor.GetItemByName(itemname, itemnumericprefix, itemquantity);
                if (item == null)
                {
                    actor.SendError("You don't have an item named " + itemname + ".\r\n");
                    return false;
                }

                if (container["id"].ToString() == item["id"].ToString())
                {
                    actor.SendError("You cannot put anything into itself.\r\n");
                    return false;
                }

                room = container;
            }

            // Code for when no container is involved.
            // Check for the target item in inventory.
            item = actor.GetItemByName(itemname, itemnumericprefix, itemquantity);
            if (item == null)
            {
                actor.SendError("The item you specified is not in your inventory.\r\n");
                return false;
            }

            // you cannot drop other players
            if (item["type"].ToString() == "user" || item["type"].ToString() == "room" || item["type"].ToString() == "mob")
            {
                actor.SendError("You cannot do that.\r\n");
                return false;
            }

            // All of something specific, rather than just 'all' of everything
            if (all)
            {
                foreach (Actor scanitem in actor.GetContents())
                {
                    if (scanitem["subtype"] == item["subtype"]) actor.PutItem(item, 0, room);
                }
                return true;
            }
            else
            {
                actor.PutItem(item, itemquantity, room);
                return true;
            }

        }

    }

    /// <summary>
    /// Equip command for player.
    /// </summary>
    public class Command_equip : Command, ICommand
    {




        public Command_equip()
        {
            name = "command_equip";
            words = new string[2] { "equip", "wield" };
            help.Command = "equip";
            help.Summary = "Equips an item on the player.";
            help.Syntax = "equip <item>";
            help.Examples = new string[2];
            help.Examples[0] = "equip helmet";
            help.Examples[1] = "equip";
            help.Description = "Type equip by itself to see your currently equipped items.";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            int numericprefix = 0; // Numeric portion of the text converted to integer
            Actor item = null;

            // Populate words array with the command words
            ArrayList words = Lib.GetWords(arguments);

            if (words.Count < 1)
            {
                // Player only typed equip, so show what they have currently equipped
                actor.Send(actor["colorexits"] + "Currently equipped items:\r\n");
                actor.Send("Head: ");
                item = Lib.GetByID(actor["wearhead"]);
                if (item != null)
                {
                    actor.Send((Lib.GetByID(actor["wearhead"]))["name"]);
                }
                actor.Send("\r\nNeck: ");
                item = Lib.GetByID(actor["wearneck"]);
                if (item != null)
                {
                    actor.Send((Lib.GetByID(actor["wearneck"]))["name"]);
                }
                actor.Send("\r\nShoulders: ");
                item = Lib.GetByID(actor["wearshoulders"]);
                if (item != null)
                {
                    actor.Send((Lib.GetByID(actor["wearshoulders"]))["name"]);
                }
                actor.Send("\r\nBack: ");
                item = Lib.GetByID(actor["wearback"]);
                if (item != null)
                {
                    actor.Send((Lib.GetByID(actor["wearback"]))["name"]);
                }
                actor.Send("\r\nArms: ");
                item = Lib.GetByID(actor["weararms"]);
                if (item != null)
                {
                    actor.Send((Lib.GetByID(actor["weararms"]))["name"]);
                }
                actor.Send("\r\nWrists: ");
                item = Lib.GetByID(actor["wearwrists"]);
                if (item != null)
                {
                    actor.Send((Lib.GetByID(actor["wearwrists"]))["name"]);
                }

                actor.Send("\r\nHands: ");
                item = Lib.GetByID(actor["wearhands"]);
                if (item != null)
                {
                    actor.Send((Lib.GetByID(actor["wearhands"]))["name"]);
                }
                actor.Send("\r\nRight Ring: ");
                item = Lib.GetByID(actor["wearrightring"]);
                if (item != null)
                {
                    actor.Send((Lib.GetByID(actor["wearrightring"]))["name"]);
                }
                actor.Send("\r\nLeft Ring: ");
                item = Lib.GetByID(actor["wearleftring"]);
                if (item != null)
                {
                    actor.Send((Lib.GetByID(actor["wearleftring"]))["name"]);
                }
                actor.Send("\r\nChest: ");
                item = Lib.GetByID(actor["wearchest"]);
                if (item != null)
                {
                    actor.Send((Lib.GetByID(actor["wearchest"]))["name"]);
                }
                actor.Send("\r\nWaist: ");
                item = Lib.GetByID(actor["wearwaist"]);
                if (item != null)
                {
                    actor.Send((Lib.GetByID(actor["wearwaist"]))["name"]);
                }
                actor.Send("\r\nLegs: ");
                item = Lib.GetByID(actor["wearlegs"]);
                if (item != null)
                {
                    actor.Send((Lib.GetByID(actor["wearlegs"]))["name"]);
                }
                actor.Send("\r\nFeet: ");
                item = Lib.GetByID(actor["wearfeet"]);
                if (item != null)
                {
                    actor.Send((Lib.GetByID(actor["wearfeet"]))["name"] + "\r\n");
                }
                actor.Send("\r\nPrimary Weapon: ");
                item = Lib.GetByID(actor["wearweapon1"]);
                if (item != null)
                {
                    actor.Send((Lib.GetByID(actor["wearweapon1"]))["name"]);
                }
                actor.Send("\r\nSecondary Weapon: ");
                item = Lib.GetByID(actor["wearweapon2"]);
                if (item != null)
                {
                    actor.Send((Lib.GetByID(actor["wearweapon2"]))["name"]);
                }
                actor.Send("\r\n");
                return true;
            }


            // First find out if the item name has a numeric prefix, 
            // which means a certain item past the first occurance of it.
            // for example, "equip 2sword" or "equip 3torch"
            // grab the item name text = txt
            string itemtxt = Lib.SplitItemNameFromNumericPrefix(arguments, ref numericprefix);
            if (itemtxt != null)
            {
                if (itemtxt == "all")
                {
                    if (Lib.YesNo(actor, "Are you sure you want to equip everything you are carrying? (y/n): "))
                    {
                        for (int tmpcounter = actor.GetContents().Count - 1; tmpcounter >= 0; tmpcounter--)
                        {
                            item = actor.GetItemAtIndex(tmpcounter);
                            // Compare this item's slot with what user may already have in that slot
                            if (item != null)
                            {
                                // Cannot already have something equipped in that slot
                                if (actor["wear" + item["equipslot"].ToString()].ToString() != String.Empty)
                                {
                                    actor.SendError("You already have an item equipped in that location.\r\n");
                                    return false;
                                }

                                if (actor.Equip(item))
                                {
                                    actor.Send("You equipped " + item["nameprefix"] + " " + item["name"] + " on your " + item["equipslot"] + ".\r\n");

                                    //add bonuses

                                }
                                else
                                {
                                    actor.SendError("You cannot equip " + item["nameprefix"] + " " + item["name"] + " on your " + item["equipslot"] + ".\r\n");
                                    if (!Lib.ConvertToBoolean(item["equipable"]))
                                    {
                                        actor.Send("It is not an equippable item.\r\n");
                                    }
                                    else if (Lib.ConvertToBoolean(item["equipped"]))
                                    {
                                        actor.Send("It is already equipped.\r\n");
                                    }
                                }
                            }
                        }
                        // Save any user changes to the db

                        actor.Save();



                    }
                    return true;
                }

                Actor tmpitem = actor.GetItemByName(actor.GetContents(), itemtxt, numericprefix, 1);
                if (tmpitem != null)
                {
                    // Cannot already have something equipped in that slot
                    if (actor["wear" + tmpitem["equipslot"].ToString()].ToString() != String.Empty)
                    {
                        actor.SendError("You already have an item equipped in that location.\r\n");
                        return false;
                    }
                    if (actor.Equip(tmpitem))
                    {
                        actor.Send("You equipped " + tmpitem["nameprefix"] + " " + tmpitem["name"] + " on your " + tmpitem["equipslot"] + ".\r\n");
                        // Tell others in the room that he equipped it
                        actor.Sayinroom(actor.GetNameUpper() + " equipped " + tmpitem["nameprefix"] + " " + tmpitem["name"] + ".");
                        // Save user changes to db
                        actor.Save();
                    }
                    else
                    {
                        actor.Send("You cannot equip " + tmpitem["nameprefix"] + " " + tmpitem["name"] + " on your " + tmpitem["equipslot"] + ".\r\n");
                        if (!Lib.ConvertToBoolean(tmpitem["equipable"]))
                        {
                            actor.Send("It is not an equippable item.\r\n");
                        }
                        else if (Lib.ConvertToBoolean(tmpitem["equipped"]))
                        {
                            actor.Send("It is already equipped.\r\n");
                        }
                    }
                    return true;
                }

            }

            actor.SendError("You have no '" + itemtxt + "'." + "\r\n");
            return false;
        }

    }

    /// <summary>
    /// Unequip command for player.
    /// </summary>
    public class Command_unequip : Command, ICommand
    {




        public Command_unequip()
        {
            name = "command_unequip";
            words = new string[2] { "unequip", "unwield" };
            help.Command = "unequip";
            help.Summary = "Unequips an item from the player.";
            help.Syntax = "unequip <item>";
            help.Examples = new string[1];
            help.Examples[0] = "unequip helmet";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            int numericprefix = 0; // Numeric portion of the text converted to integer
            Actor item = null;

            // First find out if the item name has a numeric prefix, 
            // which means a certain item past the first occurance of it.
            // for example, "equip 2sword" or "equip 3torch"
            // grab the item name text = txt
            string itemtxt = Lib.SplitItemNameFromNumericPrefix(arguments, ref numericprefix);
            if (itemtxt != null)
            {
                if (itemtxt == "all")
                {
                    if (Lib.YesNo(actor, "Are you sure you want to unequip everything you are carrying? (y/n): "))
                    {
                        for (int tmpcounter = actor.GetContents().Count - 1; tmpcounter >= 0; tmpcounter--)
                        {
                            item = actor.GetItemAtIndex(tmpcounter);
                            if (item != null)
                            {
                                if (actor.UnEquip(item))
                                {
                                    actor.Send("You unequipped " + item["nameprefix"] + " " + item["name"] + " from your " + item["equipslot"] + ".\r\n");
                                }
                                else
                                {
                                    actor.SendError("You cannot unequip " + item["nameprefix"] + " " + item["name"] + " from your " + item["equipslot"] + ".\r\n");
                                    if (!Lib.ConvertToBoolean(item["equipable"]))
                                    {
                                        actor.Send("It is not an equippable item.\r\n");
                                    }
                                    else if (!Lib.ConvertToBoolean(item["equipped"]))
                                    {
                                        actor.SendError("It is not equipped.\r\n");
                                    }
                                }
                            }
                        }
                        // Save user changes to db
                        actor.Save();
                    }
                    return true;
                }

                Actor tmpitem = actor.GetItemByName(actor.GetContents(), itemtxt, numericprefix, 1);
                if (tmpitem != null)
                {
                    if (actor.UnEquip(tmpitem))
                    {
                        actor.Send("You unequipped " + tmpitem["nameprefix"] + " " + tmpitem["name"] + " from your " + tmpitem["equipslot"] + ".\r\n");
                        // Tell others in the room that he unequipped it
                        actor.Sayinroom(actor.GetNameUpper() + " unequipped " + tmpitem["nameprefix"] + " " + tmpitem["name"] + ".");
                        actor.Save();
                    }
                    else
                    {
                        actor.SendError("You cannot unequip " + tmpitem["nameprefix"] + " " + tmpitem["name"] + " from your " + tmpitem["equipslot"] + ".\r\n");
                        if (!Lib.ConvertToBoolean(tmpitem["equipable"]))
                        {
                            actor.Send("It is not an equippable item.\r\n");
                        }
                        else if (!Lib.ConvertToBoolean(tmpitem["equipped"]))
                        {
                            actor.SendError("It is not equipped.\r\n");
                        }
                    }
                    return true;
                }
            }
            actor.SendError("You have no '" + itemtxt + "'." + "\r\n");
            return false;
        }

    }


    /// <summary>
    /// Reject command for player.  Used with accept and give.
    /// </summary>
    public class Command_reject : Command, ICommand
    {




        public Command_reject()
        {
            name = "command_reject";
            words = new string[1] { "reject" };
            help.Command = "reject";
            help.Summary = "Rejects an offer from another player to give you an object. It also notifies the other player that you rejected their offer.";
            help.Syntax = "reject";
            help.Examples = new string[1];
            help.Examples[0] = "reject";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Actor chkactor;

            if (actor["tradestate"].ToString() != "pendinggive" && actor["tradestate"].ToString() != "pendingreceive")
            {
                actor.SendError("You have no pending trades to cancel.\r\n");
                return false;
            }
            chkactor = actor.Tradetargetuser;
            chkactor.Clearbufferitems();
            actor["tradestate"] = "";
            chkactor["tradestate"] = "";
            actor.SendError("You cancelled the trade with " + chkactor.GetNameUpper() + "\r\n");
            chkactor.SendError(actor.GetNameUpper() + " cancelled the trade.\r\n");
            return true;
        }

    }

    /// <summary>
    /// Accept command for player.  Used with reject and give.
    /// </summary>
    public class Command_accept : Command, ICommand
    {




        public Command_accept()
        {
            name = "command_accept";
            words = new string[1] { "accept" };
            help.Command = "accept";
            help.Summary = "Accepts an object from another player whom offers the object. It also notifies them that you accepted their offer.";
            help.Syntax = "accept";
            help.Examples = new string[1];
            help.Examples[0] = "accept";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Actor chkactor = null;
            bool chkfound = false; // Has an item been found in a list
            bool chkitemininventory = false;
            Actor item = null;
            Actor item2 = null;

            // Check first to see if there are any pending trades
            // actor.Tradetargetuser is nothing if you are accepting your own trade.
            // actor.Tradetargetuser is mud.User if you are the right person to accept
            if (actor["tradestate"] == null) actor["tradestate"] = "";
            if (actor["tradestate"].ToString() != "pendingreceive")
            {
                actor.SendError("You have no pending trades to accept.\r\n");
                return false;
            }

            // Recall user that you were trading with
            chkactor = actor.Tradetargetuser;

            // Rerun all the trade checks to make sure nothing changed since the trade offer
            // Make sure target user is online
            if (chkactor == null)
            {
                // target user offline, cancel trade
                chkactor = actor.Tradetargetuser;
                chkactor.Clearbufferitems();
                actor["tradestate"] = "";
                actor.Tradetargetuser = null;
                chkactor.Tradetargetuser = null;
                chkactor["tradestate"] = "";
                actor.SendError("Trade cancelled. The trade user is offline.\r\n");
                return false;
            }

            if (chkactor["container"].ToString() != actor["container"].ToString())
            {
                actor.SendError("Trade cancelled. The trade user is not in the room.\r\n");
                // traders no longer in the same room, cancel trade
                chkactor = actor.Tradetargetuser;
                chkactor.Clearbufferitems();
                actor["tradestate"] = "";
                chkactor["tradestate"] = "";
                actor.Tradetargetuser = null;
                chkactor.Tradetargetuser = null;
                chkactor.SendError("Trade cancelled. You must be in the same room with people to trade.\r\n");
                return false;
            }

            chkitemininventory = true;
            chkfound = false;
            // Make sure user still has the items to give you
            // Cycle through all items to be traded and if ANY don't match their inventory, error out
            for (int counter = chkactor.Getbufferitems().Count - 1; counter >= 0; counter--)
            {
                item = chkactor.Getbufferitemat(counter);
                for (int counter2 = chkactor.GetContents().Count - 1; counter2 >= 0; counter2--)
                {
                    item2 = chkactor.GetItemAtIndex(counter2);
                    if (item["id"] == item2["id"])
                    {
                        chkfound = true;
                    }
                }
                // Every item has to be there for chkitemininventory to be true
                if (chkfound == false)
                {
                    chkitemininventory = false;
                }
            }
            if (!chkitemininventory)
            {
                actor.Send("TRADE CANCELLED. " + chkactor.GetNameUpper() + " no longer carries all the items needed for the trade." + "\r\n");
                chkactor.Send("TRADE CANCELLED. You no longer carry all the items needed for the trade.\r\n");
                chkactor["tradestate"] = "";
                actor["tradestate"] = "";
                chkactor.Clearbufferitems();
                actor.Clearbufferitems();
                chkactor.Tradetargetuser = null;
                actor.Tradetargetuser = null;
            }
            // All checks pass, do the trade
            if (chkitemininventory)
            {
                // Now complete the trade
                for (int counter = actor.Getbufferitems().Count - 1; counter >= 0; counter--)
                {
                    item = actor.Getbufferitemat(counter);
                    chkactor.Removeitem(item);
                    actor.Additem(item);
                    // unequip the item
                    // Have to do this in the database and in memory
                    item.Save();
                }
                // tell the giving user that he successfully gave the items
                chkactor.Send(chkactor.GetNameUpper() + " accepted the trade.\r\nYou gave " + actor.GetNameUpper() + ":\r\n");
                for (int counter = actor.Getbufferitems().Count - 1; counter >= 0; counter--)
                {
                    item = actor.Getbufferitemat(counter);
                    chkactor.Send(item.GetNameFullUpper() + "\r\n");
                }
                // tell the receiving user that he successfully received the items
                actor.Send("You accepted the trade from " + chkactor.GetNameUpper() + ". You received:" + "\r\n");
                for (int counter = actor.Getbufferitems().Count - 1; counter >= 0; counter--)
                {
                    item = actor.Getbufferitemat(counter);
                    actor.Send(item.GetNameFullUpper() + "\r\n");
                }
                // Tell others in the room of the receiving person that there was an exchange of items
                ArrayList exclude = new ArrayList();
                exclude.Add(chkactor);
                exclude.Add(actor);
                actor.Sayinroom(chkactor.GetNameUpper() + " gave " + actor.GetNameUpper() + ":", exclude);
                for (int counter = actor.Getbufferitems().Count - 1; counter >= 0; counter--)
                {
                    item = actor.Getbufferitemat(counter);
                    if (item["subtype"].ToString().StartsWith("stack"))
                    {
                        actor.Sayinroom(item["quantity"] + " " + item["name"] + "s", exclude);
                    }
                    else
                    {
                        actor.Sayinroom(item.GetNameFullUpper(), exclude);
                    }

                }
                // reset all the trade flags, buffers, etc.
                chkactor["tradestate"] = "";
                actor["tradestate"] = "";
                chkactor.Clearbufferitems();
                actor.Clearbufferitems();
                chkactor.Tradetargetuser = null;
                actor.Tradetargetuser = null;
            }
            return true;
        }

    }

    /// <summary>
    /// Give command for player.  Used with accept and reject.
    /// </summary>
    public class Command_give : Command, ICommand
    {




        public Command_give()
        {
            name = "command_give";
            words = new string[1] { "give" };
            help.Command = "give";
            help.Summary = "Lets you give an object or currency to another player when you're feeling a bit benevolent.";
            help.Syntax = "give [#|all] [#][item] to <player>";
            help.Examples = new string[6];
            help.Examples[0] = "give sword to bob";
            help.Examples[1] = "give 2sword to bob";
            help.Examples[2] = "give 200 coins to bob";
            help.Examples[3] = "give all to bob";
            help.Examples[4] = "give all swords to bob";
            help.Examples[5] = "give all coins to bob";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Actor item;
            Actor targetactor = null;
            Actor targetmob = null;
            string targetactorname;
            string itemname;
            bool all = false;
            int itemnumericprefix = 0;
            int itemquantity = 0;

            // Populate words array with the command words
            ArrayList words = Lib.GetWords(arguments);
            if (words.Count < 1)
            {
                actor.SendError("You must specify something to give.\r\n");
                return false;
            }
            if (words.Count < 3)
            {
                actor.SendError("You must specify someone to trade with.\r\n");
                return false;
            }
            if (actor["tradestate"].ToString() != "")
            {
                actor.SendError("You already have a pending trade with someone.\r\n");
                return false;
            }
            // Get the target user name from the word array if there is one.
            targetactorname = Lib.GetContainerName(words);
            // Determine the target item to get and get item count if there is one.
            itemname = Lib.GetItemNamePrefixAndQuantity(words, ref itemnumericprefix, ref itemquantity, ref all);
            item = actor.GetItemByName(itemname, itemnumericprefix, itemquantity);
            if (item == null && all == false)
            {
                actor.SendError("You don't have that item in your top level inventory.\r\n");
                return false;
            }

            // first of all, you cannot do this to other players no matter what
            if (item["type"].ToString() == "user" || item["type"].ToString() == "room" || item["type"].ToString() == "mob")
            {
                actor.SendError("You cannot do that.\r\n");
                return false;
            }

            // Look for a user with that name
            if (targetactorname != null)
            {
                targetactor = Lib.GetByName(targetactorname);
            }

            if (targetactor == actor)
            {
                actor.SendError("You cannot give to yourself.\r\n");
                return false;
            }


            // Didn't find a user or mob with that name
            if (targetactor == null && targetmob == null)
            {
                actor.SendError("There is no one named '" + targetactorname + "'.\r\n");
                return false;
            }

            // Check that user is in the same room
            if (targetactor != null)
            {
                if (targetactor["container"].ToString() != actor["container"].ToString())
                {
                    actor.SendError("The user is not in the room.\r\n");
                    return false;
                }
            }
            // Check that mob is in room
            if (targetmob != null)
            {
                if (targetmob["container"].ToString() != actor["container"].ToString())
                {
                    actor.SendError("The mob is not in the room.\r\n");
                    return false;
                }
            }

            if (targetactor != null)
            {
                // Did user say give all to bob?
                if (item == null && all == true)
                {
                    actor.GiveAll(targetactor);
                }
                // or did he type give all swords to bob?
                else if (item != null && all == true)
                {
                    actor.GiveAllOfType(targetactor, item);
                }
                // or did he just type give sword to bob
                else
                {
                    actor.Give(targetactor, item);
                }
                return true;
            }

            if (targetmob != null)
            {
                // Did user say give all to bob?
                if (item == null && all == true)
                {
                    if (Lib.YesNo(actor, "Are you sure you want to give items to a mob? "))
                    {
                        actor.GiveAll(targetmob);
                    }
                    else
                        return false;
                }
                // or did he type give all swords to bob?
                else if (item != null && all == true)
                {
                    if (Lib.YesNo(actor, "Are you sure you want to give items to a mob? "))
                    {
                        actor.GiveAllOfType(targetmob, item);
                    }
                    else
                        return false;
                }
                // or did he just type give sword to bob
                else
                {
                    if (Lib.YesNo(actor, "Are you sure you want to give items to a mob? "))
                    {
                        actor.Give(targetmob, item);
                    }
                    else
                        return false;
                }
                return true;
            }

            return true;
        }

    }

    /// <summary>
    /// Split command for player.
    /// </summary>
    public class Command_split : Command, ICommand
    {




        public Command_split()
        {
            name = "command_split";
            words = new string[1] { "split" };
            help.Command = "split";
            help.Summary = "Creates a new stack of items from an existing stack.";
            help.Syntax = "split <number of items> <item name>";
            help.Examples = new string[1];
            help.Examples[0] = "split 100 coin";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            ArrayList words = Lib.GetWords(arguments);
            // Command sanity check, should be a set number of parameters when using this command
            if (words.Count != 2)
            {
                actor.SendError("Wrong number of parameters for this command. Type 'help' for more information.\r\n");
                return false;
            }

            // Second word is supposed to be the number of items to split off
            // Make sure user has the item to split
            Actor tmpitem = actor.GetItemByName((string)words[1]);
            if (tmpitem == null)
            {
                actor.SendError("You don't have that item.\r\n");
                return false;
            }

            // You can only split stackable items
            if (!tmpitem["subtype"].ToString().StartsWith("stack"))
            {
                actor.SendError("That is not a stackable item, thus it cannot be split.\r\n");
                return false;
            }

            //Make sure the player has enough to make a new stack of that size
            if (Convert.ToInt32(tmpitem["quantity"]) <= Convert.ToInt32((string)words[0]))
            {
                actor.SendError("You don't have enough to make a new stack of that size.\r\n");
                return false;
            }

            // Create the new stack
            Actor newstack = new Actor();
            newstack = tmpitem.Copy();
            actor.Send("You create a new stack of " + Convert.ToInt32((string)words[0]) + " from your original stack of " + tmpitem["quantity"] + ".\r\n");
            // Set new stack's quantity 
            newstack["quantity"] = Convert.ToInt32((string)words[0]);
            // subtract split quantity from old stack
            tmpitem["quantity"] = Convert.ToInt32(tmpitem["quantity"]) - Convert.ToInt32((string)words[0]);
            // Add new stack to the world memory
            //Lib.AddActorToWorld(newstack);
            newstack.Save();

            // Save changes to old stack
            tmpitem.Save();
            // Add new stack to user inventory
            actor.Additem(newstack);
            // Save new stack
            newstack.Save();
            // Save original stack
            tmpitem.Save();
            return true;
        }

    }

    /// <summary>
    /// Cheat command for player.
    /// </summary>
    public class Command_cheat : Command, ICommand
    {




        public Command_cheat()
        {
            name = "command_cheat";
            words = new string[1] { "cheat" };
            help.Command = "cheat";
            help.Summary = "Add 10 to selected stat.";
            help.Syntax = "cheat <command name>";
            help.Examples = new string[6];
            help.Examples[0] = "cheat ah <adds 100 health>";
            help.Examples[1] = "cheat am <adds 100 mana>";
            help.Examples[2] = "cheat as <adds 100 stamina>";
            help.Examples[3] = "cheat rh <removes 100 health>";
            help.Examples[4] = "cheat rm <removes 100 mana>";
            help.Examples[5] = "cheat rs <removes 100 stamina>";


        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            System.Collections.ArrayList words = Lib.GetWords(arguments);
            if (words.Count < 1)
            {
                actor.SendError("You have to specify an attribute to raise.\r\n");
                return false;
            }

            switch ((string)words[0])
            {
                case "ah":
                    actor["health"] = Convert.ToInt32(actor["health"]) + 100;
                    break;
                case "am":
                    actor["mana"] = Convert.ToInt32(actor["mana"]) + 100;
                    break;
                case "as":
                    actor["stamina"] = Convert.ToInt32(actor["stamina"]) + 100;
                    break;
                case "rh":
                    actor["health"] = Convert.ToInt32(actor["health"]) - 100;
                    break;
                case "rm":
                    actor["mana"] = Convert.ToInt32(actor["mana"]) - 100;
                    break;
                case "rs":
                    actor["stamina"] = Convert.ToInt32(actor["stamina"]) - 100;
                    break;
            }
            return true;
        }

    }


    /// <summary>
    /// Push command for player.
    /// </summary>
    public class Command_push : Command, ICommand
    {




        public Command_push()
        {
            name = "command_push";
            words = new string[1] { "push" };
            help.Command = "push";
            help.Summary = "Pushes an object.";
            help.Syntax = "push <object>";
            help.Examples = new string[1];
            help.Examples[0] = "push windchime";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Actor item = Lib.GetActorFromArguments(actor, arguments);
            if (item == null)
            {
                return false;
            }

            item.ProcessAction(actor, command, arguments);

            return true;
        }

    }

    /// <summary>
    /// Pull command for player.
    /// </summary>
    public class Command_pull : Command, ICommand
    {




        public Command_pull()
        {
            name = "command_pull";
            words = new string[1] { "pull" };
            help.Command = "pull";
            help.Summary = "Pulls an object.";
            help.Syntax = "pull <object>";
            help.Examples = new string[1];
            help.Examples[0] = "pull dummy";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Actor room = Lib.GetByID(actor["container"].ToString());

            Actor item = null;
            int numericprefix = 0;
            string txt = Lib.SplitItemNameFromNumericPrefix(arguments, ref numericprefix);

            for (int itemcounter = room.GetContents().Count - 1; itemcounter >= 0; itemcounter--)
            {
                Actor tmpitem = room.GetItemAtIndex(itemcounter);
                // Try to match what user wants and accomodate plurals
                if (tmpitem["shortname"].ToString() == txt || tmpitem["name"].ToString() == txt || tmpitem["shortname"] + "s" == txt || tmpitem["name"] + "s" == txt)
                {
                    item = tmpitem;
                }
            }

            if (item == null)
            {
                actor.SendError("Could not find that item.\r\n");
                return false;
            }

            item.ProcessAction(actor, command, arguments);

            return true;
        }

    }

    /// <summary>
    /// Report the phase of the moon.
    /// </summary>
    public class Command_moon : Command, ICommand
    {




        public Command_moon()
        {
            name = "command_moon";
            words = new string[1] { "moon" };
            help.Command = "moon";
            help.Summary = "Reports the current phase of the moon.";
            help.Syntax = "moon";
            help.Examples = new string[1];
            help.Examples[0] = "moon";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Actor room = Lib.GetByID(actor["container"].ToString());


            if (room["subtype"].ToString() == "outdoor" || Convert.ToString(room["subtype"]) == "outdoor")
            {
                actor.Send(Lib.MoonView());
            }
            else
            {
                actor.Send("You cannot see the sky.\r\n");
            }
            return true;
        }

    }

    /// <summary>
    /// Report the state of the sun.
    /// </summary>
    public class Command_sun : Command, ICommand
    {




        public Command_sun()
        {
            name = "command_sun";
            words = new string[1] { "sun" };
            help.Command = "sun";
            help.Summary = "Reports the state of the sun.";
            help.Syntax = "sun";
            help.Examples = new string[1];
            help.Examples[0] = "sun";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Actor room = Lib.GetByID(actor["container"].ToString());


            if (room["subtype"].ToString() == "outdoor" || Convert.ToString(room["subtype"]) == "outdoor")
            {
                actor.Send(Lib.SunView());
            }
            else
            {
                actor.Send("You cannot see the sky.\r\n");
            }
            return true;
        }

    }


    /// <summary>
    /// Test command for the menu class
    /// </summary>
    public class Command_menu : Command, ICommand
    {




        public Command_menu()
        {
            name = "command_menu";
            words = new string[1] { "menu" };
            help.Command = "menu";
            help.Summary = "Tests the new menu function.";
            help.Syntax = "menu";
            help.Examples = new string[1];
            help.Examples[0] = "menu stuff";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Actor user = actor;
            Menu o_Menu = new Menu();

            o_Menu.User = user;
            o_Menu.Header = "This is a header";
            o_Menu.MenuItems = new ArrayList();
            o_Menu.MenuItems.Add("First Item");
            o_Menu.MenuItems.Add("Second Item");
            o_Menu.MenuItems.Add("Third Item");
            o_Menu.MenuItems.Add("Fourth Item");
            o_Menu.Footer = "This is a footer";
            o_Menu.Prompt = "Please make a selection: ";
            //o_Menu.MenuType		= Menu.MenuTypes.MultiChoice;
            //o_Menu.MenuType		= Menu.MenuTypes.SingleChoice;
            //o_Menu.MenuType		= Menu.MenuTypes.SingleChoiceBack;
            o_Menu.MenuType = Menu.MenuTypes.MultiChoiceBack;
            int result = 0;

            ArrayList ia_Tmp = o_Menu.ShowMenu(ref result);
            // Always catch nulls coming back from menus.
            // That usually means the client disconnected at the menu.
            if (ia_Tmp == null)
            {
                return false;
            }

            if (result == 1)
            {
                user.Send("You chose Exit.\r\n");
            }
            if (result == 1)
            {
                user.Send("You chose Back.\r\n");
            }
            foreach (int i_Tmp in ia_Tmp)
            {
                user.Send("Choice: " + i_Tmp.ToString() + "\r\n");
            }
            return true;
        }

    }
    /// <summary>
    /// Test command for the Mail Class
    /// </summary>
    public class Command_mail : Command, ICommand
    {




        public Command_mail()
        {
            name = "command_mail";
            words = new string[1] { "mail" };
            help.Command = "mail";
            help.Summary = "Tests the new mail function.";
            help.Syntax = "mail";
            help.Examples = new string[1];
            help.Examples[0] = "displays your mail";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            UserMail o_UserMail = new UserMail(actor);
            o_UserMail.Mail();
            return true;
        }

    }

    public class Command_exits : Command, ICommand
    {

        public Command_exits()
        {
            name = "command_exits";
            words = new string[2] { "exits", "exit" };
            help.Command = "exits";
            help.Summary = "Displays the available exits in your current room.";
            help.Syntax = "exits or exit";
            help.Examples = new string[1];
            help.Examples[0] = "exits";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Actor user = actor;
            Actor currentroom = Lib.GetByID(user["container"].ToString());
            SortedList currentexits = currentroom.Exits;
            int numexits = currentexits.Count;
            if (numexits == 0)
            {
                user.Send("There are no exits.\r\n");
            }
            else if (numexits == 1)
            {
                user.Send("There is an exit " + currentexits.GetKey(0).ToString() + ".\r\n");
            }
            else if (numexits == 2)
            {
                user.Send("There are exits " + currentexits.GetKey(0).ToString() + " and " + currentexits.GetKey(1).ToString() + ".\r\n");
            }
            else
            {
                string exitsmessage = "";

                for (int i = 0; i < currentexits.Count; i++)
                {
                    if (i == 0)
                    {
                        exitsmessage += "There are exits " + currentexits.GetKey(i);
                    }
                    else if (i != currentexits.Count - 1)
                    {
                        exitsmessage += ", " + currentexits.GetKey(i);
                    }
                    else
                    {
                        exitsmessage += " and " + currentexits.GetKey(i);
                    }

                }
                exitsmessage += ".\r\n";
                user.Send(exitsmessage);
            }
            return true;
        }

    }

    /// <summary>
    /// This command allows a user to add a bug to the internal MUD
    /// bug list.
    /// </summary>
    public class Command_bug : Command, ICommand
    {




        private const string SYNTAX_ERROR = "Unknown option. Try 'help <command name>' for usage information.";

        public Command_bug()
        {
            name = "command_bug";
            words = new string[2] { "bug", "feature" };
            help.Command = "bug";
            help.Summary = "Add a bug or feature to the internal MUD bug database. Help the TigerMUD developers make the engine even better.";
            help.Syntax = "bug {a description of the bug}";
            help.Examples = new string[2];
            help.Examples[0] = "bug the look command seems to be showing all objects twice.";
            help.Examples[1] = "feature what about having flying mobs?";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            ArrayList words = Lib.GetWords(arguments);
            if (words.Count < 3)
            {
                actor.SendError("Please provide more than three words about your bug or suggestion.\r\nIf possible, please tell us how to reproduce the bug to help us fix it faster.\r\n" + "Use the command 'help <command name>' for more information.\r\n");
                return false;
            }
            else
            {
                // Clean the arguments of naughty characters that the db would choke on
                actor.Sanitize(ref arguments);
                actor.AddBug(actor, arguments);
                actor.SendSystemMessage("Thank you, your bug/feature has been submitted.\r\n");
                return true;
            }
        }

    }

    /// <summary>
    /// This class allows the player to maintain a friends list.
    /// He can add friends, accept add requests, remove friends
    /// and list all friends and see their online status.
    /// </summary>
    public class Command_friend : Command, ICommand
    {




        public const string SYNTAX_ERROR = "Unknown option. Try 'help friend' for usage information.\r\n";

        public Command_friend()
        {
            name = "command_friend";
            words = new string[1] { "friend" };
            help.Command = "friend";
            help.Summary = "Add a friend to your friends list. See who is logged in. "
                + "Get notified when a friend logs in. When you add a friend, your friend "
                + "must accept your request before you will have them on your friends list.";
            help.Syntax = "friend [add|accept|list|remove] {name}";
            help.Examples = new string[4];
            help.Examples[0] = "friend add friend_name";
            help.Examples[1] = "friend accept friend_name";
            help.Examples[2] = "friend list";
            help.Examples[3] = "friend remove friend_name";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Actor user = actor;
            bool isCommandSuccess = true;

            // Get the list of arguments for the command
            string[] commandArgs = GetCommandArgs(arguments.Split(' '));

            // What command action is he executing?
            switch (arguments.Split(' ')[0])
            {
                case "add":
                    AddFriend(user, commandArgs);
                    break;
                case "remove":
                    RemoveFriend(user, commandArgs);
                    break;
                case "accept":
                    AcceptFriend(user, commandArgs);
                    break;
                case "list":
                    ListFriends(user);
                    break;
                default:
                    user.SendError(SYNTAX_ERROR);
                    isCommandSuccess = false;
                    break;
            }

            return isCommandSuccess;
        }

        private void AddFriend(Actor actor, string[] arguments)
        {
            // Is a friend specified?
            if (arguments.Length == 0)
            {
                actor.SendError("You must specify a friend's name to add.\r\n");
            }
            else if (actor["shortname"].ToString() == arguments[0])
            {
                actor.SendError("You cannot add yourself.\r\n");
            }
            else
            {
                // get each friend to add
                foreach (string friendName in arguments)
                {
                    // does the friend exist?
                    if (actor.Exists(friendName))
                    {
                        // Is the friend already part of the list?
                        if (actor.IsFriendInList(actor["shortname"].ToString(), friendName))
                        {
                            actor.SendError(friendName + " is already in your friends list.\r\n");
                        }
                        else if (actor.IsFriendPendingAuthorisation(actor["shortname"].ToString(), friendName))
                        {
                            actor.SendError(friendName + " has not yet authorized your request.\r\n");
                        }
                        else
                        {
                            // Does the friend have this actor in their list already?
                            if (actor.IsFriendInList(friendName, actor["shortname"].ToString()))
                            {
                                actor.Send(friendName + " has been added to your friends list.\r\n");
                                actor.AddFriendWithAuthorisation(actor["shortname"].ToString(),
                                    friendName);
                            }
                            else
                            {
                                // Add the friend
                                actor.AddFriend(actor["shortname"].ToString(), friendName);
                                actor.Send(friendName + " will now be asked to authorize you as a friend. Once your friend accepts, s/he will be on your friends list.\r\n");

                                // Notify the friend to be added
                                NotifyFriendForAuthorisation(actor, friendName);
                            }
                        }
                    }
                    else
                    {
                        actor.SendError(friendName + " is not a resident of this world.\r\n");
                    }
                }
            }
        }

        private void NotifyFriendForAuthorisation(Actor actor, string friendName)
        {
            // Is the actor online?
            Actor friend = Lib.Checkonline(friendName);
            if (friend != null)
            {
                // Inform the friend that they have pending authorisations.
                friend.SendAlertGood(actor["shortname"] + " is asking to be on your friends list. Type 'friend list' for more information.\r\n");
            }
        }

        private void RemoveFriend(Actor actor, string[] friends)
        {
            if (friends.Length == 0)
            {
                actor.SendError("You must specify a friend name.\r\n");
            }
            else
            {
                // Go through the list of friends
                foreach (string friendName in friends)
                {
                    // Does the friend exist
                    if (actor.Exists(friendName))
                    {
                        if (actor.IsActorWaitingForAuthorisation(actor["shortname"].ToString(),
                            friendName))
                        {
                            // Remove any pending authorisations for this friend
                            actor.RejectFriendRequest(actor["shortname"].ToString(), friendName);
                            actor.SendAnnouncement(friendName + "'s request has been cancelled.\r\n");
                        }
                        else
                        {
                            // Remove the friend from this actor's list
                            actor.RemoveFriend(actor["shortname"].ToString(), friendName);
                            actor.SendAnnouncement(friendName + " has been removed from your list.\r\n");
                        }
                    }
                    else
                    {
                        actor.SendError(friendName + " is not a resident of this world.");
                    }
                }
            }
        }

        private void AcceptFriend(Actor actor, string[] friends)
        {
            if (friends.Length == 0)
            {
                actor.SendError("You must specift a friend name.\r\n");
            }
            else
            {
                foreach (string friend in friends)
                {
                    // Is the requestor waiting for authorisation?
                    if (actor.IsActorWaitingForAuthorisation(actor["shortname"].ToString(), friend))
                    {
                        actor.AuthoriseFriend(actor["shortname"].ToString(),
                            friend);

                        // Now add the requestor to this person's friends list
                        actor.AddFriendWithAuthorisation(actor["shortname"].ToString(),
                            friend);

                        actor.SendAlertGood(friend + " has been authorized and will be added to your friends list.\r\n");
                    }
                    else
                    {
                        actor.SendError(friend + " is not waiting for authorization (could be in your friends list already).\r\n");
                    }
                }
            }
        }

        private void ListFriends(Actor actor)
        {
            DataTable friends = actor.GetFriendsList(actor["shortname"].ToString());

            // Show the list of friends
            actor.SendAnnouncement(friends.Rows.Count + " friend(s): \r\n");
            foreach (DataRow row in friends.Rows)
            {
                string outputLine;
                outputLine = row["FriendName"].ToString();

                // Check Auth status
                if ((bool)row["IsAuthorised"])
                {
                    // Check online status
                    if (Lib.Checkonline(row["FriendName"].ToString()) != null)
                    {
                        outputLine += " - Online";
                    }
                    else
                    {
                        outputLine += " - Offline";
                    }
                }
                else
                {
                    // We don't show online status to unauthorised actors.
                    outputLine += " - Pending authorization";
                }

                // Send the row to the actor.
                actor.SendAnnouncement(outputLine + "\r\n");
            }

            // Show the list of pending requests
            DataTable pendingRequests = actor.GetPendingAuthorisationRequests(actor["shortname"].ToString());

            actor.SendAnnouncement("\r\n" + pendingRequests.Rows.Count + " pending request(s):\r\n");
            foreach (DataRow row in pendingRequests.Rows)
            {
                actor.SendAnnouncement(row["ShortName"].ToString() + "\r\n");
            }
        }

        /// <summary>
        /// Removes the first command word and returns the remaining arguments.
        /// </summary>
        private string[] GetCommandArgs(string[] inputArguments)
        {
            string[] outputArguments = new string[0];

            // Avoid input arrays that consist of one argument that is 
            // an empty string
            if (!(inputArguments.Length == 1 && inputArguments[0] == String.Empty))
            {
                if (inputArguments.Length >= 2)
                {
                    outputArguments = new string[inputArguments.Length - 1];

                    // Copy the elements across.
                    for (int counter = 1; counter < inputArguments.Length; counter++)
                    {
                        outputArguments[counter - 1] = inputArguments[counter];
                    }
                }
            }

            return outputArguments;
        }

    }

    /// <summary>
    /// Builder command create.
    /// </summary>
    public class Command_create : Command, ICommand
    {




        public Command_create()
        {
            name = "command_create";
            words = new string[2] { "create", "cre" };
            help.Command = "create or cre";
            help.Summary = "Creates a new object from nothing.";
            help.Syntax = "create <object type> <shortname>";
            help.Examples = new string[3];
            help.Examples[0] = "create item longsword";
            help.Examples[1] = "create room Mariner's Club";
            help.Examples[2] = "create mob Jason the Merchant";
            help.Description = "This creates the object with a unique ID. To modify the new object, target it with the target command and then use the modify command. Note than an object's type determines its behavior like moving around and whether it can be picked up or not, etc. Do not use object types and names that are the same as command words.";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            ArrayList words = Lib.GetWords(arguments);

            if (words.Count < 1)
            {
                actor.SendError("You must specify at minimum a type and shortname for the new object.\r\n");
                return false;
            }
            if (words.Count < 2)
            {
                actor.SendError("You must specify a name for the new object.\r\n");
                return false;
            }
            if (words[0].ToString().Length < 2)
            {
                actor.SendError("Object types must be at least 2 characters. Specify the object type to create, for example an item, room, mob, etc.\r\n");
                return false;
            }

            // Get object name if it doesn't contain spaces
            string objectname = "";
            if (words.Count == 2)
            {
                objectname = words[1].ToString();
            }
            if (words[1].ToString().Length < 2)
            {
                actor.SendError("Object names must be at least 2 characters. Specify the object name to create, for example a table, pirate, fountain, etc.\r\n");
                return false;
            }

            // Get object name if it contains spaces
            if (words.Count > 2)
            {
                for (int i = 1; i < words.Count; i++)
                {
                    objectname += words[i] + " ";
                }
                objectname = objectname.Trim();

            }

            // Create the new object
            try
            {
                // Create item in the user's room
                Actor item = new Actor();
                item["type"] = words[0].ToString().ToLower();
                item["name"] = objectname;
                item["shortname"] = objectname;
                item["description"] = objectname;
                item["container"] = actor["container"].ToString();
                item["containertype"] = actor["containertype"].ToString();
                // Block regen and respawn code from triggering constantly on this object.
                // This happens when health is at zero, which is a special event for killable types.
                // We cannot know what is a killable type at design time, so give all new objects 1 healmax and 1 health to be safe.
                item["health"] = 1;
                item["healthmax"] = 1;
                item.Save();
                lock (Lib.actors.SyncRoot)
                {
                    Lib.actors.Add(item);
                }

                Actor room = actor.GetContainer();
                room.Additem(item);
                actor.Send("New item '" + item["name"] + "', id: " + item["id"] + " successfully created in the room.\r\n");
                actor["target"] = item["id"];
                actor["targettype"] = item["type"].ToString();
            }
            catch (Exception ex)
            {
                actor.SendError("Object creation failed with the error: " + ex.Message + ex.StackTrace + "\r\n");
                return false;
            }
            actor.Send("Your target is set to this new object.\r\n");
            return true;
        }

    }

    /// <summary>
    /// Builder command. Loads a XML with level definitions.
    /// </summary>
    public class Command_loadxmlroom : Command, ICommand
    {




        public Command_loadxmlroom()
        {
            name = "command_loadxmlroom";
            words = new string[2] { "loadxmlroom", "lxr" };
            help.Command = "loadXMLroom";
            help.Summary = "Loads an XML file with room definitions.";
            help.Syntax = "loadxmlroom <filename>";
            help.Examples = new string[3];
            help.Examples[0] = "loadxmlroom roomdef.xml";
            help.Examples[0] = "lxr roomdef.xml";
            help.Description = "This creates a set of rooms as defined in an XML file ";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            ArrayList words = Lib.GetWords(arguments);

            if (words.Count < 1)
            {
                actor.SendError("You must specify filename.\r\n");
                return false;

            }

            string filename = Path.GetFullPath(Path.Combine(Lib.PathtoRoot, @"XMLLevelEditors\\")) + (string)words[0];

            if (!File.Exists(filename))
            {
                actor.SendError("File does not exist.\r\n");
                return false;
            }

            // TODO: Add support for filenames longer than one word.

            // Load the file
            try
            {
                // does this make sense? No it doesn't. Fix. 
                LevelReader reader = new LevelReader(filename);
                reader.Load(filename);
            }
            catch (Exception ex)
            {
                actor.SendError("File load failed with the error: " + ex.Message + ex.StackTrace + "\r\n");
                return false;
            }
            actor.Send("The file was loaded successfully.\r\n");
            return true;
        }

    }

    /// <summary>
    /// Builder command. Loads a XML with spell definitions.
    /// </summary>
    public class Command_loadxmlspell : Command, ICommand
    {




        public Command_loadxmlspell()
        {
            name = "command_loadxmlspell";
            words = new string[2] { "loadxmlspell", "lxs" };
            help.Command = "loadXMLspell";
            help.Summary = "Loads an XML file with spell definitions.";
            help.Syntax = "loadxmlspell <filename>";
            help.Examples = new string[3];
            help.Examples[0] = "loadxmlspell spelldef.xml";
            help.Examples[0] = "lxs spelldef.xml";
            help.Description = "This creates a set of spells as defined in an XML file ";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            ArrayList words = Lib.GetWords(arguments);

            if (words.Count < 1)
            {
                actor.SendError("You must specify filename.\r\n");
                return false;

            }

            string filename = Path.GetFullPath(Path.Combine(Lib.PathtoRoot, @"XMLLevelEditors\\")) + (string)words[0];

            if (!File.Exists(filename))
            {
                actor.SendError("File does not exist.\r\n");
                return false;
            }

            // TODO: Add support for filenames longer than one word.

            // Load the file
            try
            {
                // does this make sense? No it doesn't. Fix. 
                SpellReader reader = new SpellReader(filename);
                reader.Load(filename);
            }
            catch (Exception ex)
            {
                actor.SendError("File load failed with the error: " + ex.Message + ex.StackTrace + "\r\n");
                return false;
            }
            actor.Send("The file was loaded successfully.\r\n");
            return true;
        }

    }

    /// <summary>
    /// Builder command. Loads a XML with item definitions.
    /// </summary>
    public class Command_loadxmlitem : Command, ICommand
    {




        public Command_loadxmlitem()
        {
            name = "command_loadxmlitem";
            words = new string[2] { "loadxmlitem", "lxi" };
            help.Command = "loadXMLitem";
            help.Summary = "Loads an XML file with item definitions.";
            help.Syntax = "loadxmlitem <filename>";
            help.Examples = new string[3];
            help.Examples[0] = "loadxmlitem itemdef.xml";
            help.Examples[0] = "lxi itemdef.xml";
            help.Description = "This creates a set of items as defined in an XML file ";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            ArrayList words = Lib.GetWords(arguments);

            if (words.Count < 1)
            {
                actor.SendError("You must specify filename.\r\n");
                return false;

            }

            string filename = Path.GetFullPath(Path.Combine(Lib.PathtoRoot, @"XMLLevelEditors\\")) + (string)words[0];

            if (!File.Exists(filename))
            {
                actor.SendError("File does not exist.\r\n");
                return false;
            }

            // TODO: Add support for filenames longer than one word.

            // Load the file
            try
            {
                // does this make sense? No it doesn't. Fix. 
                ItemReader reader = new ItemReader(filename);
                reader.Load(filename);
            }
            catch (Exception ex)
            {
                actor.SendError("File load failed with the error: " + ex.Message + ex.StackTrace + "\r\n");
                return false;
            }
            actor.Send("The file was loaded successfully.\r\n");
            return true;
        }

    }


    /// <summary>
    /// Builder command. Loads an XML with mob definitions.
    /// </summary>
    public class Command_loadxmlmob : Command, ICommand
    {

        public Command_loadxmlmob()
        {
            name = "command_loadxmlmob";
            words = new string[2] { "loadxmlmob", "lxm" };
            help.Command = "loadXMLmob";
            help.Summary = "Loads an XML file with mob definitions.";
            help.Syntax = "loadxmlmob <filename>";
            help.Examples = new string[3];
            help.Examples[0] = "loadxmlmob mobdef.xml";
            help.Examples[0] = "lxm mobdef.xml";
            help.Description = "This creates a set of computer controlled actors (mobs) as defined in an XML file ";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            ArrayList words = Lib.GetWords(arguments);

            if (words.Count < 1)
            {
                actor.SendError("You must specify filename.\r\n");
                return false;

            }

            string filename = Path.GetFullPath(Path.Combine(Lib.PathtoRoot, @"XMLLevelEditors\\")) + (string)words[0];

            if (!File.Exists(filename))
            {
                actor.SendError("File does not exist.\r\n");
                return false;
            }

            // TODO: Add support for filenames longer than one word.

            // Load the file
            try
            {
                // does this make sense? No it doesn't. Fix. 
                MobReader reader = new MobReader(filename);
                reader.Load(filename);
            }
            catch (Exception ex)
            {
                actor.SendError("File load failed with the error: " + ex.Message + ex.StackTrace + "\r\n");
                return false;
            }
            actor.Send("The file was loaded successfully.\r\n");
            return true;
        }

    }



    /// <summary>
    /// Lists a the stats of the user.
    /// </summary>
    public class Command_stats : Command, ICommand
    {




        // TODO: Add support for viewing other users statistics
        public Command_stats()
        {
            name = "command_stats";
            words = new string[3] { "stats", "st", "me" };
            help.Command = "stats or st or me";
            help.Summary = "Lists your statistics.";
            help.Syntax = "stats";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            actor.Send(Lib.Ansifwhite + "-=Character Status=-\r\n\n");
            actor.Send(Lib.Ansifwhite + "Name: " + actor["name"].ToString() + "\r\n");
            actor.Send(Lib.Ansifwhite + "Health: " + actor["health"].ToString() + "/" + actor["healthmax"].ToString() + "\r\n");
            actor.Send(Lib.Ansifwhite + "Mana: " + actor["mana"].ToString() + "/" + actor["manamax"].ToString() + "\r\n");
            actor.Send(Lib.Ansifwhite + "Stamina: " + actor["staminamax"].ToString() + "\r\n\n");

            actor.Send(Lib.Ansifwhite + "Strength: " + actor["strength"].ToString() + "\r\n");
            actor.Send(Lib.Ansifwhite + "Intellect: " + actor["intellect"].ToString() + "\r\n");
            actor.Send(Lib.Ansifwhite + "Agility: " + actor["agility"].ToString() + "\r\n");
            actor.Send(Lib.Ansifwhite + "Spirit: " + actor["spirit"].ToString() + "\r\n\n");


            Actor weapon1 = Lib.GetByID(actor["wearweapon1"]);

            //write weapon details
            if (weapon1 != null)
            {
                actor.Send(Lib.Ansifwhite + "Weapon: " + weapon1["name"].ToString() + "\r\n");
                actor.Send(Lib.Ansifwhite + "Damage: " + weapon1["damagemin"].ToString() + "-" + weapon1["damagemax"].ToString() + "\r\n\n");
            }

            return true;

        }

    }


    /// <summary>
    /// Lists a item by id number or all items.
    /// </summary>
    public class Command_list : Command, ICommand
    {




        public Command_list()
        {
            name = "command_list";
            words = new string[2] { "list", "li" };
            help.Command = "listi or li";
            help.Summary = "Lists the objects in the world of a certain type.";
            help.Syntax = "list <object type>";
            help.Examples = new string[3];
            help.Examples[0] = "list rooms";
            help.Examples[1] = "list mobs";
            help.Examples[2] = "list all";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            if (arguments.Length < 1)
            {
                actor.SendError("You must specify an object type to list.\r\n");
                return false;
            }
            // User typed the command with no arguments
            Actor item = null;
            Actor container = null;
            string type = arguments.ToLower();
            if (type != null)
            {
                type = type.Trim();
            }
            actor.Send(actor.PaddedLineItem(actor["colorexits"] + "Item Name", "Item ID", "(Item Container)", '-', Convert.ToInt32(actor["screenwidth"])) + "\r\n");
            for (int itemcount = Lib.GetWorldItems().Count - 1; itemcount >= 0; itemcount--)
            {
                if (Lib.GetWorldItems()[itemcount] != null)
                {
                    item = (Actor)Lib.GetWorldItems()[itemcount];
                    if (item["type"].ToString() == type || item["type"].ToString() + "s" == type || item["subtype"].ToString() == type || item["subtype"] + "s" == type || type == "all")
                    {
                        container = item.GetContainer();
                        if (container != null)
                        {
                            actor.Send(actor.PaddedLineItem(item["name"].ToString(), item["id"].ToString(), "(" + container["name"].ToString() + ")", ' ', Convert.ToInt32(actor["screenwidth"])) + "\r\n");
                        }
                        else
                        {
                            actor.Send(actor.PaddedLineItem(item["name"].ToString(), item["id"].ToString(), "(none)", ' ', Convert.ToInt32(actor["screenwidth"])) + "\r\n");
                        }
                    }
                }
            }
            return true;

        }

    }

    /// <summary>
    /// Lists a item by id number or all items.
    /// </summary>
    public class Command_wizlist : Command, ICommand
    {




        public Command_wizlist()
        {
            name = "command_wizlist";
            words = new string[3] { "wizlist", "admins", "adminlist" };
            help.Command = "wizlist or admins or adminlist";
            help.Summary = "Lists any administrators who are currently online.";
            help.Syntax = "wizlist";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            // User typed the command with no arguments
            int counter = 0;
            Actor target = null;
            actor.Send(Lib.Ansifgreen + "\nOnline admins:\r\n\n");
            for (int itemcount = Lib.GetWorldItems().Count - 1; itemcount >= 0; itemcount--)
            {
                if (Lib.GetWorldItems()[itemcount] != null)
                {
                    target = (Actor)Lib.GetWorldItems()[itemcount];
                    if (target["accesslevel"] != null)
                    {
                        if (Convert.ToInt32(target["accesslevel"]) >= (int)AccessLevel.Admin)
                        {
                            if (target["connected"] != null)
                            {
                                if (Lib.ConvertToBoolean(target["connected"]))
                                {
                                    counter++;
                                    actor.Send(Lib.Ansifboldgreen + target.GetNameUpper() + "\r\n");
                                }
                            }
                        }
                    }
                }
            }
            actor.Send(Lib.Ansifgreen + "\r\n" + counter + " admin(s) online.\r\n");
            return true;

        }

    }

    /// <summary>
    /// Destroys the target object.
    /// </summary>
    public class Command_destroy : Command, ICommand
    {




        public Command_destroy()
        {
            name = "command_destroy";
            words = new string[1] { "destroy" };
            help.Command = "destroy";
            help.Summary = "Destroys targeted object.";
            help.Syntax = "destroy";
            help.Examples = new string[1];
            help.Examples[0] = "destroy";
            help.Description = "Target the item to destroy, then 'destroy'.";
        }
        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Actor tmpitem = new Actor();
            if (actor["target"].ToString() == "")
            {
                actor.SendError("You don't have a target to destroy.  Use the 'target' command to target something.\r\n");
                return false;
            }
            if (actor["targettype"].ToString() == "user")
            {
                if (Convert.ToInt32(actor["accesslevel"]) < (int)AccessLevel.UberAdmin)
                {
                    actor.SendError("You may not destroy users.\r\n");
                    return false;
                }
                else
                {

                    try
                    {
                        tmpitem = Lib.GetByID(actor["target"].ToString());
                        if (Convert.ToInt32(tmpitem["accesslevel"]) == (int)AccessLevel.UberAdmin)
                        {
                            actor.SendError("You may not destroy other UberAdmins!\r\n");
                            return false;
                        }
                    }
                    catch
                    {
                        actor.SendError("Target is no longer there.\r\n");
                        actor.ClearTarget();
                        return false;
                    }

                }
            }
            try
            {
                tmpitem = Lib.GetByID(actor["target"].ToString());
                tmpitem.Destroy();
                actor.Send("Object destruction succeeded.\r\n");
                actor.ClearTarget();
            }
            catch (Exception ex)
            {
                actor.SendError("Object destruction failed with the error: " + ex.Message + ex.StackTrace + "\r\n");
                return false;
            }
            return true;
        }

    }

    /// <summary>
    /// Destroys all users.
    /// </summary>
    public class Command_destroyallusers : Command, ICommand
    {




        public Command_destroyallusers()
        {
            name = "command_destroyallusers";
            words = new string[2] { "destroyallusers", "purge" };
            help.Command = "destroy";
            help.Summary = "Destroys all users.";
            help.Syntax = "destroyall";
            help.Examples = new string[1];
            help.Examples[0] = "destroyall";
        }
        public override bool DoCommand(Actor actor, string command, string arguments)
        {

            Actor item = null;

            for (int itemcount = Lib.GetWorldItems().Count - 1; itemcount >= 0; itemcount--)
            {
                if (Lib.GetWorldItems()[itemcount] != null)
                {
                    item = (Actor)Lib.GetWorldItems()[itemcount];
                    if (item["type"].ToString() == "user" || item["subtype"].ToString() == "user")
                    {
                        // delete the user

                        if (Convert.ToInt32(actor["accesslevel"]) < (int)AccessLevel.UberAdmin)
                        {
                            actor.SendError("You may not destroy users.\r\n");
                            return false;
                        }


                        try
                        {
                            if (Convert.ToInt32(item["accesslevel"]) < (int)AccessLevel.UberAdmin)
                            {
                                actor.Send("Object destruction succeeded. User \"" + item["name"] + "\" returns to dust... \r\n");
                                item.Destroy();

                            }

                        }
                        catch (Exception ex)
                        {
                            actor.SendError("Object destruction failed with the error: " + ex.Message + ex.StackTrace + "\r\n");
                            return false;
                        }

                    }

                }

            }
            return true;
        }

    }



    public class Command_dig : Command, ICommand
    {




        public Command_dig()
        {
            name = "command_dig";
            words = new string[1] { "dig" };
            help.Command = "dig";
            help.Summary = "Set dig mode on or off";
            help.Syntax = "dig";
            help.Examples = new string[1];
            help.Examples[0] = "dig";
            help.Description = "Sets builders dig toggle on or off";
        }
        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            try
            {
                if ((string)actor["digging"] == "true")
                {
                    actor["digging"] = "false";
                    actor.Send("Digging is now off.\r\n");
                }
                else
                {
                    actor["digging"] = "true";
                    actor.Send("Digging is now on.\r\n");
                }
            }
            catch
            {
                actor["digging"] = "true";
                actor.Send("Digging is now on.\r\n");
            }
            return true;
        }

    }

    /// <summary>
    /// Copy command for player.
    /// </summary>
    public class Command_copy : Command, ICommand
    {




        public Command_copy()
        {
            name = "command_copy";
            words = new string[1] { "copy" };
            help.Command = "copy";
            help.Summary = "Copies targeted object.";
            help.Syntax = "copy [<number>]";
            help.Examples = new string[2];
            help.Examples[0] = "copy ";
            help.Examples[1] = "copy 3";
            help.Description = "Target the item to copy, then 'copy'. To make more then 1 copy, use 'copy [n]', where [n] is the number you want";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {

            ArrayList words = Lib.GetWords(arguments);
            Actor tmpitem = null;
            string tmpItems = "";
            int itemquantity = 0;

            if (actor["target"].ToString() == "")
            {
                actor.SendError("You don't have a target to modify. Use the 'target' command to target something.\r\n");
                return false;
            }

            // Get object to modify
            tmpitem = Lib.GetByID(actor["target"].ToString());
            if (tmpitem == null)
            {
                actor.SendError("Object not found.\r\n");
                return false;
            }

            if (tmpitem["type"].ToString() == "user")
            {
                actor.SendError("You may not copy users.\r\n");
                return false;
            }

            if (words.Count == 0)
            {
                CopyItem(actor, tmpitem, 1);
                return true;
            }

            if (words.Count == 1)
            {
                tmpItems = words[0].ToString();
                if (Lib.IsNumeric((string)words[0]))
                {
                    // Then this word is our quantity.
                    itemquantity = Convert.ToInt32(words[0]);
                    CopyItem(actor, tmpitem, itemquantity);
                }
                else
                {
                    actor.SendError("You must specify a number to clone");
                    return false;
                }

            }
            return true;

        }

        public bool CopyItem(Actor actor, Actor tmpitem, int itemquantity)
        {
            //Actor item = new Actor;
            string msg = "";
            string itemname = "";
            if (itemquantity == 0)
            {
                return false;
            }
            for (int i = 0; i < itemquantity; i++)
            {
                try
                {
                    // Create item in the user's room
                    Actor item = tmpitem.Copy();
                    itemname = item["name"].ToString();
                    item["container"] = actor["container"].ToString();
                    item["containertype"] = actor["containertype"].ToString();
                    item.Save();
                    lock (Lib.GetWorldItems().SyncRoot)
                    {
                        Lib.GetWorldItems().Add(item);
                    }
                    Actor room = actor.GetContainer();
                    room.Additem(item);
                    if (itemquantity == 1)
                    {
                        actor["target"] = item["id"];
                        actor["targettype"] = item["type"].ToString();
                    }

                }
                catch (Exception ex)
                {
                    actor.SendError("Object creation failed with the error: " + ex.Message + ex.StackTrace + "\r\n");
                    return false;
                }
            }
            if (itemquantity == 1)
            {
                msg = "1 new " + itemname + " successfully created in the room.\r\n";
            }
            else
            {
                msg = itemquantity + " new " + itemname + "s successfully created in the room.\r\n";
            }
            actor.Send(msg);
            return true;
        }

    }

    /// <summary>
    /// Builder command modify.
    /// </summary>
    public class Command_modify : Command, ICommand
    {




        public Command_modify()
        {
            name = "command_modify";
            words = new string[2] { "modify", "mod" };
            help.Command = "modify or mod";
            help.Summary = "Displays, modifies and/or saves the properties or states of an object that you have targeted.";
            help.Syntax = "modify [<type> <state name> <data type> <value>] || [<type> <property name> <value>]";
            help.Examples = new string[6];
            help.Examples[0] = "modify or mod";
            help.Examples[1] = "modify property Damagemin 10";
            help.Examples[2] = "modify property Worth 900";
            help.Examples[3] = "modify state Equipable System.Boolean true";
            help.Examples[4] = "modify state Equipslot System.String weapon1";
            help.Examples[5] = "modify save";

            help.Description = "You only have to specify <data type> when modifying states, not properties. To see the targeted object's properties and states, type 'modify'. Type 'modify save' to save your changes.";
        }

        public bool ShowActions(Actor actor, Actor user)
        {
            string[] actions = actor.GetActionWordList();
            user.Send(user["colorexits"] + actor.GetNameUpper() + " has the following actions:\r\n");
            foreach (string action in actions)
            {
                user.Send(action + " : " + actor.GetActionByWord(action).Name + "\r\n");
            }
            return true;
        }

        public bool ShowStates(Actor actor, Actor user)
        {
            ArrayList states = new ArrayList();

            user.Send(user["colorexits"] + actor.GetNameUpper() + " has the following states:\r\n");
            // Cycle through and display states that the actor has
            System.Collections.IEnumerator statenames = actor.states.Keys.GetEnumerator();
            while (statenames.MoveNext())
            {
                states.Add(statenames.Current);
                //string name = (string)statenames.Current;
                //user.Send("(Read-Write) " + (actor[name]).GetType().ToString() + " " + user["colorexits"] + name + " = " + user["colormobs"] + actor[name] + "\r\n");
            }
            states.Sort(new StateComparer());
            foreach (Object state in states)
            {
                string name = state.ToString();
                //string name = (string)statenames.Current;
                user.Send("(Read-Write) " + (actor[name]).GetType().ToString() + " " + user["colorexits"] + name + " = " + user["colormobs"] + actor[name] + "\r\n");
            }

            return true;
        }

        public class Comparer : IComparer
        {
            CaseInsensitiveComparer cic = new CaseInsensitiveComparer();
            PropertyInfo newx = null;
            PropertyInfo newy = null;
            int result = 0;

            int IComparer.Compare(Object x, Object y)
            {
                newx = (PropertyInfo)x;
                newy = (PropertyInfo)y;
                result = cic.Compare(newx.Name, newy.Name);
                return result;
            }
        }

        public class StateComparer : IComparer
        {
            CaseInsensitiveComparer cic = new CaseInsensitiveComparer();

            int result = 0;

            int IComparer.Compare(Object x, Object y)
            {
                result = cic.Compare(x.ToString(), y.ToString());
                return result;
            }
        }

        public bool ShowProperties(Object tmptarget, Actor user)
        {
            ArrayList members = new ArrayList();
            MemberInfo[] arrayMemberInfo;
            MemberInfo[] memberinfo2;
            object property = null;
            Type t = tmptarget.GetType();
            PropertyInfo pi = null;
            string description = "";
            arrayMemberInfo = t.FindMembers(MemberTypes.Property,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                    new MemberFilter(DelegateToSearchCriteria), "ReferenceEquals");
            user.Send("Object has the following properties:\r\n");
            for (int i = 0; i < arrayMemberInfo.Length; i++)
            {
                try
                {
                    pi = t.GetProperty(arrayMemberInfo[i].Name);
                    description += "(" + (pi.CanWrite ? "Read-Write" : "ReadOnly") + ") ";
                    description += pi.PropertyType + " ";
                    description += user["colormobs"] + arrayMemberInfo[i].Name + user["colorexits"] + " = " + user["colormessages"];

                    try
                    {
                        property = pi.GetValue(tmptarget, null);
                        description += user["colorexits"] + property.ToString() + "\r\n" + user["colormessages"];
                    }
                    catch
                    {
                        description += "\r\n";
                    }
                    if (property != null)
                    {
                        try
                        {
                            if (property.ToString() == "System.Collections.SortedList")
                            {
                                memberinfo2 = (property.GetType()).GetMembers();
                                foreach (DictionaryEntry exit in (SortedList)property)
                                {
                                    description += user["colorexits"] + "\t" + exit.Key + "\r\n" + user["colormessages"];
                                }
                            }
                            if (property.ToString() == "System.Collections.ArrayList")
                            {
                                memberinfo2 = (property.GetType()).GetMembers();
                                foreach (object thing in (ArrayList)property)
                                {
                                    description += user["colorexits"] + "\t" + thing.ToString() + "\r\n" + user["colormessages"];
                                }
                            }
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    user.SendError("Viewing the properties failed with the error: " + ex.Message + ex.StackTrace + "\r\n");
                    i++;
                }
            }
            user.Send(description + "\r\n");
            return true;
        }

        public bool ChangeState(Actor tmptarget, Actor actor, string propertyName, object propertyValue, string datatype)
        {
            // Find out what data type they specified and convert the value to that data type
            switch (datatype.ToLower())
            {
                case "system.boolean":
                    propertyValue = Lib.ConvertToBoolean(propertyValue);
                    break;
                case "boolean":
                    propertyValue = Lib.ConvertToBoolean(propertyValue);
                    break;
                case "bool":
                    propertyValue = Lib.ConvertToBoolean(propertyValue);
                    break;
                case "system.byte":
                    propertyValue = Convert.ToByte(propertyValue);
                    break;
                case "byte":
                    propertyValue = Convert.ToByte(propertyValue);
                    break;
                case "system.char":
                    propertyValue = Convert.ToChar(propertyValue);
                    break;
                case "char":
                    propertyValue = Convert.ToChar(propertyValue);
                    break;
                case "system.datetime":
                    propertyValue = Convert.ToDateTime(propertyValue);
                    break;
                case "datetime":
                    propertyValue = Convert.ToDateTime(propertyValue);
                    break;
                case "system.decimal":
                    propertyValue = Convert.ToDecimal(propertyValue);
                    break;
                case "decimal":
                    propertyValue = Convert.ToDecimal(propertyValue);
                    break;
                case "system.double":
                    propertyValue = Convert.ToDouble(propertyValue);
                    break;
                case "double":
                    propertyValue = Convert.ToDouble(propertyValue);
                    break;
                case "system.int16":
                    propertyValue = Convert.ToInt16(propertyValue);
                    break;
                case "int16":
                    propertyValue = Convert.ToInt16(propertyValue);
                    break;
                case "system.int32":
                    propertyValue = Convert.ToInt32(propertyValue);
                    break;
                case "int32":
                    propertyValue = Convert.ToInt32(propertyValue);
                    break;
                case "int":
                    propertyValue = Convert.ToInt32(propertyValue);
                    break;
                case "system.int64":
                    propertyValue = Convert.ToInt64(propertyValue);
                    break;
                case "int64":
                    propertyValue = Convert.ToInt64(propertyValue);
                    break;
                case "system.sbyte":
                    propertyValue = Convert.ToSByte(propertyValue);
                    break;
                case "sbyte":
                    propertyValue = Convert.ToSByte(propertyValue);
                    break;
                case "system.single":
                    propertyValue = Convert.ToSingle(propertyValue);
                    break;
                case "single":
                    propertyValue = Convert.ToSingle(propertyValue);
                    break;
                case "system.string":
                    propertyValue = Convert.ToString(propertyValue);
                    break;
                case "string":
                    propertyValue = Convert.ToString(propertyValue);
                    break;
                case "system.uint16":
                    propertyValue = Convert.ToUInt16(propertyValue);
                    break;
                case "uint16":
                    propertyValue = Convert.ToUInt16(propertyValue);
                    break;
                case "system.uint32":
                    propertyValue = Convert.ToUInt32(propertyValue);
                    break;
                case "uint32":
                    propertyValue = Convert.ToUInt32(propertyValue);
                    break;
                case "system.uint64":
                    propertyValue = Convert.ToUInt64(propertyValue);
                    break;
                case "uint64":
                    propertyValue = Convert.ToUInt64(propertyValue);
                    break;
                default:
                    propertyValue = Convert.ToString(propertyValue);
                    break;
            }

            // Set the state
            try
            {
                tmptarget[propertyName] = propertyValue;
                actor.Send("The state named " + actor["colorexits"] + propertyName + " is now set to " + propertyValue.GetType().ToString() + " value: " + actor["colormobs"] + propertyValue.ToString() + "\r\n");
            }
            catch (Exception ex)
            {
                actor.SendError("Modify failed with the error: " + ex.Message + ex.StackTrace + "\r\n");
                return false;
            }
            return true;

        }


        public bool ChangeProperty(Actor tmptarget, Actor actor, string propertyName, object propertyValue)
        {
            try
            {
                MemberInfo[] arrayMemberInfo;
                Type t = tmptarget.GetType();
                arrayMemberInfo = t.FindMembers(MemberTypes.Property,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                    new MemberFilter(DelegateToSearchCriteria), "ReferenceEquals");
                PropertyInfo pi = null;
                //string name = words[0].ToString();
                pi = t.GetProperty(propertyName);
                if (pi == null)
                {
                    actor.SendError("Property name not found. Check spelling since these are case sensitive.\r\n");
                    return false;
                }

                // Get value to assign, which is the rest of the string minus the property name
                //object propertyValue = arguments.Replace(name + " ","");

                switch (pi.PropertyType.ToString())
                {
                    case "System.Boolean":
                        propertyValue = Lib.ConvertToBoolean(propertyValue);
                        break;
                    case "System.Byte":
                        propertyValue = Convert.ToByte(propertyValue);
                        break;
                    case "System.Char":
                        propertyValue = Convert.ToChar(propertyValue);
                        break;
                    case "System.DateTime":
                        propertyValue = Convert.ToDateTime(propertyValue);
                        break;
                    case "System.Decimal":
                        propertyValue = Convert.ToDecimal(propertyValue);
                        break;
                    case "System.Double":
                        propertyValue = Convert.ToDouble(propertyValue);
                        break;
                    case "System.Int16":
                        propertyValue = Convert.ToInt16(propertyValue);
                        break;
                    case "System.Int32":
                        propertyValue = Convert.ToInt32(propertyValue);
                        break;
                    case "System.Int64":
                        propertyValue = Convert.ToInt64(propertyValue);
                        break;
                    case "System.SByte":
                        propertyValue = Convert.ToSByte(propertyValue);
                        break;
                    case "System.Single":
                        propertyValue = Convert.ToSingle(propertyValue);
                        break;
                    case "System.String":
                        propertyValue = Convert.ToString(propertyValue);
                        break;
                    case "System.UInt16":
                        propertyValue = Convert.ToUInt16(propertyValue);
                        break;
                    case "System.UInt32":
                        propertyValue = Convert.ToUInt32(propertyValue);
                        break;
                    case "System.UInt64":
                        propertyValue = Convert.ToUInt64(propertyValue);
                        break;
                    default:
                        propertyValue = Convert.ToString(propertyValue);
                        break;
                }

                t.InvokeMember
                    (propertyName
                    , BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Instance
                    | BindingFlags.SetProperty
                    , null
                    , tmptarget
                    , new object[] { propertyValue }
                    );
                //actor.Send("Property '" + propertyName + "' is now set to: '" + propertyValue.ToString() + "'\r\n" );
                actor.Send("The property named " + actor["colorexits"] + propertyName + " is now set to " + propertyValue.GetType().ToString() + " value: " + actor["colormobs"] + propertyValue.ToString() + "\r\n");
            }
            catch (Exception ex)
            {
                actor.SendError("Modify failed with the error: " + ex.Message + ex.StackTrace + "\r\n");
                return false;
            }
            return true;
        }



        public bool SaveProperties(Actor tmptarget, Actor user)
        {
            try
            {
                tmptarget.Save();
                user.Send("Changes saved.\r\n");
                return true;
            }
            catch (Exception ex)
            {
                user.SendError("Error trying to save the property: " + ex.Message + ex.StackTrace + "\r\n");
                return false;
            }
        }



        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            ArrayList words = Lib.GetWords(arguments);
            Actor tmpitem = null;

            if (actor["target"].ToString() == "")
            {
                actor.SendError("You don't have a target to modify. Use the 'target' command to target something.\r\n");
                return false;
            }

            // Get object to modify
            tmpitem = Lib.GetByID(actor["target"].ToString());
            if (tmpitem == null)
            {
                actor.SendError("Object not found.\r\n");
                return false;
            }

            // No paramters, so show all properties, actions, and states
            if (words.Count == 0)
            {
                ShowProperties(tmpitem, actor);
                ShowActions(tmpitem, actor);
                ShowStates(tmpitem, actor);
                return true;
            }
            if (words.Count == 1)
            {
                if (words[0].ToString().ToLower() == "save")
                {
                    SaveProperties(tmpitem, actor);
                    return true;
                }
                actor.SendError("You must specify a type, name, datatype and value to set for a property or state. Check your syntax.\r\n");
                return false;
            }

            // Modifying a state
            if (words.Count >= 4)
            {
                // property or state
                string type = words[0].ToString().ToLower();

                if (type != "property" && type != "state")
                {
                    actor.SendError("You must specify if you are trying to change an object's 'property' or a 'state'. Check your syntax.\r\n");
                    return true;
                }

                // name of property or state
                string name = words[1].ToString();
                // data type of the property or state
                string datatype = words[2].ToString();
                // Value to use for the property or state are the last words in the arguments
                // Remove the type, name, and datatype from the string and use what's left as the value

                //string propertyValue = arguments.Replace(type + " ", "");
                //propertyValue = propertyValue.Replace(name + " ", "");
                //propertyValue = propertyValue.Replace(datatype + " ", "");

                string propertyValue = arguments.Substring(type.Length + 1, (arguments.Length - type.Length - 1));
                propertyValue = propertyValue.Substring(name.Length + 1, (propertyValue.Length - name.Length - 1));
                propertyValue = propertyValue.Substring(datatype.Length + 1, (propertyValue.Length - datatype.Length - 1));


                // Prevent elevation of privilege exploit
                // Add additional security conditions for the modify command here
                if (name.ToLower() == "accesslevel" && Convert.ToInt32(actor["accesslevel"]) < (int)AccessLevel.Admin)
                {
                    actor.SendError("You have insufficient permissions to change someone's access level.\r\n");
                    return true;
                }
                if (name.ToLower() == "password")
                {
                    actor.SendError("This value is encrypted. Do not use this command to change passwords.\r\n");
                    return true;
                }
                ChangeState(tmpitem, actor, name, propertyValue, datatype);
                return true;
            }

            // Modifying a property
            if (words.Count >= 3)
            {
                // property or state
                string type = words[0].ToString().ToLower();
                if (type != "property")
                {
                    actor.SendError("You must specify a type, name, datatype and value to set for a property or state. Check your syntax.\r\n");
                    return false;
                }

                // name of property or state
                string name = words[1].ToString();

                string propertyValue = arguments.Substring(type.Length + 1, (arguments.Length - type.Length - 1));
                propertyValue = propertyValue.Substring(name.Length + 1, (propertyValue.Length - name.Length - 1));

                // Prevent elevation of privilege exploit
                // Add additional security conditions for the modify command here
                if (name.ToLower() == "accesslevel" && Convert.ToInt32(actor["accesslevel"]) < (int)AccessLevel.Admin)
                {
                    actor.SendError("You have insufficient permissions to change someone's access level.\r\n");
                    return true;
                }
                if (name.ToLower() == "password")
                {
                    actor.SendError("This value is encrypted. Do not use this command to change passwords.\r\n");
                    return true;
                }
                ChangeProperty(tmpitem, actor, name, propertyValue);
                return true;
            }



            actor.SendError("You must specify a type, name, datatype and value to set for a property or state. Check your syntax.\r\n");
            return false;
        }
        public static bool DelegateToSearchCriteria(MemberInfo objMemberInfo, Object objSearch)
        {
            // Compare the name of the member function with the filter criteria.
            return true;
        }


    }


    /// <summary>
    /// Add an Action to an Actor.
    /// </summary>
    public class Command_addaction : Command, ICommand
    {




        public Command_addaction()
        {
            name = "command_addaction";
            words = new string[1] { "addaction" };
            help.Command = "addaction";
            help.Summary = "Adds and Action to an Item, Mob, or Player.";
            help.Syntax = "addaction [action name]";
            help.Examples = new string[1];
            help.Examples[0] = "addaction dummy_push";
            help.Description = "Adds and Action to an Item, Mob, or Player.  Use 'Target' to specify the Actor.";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            ArrayList words = Lib.GetWords(arguments);

            if (actor["target"].ToString() == "")
            {
                actor.SendError("You don't have a target to modify. Use the 'target' command to target something.\r\n");
                return false;
            }

            Actor tmpactor;

            // Get object to modify
            tmpactor = Lib.GetByID(actor["target"].ToString());

            if (tmpactor == null)
            {
                actor.SendError("Object not found.\r\n");
                return false;
            }

            Action tmpaction = (Action)actor.GetActionByName(arguments);
            if (tmpaction == null)
            {
                actor.SendError("Action not found.\r\n");
                return false;
            }
            else
            {
                try
                {
                    tmpactor.AddActionWord(tmpaction);
                }
                catch
                {
                    actor.SendError("Failed to add action.\r\n");
                    return false;
                }
            }

            return true;
        }

    }



    /// <summary>
    /// Add an Action to an Actor.
    /// </summary>
    public class Command_enablecommand : Command, ICommand
    {




        public Command_enablecommand()
        {
            name = "command_enablecommand";
            words = new string[2] { "enablecommand", "ec" };
            help.Command = "enablecommand";
            help.Summary = "Enables a command so that players can use it and sets a minimum access level.";
            help.Syntax = "enablecommand <command name> <access level>";
            help.Examples = new string[2];
            help.Examples[0] = "enablecommand mynewcommand 0";
            help.Examples[1] = "ec mynewcommand 0";
            help.Description = "Player commands can be enabled and disabled in game without removing the code associated with the command itself. This step is also required for any newly-compiled commands for them to be used by players.";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            ArrayList words = Lib.GetWords(arguments);
            if (words.Count != 2)
            {
                actor.SendError("You must specify and command name and a security level for the new command. Check your syntax.\r\n");
                return false;
            }

            string commandname = words[0].ToString().ToLower();
            string accessleveltext = words[1].ToString().ToLower();
            int accesslevel = 0;


            try
            {
                accesslevel = Convert.ToInt32(accessleveltext);
            }
            catch
            {
                actor.SendError("Access level must be a numeric value. Check your syntax.\r\n");
                return false;
            }
            ICommand enablecommand = Lib.GetCommandByName(commandname);
            if (enablecommand == null)
            {
                actor.SendError("No command exists named: " + commandname + ". Use the name of the command as defined in the code, which is different than what players would type as the name of the command.\r\n");
                return false;
            }
            switch (accesslevel)
            {
                case (int)AccessLevel.System:
                    Lib.AddCommandWord(Lib.SystemCommandWords, enablecommand);
                    Lib.AddCommandToDb(AccessLevel.System, enablecommand.Name);
                    break;
                case (int)AccessLevel.Player:
                    Lib.AddCommandWord(Lib.PlayerCommandWords, enablecommand);
                    Lib.AddCommandToDb(AccessLevel.Player, enablecommand.Name);
                    break;
                case (int)AccessLevel.Builder:
                    Lib.AddCommandWord(Lib.BuilderCommandWords, enablecommand);
                    Lib.AddCommandToDb(AccessLevel.Builder, enablecommand.Name);
                    break;
                case (int)AccessLevel.Admin:
                    Lib.AddCommandWord(Lib.AdminCommandWords, enablecommand);
                    Lib.AddCommandToDb(AccessLevel.Admin, enablecommand.Name);
                    break;
                case (int)AccessLevel.UberAdmin:
                    Lib.AddCommandWord(Lib.UberAdminCommandWords, enablecommand);
                    Lib.AddCommandToDb(AccessLevel.UberAdmin, enablecommand.Name);
                    break;
                default:
                    actor.SendError("Incorrect access level provided. Check your access levels and the available values.\r\n");
                    return false;
            }
            actor.Send("Successfully enabled command '" + commandname + "' for players with a minimum access level of '" + accesslevel + "'.\r\n");
            return true;
        }

    }

    #region SystemCommands

    /// <summary>
    /// System command log.  Shows the log to the player.
    /// </summary>
    public class Command_log : Command, ICommand
    {




        public Command_log()
        {
            name = "command_log";
            words = new string[1] { "log" };
            help.Command = "log";
            help.Summary = "Displays the mud system log.";
            help.Syntax = "log";
            help.Examples = new string[1];
            help.Examples[0] = "log";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            actor.Send(Lib.log.Read());
            return true;
        }

    }

    /// <summary>
    /// System command log.  Shows the log to the player.
    /// </summary>
    public class Command_commandlog : Command, ICommand
    {




        public Command_commandlog()
        {
            name = "command_commandlog";
            words = new string[1] { "commandlog" };
            help.Command = "commandlog";
            help.Summary = "Displays the mud command log.";
            help.Syntax = "commandlog";
            help.Examples = new string[1];
            help.Examples[0] = "commandlog";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Stream str = File.Open(Path.GetFullPath(Path.Combine(Lib.PathtoRoot, Lib.commandLogFileName)), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            using (StreamReader sr = new StreamReader(str))
            {
                //This allows you to do one Read operation.
                actor.Send(sr.ReadToEnd());
            }
            return true;
        }

    }

    /// <summary>
    /// System command log.  Shows the log to the player.
    /// </summary>
    public class Command_systemlog : Command, ICommand
    {




        public Command_systemlog()
        {
            name = "command_systemlog";
            words = new string[1] { "systemlog" };
            help.Command = "systemlog";
            help.Summary = "Displays the mud system log.";
            help.Syntax = "systemlog";
            help.Examples = new string[1];
            help.Examples[0] = "systemlog";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Stream str = File.Open(Path.GetFullPath(Path.Combine(Lib.PathtoRoot, Lib.serverLogFileName)), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            using (StreamReader sr = new StreamReader(str))
            {
                //This allows you to do one Read operation.
                actor.Send(sr.ReadToEnd());
            }
            return true;
        }
    }


    /// <summary>
    /// System command time.  Show the current server date/time.
    /// </summary>
    public class Command_time : Command, ICommand
    {




        public Command_time()
        {
            name = "command_time";
            words = new string[3] { "time", "date", "day" };
            help.Command = "time or date or day";
            help.Summary = "Show the system time and date.";
            help.Syntax = "time";
            help.Examples = new string[1];
            help.Examples[0] = "time";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            actor.Send("The game date and time is " + Lib.Ansifboldwhite + Lib.Gametime.ToLongDateString() + " " + Lib.Gametime.ToShortTimeString() + ".\r\n");
            return true;
        }

    }

    /// <summary>
    /// Test.
    /// </summary>
    public class Command_test : Command, ICommand
    {




        public Command_test()
        {
            name = "command_test";
            words = new string[1] { "test" };
            help.Command = "testing";
            help.Summary = "testing123";
            help.Syntax = "test";
            help.Examples = new string[1];
            help.Examples[0] = "test";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Actor user = actor;
            user.Send("This is a test.\r\n");
            return true;
        }

    }


    /// <summary>
    /// Shows the state of the weather including celestial objects.
    /// </summary>
    public class Command_weather : Command, ICommand
    {




        public Command_weather()
        {
            name = "command_weather";
            words = new string[2] { "weather", "sky" };
            help.Command = "weather";
            help.Summary = "Shows the state of the weather including celestial objects.";
            help.Syntax = "weather or sky";
            help.Examples = new string[2];
            help.Examples[0] = "weather";
            help.Examples[1] = "sky";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            actor.Send(Lib.SunView());
            actor.Send(Lib.MoonView());
            return true;
        }

    }




    /// <summary>
    /// System command quit.  Quits the mud.
    /// </summary>
    public class Command_quit : Command, ICommand
    {




        public Command_quit()
        {
            name = "command_quit";
            words = new string[3] { "quit", "logout", "logoff" };
            help.Command = "quit";
            help.Summary = "Saves your character and logs it off.";
            help.Syntax = "quit or logout or logoff";
            help.Examples = new string[3];
            help.Examples[0] = "quit";
            help.Examples[0] = "logout";
            help.Examples[0] = "logoff";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            actor["connected"] = false;
            actor.Disco(actor, "player used quit command");
            return true;
        }

    }

    /// <summary>
    /// System command version.  Show the mud version.
    /// </summary>
    public class Command_version : Command, ICommand
    {



        public Command_version()
        {
            name = "command_version";
            words = new string[2] { "version", "ver" };
            help.Command = "version or ver";
            help.Summary = "Displays the TigerMUD server version.";
            help.Syntax = "version";
            help.Examples = new string[1];
            help.Examples[0] = "version";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            actor.Send("TigerMUD server version " + Lib.Serverversion + ".\r\n");
            return true;
        }

    }

    /// <summary>
    /// System command score.  Show the player's score.
    /// </summary>
    public class Command_score : Command, ICommand
    {



        public Command_score()
        {
            name = "command_score";
            words = new string[2] { "score", "sc" };
            help.Command = "score or sc\r\n";
            help.Summary = "Show the player's score.\r\n";
            help.Syntax = "score\r\n";
            help.Examples = new string[1];
            help.Examples[0] = "score\r\n shows the player's score.\r\n";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            Actor user = actor;
            Actor room = Lib.GetByID(user["container"].ToString());
            TimeSpan playedtime = new TimeSpan(Convert.ToInt64(Convert.ToInt64(user["played"]) * 1000000)); // Converting to .NET ticks (100 nanoseconds each)
            TimeSpan characterage = new TimeSpan(Convert.ToInt64((Lib.GetTime() - Convert.ToInt64(user["created"])) * 1000000));
            user.Send("Your name is " + user["shortnameupper"] + "\r\n");
            user.Send("Your reputation is " + user["reputation"] + " and you have " + user["kudostogive"] + " kudos to give to others.\r\n");
            user.Send("Your level is " + user["userlevel"] + " and you have " + user["experience"] + " experience points.\r\n");
            user.Send("You are in the room named: " + room["name"] + ", room id " + user["container"].ToString() + ".\r\n");
            user.Send("Your security access level is " + Convert.ToInt32(user["accesslevel"]) + ".\r\n");
            user.Send("You have played this character for " + Convert.ToInt32(playedtime.TotalHours) + " total hours (real time).\r\n");
            user.Send("Your character is " + Convert.ToInt32(characterage.TotalDays) + " days old (real time).\r\n");

            user.Send("Health:\t\t" + Convert.ToInt32(user["health"]) + "\r\n");
            user.Send("Stamina:\t" + Convert.ToInt32(user["stamina"]) + "\r\n");
            user.Send("Mana:\t\t" + Convert.ToInt32(user["mana"]) + "\r\n");

            user.Send("Strength:\t" + user["strength"] + "\r\n");
            user.Send("Agility:\t" + user["agility"] + "\r\n");
            user.Send("Intellect:\t" + user["intellect"] + "\r\n");
            user.Send("Spirit:\t\t" + user["spirit"] + "\r\n");

            user.Send("Your access level is: " + user["accesslevel"] + "\r\n");

            user.Showskills();
            user.Showspells();

            //user.Send( "The room you are in has the following states:\r\n");
            //// Cycle through and display states that the room has
            //System.Collections.IEnumerator roomnames = room.states.Keys.GetEnumerator();
            //while (roomnames.MoveNext())
            //{
            //    string name = (string)roomnames.Current;
            //    user.Send(name.Substring(0,1).ToUpper() + name.Substring(1) + " - " + room[name] + "\r\n");
            //}

            ////// demo showing how to create a new user state and save it
            ////DateTime dt = System.DateTime.Now;
            //user["datetimetest"] = DateTime.Now;

            //user.Send("You have the following states assigned to you:\r\n");
            ////// Cycle through and display states that the actor has
            //System.Collections.IEnumerator statenames = actor.states.Keys.GetEnumerator();
            //while (statenames.MoveNext())
            //{
            //    string name = (string)statenames.Current;
            //    actor.Send(name.Substring(0, 1).ToUpper() + name.Substring(1) + " - " + actor[name] + "\r\n");
            //}

            return true;
        }

    }

    /// <summary>
    /// System command who.  Show the players currently online.
    /// </summary>
    public class Command_who : Command, ICommand
    {



        public Command_who()
        {
            name = "command_who";
            words = new string[1] { "who" };
            help.Command = "who";
            help.Summary = "Displays a list of characters currently online.";
            help.Syntax = "who";
            help.Examples = new string[1];
            help.Examples[0] = "who";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            int counter = 0;
            actor.Send(Lib.Ansifgreen + "\nOnline users:\r\n\n");

            for (int i = Lib.GetWorldItems().Count - 1; i >= 0; i--)
            {
                Actor tmpactor = (Actor)Lib.GetWorldItems()[i];
                if (tmpactor["type"].ToString() == "user")
                {
                    if (tmpactor["connected"] != null)
                    {
                        if (Lib.ConvertToBoolean(tmpactor["connected"]))
                        {
                            counter++;
                            actor.Send(Lib.Ansifboldgreen + tmpactor.GetNameUpper() + "\r\n");
                        }
                    }
                    else
                        tmpactor["connected"] = false;
                }
            }
            actor.Send(Lib.Ansifgreen + "\r\n" + counter + " user(s) online.\r\n");
            return true;
        }

    }

    /// <summary>
    /// System command who.  Show the players currently online.
    /// </summary>
    public class Command_sql : Command, ICommand
    {



        public Command_sql()
        {
            name = "command_sql";
            words = new string[1] { "sql" };
            help.Command = "sql";
            help.Summary = "Runs a raw sql query. WARNING! If you are not intimate with sql statements, then avoid this.";
            help.Syntax = "sql";
            help.Examples = new string[1];
            help.Examples[0] = "sql select * from mudactorstates where name='equipable'";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            ArrayList words = Lib.GetWords(arguments);

            if (words.Count < 3)
            {
                actor.SendError("You must provide a valid sql statement to run. See help.\r\n");
                return false;
            }
            DataTable dt = null;

            try
            {
                string message = "";
                if (words[0].ToString() == "select")
                {
                    dt = Lib.dbService.Actor.RunQuery(arguments);
                    if (dt.Rows.Count > 0)
                    {
                        message += Lib.Ansifboldwhite;
                        // Send column names first
                        for (System.Int32 iCol = 0; iCol < dt.Columns.Count; iCol++)
                        {
                            message += dt.Columns[iCol].ColumnName + " - ";
                        }
                        message += "\r\n";
                        message += Lib.Ansifwhite;

                        foreach (DataRow row in dt.Rows)
                        {
                            object[] rowsarray = row.ItemArray;
                            for (int i = 0; i < rowsarray.Length; i++)
                            {
                                message += " " + rowsarray[i].ToString() + ",";
                            }
                            message += "\r\n";
                        }

                    }
                    actor.Send(message);

                }
                else
                    actor.SendError(Lib.dbService.Actor.RunNonQuery(arguments));
            }
            catch
            {
                return false;
            }
            return true;
        }

    }

    public class Command_delayed : Command, ICommand
    {



        public Command_delayed()
        {
            name = "command_delayed";
            words = new string[1] { "delayed" };
            help.Command = "delayed";
            help.Summary = "Displays a list of delayed commands in the arraylist for debugging.";
            help.Syntax = "delayed";
            help.Examples = new string[1];
            help.Examples[0] = "delayed";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            actor.Send(actor.DisplayDelayedCommands());
            return true;
        }

    }



    /// <summary>
    /// System command more.  Shows the next screen of multiscreen text.
    /// </summary>
    public class Command_more : Command, ICommand
    {



        public Command_more()
        {
            name = "command_more";
            words = new string[2] { "more", "m" };
            help.Command = "more or m";
            help.Summary = "Displays the next page of text.";
            help.Syntax = "more";
            help.Examples = new string[1];
            help.Examples[0] = "more";
            help.Description = "When the text being displayed is longer than the current screen, the rest of the text is cached and made available by use of this command.";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            // Protect from nulls
            if (command == null) command = "";
            if (arguments == null) arguments = "";

            // Set more prompt
            if (arguments.Length > 0)
            {
                switch (arguments.ToLower())
                {
                    case "on":
                        actor["more"] = true;
                        actor.Send("More prompt is now on.\r\n");
                        break;
                    case "off":
                        actor["more"] = false;
                        actor.Send("More prompt is now off.\r\n");
                        break;
                    default:
                        actor.SendError("Incorrect option. Check your syntax.\r\n");
                        break;
                }
                return true;
            }

            // Run more command
            if (actor["morebuffer"] == null) actor["morebuffer"] = "";
            string oldbuffer = actor["morebuffer"].ToString();
            actor["morebuffer"] = "";
            actor.Send(actor["colorlast"] + oldbuffer);
            return true;
        }

    }

    /// <summary>
    /// System command bigtext.  The big text command is just to test the word wrapping by sending lots of words
    /// </summary>
    public class Command_bigtext : Command, ICommand
    {



        public Command_bigtext()
        {
            name = "command_bigtext";
            words = new string[1] { "bigtext" };
            help.Command = "bigtext";
            help.Summary = "Shows a lot of text.";
            help.Syntax = "bigtext";
            help.Examples = new string[1];
            help.Examples[0] = "bigtext";
            help.Description = "Used for troubleshooting purposes, this command shows a large excerpt of text.";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            string txt = "In Congress, July 4, 1776. The unanimous Declaration of the thirteen united States of America. When in the Course of human events, it becomes necessary for one people to dissolve the political bands which have Connected them with another, and to assume among the powers of the earth, the separate and equal station to which the Laws of Nature and of Nature's God entitle them, a decent respect to the opinions of mankind requires that they should declare the causes which impel them to the separation. We hold these truths to be self-evident, that all men are created equal, that they are endowed by their Creator with certain unalienable Rights, that among these are Life, Liberty and the pursuit of Happiness. That to secure these rights, Governments are instituted among Men, deriving their just Powers from the consent of the governed, -- That whenever any Form of Government becomes destructive of these ends, it is the Right of the People to alter or to abolish it, and to institute new Government, laying its foundation on such principles and organizing its powers in such form, as to them shall seem most likely to effect their Safety and Happiness.";
            txt = txt + "Prudence, indeed, will dictate that Governments long established should not be changed for light and transient causes; and accordingly all experience hath shewn, that mankind are more disposed to suffer, while evils are sufferable, than to right themselves by abolishing the forms to which they are accustomed. But when a long train of abuses and usurpations, pursuing invariably the same Object evinces a design to reduce them under absolute Despotism, it is their right, it is their duty, to throw off such Government, and to provide new guards for their future security.";
            txt = txt + "Such has been the patient sufferance of these Colonies; and such is now the necessity which constrains them to alter their former Systems of Government. -- The history of the present King of Great Britain is a history of repeated injuries and usurpations, all having in direct object the establishment of an absolute Tyranny over these States. To prove this, let facts be submitted to a candid world. He has refused his Assent to Laws, the most wholesome and necessary for the public good. He has forbidden his Governors to pass Laws of immediate and pressing importance, unless suspended in their operation till his Assent should be obtained; and when so suspended, he has utterly neglected to attend to them.";
            txt = txt + "He has refused to pass other Laws for the accommodation of large districts of people, unless those people would relinquish the right of Representation in the Legislature, a right inestimable to them and formidable to tyrants only. He has called together legislative bodies at places unusual, uncomfortable, and distant from the depository of their Public Records, for the sole purpose of fatiguing them into compliance with his measures. He has dissolved Representative Houses repeatedly, for opposing with manly firmness his invasions on the rights of the people. He has refused for a long time, after such dissolutions, to cause others to be elected; whereby the Legislative Powers, incapable of Annihilation, have returned to the People at large for their exercise; the State remaining in the mean time exposed to all the dangers of invasion from without, and convulsions within.";
            txt = txt + "He has endeavoured to prevent the population of these States; for that purpose obstructing the Laws for Naturalization of Foreigners; refusing to pass others to encourage their migrations hither, and raising the conditions of new Appropriations of Lands. He has obstructed the Administration of Justice, by refusing his Assent to Laws for establishing Judiciary Powers. He has made Judges dependent on his Will alone, for the tenure of their offices, and the amount and payment of their salaries. He has erected a multitude of New Offices, and sent hither swarms of Officers to harrass our People, and eat out their substance. He has kept among us, in times of peace, Standing Armies without the Consent of our legislatures. He has affected to render the Military independent of and superior to the Civil Power.";
            txt = txt + "He has combined with others to subject us to a jurisdiction foreign to our constitution, and unacknowledged by our laws; giving his Assent to their Acts of pretended Legislation: For Quartering large bodies of armed troops among us: For protecting them, by a mock Trial, from Punishment for any Murders which they should commit on the Inhabitants of these States: For cutting off our Trade with all parts of the world: For imposing Taxes on us without our Consent: For depriving us in many cases, of the benefits of Trial by Jury: For transporting us beyond seas to be tried for pretended offences: For abolishing the free system of English Laws in a neighbouring Province, establishing therein an Arbitrary government, and enlarging its Boundaries so as to render it at once an example and fit instrument for introducing the same absolute rule into these Colonies:";
            txt = txt + "For taking away our Charters, abolishing our most valuable Laws, and altering fundamentally the forms of our Governments: For suspending our own Legislature, and declaring themselves invested with power to legislate for us in all cases whatsoever. He has abdicated Government here, by declaring us out of his Protection and waging War against us. He has plundered our seas, ravaged our Coasts, burnt our towns, and Destroyed the lives of our people. He is at this time transporting large Armies of foreign Mercenaries to compleat the works of death, desolation and tyranny, already begun with circumstances of Cruelty and perfidy scarcely paralleled in the most barbarous ages, and totally unworthy the Head of a civilized nation. He has constrained our fellow Citizens taken Captive on the high Seas to bear Arms against their Country, to become the executioners of their friends and Brethren, or to fall themselves by their Hands.";
            txt = txt + "He has excited domestic insurrections amongst us, and has endeavoured to bring on the inhabitants of our frontiers, the merciless Indian Savages, whose known rule of warfare, is an undistinguished destruction of all ages, sexes and conditions. In every stage of these Oppressions we have Petitioned for Redress in the most humble terms: Our repeated Petitions have been answered only by repeated injury. A Prince, whose character is thus marked by every act which may define a Tyrant, is unfit to be the ruler of a free people. Nor have we been wanting in attention to our Brittish brethren. We have warned them from time to time of attempts by their legislature to extend an unwarrantable jurisdiction over us. We have reminded them of the circumstances of our emigration and settlement here. We have appealed to their native justice and magnanimity, and we have conjured them by the ties of our common kindred to disavow these usurpations, which, would inevitably interrupt our connections and correspondence. They too have been deaf to the voice of justice and of consanguinity. We must, therefore, acquiesce in the necessity, which denounces our Separation, and hold them, as we hold the rest of mankind, Enemies in War, in Peace Friends.";
            txt = txt + "We, therefore, the Representatives of the united States of America, in General Congress, Assembled, appealing to the Supreme Judge of the world for the rectitude of our intentions, do, in the Name, and by Authority of the good People of these Colonies, solemnly publish and declare, That these United Colonies are, and of Right ought to be Free and Independent States; that they are absolved from all Allegiance to the British Crown, and that all political Connection between them and the State of Great Britain, is and ought to be totally dissolved; and that as Free and Independent States, they have full Power to levy War, conclude Peace, contract Alliances, establish Commerce, and to do all other Acts and Things which Independent States may of right do.";
            txt = txt + "And for the support of this Declaration, with a firm reliance on the protection of Divine Providence, we mutually pledge to each other our Lives, our Fortunes and our sacred Honor.\r\n"; // Added a missing \r\n statement -- Drevlan 09.27.04
            actor.Send(txt);
            return true;
        }

    }

    /// <summary>
    /// System command colors.  Shows the current color scheme.
    /// </summary>
    public class Command_colors : Command, ICommand
    {



        public Command_colors()
        {
            name = "command_colors";
            words = new string[1] { "colors" };
            help.Command = "colors";
            help.Summary = "displays your current text color preferences.";
            help.Syntax = "colors";
            help.Examples = new string[1];
            help.Examples[0] = "colors";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            string txt = actor["colorcommandprompt"] + "<Command Prompt> ";
            txt += actor["colorcommandtext"] + "<Command Text> ";
            txt += actor["colorerrors"] + "<Errors> ";
            txt += actor["colorexits"] + "<Exits> ";
            txt += actor["coloritems"] + "<Items> ";
            txt += "<Messages> ";
            txt += actor["colormobs"] + "<Mobs> ";
            txt += actor["colorpeople"] + "<People> ";
            txt += actor["colorroomdescr"] + "<Room Desciptions> ";
            txt += actor["colorroomname"] + "<Room Names> ";
            txt += actor["coloralertgood"] + "<Alert Good> ";
            txt += actor["coloralertbad"] + "<Alert Bad> ";
            txt += actor["colorsystemmessage"] + "<System Messages> ";
            txt += actor["colorannouncement"] + "<Announcements>\r\n";

            actor.Send(txt);
            return true;
        }

    }

    /// <summary>
    /// System command screen.  Sets the width of the player's screen.
    /// </summary>
    public class Command_screen : Command, ICommand
    {



        public Command_screen()
        {
            name = "command_screen";
            words = new string[1] { "screen" };
            help.Command = "screen";
            help.Summary = "lets you set up your screen width to the value you specify for word wrapping purposes.";
            help.Syntax = "screen #";
            help.Examples = new string[1];
            help.Examples[0] = "Screen 80";
            help.Description = "The default for this value is 80 characters.";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            if (arguments.Length < 1)
            {
                actor.Send("Your screen with is " + actor["screenwidth"] + " characters.\r\n");
                return true;
            }
            else
            {
                if (Convert.ToInt32(arguments) < 40)
                {
                    actor.SendError("You must specify a numberic value for screen width greater than 39.\r\n");
                }
                else
                {
                    try
                    {
                        actor["screenwidth"] = Convert.ToInt32(arguments);
                        actor.Send("Your screen with is now " + actor["screenwidth"] + " characters.\r\n");
                    }
                    catch // have to catch if user entered text instead of a numeric value
                    {
                        actor.SendError("You must specify a numberic value for screen width like 'screen 80'.\r\n");
                    }
                }
            }
            return true;
        }

    }

    /// <summary>
    /// System command wordwrap.  Sets the wordwrap feature on or off.
    /// </summary>
    public class Command_wordwrap : Command, ICommand
    {



        public Command_wordwrap()
        {
            name = "command_wordwrap";
            words = new string[1] { "wordwrap" };
            help.Command = "wordwrap";
            help.Summary = "Sets the wordwrap feature on or off.";
            help.Syntax = "wordwrap <on|off>";
            help.Examples = new string[2];
            help.Examples[0] = "wordwrap on";
            help.Examples[1] = "wordwrap off";
            help.Description = "The wordwrap features prevents words from being broken if they run to the edge of the screen.  Instead, they will appeal whole on the next line.";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            switch (arguments.ToLower())
            {
                case "on":
                    actor["wordwrap"] = true;
                    actor.Send("Word wrapping is now on.\r\n");
                    break;
                case "off":
                    actor["wordwrap"] = false;
                    actor.Send("Word wrapping is now off.\r\n");
                    break;
                default:
                    actor.Send("Word wrapping is currently " + (Lib.ConvertToBoolean(actor["wordwrap"]) ? "on" : "off") + ".\r\n");
                    break;
            }
            return true;
        }

    }

    /// <summary>
    /// System command wordwrap.  Sets the moreprompt feature on or off.
    /// </summary>
    public class Command_moreprompt : Command, ICommand
    {



        public Command_moreprompt()
        {
            name = "command_moreprompt";
            words = new string[1] { "moreprompt" };
            help.Command = "moreprompt";
            help.Summary = "Sets the more prompt feature on or off.";
            help.Syntax = "moreprompt <on|off>";
            help.Examples = new string[2];
            help.Examples[0] = "moreprompt on";
            help.Examples[1] = "moreprompt off";
            help.Description = "If on, when the text being displayed is longer than the current screen, the rest of the text is cached and made available by use of 'more' command.";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            switch (arguments.ToLower())
            {
                case "on":
                    actor["more"] = true;
                    actor.Send("More prompt is now on.\r\n");
                    break;
                case "off":
                    actor["more"] = false;
                    actor.Send("More prompt is now off.\r\n");
                    break;
                default:
                    actor.Send("More prompt is currently " + (Lib.ConvertToBoolean(actor["more"]) ? "on" : "off") + ".\r\n");
                    break;
            }
            return true;
        }

    }


    public class Command_changepassword : Command, ICommand
    {



        public Command_changepassword()
        {
            name = "command_changepassword";
            words = new string[2] { "changepassword", "cp" };
            help.Command = "changepassword or cp";
            help.Summary = "Changes user's password";
            help.Syntax = "changepassword <password>";
            help.Examples = new string[2];
            help.Examples[0] = "changepassword hello";
            help.Examples[1] = "cp hello";
            help.Description = "Changes the user's password";
        }
        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            ArrayList words = Lib.GetWords(arguments);
            if (words.Count < 1)
            {
                actor.SendError("You must specify a new password.\r\n");
                return false;
            }
            else
            {
                string newpassword = (string)words[0];
                if (!actor.ValidatePassword(newpassword))
                {
                    actor.SendError("Invalid Password.  The password cannot match your username, length must be 5-15 characters, and only contain alphanumeric characters.\r\n");
                    return false;
                }
                else
                {
                    actor["userpassword"] = Lib.Encrypt(newpassword);
                    actor.Save();
                    //Lib.UpdateUserPassword(actor["shortname"], newpassword);
                    actor.Send("Password has been changed and will take effect when you log back on.\r\n");
                    return true;
                }
            }

        }

    }


    /// <summary>
    /// System command colortest.  Shows the ansi colors.
    /// </summary>
    public class Command_colortest : Command, ICommand
    {




        public Command_colortest()
        {
            name = "command_colortest";
            words = new string[1] { "colortest" };
            help.Command = "colortest";
            help.Summary = "Displays a block of colored text to test ANSI color.";
            help.Syntax = "colortest";
            help.Examples = new string[1];
            help.Examples[0] = "colortest";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            string background = "";
            for (int tmpcounter = 1; tmpcounter <= 8; tmpcounter++)
            {
                switch (tmpcounter)
                {
                    case 1:
                        actor.Send(Lib.Ansibblack + "Background black: \r\n");
                        background = Lib.Ansibblack;
                        break;
                    case 2:
                        actor.Send(Lib.Ansibblue + "Background blue: \r\n");
                        background = Lib.Ansibblue;
                        break;
                    case 3:
                        actor.Send(Lib.Ansibcyan + "Background cyan: \r\n");
                        background = Lib.Ansibcyan;
                        break;
                    case 4:
                        actor.Send(Lib.Ansibgreen + "Background green: \r\n");
                        background = Lib.Ansibgreen;
                        break;
                    case 5:
                        actor.Send(Lib.Ansibpurple + "Background purple: \r\n");
                        background = Lib.Ansibpurple;
                        break;
                    case 6:
                        actor.Send(Lib.Ansibred + "Background red: \r\n");
                        background = Lib.Ansibred;
                        break;
                    case 7:
                        actor.Send(Lib.Ansibwhite + "Background white: \r\n");
                        background = Lib.Ansibwhite;
                        break;
                    case 8:
                        actor.Send(Lib.Ansibyellow + "Background yellow: \r\n");
                        background = Lib.Ansibyellow;
                        break;
                }
                actor.Send(Lib.Ansifblack + background + "ANSI color test - Ansifblack\r\n");
                actor.Send(Lib.Ansifblue + background + "ANSI color test - Ansifblue\r\n");
                actor.Send(Lib.Ansifcyan + background + "ANSI color test - Ansifcyan\r\n");
                actor.Send(Lib.Ansifgreen + background + "ANSI color test - Ansifgreen\r\n");
                actor.Send(Lib.Ansifpurple + background + "ANSI color test - Ansifpurple\r\n");
                actor.Send(Lib.Ansifred + background + "ANSI color test - Ansifred\r\n");
                actor.Send(Lib.Ansifwhite + background + "ANSI color test - Ansifwhite\r\n");
                actor.Send(Lib.Ansifyellow + background + "ANSI color test - Ansifyellow\r\n");
                actor.Send(Lib.Ansifboldblue + background + "ANSI color test - Ansifboldblue\r\n");
                actor.Send(Lib.Ansifboldcyan + background + "ANSI color test - Ansifboldcyan\r\n");
                actor.Send(Lib.Ansifboldgreen + background + "ANSI color test - Ansifboldgreen\r\n");
                actor.Send(Lib.Ansifboldpurple + background + "ANSI color test - Ansifboldpurple\r\n");
                actor.Send(Lib.Ansifboldred + background + "ANSI color test - Ansifboldred\r\n");
                actor.Send(Lib.Ansifboldwhite + background + "ANSI color test - Ansifboldwhite\r\n");
                actor.Send(Lib.Ansifboldyellow + background + "ANSI color test - Ansifboldyellow\r\n");
                actor.Send(Lib.Ansireset + "\r\n");
            }
            actor.Send(Lib.Ansireset);

            return true;
        }

    }

    /// <summary>
    /// System command again.  Repeats the last command.
    /// </summary>
    public class Command_again : Command, ICommand
    {



        public Command_again()
        {
            name = "command_again";
            words = new string[2] { "again", "." };
            help.Command = "again or .";
            help.Summary = "Repeats the last command you typed.";
            help.Syntax = "again or . (period / full-stop character)";
            help.Examples = new string[2];
            help.Examples[0] = "again";
            help.Examples[1] = ".";

        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {


            Commandprocessor cp = new Commandprocessor();
            // Ensure actor actually has a command to do again
            if (actor["lastmessagefrom"].ToString() != "")
            {
                //Parse command word from arguments
                string[] splitcommand = Lib.SplitCommand(actor["lastmessage"].ToString());
                string word = splitcommand[0];
                arguments = splitcommand[1];
                word = word.ToLower();
                Command truecommand = (Command)actor.GetCommandByWord(word);
                if (truecommand != null)
                {
                    truecommand.DoCommand(actor, word, arguments);
                }
                actor.Showprompt();
                return true;
            }
            actor.Showprompt();
            return true;
        }

    }

    /// <summary>
    /// Allows admins to view the bug list.
    /// </summary>
    public class Command_buglist : Command, ICommand
    {



        public const string SYNTAX_ERROR = "Unknown option. Try 'help buglist' for usage information.";
        private Actor user;

        public Command_buglist()
        {
            name = "command_buglist";
            words = new string[3] { "buglist", "listbug", "listbugs" };
            help.Command = "buglist, listbug, or listbugs";
            help.Summary = "View bugs added by players. Have the list emailed to you. Clear bugs from the list.";
            help.Syntax = "buglist [email {email address} | all | clear]";
            help.Examples = new string[4];
            help.Examples[0] = "buglist\t\t\tShows a list of new bugs";
            help.Examples[1] = "buglist email admin@example.com\tSends the buglist to the user's email address";
            help.Examples[2] = "buglist all\t\tShows all bugs (read and unread)";
            help.Examples[3] = "buglist clear\t\tClears all bugs from the list";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            bool isSuccess = true;
            user = actor;

            // Get the arguments
            string[] args = arguments.Split(' ');
            // Which modifier?
            switch (args[0].ToLower())
            {
                case "":
                    ShowAllBugs(actor);
                    break;
                case "all":
                    ShowAllBugs(actor);
                    break;
                case "new":
                    ShowNewBugs(actor);
                    break;
                case "email":
                    EmailBugList(actor, args);
                    break;
                case "clear":
                    ClearAllBugs(actor);
                    break;
                default:
                    user.SendError(SYNTAX_ERROR);
                    isSuccess = false;
                    break;
            }
            return isSuccess;
        }

        /// <summary>
        /// Shows all new bugs.
        /// </summary>
        private void ShowNewBugs(Actor actor)
        {
            DataTable bugTable = actor.GetBugList(false);

            if (bugTable.Rows.Count == 0)
            {
                user.SendSystemMessage("No new bugs.\r\n");
            }
            else
            {
                user.SendSystemMessage("New bugs (" + bugTable.Rows.Count + " bugs in total):\r\n");
                foreach (DataRow row in bugTable.Rows)
                {
                    user.SendSystemMessage("\tBug " + row["BugId"] +
                        " new (" + row["UserShortName"] + "):\t\t");
                    user.SendAnnouncement(row["BugText"] + "\r\n");

                    // Mark this bug as read
                    actor.MarkBugAsRead((int)row["BugId"]);
                }
                user.SendSystemMessage(bugTable.Rows.Count + " bugs in total.\r\n");
            }
        }

        /// <summary>
        /// Send a full bug list to the user
        /// </summary>
        private void EmailBugList(Actor actor, string[] args)
        {
            if (args.Length == 0)
            {
                user.SendError("Please specify an email address.\r\n");
            }
            else
            {
                string emailAddress = args[0];

                if (!Lib.ValidateEmail(emailAddress))
                {
                    user.SendError("Please specify a valid email address.\r\n");
                }
                else
                {
                    MailMessage userMessage = PrepareMailMessage(actor, emailAddress);
                    Lib.SendEmail(userMessage);
                }
            }
        }

        /// <summary>
        /// Shows all bugs.
        /// </summary>
        private void ShowAllBugs(Actor actor)
        {
            DataTable bugTable = actor.GetBugList(true);
            user.SendSystemMessage("All bugs (" + bugTable.Rows.Count + " bugs in total):\r\n");

            foreach (DataRow row in bugTable.Rows)
            {
                bool isNew = !((bool)row["IsRead"]);
                string modifier = "";
                if (isNew)
                {
                    modifier = " new";
                }

                user.SendSystemMessage("\tBug " + row["BugId"] +
                    modifier +
                    " (" + row["UserShortName"] + "):\t\t");
                user.SendAnnouncement(row["BugText"] + "\r\n");

                // Mark this bug as read
                actor.MarkBugAsRead((int)row["BugId"]);
            }
            user.SendSystemMessage(bugTable.Rows.Count + " bugs in total.\r\n");
        }

        /// <summary>
        /// Remove all bugs from the database
        /// </summary>
        private void ClearAllBugs(Actor actor)
        {
            user.SendAlertBad("Are you sure you want to delete all bugs? (y/n): ");
            string response = user.UserSocket.GetResponse();
            if (response.Trim().ToLower().StartsWith("y"))
            {
                actor.ClearBugs(true);
                user.SendSystemMessage("All bugs have been deleted.\r\n");
            }
            else
            {
                user.SendSystemMessage("No bugs were deleted.\r\n");
            }
        }

        #region Bug Email
        /// <summary>
        /// Creates an email with the bug list, for emailing to a user.
        /// </summary>
        /// <param name="toAddress">The message recipient.</param>
        /// <returns>A new mail message.</returns>
        private MailMessage PrepareMailMessage(Actor actor, string toAddress)
        {
            // Get the list of bugs
            DataTable bugTable = actor.GetBugList(true);

            MailMessage message = new MailMessage();
            message.To = toAddress;
            message.From = "TigerMUD Admin";
            message.Subject = "Bug list (" + bugTable.Rows.Count + " bugs)";
            message.BodyFormat = MailFormat.Text;

            // Dump the bugs into the email
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("Hello,\n\nYou have requested the TigerMUD bug list to be sent to this email address. ");
            builder.Append("This is a list of bugs that your MUD players have submitted using the 'bug' command.\n\n");

            builder.Append("New bugs (" + bugTable.Rows.Count + " bugs in total):\n");
            foreach (DataRow row in bugTable.Rows)
            {
                builder.Append("Bug " + row["BugId"] +
                    " (" + row["UserShortName"] + ") :" +
                    row["BugText"] +
                    "\n");

                // Mark this bug as read
                actor.MarkBugAsRead((int)row["BugId"]);
            }
            user.SendSystemMessage(bugTable.Rows.Count + " bugs in total.");

            message.Body = builder.ToString();
            return message;
        }

        #endregion
    }

    #endregion SystemCommands


    #region UberAdminCommands

    /// <summary>
    /// System command dellog.  Deletes the log.
    /// </summary>
    public class Command_dellog : Command, ICommand
    {



        public Command_dellog()
        {
            name = "command_dellog";
            words = new string[1] { "dellog" };
            help.Command = "dellog";
            help.Summary = "Clears the mud system log.";
            help.Syntax = "dellog";
            help.Examples = new string[1];
            help.Examples[0] = "dellog";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            if (Lib.YesNo(actor, "Are you sure you want to delete the entire log? (y/n): "))
            {
                actor.Send(Lib.DeleteServerLog());
                Lib.AddServerLogEntry(actor, "LOG DELETED (" + actor["shortname"] + ")");
            }
            return true;
        }

    }

    /// <summary>
    /// System command dellog.  Deletes the log.
    /// </summary>
    public class Command_command : Command, ICommand
    {



        public Command_command()
        {
            name = "command_command";
            words = new string[1] { "command" };
            help.Command = "command";
            help.Summary = "Adds, changes or removes the access level of a command.";
            help.Syntax = "command add|remove|info <commandname> [<accesslevel>]";
            help.Examples = new string[1];
            help.Examples[0] = "command add Command_say player";
            help.Examples[0] = "command remove Command_say";
            help.Examples[0] = "command info Command_say";
            help.Description = "This command will change the access level for a command, or show the information about its current access level.  If a command already has an access level, adding it to another level will remove it from the original level.  The command's words will all be added automatically.";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            string[] words = arguments.Split(new char[] { ' ' }, 2);
            if (words.Length < 2)
            {
                actor.SendError("You must specify what you want to do, and what command to do it to.  Please see 'help command' for details.\r\n");
                return false;
            }
            string action = words[0];
            switch (action.ToLower())
            {
                case "add":
                    //return DoAdd(actor, words[1]);
                    return actor.AddCommandWord(actor, words[1]);
                case "remove":
                    //return DoRemove(actor, words[1]);
                    return actor.RemoveCommandWord(actor, words[1]);
                case "info":
                    return DoInfo(actor, words[1]);
                default:
                    actor.SendError("You must specify add, remove or info.  Please see 'help command' for details.\r\n");
                    return false;
            }
        }

        private bool DoInfo(Actor actor, string commandname)
        {
            int accesslevel = actor.GetAccessLevel(commandname);

            string txt = "";

            switch (accesslevel)
            {
                case -1:
                    txt = "Command does not currently have an access level.\r\n";
                    break;
                case (int)AccessLevel.System:
                    txt = "Current access level is System.\r\n";
                    break;
                case (int)AccessLevel.Player:
                    txt = "Current access level is Player.\r\n";
                    break;
                case (int)AccessLevel.Builder:
                    txt = "Current access level is Builder.\r\n";
                    break;
                case (int)AccessLevel.Admin:
                    txt = "Current access level is Admin.\r\n";
                    break;
                case (int)AccessLevel.UberAdmin:
                    txt = "Current access level is UberAdmin.\r\n";
                    break;
                default:
                    txt = "Command has an invalid access level: " + accesslevel.ToString() + "\r\n";
                    break;
            }

            actor.Send(txt);

            return true;
        }

    }

    /// <summary>
    /// Locks a player account.
    /// </summary>
    public class Command_lockuser : Command, ICommand
    {



        private const string SYNTAX_ERROR = "Unknown arguments. Please type 'help lockuser' for more info.";

        public Command_lockuser()
        {
            name = "command_lockuser";
            words = new string[1] { "lockuser" };
            help.Command = "lockuser";
            help.Summary = "Locks a user out of the mud.";
            help.Syntax = "lockuser {username}";
            help.Examples = new string[1];
            help.Examples[0] = "lockuser badCompany";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            bool returnCode = false;

            if (arguments.Trim() != String.Empty)
            {
                // Does the user exist?
                if (actor.Exists(arguments))
                {
                    // Get this users ID from the database.
                    Actor tmpactor = Lib.GetByName(arguments);
                    // Lock the user
                    if (tmpactor != null)
                    {
                        tmpactor["lockedout"] = true;
                        tmpactor.Save();
                        actor.SendSystemMessage("The user has been locked.\r\n");
                    }
                    else
                    {
                        actor.SendError("Couldn't find that user to lock out.\r\n");
                        return false;
                    }


                    // Kick the user if he's online.
                    Actor lockedactor = Lib.GetByName(arguments);
                    if (lockedactor != null)
                    {
                        lockedactor.SendError("You have been locked out of the MUD. Contact the MUD Administrator.\r\n");
                        actor.Disco(actor, "player was kicked by admin");
                        actor.SendSystemMessage("The user has been kicked off the mud.\r\n");
                    }
                    returnCode = true;
                }
                else
                {
                    actor.SendError("That user does not exist.\r\n");
                }
            }
            else
            {
                actor.SendError("Please specify a user to lock.\r\n");
            }

            return returnCode;
        }

    }

    /// <summary>
    /// Locks a player account.
    /// </summary>
    public class Command_unlockuser : Command, ICommand
    {



        private const string SYNTAX_ERROR = "Unknown arguments. Please type 'help unlockuser' for more info.";

        public Command_unlockuser()
        {
            name = "system_unlockuser";
            words = new string[1] { "unlockuser" };
            help.Command = "unlockuser";
            help.Summary = "Unlocks a user and allows her to log into the mud.";
            help.Syntax = "unlockuser {username}";
            help.Examples = new string[1];
            help.Examples[0] = "unlockuser badCompany";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            bool returnCode = false;

            if (arguments.Trim() != String.Empty)
            {
                // Get this actor from the database.
                Actor tmpactor = Lib.GetByName(arguments);
                // unlock the actor
                if (tmpactor != null)
                {
                    tmpactor["lockedout"] = false;
                    tmpactor.Save();
                    actor.SendSystemMessage("The actor has been unlocked.\r\n");
                }
                else
                {
                    actor.SendError("Couldn't find that actor to unlock.\r\n");
                    return false;
                }
                actor.SendSystemMessage("The actor has been unlocked.\r\n");
                returnCode = true;
            }
            else
            {
                actor.SendError("Please specify a actor to unlock.\r\n");
            }

            return returnCode;
        }



        /// <summary>
        /// Reloads Scripts and Plugins
        /// </summary>
        public class Command_reloadscripts : Command, ICommand
        {



            private const string SYNTAX_ERROR = "Unknown arguments. Please type 'help lockuser' for more info.";

            public Command_reloadscripts()
            {
                name = "command_reloadscripts";
                words = new string[1] { "reloadscripts" };
                help.Command = "reloadscripts";
                help.Summary = "Reloads the scripts and plugins.";
                help.Syntax = "reloadscripts";
                help.Examples = new string[1];
                help.Examples[0] = "reloadscripts";
            }

            public override bool DoCommand(Actor actor, string command, string arguments)
            {


                return true;
            }

        }

    }
    #endregion


}