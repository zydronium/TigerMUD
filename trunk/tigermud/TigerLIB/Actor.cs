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

TigerMUD. A Multi Actor Dungeon engine.
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
using System.Net.Sockets;
using System.Net;
using System.Data;
using System.Text;
using System.Threading;
using TigerMUD.CommsLib;
using System.Web.Mail;


namespace TigerMUD
{
    /// <summary>
    /// Description of Actor.
    /// </summary>
    public class Actor : IActionResponse
    {
        protected ArrayList items = new ArrayList(); // user inventory
        public ArrayList Spells = new ArrayList(); // user known spells
        protected Hashtable actionwords = new Hashtable();
        protected Hashtable commandwords = new Hashtable();
        //public Hashtable State = new Hashtable();
        protected ArrayList HateList = new ArrayList();
        Actor tradetargetuser; // user you are trading with
        ArrayList bufferitems = new ArrayList(); // stores "to-be" traded items until both users confirm trade
        // bool incombat = false;
        public IUserSocket userSocket;
        public HateTable hatetable = new HateTable();
        SortedList exits = new SortedList();

        // indexer
        public Hashtable states = new Hashtable();
        // String-based indexer to store states

        public object this[string statename]
        {
            get 
            {
                // check for nulls
                if (!states.ContainsKey(statename))
                {
                    // String zero because with all the converts,
                    // it works with bool, string, int, and datetime
                    //return "0";
                    return null;
                }
                return states[statename];
            }
            set 
            {
                // Increment version number because actor state has changed
                // Use version number to decide if actor changed and must be saved
                this.version = this.version + 1;
                // Version numbers only go up to 90000
                if (this.version > 90000)
                {
                    this.version = 1;
                }
                states[statename] = value;
            }
        }
        
        private int version = 0;
        private int oldversion = 0;

        public int Version
        {
            get { return version; }
            set { version = value; }
        }

        public int OldVersion
        {
            get { return oldversion; }
            set { oldversion = value; }
        }

        public Actor()
        {
            this["id"] = System.Guid.NewGuid().ToString();
            this["name"] = "";
            this["type"] = "user";
            this["screenwidth"] = 80;
            this["lastlogindate"] = DateTime.Now.ToString();
        }

        public Actor(string shortname, IUserSocket userSocket)
        {
            this["id"] = System.Guid.NewGuid().ToString();
            this["shortname"] = shortname;
            this["name"] = "";
            this["type"] = "user";
            this["screenwidth"] = 80;
            this["lastlogindate"] = DateTime.Now.ToString();
            this.userSocket = userSocket;
        }

        
        public bool GiveAlltoUser(Actor targetuser)
        {
            foreach (Actor chkitem in this.GetContents())
            {
                // Put the item into the targer user's trade buffer
                //targetuser.Addbufferitem(item);
                this.Addbufferitem(chkitem);
                targetuser["tradestate"] = "pendingreceive";
                this["tradestate"] = "pendinggive";
                targetuser.Tradetargetuser = this;
                this.Tradetargetuser = targetuser;
            }
            this.Send("You are trying to give " + targetuser["name"] + " the folowing items:\r\n");
            targetuser.Send(this["name"] + " is trying to give you:\r\n");
            foreach (Actor scanitem in this.Getbufferitems())
            {
                if (scanitem["subtype"].ToString().StartsWith("stack"))
                {
                    this.Send(scanitem["quantity"] + " " + scanitem["name"] + "s.\r\n");
                    targetuser.Send(scanitem["quantity"] + " " + scanitem["name"] + "s.\r\n");
                }
                else
                {
                    this.Send(scanitem["nameprefix"] + " " + scanitem["name"] + ".\r\n");
                    targetuser.Send(scanitem["nameprefix"] + " " + scanitem["name"] + ".\r\n");
                }
            }
            this.Send("Type REJECT to cancel the trade.\r\n");
            targetuser.Send("Type ACCEPT or REJECT.\r\n");

            this.Save();
            targetuser.Save();
            return true;
        }

        public bool GiveAll(Actor actor)
        {
            if (actor["type"].ToString() == "user")
                GiveAlltoUser(actor);
            else
                GiveAlltoMob(actor);
            return true;
        }


        public bool GiveAlltoMob(Actor targetmob)
        {
            this.Send("You gave the following items to " + targetmob["name"] + ":\r\n");

            for (int i = this.GetContents().Count - 1; i >= 0; i--)
            {
                Actor chkitem = this.GetItemAtIndex(i);

                // Move items to mob
                this.Removeitem(chkitem);
                chkitem["container"] = targetmob["id"];
                chkitem["containertype"] = "mob";
                chkitem.Save();
                if (chkitem["subtype"].ToString().StartsWith("stack"))
                {
                    this.Send(chkitem["quantity"] + " " + chkitem["name"] + "s.\r\n");
                }
                else
                {
                    this.Send(chkitem["nameprefix"] + " " + chkitem["name"] + ".\r\n");
                }
            }
            this.Save();
            targetmob.Save();
            return true;
        }


        public bool GiveAllOfTypetoUser(Actor targetuser, Actor item)
        {
            foreach (Actor chkitem in this.GetContents())
            {
                if (chkitem["name"] == item["name"] || chkitem["name"].ToString() + "s" == item["name"].ToString() || chkitem["shortname"].ToString() == item["name"].ToString() || chkitem["shortname"] + "s" == item["name"].ToString())
                {
                    // Put the item into the targer user's trade buffer
                    targetuser.Addbufferitem(item);
                    this.Addbufferitem(chkitem);
                    targetuser["tradestate"] = "pendingreceive";
                    this["tradestate"] = "pendinggive";
                    targetuser.Tradetargetuser = this;
                    this.Tradetargetuser = targetuser;
                }
            }
            this.Send("You are trying to give " + targetuser["name"] + " the folowing items:\r\n");
            targetuser.Send(this["name"] + " is trying to give you: \r\n");
            foreach (Actor scanitem in this.Getbufferitems())
            {
                if (scanitem["subtype"].ToString().StartsWith("stack"))
                {
                    this.Send(scanitem["quantity"] + " " + scanitem["name"] + "s.\r\n");
                    targetuser.Send(scanitem["quantity"] + " " + scanitem["name"] + "s.\r\n");
                }
                else
                {
                    this.Send(scanitem["nameprefix"] + " " + scanitem["name"] + ".\r\n");
                    targetuser.Send(scanitem["nameprefix"] + " " + scanitem["name"] + ".\r\n");
                }
            }
            this.Send("Type REJECT to cancel the trade.\r\n");
            targetuser.Send("Type ACCEPT or REJECT.\r\n");

            this.Save();
            targetuser.Save();
            return true;
        }

        public bool GiveAllOfType(Actor actor, Actor item)
        {
            if (actor["type"].ToString() == "user")
                GiveAllOfTypetoUser(actor, item);
            else
                GiveAllOfTypetoMob(actor, item);
            return true;
        }


        public bool GiveAllOfTypetoMob(Actor targetmob, Actor item)
        {
            this.Send("You gave the following items to " + targetmob["name"] + ":\r\n");
            for (int i = this.GetContents().Count - 1; i >= 0; i--)
            {
                Actor chkitem = this.GetItemAtIndex(i);
                if (chkitem["name"] == item["name"] || chkitem["name"].ToString() + "s" == item["name"].ToString() || chkitem["shortname"].ToString() == item["name"].ToString() || chkitem["shortname"] + "s" == item["name"].ToString())
                {
                    // Move items to mob
                    this.Removeitem(chkitem);
                    chkitem["container"] = targetmob["id"];
                    chkitem["containertype"] = "mob";
                    chkitem.Save();
                    if (chkitem["subtype"].ToString().StartsWith("stack"))
                    {
                        this.Send(chkitem["quantity"] + " " + chkitem["name"] + "s.\r\n");
                    }
                    else
                    {
                        this.Send(chkitem["nameprefix"] + " " + chkitem["name"] + ".\r\n");
                    }
                }
            }
            this.Save();
            targetmob.Save();
            return true;
        }




        public bool GivetoUser(Actor targetuser, Actor item)
        {
            // Put the item into the targer user's trade buffer
            targetuser.Addbufferitem(item);
            this.Addbufferitem(item);
            targetuser["tradestate"] = "pendingreceive";
            this["tradestate"] = "pendinggive";
            targetuser.Tradetargetuser = this;
            this.Tradetargetuser = targetuser;
            if (item["subtype"].ToString().StartsWith("stack"))
            {
                this.Send("You are trying to give " + item["quantity"] + " " + item["name"] + "s to " + targetuser["name"] + ".\r\n");
                targetuser.Send(this["name"] + " is trying to give you " + item["quantity"] + " " + item["name"] + "s.\r\n");
            }
            else
            {
                this.Send("You are trying to give " + item["nameprefix"] + " " + item["name"] + " to " + targetuser["name"] + ".\r\n");
                targetuser.Send(this["name"] + " is trying to give you " + item["nameprefix"] + " " + item["name"] + ".\r\n");
            }
            this.Send("Type REJECT to cancel the trade.\r\n");
            targetuser.Send("Type ACCEPT or REJECT.\r\n");

            this.Save();
            targetuser.Save();
            return true;
        }

        public bool Give(Actor actor, Actor item)
        {
            if (actor["type"].ToString() == "user")
                GivetoUser(actor, item);
            else
                GivetoMob(actor, item);
            return true;
        }


        public bool GivetoMob(Actor targetmob, Actor item)
        {
            // Move items to mob
            this.Removeitem(item);
            item["container"] = targetmob["id"];
            item["containertype"] = "mob";
            item.Save();
            if (item["subtype"].ToString().StartsWith("stack"))
            {
                this.Send("You gave " + item["quantity"] + " " + item["name"] + "s to " + targetmob["name"] + ".\r\n");
            }
            else
            {
                this.Send("You gave " + item["nameprefix"] + " " + item["name"] + " to " + targetmob["name"] + ".\r\n");
            }
            this.Save();
            targetmob.Save();
            return true;
        }

        public bool Stateexists(string Name)
        {
            return Lib.dbService.Actor.StateExists(this["id"].ToString(),
                Name);
        }

        /// <summary>
        /// Load and set proper data type for actor states. You do not have to use the DataTable that gets returned.
        /// </summary>
        public DataTable Loadstate()
        {
            string name = String.Empty;
            string setting = String.Empty;
            string datatype = String.Empty;

            DataTable dt = Lib.dbService.Actor.LoadActorState(this["id"].ToString());
            if (dt == null)
            {
                return null;
            }
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow datarow in dt.Rows)
                {
                    // Get states from the db
                    name = Convert.ToString(datarow["name"]);
                    setting = Convert.ToString(datarow["setting"]);
                    datatype = Convert.ToString(datarow["datatype"]);

                    switch (datatype)
                    {
                        case "System.Boolean":
                            this[name] = Lib.ConvertToBoolean(setting);
                            break;
                        case "System.Byte":
                            this[name] = Convert.ToByte(setting);
                            break;
                        case "System.Char":
                            this[name] = Convert.ToChar(setting);
                            break;
                        case "System.DateTime":
                            this[name] = Convert.ToDateTime(setting);
                            break;
                        case "System.Decimal":
                            this[name] = Convert.ToDecimal(setting);
                            break;
                        case "System.Double":
                            this[name] = Convert.ToDouble(setting);
                            break;
                        case "System.Int16":
                            this[name] = Convert.ToInt16(setting);
                            break;
                        case "System.Int32":
                            this[name] = Convert.ToInt32(setting);
                            break;
                        case "System.Int64":
                            this[name] = Convert.ToInt64(setting);
                            break;
                        case "System.SByte":
                            this[name] = Convert.ToSByte(setting);
                            break;
                        case "System.Single":
                            this[name] = Convert.ToSingle(setting);
                            break;
                        case "System.String":
                            this[name] = Convert.ToString(setting);
                            break;
                        case "System.UInt16":
                            this[name] = Convert.ToUInt16(setting);
                            break;
                        case "System.UInt32":
                            this[name] = Convert.ToUInt32(setting);
                            break;
                        case "System.UInt64":
                            this[name] = Convert.ToUInt64(setting);
                            break;
                        default:
                            this[name] = Convert.ToString(setting);
                            break;
                    }
                }

                
            }
            return dt;
        }

        /// <summary>
        /// Save all of this actor's states to the database.
        /// </summary>
        public void Savestates()
        {
            string name = String.Empty;
            string setting = String.Empty;
            string datatype = String.Empty;

            // Don't save an actor spell if it hasn't changed since being loaded
            if (this.Version == this.OldVersion)
            {
                return;
            }

            // Set any values you want to reset before saving the actor
            if (this["type"].ToString() == "user" || this["type"].ToString() == "mob")
            {
                this["combatactive"] = false;
            }

            // Important performance step here.
            // Don't save the states unless they've changed.
            // This is especially important on MS Access databases
            // since writes are hundreds of times slower than reads.

            Hashtable testht = new Hashtable();
            DataTable testdt = Lib.dbService.Actor.LoadActorState(this["id"].ToString());
            // Get states from db and load into test hashtable
            foreach (DataRow datarow in testdt.Rows)
            {
                // Get states from the db
                name = Convert.ToString(datarow["name"]);
                setting = Convert.ToString(datarow["setting"]);
                datatype = Convert.ToString(datarow["datatype"]);

                switch (datatype)
                {
                    case "System.Boolean":
                        testht[name] = Lib.ConvertToBoolean(setting);
                        break;
                    case "System.Byte":
                        testht[name] = Convert.ToByte(setting);
                        break;
                    case "System.Char":
                        testht[name] = Convert.ToChar(setting);
                        break;
                    case "System.DateTime":
                        testht[name] = Convert.ToDateTime(setting);
                        break;
                    case "System.Decimal":
                        testht[name] = Convert.ToDecimal(setting);
                        break;
                    case "System.Double":
                        testht[name] = Convert.ToDouble(setting);
                        break;
                    case "System.Int16":
                        testht[name] = Convert.ToInt16(setting);
                        break;
                    case "System.Int32":
                        testht[name] = Convert.ToInt32(setting);
                        break;
                    case "System.Int64":
                        testht[name] = Convert.ToInt64(setting);
                        break;
                    case "System.SByte":
                        testht[name] = Convert.ToSByte(setting);
                        break;
                    case "System.Single":
                        testht[name] = Convert.ToSingle(setting);
                        break;
                    case "System.String":
                        testht[name] = Convert.ToString(setting);
                        break;
                    case "System.UInt16":
                        testht[name] = Convert.ToUInt16(setting);
                        break;
                    case "System.UInt32":
                        testht[name] = Convert.ToUInt32(setting);
                        break;
                    case "System.UInt64":
                        testht[name] = Convert.ToUInt64(setting);
                        break;
                    default:
                        testht[name] = Convert.ToString(setting);
                        break;
                }
            }

            // Protect against saving actors with no shortname and shortnameupper
            if (this["shortname"].ToString() == "")
            {
                this["shortname"] = "placeholder";
                this["shortnameupper"] = Lib.FirstToUpper(this["shortname"].ToString());
            }
            else
                this["shortnameupper"] = Lib.FirstToUpper(this["shortname"].ToString());

            // Now compare testhashtable against actor hashtable
            System.Collections.IEnumerator names = states.Keys.GetEnumerator();
                    
            // TODO occasional Exceptions here about collection "State" was modified
            while (names.MoveNext())
            {
                string chkname = (string)names.Current;

                // Nulls can happen when a new state is added but not saved yet
                if (testht[chkname] != null)
                {
                    if (this[chkname].ToString() != testht[chkname].ToString())
                    {
                        Lib.dbService.Actor.SaveActorState(this,
                            chkname);
                    }
                }
                else
                {
                    // Detected a new state, so add it to the database
                    Lib.dbService.Actor.SaveActorState(this,
                            chkname);
                }
            }
        }

        ///// <summary>
        ///// Save a given actor's states to the database.
        ///// </summary>
        //public void Savestate(string statename)
        //{
        //    System.Collections.IEnumerator names = states.Keys.GetEnumerator();
        //    while (names.MoveNext())
        //    {
        //        string name = (string)names.Current;
        //        if (statename == name)
        //        {
        //            Lib.dbService.Actor.SaveActorState(this,
        //            name);
        //        }
        //    }
        //}

        /// <summary>
        /// Put an item into the Orphanage object when it has an invalid container.
        /// </summary>
        /// <returns></returns>
        public bool Orphan()
        {
            // Find the Orphanage object
            foreach (Actor actor in Lib.actors)
            {
                if (actor["subtype"].ToString() == "orphanage")
                {
                    this["container"] = actor["id"];
                    this["containertype"] = actor["type"].ToString();
                }
            }
            return true;
        }

        public bool PutItem(Actor item, int itemquantity, Actor destination)
        {
            Actor newstack = null;
            bool supressnotification = false;

            // first of all, you cannot do this to other players no matter what
            if (item["type"].ToString() == "user" || item["type"].ToString() == "room" || item["type"].ToString() == "mob")
            {
                this.SendError("You cannot do that to " + item.GetNameFull() + ".\r\n");
                return false;
            }

            if (destination["container"].ToString() == this["id"].ToString())
            {
                // don't notify the room because this was just a move between related containers
                supressnotification = true;
            }

            if (!item["subtype"].ToString().StartsWith("stack") && itemquantity < 2)
            {
                // We're just moving the item with no special conditions.

                // remove item from player
                this.Removeitem(item);
                // Add item to destination
                destination.Additem(item);
                item.Save();
                if (destination["type"].ToString() == "user" || destination["type"].ToString() == "mob")
                {
                    this.Send("You gave " + item.GetNameFull() + " to " + destination.GetNameFull() + ".\r\n");
                    if (!supressnotification) this.Sayinroom(this.GetNameUpper() + " gave " + item.GetNameFull() + " to " + destination.GetNameFull() + ".");
                    return true;
                }
                if (destination["type"].ToString() == "room")
                {
                    this.Send("You dropped " + item.GetNameFull() + " into the room.\r\n");
                    if (!supressnotification) this.Sayinroom(this.GetNameUpper() + " dropped " + item.GetNameFull() + " into the room.");
                    return true;
                }

                this.Send("You put " + item.GetNameFull() + " into " + destination.GetNameFull() + ".\r\n");
                if (!supressnotification) this.Sayinroom(this.GetNameUpper() + " put " + item.GetNameFull() + " into " + destination.GetNameFull() + ".");
                return true;
            }

            // Look for an already existing stack to determine if this is a stack move
            // or a move and merge. 
            Actor scanitem = destination.GetMatchingStack(item);

            if (scanitem != null)
            {
                if (Convert.ToInt32(item["quantity"]) == itemquantity || itemquantity == 0)
                {
                    // This condition is where you have 238 coins and
                    // a user types 'get 238 coins'. An exact match, so move the
                    // whole stack. 

                    // Add items to existing inventory stack
                    scanitem["quantity"] = Convert.ToInt32(scanitem["quantity"]) + Convert.ToInt32(item["quantity"]);
                    // Save the changes to this item
                    scanitem.Save();

                    if (destination["type"].ToString() == "user" || destination["type"].ToString() == "mob")
                    {
                        this.Send("You gave " + item["quantity"] + " " + item.GetNameFull() + "s to " + destination.GetNameFull() + ".\r\n");
                        if (!supressnotification) this.Sayinroom(this.GetNameUpper() + " gave " + item["quantity"] + " " + item.GetNameFull() + "s to " + destination.GetNameFull() + ".");
                        return true;
                    }
                    if (destination["type"].ToString() == "room")
                    {
                        this.Send("You dropped " + item["quantity"] + " " + item["name"] + "s into the room.\r\n");
                        if (!supressnotification) this.Sayinroom(this.GetNameUpper() + " took " + itemquantity + " " + item["name"] + "s from the room.");
                        item.Destroy();
                        return true;
                    }
                    this.Send("You put " + item["quantity"] + " " + item["name"] + "s into " + destination.GetNameFull() + ".\r\n");
                    if (!supressnotification) this.Sayinroom(this.GetNameUpper() + " put " + item["quantity"] + " " + item["name"] + "s into " + destination.GetNameFull() + ".");

                    item.Destroy();
                    return true;
                }

                // Decide if we need the whole stack or just part of it
                if (Convert.ToInt32(item["quantity"]) > itemquantity)
                {
                    // We just need part of the stack.
                    // Add quantity to the inventory stack
                    scanitem["quantity"] = Convert.ToInt32(scanitem["quantity"]) + itemquantity;
                    // remove quantity from stack in the destination
                    item["quantity"] = Convert.ToInt32(item["quantity"]) - itemquantity;
                    // Save the changes to this item
                    scanitem.Save();
                    // Save the changes to this item
                    item.Save();
                    if (destination["type"].ToString() == "user" || destination["type"].ToString() == "mob")
                    {
                        this.Send("You gave " + itemquantity + " " + item.GetNameFull() + "s to " + destination.GetNameFull() + ".\r\n");
                        if (!supressnotification) this.Sayinroom(this.GetNameUpper() + " gave " + itemquantity + " " + item.GetNameFull() + "s to " + destination.GetNameFull() + ".");
                        return true;
                    }
                    if (destination["type"].ToString() == "room")
                    {
                        this.Send("You dropped " + itemquantity + " " + item["name"] + "s into the room.\r\n");
                        if (!supressnotification) this.Sayinroom(this.GetNameFull() + " took " + itemquantity + " " + item["name"] + "s from the room.");
                        return true;
                    }
                    this.Send("You put " + item.GetNameFull() + "s into " + destination.GetNameFull() + ".\r\n");
                    if (!supressnotification) this.Sayinroom(this.GetNameUpper() + " put " + item.GetNameFull() + " into " + destination.GetNameFull() + ".");
                    return true;
                }



                if (Convert.ToInt32(item["quantity"]) < itemquantity)
                {
                    // Here the user asks for a stack of a certain size, but we found a stack smaller
                    // than that, so skip it. We assume there will be another stack of a 
                    // larger size later in the array and we'll catch it. 
                    // Otherwise, we tell the user there wasn't a stack as big as specified.
                }
                else
                {
                    // This condition is where you have any number of coins and
                    // a user types 'get coins'. Not an exact match, but just take
                    // the stack anyways, because you can pickup
                    // stacks without typing the exact number of items.

                    // Add items to existing inventory stack
                    scanitem["quantity"] = Convert.ToInt32(scanitem["quantity"]) + Convert.ToInt32(item["quantity"]);
                    // Save the changes to this item
                    scanitem.Save();
                    // Remove from the destination
                    this.Removeitem(item);

                    if (destination["type"].ToString() == "user" || destination["type"].ToString() == "mob")
                    {
                        this.Send("You gave " + item["quantity"] + " " + item["name"] + "s to " + destination.GetNameFull() + ".\r\n");
                        if (!supressnotification) this.Sayinroom(this.GetNameUpper() + " gave " + item["quantity"] + " " + item["name"] + "s to " + destination.GetNameFull() + ".");
                        return true;
                    }
                    if (destination["type"].ToString() == "room")
                    {
                        this.Send("You dropped " + item["quantity"] + " " + item["name"] + "s into the room.\r\n");
                        if (!supressnotification) this.Sayinroom(this.GetNameFull() + " took " + item["quantity"] + " " + item["name"] + "s from the room.");
                        return true;
                    }
                    this.Send("You put " + item["quantity"] + " " + item["name"] + "s into " + destination.GetNameFull() + ".\r\n");
                    if (!supressnotification) this.Sayinroom(this.GetNameUpper() + " put " + item["quantity"] + " " + item["name"] + "s into " + destination.GetNameFull() + ".");

                    item.Destroy();
                    return true;
                }
            }

            // Didn't find an existing stack to merge with

            // Actor was stackable but never found an existing stack
            // to merge with. So create a new stack in inventory with 
            // the new quantity value, and remove that quantity 
            // from the existing stack.

            if (Convert.ToInt32(item["quantity"]) == itemquantity || itemquantity == 0)
            {
                // This condition is where you have 238 coins and
                // a user types 'get 238 coins'. An exact match, so take the
                // whole stack. 

                // remove item from destination
                this.Removeitem(item);
                // Add item to user inventory in memory
                destination.Additem(item);
                item.Save();
                if (destination["type"].ToString() == "user" || destination["type"].ToString() == "mob")
                {
                    this.Send("You gave " + item["quantity"] + " " + item["name"] + "s to " + destination.GetNameFull() + ".\r\n");
                    if (!supressnotification) this.Sayinroom(this.GetNameUpper() + " gave " + item["quantity"] + " " + item["name"] + "s to " + destination.GetNameFull() + ".");
                    return true;
                }
                if (destination["type"].ToString() == "room")
                {
                    this.Send("You dropped " + item["quantity"] + " " + item["name"] + "s into the room.\r\n");
                    if (!supressnotification) this.Sayinroom(this.GetNameFull() + " took " + item["quantity"] + " " + item["name"] + "s from the room.");
                    return true;
                }
                this.Send("You put " + item.GetNameFull() + "s into " + destination.GetNameFull() + ".\r\n");
                if (!supressnotification) this.Sayinroom(this.GetNameUpper() + " put " + item.GetNameFull() + " into " + destination.GetNameFull() + ".");
                return true;
            }

            // Check if we are taking the whole stack or just part of it.
            // If we are taking whole stack, then we skip down to the else statement.
            if (Convert.ToInt32(item["quantity"]) > itemquantity)
            {
                // We're taking only part of the stack, so create a new stack in inventory.
                newstack = item.Copy();
                // Set quantity to the amount we are taking from the old stack.
                newstack["quantity"] = itemquantity;
                // Add new item to memory
                lock (Lib.actors.SyncRoot)
                {
                    Lib.actors.Add(newstack);
                }
                // Add new stack to destination
                destination.Additem(newstack);
                // Add new stack to the database.
                newstack.Save();
                // Reduce the quantity of the old stack.
                item["quantity"] = Convert.ToInt32(item["quantity"]) - itemquantity;
                // save changes to old stack.
                item.Save();
                if (destination["type"].ToString() == "user" || destination["type"].ToString() == "mob")
                {
                    this.Send("You gave " + itemquantity + " " + item.GetNameFull() + "s to " + destination.GetNameFull() + ".\r\n");
                    if (!supressnotification) this.Sayinroom(this.GetNameUpper() + " gave " + itemquantity + " " + item.GetNameFull() + "s to " + destination.GetNameFull() + ".");
                    return true;
                }
                if (destination["type"].ToString() == "room")
                {
                    this.Send("You dropped " + itemquantity + " " + item["name"] + "s into the room.\r\n");
                    if (!supressnotification) this.Sayinroom(this.GetNameFull() + " took " + itemquantity + " " + item["name"] + "s from the room.");
                    return true;
                }
                this.Send("You put " + item.GetNameFull() + "s into " + destination.GetNameFull() + ".\r\n");
                if (!supressnotification) this.Sayinroom(this.GetNameUpper() + " put " + item.GetNameFull() + " into " + destination.GetNameFull() + ".");
                return true;
            }
            if (Convert.ToInt32(item["quantity"]) < itemquantity)
            {
                // Here the user asks for a stack of a certain size, but we found a stack smaller
                // than that, so skip it. We assume there will be another stack of a 
                // larger size later in the array and we'll catch it. 
                // Otherwise, we tell the user there wasn't a stack as big as specified.
            }


            this.SendError("There was no stack big enough to get that many.\r\n");
            return false;
        }



        public Actor GetMatchingStack(Actor item)
        {
            // Search inventory for matching stacks to merge with
            foreach (Actor scanitem in this.GetContents())
            {
                // Found a stack that is the same as the stack in the room
                if (scanitem["subtype"] == item["subtype"])
                {
                    return scanitem;
                }
            }
            return null;
        }

        public Actor GetMatchingStackInContainer(Actor item, Actor container)
        {
            // Search inventory for matching stacks to merge with
            for (int i = container.GetContents().Count - 1; i >= 0; i--)
            {
                Actor scanitem = container.GetItemAtIndex(i);
                // Found a stack that is the same as the stack in the room
                if (scanitem["subtype"] == item["subtype"])
                {
                    return scanitem;
                }
            }
            return null;
        }

        public bool TakeItem(Actor item, int itemquantity, Actor origin)
        {
            Actor newstack = null;
            bool supressnotification = false;

            // first of all, you cannot do this to other players no matter what
            if (item["type"].ToString() == "user" || item["type"].ToString() == "room" || item["type"].ToString() == "mob")
            {
                this.SendError("You cannot do that to " + item.GetNameFull()+ ".\r\n");
                return false;
            }

            if (origin["container"].ToString() == this["id"].ToString())
            {
                // don't notify the room because this was just a move between related containers
                supressnotification = true;
            }

            // itemquantity=o means take all
            if (!item["subtype"].ToString().StartsWith("stack") && itemquantity < 2)
            {
                // We're just moving the item with no special conditions.

                // remove item from origin
                origin.Removeitem(item);
                // Add item to user inventory in memory
                this.Additem(item);
                item.Save();
                if (origin["type"].ToString() == "room")
                {
                    this.Send("You took " + item.GetNameFull() + " from the room.\r\n");
                    if (!supressnotification) this.Sayinroom(this.GetNameUpper() + " took " + item.GetNameFull() + " from the room.");
                    return true;
                }

                this.Send("You took " + item.GetNameFull() + " from " + origin.GetNameFull() + ".\r\n");
                if (!supressnotification) this.Sayinroom(this.GetNameUpper() + " took " + item.GetNameFull() + " from " + origin.GetNameFull() + ".");
                return true;
            }

            // Look for an already existing stack to determine if this is a stack move
            // or a move and merge. 
            Actor scanitem = GetMatchingStack(item);

            if (scanitem != null)
            {
                // itemquantity zero or match means take all of the item
                if (Convert.ToInt32(item["quantity"]) == itemquantity || itemquantity == 0)
                {
                    // This condition is where you have 238 coins and
                    // a user types 'get 238 coins'. An exact match, so take the
                    // whole stack. 

                    // Add items to existing inventory stack
                    scanitem["quantity"] = Convert.ToInt32(scanitem["quantity"]) + Convert.ToInt32(item["quantity"]);
                    // Save the changes to this item
                    scanitem.Save();
                    // Destroy stack in the origin
                    origin.Removeitem(item);

                    if (origin["type"].ToString() == "room")
                    {
                        this.Send("You took " + item["quantity"] + " " + item["name"] + "s from the room.\r\n");
                        if (!supressnotification) this.Sayinroom(this.GetNameFull() + " took " + item["quantity"] + " " + item["name"] + "s from the room.");
                        return true;
                    }
                    this.Send("You took " + item["quantity"] + " " + item["name"] + "s from " + origin.GetNameFull() + ".\r\n");
                    if (!supressnotification) this.Sayinroom(this.GetNameUpper() + " took " + item["quantity"] + " " + item["name"] + "s from " + origin.GetNameFull() + ".");

                    item.Destroy();
                    return true;
                }
                // Decide if we need the whole stack or just part of it
                if (Convert.ToInt32(item["quantity"]) > itemquantity)
                {
                    // We just need part of the stack.
                    // Add quantity to the inventory stack
                    scanitem["quantity"] = Convert.ToInt32(scanitem["quantity"]) + itemquantity;
                    // remove quantity from stack in the origin
                    item["quantity"] = Convert.ToInt32(item["quantity"]) - itemquantity;
                    // Save the changes to this item
                    scanitem.Save();
                    // Save the changes to this item
                    item.Save();
                    if (origin["type"].ToString() == "room")
                    {
                        this.Send("You took " + itemquantity + " " + item["name"] + "s from the room.\r\n");
                        if (!supressnotification) this.Sayinroom(this.GetNameFull() + " took " + itemquantity + " " + item["name"] + "s from the room.");
                        return true;
                    }
                    this.Send("You took " + item.GetNameFull() + "s from " + origin.GetNameFull() + ".\r\n");
                    if (!supressnotification) this.Sayinroom(this.GetNameUpper() + " took " + item.GetNameFull() + " from " + origin.GetNameFull() + ".");
                    return true;
                }


                if (Convert.ToInt32(item["quantity"]) < itemquantity)
                {
                    // Here the user asks for a stack of a certain size, but we found a stack smaller
                    // than that, so skip it. We assume there will be another stack of a 
                    // larger size later in the array and we'll catch it. 
                    // Otherwise, we tell the user there wasn't a stack as big as specified.
                }
                else
                {
                    // This condition is where you have any number of coins and
                    // a user types 'get coins'. Not an exact match, but just take
                    // the stack anyways, because you can pickup
                    // stacks without typing the exact number of items.

                    // Add items to existing inventory stack
                    scanitem["quantity"] = Convert.ToInt32(scanitem["quantity"]) + Convert.ToInt32(item["quantity"]);
                    // Save the changes to this item
                    scanitem.Save();
                    // Remove from the origin
                    origin.Removeitem(item);

                    if (origin["type"].ToString() == "room")
                    {
                        this.Send("You took " + item["quantity"] + " " + item["name"] + "s from the room.\r\n");
                        if (!supressnotification) this.Sayinroom(this.GetNameFull() + " took " + item["quantity"] + " " + item["name"] + "s from the room.");
                        return true;
                    }
                    this.Send("You took " + item["quantity"] + " " + item["name"] + "s from " + origin.GetNameFull() + ".\r\n");
                    if (!supressnotification) this.Sayinroom(this.GetNameUpper() + " took " + item["quantity"] + item["name"] + "s from " + origin.GetNameFull() + ".");
                    item.Destroy();
                    return true;
                }
            }

            // Didn't find an existing stack to merge with

            // Actor was stackable but never found an existing stack
            // to merge with. So create a new stack in inventory with 
            // the new quantity value, and remove that quantity 
            // from the existing stack.

            if (Convert.ToInt32(item["quantity"]) == itemquantity || itemquantity == 0)
            {
                // This condition is where you have 238 coins and
                // a user types 'get 238 coins'. An exact match, so take the
                // whole stack. 

                // remove item from origin
                origin.Removeitem(item);
                // Add item to user inventory in memory
                this.Additem(item);
                item.Save();
                if (origin["type"].ToString() == "room")
                {
                    this.Send("You took " + item["quantity"] + " " + item["name"] + "s from the room.\r\n");
                    if (!supressnotification) this.Sayinroom(this.GetNameFull() + " took " + item["quantity"] + " " + item["name"] + "s from the room.");
                    return true;
                }
                this.Send("You took " + item["quantity"] + " " + item["name"] + "s from " + origin.GetNameFull() + ".\r\n");
                if (!supressnotification) this.Sayinroom(this.GetNameUpper() + " took " + item["quantity"] + " " + item["name"] + "s from " + origin.GetNameFull() + ".");
                return true;
            }

            // Check if we are taking the whole stack or just part of it.
            // If we are taking whole stack, then we skip down to the else statement.
            if (Convert.ToInt32(item["quantity"]) > itemquantity)
            {
                // We're taking only part of the stack, so create a new stack in inventory.
                newstack = item.Copy();
                // Set quantity to the amount we are taking from the old stack.
                newstack["quantity"] = itemquantity;
                // Add new item to memory
                lock (Lib.actors.SyncRoot)
                {
                    Lib.actors.Add(newstack);
                }
                // Add new stack to inventory in memory.
                this.Additem(newstack);
                // Add new stack to the database.
                newstack.Save();
                // Reduce the quantity of the old stack.
                item["quantity"] = Convert.ToInt32(item["quantity"]) - itemquantity;
                // save changes to old stack.
                item.Save();
                if (origin["type"].ToString() == "room")
                {
                    this.Send("You took " + itemquantity + " " + item["name"] + "s from the room.\r\n");
                    if (!supressnotification) this.Sayinroom(this.GetNameFull() + " took " + itemquantity + " " + item["name"] + "s from the room.");
                    return true;
                }
                this.Send("You took " + item.GetNameFull() + "s from " + origin.GetNameFull() + ".\r\n");
                if (!supressnotification) this.Sayinroom(this.GetNameUpper() + " took " + item.GetNameFull() + " from " + origin.GetNameFull() + ".");
                return true;
            }
            if (Convert.ToInt32(item["quantity"]) < itemquantity)
            {
                // Here the user asks for a stack of a certain size, but we found a stack smaller
                // than that, so skip it. We assume there will be another stack of a 
                // larger size later in the array and we'll catch it. 
                // Otherwise, we tell the user there wasn't a stack as big as specified.
            }


            this.SendError("There was no stack big enough to get that many.\r\n");
            return false;
        }


        /// <summary>
        /// This method returns a new item exactly the same as the current one, but with a new GUID.
        /// This is great for cloning items safely.
        /// </summary>
        /// <returns></returns>
        public Actor Copy()
        {

            // Copy all states from the original actor to the copy actor
            Actor newitem = new Actor();

            // Everything the same, except the new GUID, because item["id"] must be unique.
            newitem["id"] = System.Guid.NewGuid().ToString();

            // Cycle through and display states that the actor has
            System.Collections.IEnumerator names = this.states.Keys.GetEnumerator();

            // Enumerate the original and fill the copy with the values
            // TODO occasional Exceptions here about collection "State" was modified
            while (names.MoveNext())
            {
                string name = names.Current.ToString();

                // Nulls can happen when a new state is added but not saved yet
                if (newitem[name] == null)
                {
                    newitem[name] = this[name];

                }
                if (newitem[name].ToString() !=this[name].ToString() )
                {
                    newitem[name] = this[name];
                }
            }

            // Don't copy items and exits to avoid dupes
            //newitem.Exits = this.Exits;
            //newitem.items = this.items;
           
            return newitem;
        }


        public bool Killable
        {
            get
            {
                if (this["type"].ToString() == "user" || this["type"].ToString() == "mob")
                    return true;
                else
                    return false;
            }
        }


        public void Kudosadd(int kudos)
        {
            this["reputation"] = Convert.ToInt32(this["reputation"]) + kudos;

        }

        public void Kudosremove(int kudos)
        {
            this["kudostogive"] = Convert.ToInt32(this["kudostogive"]) - kudos;
        }



      

        public bool SendAlertBad(string message)
        {
            this.Send(this["coloralertbad"] + message);
            return true;
        }

        public bool SendAlertGood(string message)
        {
            this.Send(this["coloralertgood"] + message);
            return true;
        }

        public bool SendAnnouncement(string message)
        {
            this.Send(this["colorannouncement"] + message);
            return true;
        }

        public bool SendPrompt(string message)
        {
            this.Send(this["colorcommandprompt"] + message);
            return true;
        }

        public bool SendCommand(string message)
        {
            this.Send(this["colorcommandtext"] + message);
            return true;
        }

        public bool SendError(string message)
        {
            this.Send(this["colorerrors"] + message);
            return true;
        }

        public bool SendExit(string message)
        {
            this.Send(this["colorexits"] + message);
            return true;
        }

        public bool SendItem(string message)
        {
            this.Send(this["coloritems"] + message);
            return true;
        }

        public bool SendMobs(string message)
        {
            this.Send(this["colormobs"] + message);
            return true;
        }

        public bool SendPeople(string message)
        {
            this.Send(this["colorpeople"] + message);
            return true;
        }

        public bool SendRoomDescription(string message)
        {
            this.Send(this["colorroomdescr"] + message);
            return true;
        }

        public bool SendRoomName(string message)
        {
            this.Send(this["colorroomname"] + message);
            return true;
        }

        public bool SendSystemMessage(string message)
        {
            this.Send(this["colorsystemmessage"] + message);
            return true;
        }

        /// <summary>
        /// Catch-all for when a Send command only has a hashtable value, which is an object not string.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool Send(object message)
        {

            bool retval = Send(message.ToString());
            return retval;
        }


        /// <summary>
        /// This is the main send function for sending text to players 
        /// </summary>
        /// <remarks>
        /// Important note: MORE works only on individual sends that exceed the max lines per page.
        /// It word wraps and offers the MORE command for displaying multiple pages of text.
        /// For example, you could invoke a hundred sends individually, but if the individual sends don't 
        /// exceeed maximum lines per page, then you would never see the player asked to type 'MORE'.
        /// </remarks>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool Send(string message)
        {
            if (this["connected"] != null)
            {
                if (Lib.ConvertToBoolean(this["connected"])==false || userSocket.Connected==false)
                {
                    return false;
                }
            }
            // Always add colormessage to the front and colorcommandtext to the end of all strings
            // This ensures a string will be colored correctly and leave behind the correct color
            // no matter what color codes the coder may have forgotten to tag them with.
            // But if a coder specifies colors, then these colors will be trumped by the specified ones.
            message = this["colormessages"] + message + this["colorcommandtext"];

            message = Lib.Colorize(message); // See if there are any color codes in the string

            // The flag defaults to word wrap, so these checks are to find reasons why we shouldn't
            // At the end, the flag should be set the way we want.
            if (Lib.ConvertToBoolean(this["wordwrap"]))
            {
                // The message has to be at least this["screenwidth"]+1 characters before we care about wrapping
                message = Lib.WordWrap(message, Convert.ToInt32(this["screenwidth"]));
            }

            if (Lib.ConvertToBoolean(this["more"]))
            {
                int linesperpage = 21;
                string morebuffer = "";
                // Can we skip the 'more' code?
                if (Lib.CountReturns(message) > linesperpage - 1)
                {
                    message = Lib.More(message, ref morebuffer, Convert.ToInt32(this["screenwidth"]));
                    this["morebuffer"] = morebuffer;
                }

            }
            try
            {
                if (this.UserSocket != null && this.UserSocket.Connected)
                {
                    this.UserSocket.Send(message);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Lib.log.Add("User.Send", DateTime.Now + " EXCEPTION " + ex.Message + ex.StackTrace);
                return true;
            }
            return false;
        }



        /// <summary>
        /// Actor die routine.
        /// </summary>
        /// <param name="actor"></param>
        public void UserDie(Actor actor)
        {
            if (this["type"].ToString() != "user" && this["type"].ToString() != "mob")
            {
                return;
            }


            // actor is the killer in this case
            // clear combat variables
            this["target"] = "";
            this["targettype"] = "";

            this.ClearTemporaryEffects();

            this["deathticks"] = Lib.GetTime();
            this.SendError("You died.\r\n");
            this.Sayinroom(this["name"] + " falls dead!");

            if (actor != null)
            {
                actor.Send("You killed " + this["name"] + "!\r\n");

                if (actor["type"].ToString() == "user")
                {
                    // Announce PvP kills across the whole world
                    this.Sayinworld(actor["name"] + " killed " + this["name"] + "!\r\n", actor);
                }
                else
                    // regular mob kills are just announced in the current room
                    this.Sayinroom(actor["name"] + " killed " + this["name"] + "!\r\n", actor);
            }
            else
            {
                // Attacker was null. Unusual occurrence, but let's handle correctly.
                this.Sayinworld("Someone killed " + this["name"] + "!\r\n", actor);
            }

            this.Send("Resurrecting you at a graveyard...\r\n");
            Actor respawnlocation = Lib.GetSpecialLocation("graveyard", actor["zone"].ToString());

            Actor room = Lib.GetByID(actor["container"].ToString());

            // Remove user from current room
            room.Removeitem(this);

            // Add user to new room
            respawnlocation.Additem(this);

            this["container"] = respawnlocation["id"];
            this["containertype"] = respawnlocation["type"].ToString();

            // Send 'he arrived' messages
            this.Sayinroom(this.GetNameUpper() + " appears, having been reanimated from death.");
            this["health"] = 1;
            this.Save();
            this.Showroom();
            this.Showprompt();
        }

        public bool CombatStop()
        {
            this["combatactive"] = false;
            this["target"] = "";
            this["targettype"] = "";
            return true;
        }

        /// <summary>
        /// Actor combat
        /// </summary>
        /// <returns></returns>
        public bool UserCombat()
        {
            // check if in combat
            if (!Lib.ConvertToBoolean(this["combatactive"]))
            {
                return false;
            }
            // Is user alive?
            if (Convert.ToInt32(this["health"]) < 1)
            {
                // clear combat variables
                this.CombatStop();
                return false;
            }
            // check if you have a target
            if (this["target"].ToString() == "")
            {
                return false;
            }

            Actor targetuser = null;
            
            // Target must be a user then
            targetuser = Lib.GetByID(this["target"].ToString());

            // Ensure target is online
            if (targetuser["type"].ToString()=="user" && !Lib.ConvertToBoolean(targetuser["connected"]))
            {
                this.CombatStop();
                return false;
            }
            // Ensure target is in the room
            if (targetuser["container"].ToString() != this["container"].ToString())
            {
                this.CombatStop();
                return false;
            }
            // Don't attack corpses
            if (Convert.ToInt32(targetuser["health"]) < 1)
            {
                this.CombatStop();
                return false;
            }

            // Skip player's combat turn if in the middle of casting a spell
            if (this["spellpending"].ToString() != "")
            {
                return false;
            }

            // Don't hit yourself!
            if (this["target"].ToString() == this["id"].ToString())
            {
                this.SendAlertBad("YOU ARE TARGETING YOURSELF.\r\n");
                return false;
            }

            Actor weapon1 = this.GetWeaponPrimary();
            Actor weapon2 = this.GetWeaponSecondary();

            if (weapon1 == null && weapon2 == null)
            {
                // No combat because the user has no weapons equipped
                this.SendError("You have no weapon equipped to fight with.\r\n");
                return false;
            }

            long weaponspeedmodifier = Convert.ToInt32(((Convert.ToInt32(this["agility"]) * .6) / 100) - 1);
            long weapon1effectivespeed = 0;
            long weapon1effectivespeedticks = 0;
            long weapon2effectivespeed = 0;
            long weapon2effectivespeedticks = 0;
            long slowestweaponticks = 0;
            long weapon1dmg = 0;
            long weapon2dmg = 0;
            bool weapon1hit = false;
            bool weapon2hit = false;
            if (weapon1 != null)
            {
                weapon1effectivespeed = Convert.ToInt32(weapon1["speed"]) - (Convert.ToInt32(weapon1["speed"]) * weaponspeedmodifier);
                if (weapon1effectivespeed < 1) weapon1effectivespeed = 1;
                weapon1effectivespeedticks = weapon1effectivespeed * 12;
            }
            if (weapon2 != null)
            {
                weapon2effectivespeed = Convert.ToInt32(weapon2["speed"]) - (Convert.ToInt32(weapon2["speed"]) * weaponspeedmodifier);
                if (weapon2effectivespeed < 1) weapon2effectivespeed = 1;
                weapon2effectivespeedticks = weapon2effectivespeed * 12;
            }

            // Determine slowest weapon and use it for timing
            slowestweaponticks = Math.Max(weapon1effectivespeedticks, weapon2effectivespeedticks);

            // Check if time to strike for each weapon
            //Lib.PrintLine("Hit when " + Lib.GetTime() + " is greater than " + (Convert.ToInt64(this["lastattack"]) + slowestweaponticks) + ".");
            if (Lib.GetTime() > (Convert.ToInt64(this["lastattack"]) + slowestweaponticks))
            {
                this["lastattack"] = Lib.GetTime();
                 
                Randomizer rand = new Randomizer();

                weapon1hit = this.CheckHit(targetuser);

                // If either weapon hit
                if (weapon1hit || weapon2hit)
                {
                    if (weapon1 != null)
                    {
                        // Calculate damage done
                        weapon1dmg = rand.getrandomnumber(Convert.ToInt32(weapon1["damagemin"]), Convert.ToInt32(weapon1["damagemax"]));
                        // If target is still in the room and alive
                        if (targetuser["container"].ToString() == this["container"].ToString() && Convert.ToInt32(targetuser["health"]) > 0)
                        {
                            // if target is a mob, add player damage to mob hate level
                            if (targetuser["type"].ToString() == "mob")
                            {
                                Actor.HateTarget hateplayer = new Actor.HateTarget();
                                hateplayer.actor = this;
                                (targetuser).hatetable.IncrementHate(hateplayer, Convert.ToInt32(weapon1dmg));
                            }
                            this.Send(this["coloritems"] + "You hit " + targetuser["name"] + " for " + weapon1dmg + " damage!\r\n");
                            targetuser.SendError(this["shortnameupper"] + "'s " + weapon1["name"] + " hits you! ");
                            if (targetuser["type"].ToString() == "user")
                            {
                                this.Sayinroom(this["shortnameupper"] + "'s " + weapon1["name"] + " hits " + targetuser["name"] + "!", targetuser);
                            }
                            else
                                this.Sayinroom(this["shortnameupper"] + "'s " + weapon1["name"] + " hits " + targetuser["name"] + "!");

                            targetuser.RemoveHealth(this, Convert.ToInt32(weapon1dmg), 0);
                            if (Convert.ToInt32(targetuser["health"]) < 1)
                            {
                                this.CombatStop();
                                return false;
                            }
                        }
                        else
                        {
                            this.CombatStop();
                            return false;
                        }
                    }
                    if (weapon2 != null)
                    {
                        // Calculate damage done
                        weapon2dmg = rand.getrandomnumber(Convert.ToInt32(weapon2["damagemin"]), Convert.ToInt32(weapon2["damagemax"]));
                        // If target is still in the room
                        if (targetuser["container"].ToString() == this["container"].ToString() && Convert.ToInt32(targetuser["health"]) > 0)
                        {
                            // if target is a mob, add player damage to mob hate level
                            if (targetuser["type"].ToString() == "mob")
                            {
                                Actor.HateTarget hateplayer = new Actor.HateTarget();
                                hateplayer.actor = this;
                                //player.hate=weapon1dmg;
                                ((Actor)targetuser).hatetable.IncrementHate(hateplayer, Convert.ToInt32(weapon2dmg));
                            }

                            this.Send(this["coloritems"] + "You hit " + targetuser["name"] + " for " + weapon2dmg + " damage!\r\n");
                            targetuser.SendError(this["shortnameupper"] + "'s " + weapon2["name"] + " hits you! ");
                            this.Sayinroom(this["shortnameupper"] + "'s " + weapon2["name"] + " hits " + targetuser["name"] + "!");
                            targetuser.RemoveHealth(this, Convert.ToInt32(weapon2dmg), 0);
                            if (Convert.ToInt32(targetuser["health"]) < 1)
                            {
                                this.CombatStop();
                                return false;
                            }
                        }
                        else
                        {
                            this.CombatStop();
                            return false;
                        }
                    }
                }
                else
                {
                    this.Send("You miss " + targetuser["name"] + ".\r\n");
                    targetuser.Send(this["shortnameupper"] + "'s " + weapon1["name"] + " misses you!\r\n");
                    if (targetuser["type"].ToString() == "user")
                    {
                        this.Sayinroom(this["shortnameupper"] + "'s " + weapon1["name"] + " misses " + targetuser["name"] + "!", targetuser);
                    }
                    else
                        this.Sayinroom(this["shortnameupper"] + "'s " + weapon1["name"] + " misses " + targetuser["name"] + "!");
                }
                if (Convert.ToInt32(targetuser["health"]) < 1 || targetuser["container"].ToString() != this["container"].ToString())
                {
                    this.CombatStop();
                    return false;
                }
            }
            return true;
        }

        public bool CheckHit(Actor targetuser)
        {
            Randomizer rand = new Randomizer();
            // Calculate chance to hit
            if (Convert.ToInt32(this["agility"]) > Convert.ToInt32(targetuser["agility"]))
            {
                // Attacker agility beats target
                long hit = rand.getrandomnumber(1, Convert.ToInt32(this["agility"]));
                if (hit < Convert.ToInt32(targetuser["agility"]))
                {
                    // HIT!
                    return true;
                }
            }
            if (Convert.ToInt32(this["agility"]) < Convert.ToInt32(targetuser["agility"]))
            {
                // Target agility beats attacker
                long hit = rand.getrandomnumber(1, Convert.ToInt32(targetuser["agility"]));
                if (hit < Convert.ToInt32(this["agility"]))
                {
                    // HIT!
                    return true;
                }
            }
            if (Convert.ToInt32(this["agility"]) == Convert.ToInt32(targetuser["agility"]))
            {
                // Even agility match
                long hit = rand.getrandomnumber(1, 100);
                if (hit <= 50)
                {
                    // HIT!
                    return true;
                }
            }
            return false;
        }


        // Show the prompt to the user
        public void Showprompt()
        {
            this.Send(Lib.Ansireset + this["colorcommandprompt"] + "(" + Lib.Gametime.ToString("h:mmt") + " " + Convert.ToInt32(this["health"]) + "h " + this["mana"] + "m) Command: " + this["colorcommandtext"]);
        }

        /// <summary>
        /// Run a command given it's name
        /// </summary>
        /// <returns></returns>
        public bool RunCommand(string commandname, string arguments)
        {
            // Call the target command for code reuse
            bool result = false;
            ICommand launchcommand = Lib.GetCommandByWord(Convert.ToInt32(this["accesslevel"]), commandname);
            if (launchcommand != null)
            {
                result = launchcommand.DoCommand(this, launchcommand.Name, arguments);
            }
            return result;
        }


        public void Showroom()
        {
            Actor room;
            object chkobject = new object();
            ArrayList chkusers = new ArrayList();
            ArrayList chkmobs = new ArrayList();
            Actor chkmob = new Actor();
            Actor chkuser = new Actor();

            lock (Lib.actors.SyncRoot)
            {
                room = this.GetContainer();
            }

            this.Send("\r\n" + this["colorroomname"] + room["name"] + Lib.Ansireset + "\r\n" + this["colorroomdescr"] + room["descr"] + "\r\n");

            string exitstring = "";
            if (room.Exits.Count < 1)
            {
                exitstring = " None";
            }
            else
            {
                foreach (DictionaryEntry exit in room.Exits)
                {
                    exitstring += " " + exit.Key;
                }
            }

            this.Send(this["colorroomdescr"] + "Exits:" + this["colorexits"] + exitstring + "\r\n");

            // Get room contents and start filtering, exclude self in list
            // Get sorted list to start with
            ArrayList contents = room.GetContents(this);
            ArrayList mobs = new ArrayList();
            ArrayList people = new ArrayList();
            ArrayList items = new ArrayList();

            // Run through contents and put into buckets for display
            foreach (Actor actor in contents)
            {
                switch (actor["type"].ToString())
                {
                    case "mob":
                        mobs.Add(actor);
                        break;
                    case "user":
                        people.Add(actor);
                        break;
                    default:
                        items.Add(actor);
                        break;
                }
            }
            // Send items
            this.Send(this["coloritems"] + Lib.FormatContentsIntoSentence(items) + "\r\n");
            // Send mobs
            this.Send(this["colormobs"] + Lib.FormatContentsIntoSentence(mobs) + "\r\n");
            // Send users
            this.Send(this["colorpeople"] + Lib.FormatContentsIntoSentence(people) + "\r\n");
        }


        /// <summary>
        /// Completely save all of an actor's data
        /// </summary>
        public void Save()
        {
            try
            {
                // Don't forget to save actor states
                this.Savestates();
            }
            catch (Exception ex)
            {
                Lib.PrintLine("Saving user to the database failed. The error was: " + ex.Message + ex.StackTrace);
                throw;
            }
        }


        /// <summary>
        /// The user's IP Address.
        /// </summary>
        public string UserIPAddress
        {
            get
            {
                if (userSocket != null)
                {
                    return userSocket.ClientEndpointId;
                }
                else
                    return "null";
                // return ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
            }
        }

        // Actor methods are here
        public string Getsockethandle()
        {
            return this.UserSocket.UniqueId;
        }
        public void Addbufferitem(Actor item)
        {
            bufferitems.Add(item);
        }
        public void Clearbufferitems()
        {
            bufferitems.Clear();
        }
        public void Removebufferitem(Actor item)
        {
            bufferitems.Remove(item);
        }
        public void Removebufferitemat(int index)
        {
            bufferitems.RemoveAt(index);
        }
        public Actor Getbufferitemat(int index)
        {
            return (Actor)bufferitems[index];
        }
        public ArrayList Getbufferitems()
        {
            return bufferitems;
        }

        public Actor Tradetargetuser
        {
            get { return tradetargetuser; }
            set { tradetargetuser = value; }
        }

        public string DisplayDelayedCommands()
        {
            return Lib.Delayedcommands.Display();

        }

        public IUserSocket UserSocket
        {
            get { return userSocket; }
            set { userSocket = value; }
        }

        //public bool CombatActive
        //{
        //    get { return incombat; }
        //    set { incombat = value; }
        //}


        /// <summary>
        /// Hate table for mob combat AI.
        /// Controls which enemy the mob is targetting at the time.
        /// </summary>
        public struct HateTarget
        {
            public Actor actor; // A target player or mob
            public int hate; // Numeric amount that the mob hates the target
        }

        public Actor GetWeaponPrimary()
        {
            foreach (Actor item in this.items)
            {
                if (item["equipslot"] != null && item["equipslot"].ToString() == "weapon1")
                {
                    // Is equipped?
                    if (Lib.ConvertToBoolean(item["equipped"]))
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public Actor GetWeaponSecondary()
        {
            foreach (Actor item in this.items)
            {
                if (item["equipslot"] != null && item["equipslot"].ToString() == "weapon2")
                {
                    // Is equipped?
                    if (Lib.ConvertToBoolean(item["equipped"]))
                    {
                        return item;
                    }
                }
            }
            return null;
        }


        public bool ProcessAction(Actor actor, string action, string arguments)
        {
            IAction tempaction = this.GetActionByWord(action);
            if (tempaction != null)
            {
                return tempaction.DoAction(actor, this, action, arguments);
            }
            else
            {
                return false;
            }
        }


        public virtual void Showskills()
        {
            this.Send(this["colormessages"] + "You have the following skills:\r\n");

            // Cycle through and display skills the user has
            System.Collections.IEnumerator names = this.states.Keys.GetEnumerator();
            while (names.MoveNext())
            {
                string name = (string)names.Current;
                if (name.StartsWith("skillknown_"))
                {
                    string text = name.Replace("skillknown_", "");
                    this.Send(text.Substring(0, 1).ToUpper() + text.Substring(1) + " - " + this[name] + "\r\n");
                }
            }
            return;
        }


        public virtual void Showspells()
        {
            this.Send("You know the following spells:\r\n");
            // Cycle through and display spells the user has
            System.Collections.IEnumerator names = this.states.Keys.GetEnumerator();
            names.Reset();
            while (names.MoveNext())
            {
                string name = (string)names.Current;
                if (name.StartsWith("spellknown_"))
                {
                    string text = name.Replace("spellknown_", "");
                    Spell spell = Lib.GetSpellByName(text);
                    if (spell != null)
                    {
                        this.Send(spell["name"] + " (" + text + ") - " + this[name] + "\r\n");
                    }
                }
            }
            return;
        }




        public virtual bool Alive()
        {
            if ((Convert.ToInt32(this["health"]) <= 0))
            {
                return false;
            }
            return true;
        }


        public virtual void AddHealth(Actor actor, int amount, float duration)
        {
            this["health"] = Convert.ToInt32(this["health"]) + amount;
            this.SendAlertGood("You gained " + amount + " health!\r\n");
            if (Convert.ToInt32(this["health"]) > Convert.ToInt32(this["healthmax"]))
            {
                this["health"] = Convert.ToInt32(this["healthmax"]);
            }
        }

        public virtual void AddMana(Actor actor, int amount, float duration)
        {
            this["mana"] = Convert.ToInt32(this["mana"]) + amount;
            this.SendAlertGood("You gained " + amount + " mana!\r\n");
            if (Convert.ToInt32(this["mana"]) > Convert.ToInt32(this["manamax"]))
            {
                this["mana"] = this["manamax"];
            }
        }

        public virtual void AddStamina(Actor actor, int amount, float duration)
        {
            this["stamina"] = Convert.ToInt32(this["stamina"]) + amount;
            this.SendAlertGood("You gained " + amount + " stamina!\r\n");
            if (Convert.ToInt32(this["stamina"]) > Convert.ToInt32(this["staminamax"]))
            {
                this["stamina"] = this["staminamax"];
            }
        }
        
        public virtual void AddHealthMax(Actor actor, int amount, float duration)
        {
            // store old stat value and the expiration time of the buff
            this["buffhealthmaxduration"] = Convert.ToInt64(Lib.GetExpirationTicks(duration));
            this["buffhealthmax"] = Convert.ToInt32(amount);
            this["healthmax"] = Convert.ToInt32(this["healthmax"]) + amount;
            this.SendAlertGood("Your maximum health has increased by " + amount + " points!\r\n");
        }

        public virtual void AddManaMax(Actor actor, int amount, float duration)
        {
            // store old stat value and the expiration time of the buff
            this["buffmanamaxduration"] = Convert.ToInt64(Lib.GetExpirationTicks(duration));
            this["buffmanamax"] = Convert.ToInt32(amount);
            this["manamax"] = Convert.ToInt32(this["manamax"]) + amount;
            this.SendAlertGood("Your maximum mana has increased by " + amount + " points!\r\n");

        }

        public virtual void AddStaminaMax(Actor actor, int amount, float duration)
        {

            // store old stat value and the expiration time of the buff
            this["buffstaminamaxduration"] = Convert.ToInt64(Lib.GetExpirationTicks(duration));
            this["buffstaminamax"] = Convert.ToInt32(amount);
            this["staminamax"] = Convert.ToInt32(this["staminamax"]) + amount;
            this.SendAlertGood("Your maximum stamina has increased by " + amount + " points!\r\n");

        }

        public virtual void AddStrength(Actor actor, int amount, float duration)
        {

            // store old stat value and the expiration time of the buff
            this["buffstrengthduration"] = Convert.ToInt64(Lib.GetExpirationTicks(duration));
            this["buffstrength"] = Convert.ToInt32(amount);
            this["strength"] = Convert.ToInt32(this["strength"]) + amount;
            this.SendAlertGood("Your strength has increased by " + amount + " points!\r\n");
        }

        public virtual void AddIntellect(Actor actor, int amount, float duration)
        {

            // store old stat value and the expiration time of the buff
            this["buffintellectduration"] = Convert.ToInt64(Lib.GetExpirationTicks(duration));
            this["buffintellect"] = Convert.ToInt32(amount);
            this["intellect"] = Convert.ToInt32(this["intellect"]) + amount;
            this.SendAlertGood("Your intellect has increased by " + amount + " points!\r\n");
        }

        public virtual void AddSpirit(Actor actor, int amount, float duration)
        {

            // store old stat value and the expiration time of the buff
            this["buffspiritduration"] = Convert.ToInt64(Lib.GetExpirationTicks(duration));
            this["buffspirit"] = Convert.ToInt32(amount);
            this["spirit"] = Convert.ToInt32(this["spirit"]) + amount;
            this.SendAlertGood("Your spirit has increased by " + amount + " points!\r\n");
        }

        public virtual void AddAgility(Actor actor, int amount, float duration)
        {

            // store old stat value and the expiration time of the buff
            this["buffagilityduration"] = Convert.ToInt64(Lib.GetExpirationTicks(duration));
            this["buffagility"] = Convert.ToInt32(amount);
            this["agility"] = Convert.ToInt32(this["agility"]) + amount;
            this.SendAlertGood("Your agility has increased by " + amount + " points!\r\n");
        }



        public virtual void RemoveHealth(Actor actor, int amount, float duration)
        {
            this["health"] = Convert.ToInt32(this["health"]) - amount;
            this.SendError("You lost " + amount + " health!\r\n");
            if (Convert.ToInt32(this["health"]) <= 0)
            {
                this["health"] = 0;
                this.Die(actor);
            }
        }

        public virtual void RemoveMana(Actor actor, int amount, float duration)
        {
            this["mana"] = Convert.ToInt32(this["mana"]) - amount;
            this.SendError("You lost " + amount + " mana!\r\n");
            if (Convert.ToInt32(this["mana"]) < 0)
            {
                this["mana"] = 0;
            }
        }

        public virtual void RemoveStamina(Actor actor, int amount, float duration)
        {
            this["stamina"] = Convert.ToInt32(this["stamina"]) - amount;
            this.SendError("You lost " + amount + " stamina!\r\n");
            if (Convert.ToInt32(this["stamina"]) < 0)
            {
                this["stamina"] = 0;
            }
        }

        public virtual void RemoveHealthMax(Actor actor, int amount, float duration)
        {
            // store old stat value and the expiration time of the buff
            this["debuffhealthmaxduration"] = Convert.ToInt64(Lib.GetExpirationTicks(duration));
            this["debuffhealthmax"] = Convert.ToInt32(amount);
            this["healthmax"] = Convert.ToInt32(this["healthmax"]) - amount;
            this.SendError("Your maximum health has lowered by " + amount + " points!\r\n");
            if (Convert.ToInt32(this["health"]) > Convert.ToInt32(this["healthmax"]))
            {
                this["health"] = Convert.ToInt32(this["healthmax"]);
            }

        }

        public virtual void RemoveManaMax(Actor actor, int amount, float duration)
        {
            // store old stat value and the expiration time of the buff
            this["debuffmanamaxduration"] = Convert.ToInt64(Lib.GetExpirationTicks(duration));
            this["debuffmanamax"] = Convert.ToInt32(amount);
            this["manamax"] = Convert.ToInt32(this["manamax"]) - amount;
            this.SendError("Your maximum mana has lowered by " + amount + " points!\r\n");
            if (Convert.ToInt32(this["mana"]) > Convert.ToInt32(this["manamax"]))
            {
                this["mana"] = this["manamax"];
            }

        }

        public virtual void RemoveStaminaMax(Actor actor, int amount, float duration)
        {

            // store old stat value and the expiration time of the buff
            this["debuffstaminamaxduration"] = Convert.ToInt64(Lib.GetExpirationTicks(duration));
            this["debuffstaminamax"] = Convert.ToInt32(amount);
            this["staminamax"] = Convert.ToInt32(this["staminamax"]) - amount;
            this.SendError("Your maximum stamina has lowered by " + amount + " points!\r\n");
            if (Convert.ToInt32(this["stamina"]) > Convert.ToInt32(this["staminamax"]))
            {
                this["stamina"] = this["staminamax"];
            }

        }

        public virtual void RemoveStrength(Actor actor, int amount, float duration)
        {

            // store old stat value and the expiration time of the buff
            this["debuffstrengthduration"] = Convert.ToInt64(Lib.GetExpirationTicks(duration));
            this["debuffstrength"] = Convert.ToInt32(amount);
            this["strength"] = Convert.ToInt32(this["strength"]) - amount;
            this.SendError("Your strength has lowered by " + amount + " points!\r\n");
        }

        public virtual void RemoveIntellect(Actor actor, int amount, float duration)
        {

            // store old stat value and the expiration time of the buff
            this["debuffintellectduration"] = Convert.ToInt64(Lib.GetExpirationTicks(duration));
            this["debuffintellect"] = Convert.ToInt32(amount);
            this["intellect"] = Convert.ToInt32(this["intellect"]) - amount;
            this.SendError("Your intellect has lowered by " + amount + " points!\r\n");
        }

        public virtual void RemoveSpirit(Actor actor, int amount, float duration)
        {

            // store old stat value and the expiration time of the buff
            this["debuffspiritduration"] = Convert.ToInt64(Lib.GetExpirationTicks(duration));
            this["debuffspirit"] = Convert.ToInt32(amount);
            this["spirit"] = Convert.ToInt32(this["spirit"]) - amount;
            this.SendError("Your spirit has lowered by " + amount + " points!\r\n");
        }

        public virtual void RemoveAgility(Actor actor, int amount, float duration)
        {

            // store old stat value and the expiration time of the buff
            this["debuffagilityduration"] = Convert.ToInt64(Lib.GetExpirationTicks(duration));
            this["debuffagility"] = Convert.ToInt32(amount);
            this["agility"] = Convert.ToInt32(this["agility"]) - amount;
            this.SendError("Your agility has lowered by " + amount + " points!\r\n");
        }

        public bool IsKnownSpell(Spell spell)
        {
            for (int i = this.Spells.Count - 1; i >= 0; i--)
            {
                System.Collections.IEnumerator names = this.states.Keys.GetEnumerator();
                while (names.MoveNext())
                {
                    string name = (string)names.Current;

                    // found a known spell
                    if (name.StartsWith("spellknown_"))
                    {
                        string spellname = name.Replace("spellknown_", "");

                        // is it the spell they are trying to cast?
                        if (spellname == spell["shortname"].ToString())
                        {
                            return true;
                        }
                    }
                }
            }
            // didn't know that spell
            return false;
        }

        public bool HasSkill(string skillname)
        {
            // cycle through states to find a given skill name
            for (int i = this.states.Count - 1; i >= 0; i--)
            {
                System.Collections.IEnumerator names = this.states.Keys.GetEnumerator();
                while (names.MoveNext())
                {
                    string name = (string)names.Current;
                    // found a known spell
                    if (name == skillname)
                    {
                        return true;
                    }
                }
            }
            // didn't have the skill
            return false;
        }

        public void ClearTarget()
        {
            this["target"] = "";
            this["targettype"] = "";
            return;
        }


        /// <summary>
        /// Clears all pending spell states, for example upon death.
        /// </summary>
        /// <returns>Nothing</returns>
        public void ClearTemporaryEffects()
        {
            this.states.Remove("healthpoisondamage");
            this.states.Remove("healthpoisoncaster");

            this.states.Remove("staminapoisondamage");
            this.states.Remove("staminapoisoncaster");

            this.states.Remove("manapoisondamage");
            this.states.Remove("manapoisoncaster");

            this.states.Remove("buffhealthmax");
            this.states.Remove("buffhealthmaxcaster");

            this.states.Remove("debuffhealthmax");
            this.states.Remove("debuffhealthmaxcaster");

            this.states.Remove("buffstaminamax");
            this.states.Remove("buffstaminamaxcaster");

            this.states.Remove("debuffstaminamax");
            this.states.Remove("debuffstaminamaxcaster");

            this.states.Remove("buffmanamax");
            this.states.Remove("buffmanamaxcaster");

            this.states.Remove("debuffmanamax");
            this.states.Remove("debuffmanamaxcaster");

            this.states.Remove("buffstrength");
            this.states.Remove("buffstrengthcaster");

            this.states.Remove("debuffstrength");
            this.states.Remove("debuffstrengthcaster");

            this.states.Remove("buffagility");
            this.states.Remove("buffagilitycaster");

            this.states.Remove("debuffagility");
            this.states.Remove("debuffagilitycaster");

            this.states.Remove("buffspirit");
            this.states.Remove("buffspiritcaster");

            this.states.Remove("debuffspirit");
            this.states.Remove("debuffspiritcaster");

            this.states.Remove("buffintellect");
            this.states.Remove("buffintellectcaster");

            this.states.Remove("debuffintellect");
            this.states.Remove("debuffintellectcaster");

            return;
        }

        /// <summary>
        /// Triggers spells and effects that run several times over the course of thier duration
        /// </summary>
        /// <returns>Nothing</returns>
        public void CycleStates()
        {
            if (this["spellpending"] != null)
            {
                // First, run any spells
                if (this["spellpending"].ToString() != "")
                {
                    if (Lib.GetTime() >= Convert.ToInt64(this["spellwindupticks"]))
                    {
                        // Get pending spell
                        Spell spell = Spell.GetByID(this["spellpending"].ToString());
                        Actor target = null;
                        target = Lib.GetByID(this["target"].ToString());
                        spell.Cast(this, target);
                        this["spellpending"] = "";
                    }
                }
            }

            if (this["healthpoisondamage"] != null)
            {
                if (Lib.GetTime() >= Convert.ToInt64(this["healthpoisonendticks"]))
                {
                    this.states.Remove("healthpoisondamage");
                }
                else if (Lib.GetTime() >= (Convert.ToInt64(this["healthpoisonlastticks"]) + Convert.ToInt64(this["healthpoisonfrequency"])))
                {
                    // Check if it is time to trigger the state based on its frequency
                    // Where current time > lasttrigger+frequency
                    // If it's time, so run the effect and update last trigger time
                    this.RemoveHealth(Lib.GetByID(this["healthpoisoncaster"].ToString()), Convert.ToInt32(this["healthpoisondamage"]), 0);
                    this["healthpoisonlastticks"] = Convert.ToString(Lib.GetTime());
                }
            }

            if (this["staminapoisondamage"] != null)
            {
                if (Lib.GetTime() >= Convert.ToInt64(this["staminapoisonendticks"]))
                {
                    this.states.Remove("staminapoisondamage");
                }
                else if (Lib.GetTime() >= (Convert.ToInt64(this["staminapoisonlastticks"]) + Convert.ToInt64(this["staminapoisonfrequency"])))
                {
                    // Check if it is time to trigger the state based on its frequency
                    // Where current time > lasttrigger+frequency
                    // If it's time, so run the effect and update last trigger time
                    this.RemoveHealth(Lib.GetByID(this["staminapoisoncaster"].ToString()), Convert.ToInt32(this["staminapoisondamage"]), 0);
                    this["staminapoisonlastticks"] = Convert.ToString(Lib.GetTime());
                }
            }

            if (this["manapoisondamage"] != null)
            {
                if (Lib.GetTime() >= Convert.ToInt64(this["manapoisonendticks"]))
                {
                    this.states.Remove("manapoisondamage");
                }
                else if (Lib.GetTime() >= (Convert.ToInt64(this["manapoisonlastticks"]) + Convert.ToInt64(this["manapoisonfrequency"])))
                {
                    // Check if it is time to trigger the state based on its frequency
                    // Where current time > lasttrigger+frequency
                    // If it's time, so run the effect and update last trigger time
                    this.RemoveHealth(Lib.GetByID(this["manapoisoncaster"].ToString()), Convert.ToInt32(this["manapoisondamage"]), 0);
                    this["manapoisonlastticks"] = Convert.ToString(Lib.GetTime());
                }
            }
        }

        /// <summary>
        /// Finds actor states/buffs that have expired and removes them
        /// </summary>
        /// <returns>Nothing</returns>
        public void ExpireStates()
        {
            System.Collections.IEnumerator names = this.states.Keys.GetEnumerator();
            while (names.MoveNext())
            {
                string name = (string)names.Current;
                if (name == "debuffhealthmax")
                {
                    // Check if buff/debuff has expired and reset the stat if so
                    if (Lib.GetTime() > Convert.ToInt64(this["debuffhealthmaxduration"]))
                    {
                        this["healthmax"] = Convert.ToInt32(this["healthmax"]) + Convert.ToInt32(this["debuffhealthmax"]);
                        // Remove buff/debuff state
                        this.states.Remove("debuffhealthmaxduration");
                        this.states.Remove("debuffhealthmax");
                        this.states.Remove("debuffhealthmaxcaster");
                        this.SendAlertBad("Your maximum health returns to normal.\r\n");
                       

                    }
                }
                if (name == "buffhealthmax")
                {
                    // Check if buff/debuff has expired and reset the stat if so
                    if (Lib.GetTime() > Convert.ToInt64(this["buffhealthmaxduration"]))
                    {
                        this["healthmax"] = Convert.ToInt32(this["healthmax"]) - Convert.ToInt32(this["buffhealthmax"]);
                        // Remove buff/debuff state
                        this.states.Remove("buffhealthmaxduration");
                        this.states.Remove("buffhealthmax");
                        this.states.Remove("buffhealthmaxcaster");
                        this.SendAlertBad("Your maximum health returns to normal.\r\n");
                    }
                }
                else if (name == "debuffmanamax")
                {
                    // Check if buff/debuff has expired and reset the stat if so
                    if (Lib.GetTime() > Convert.ToInt64(this["debuffmanamaxduration"]))
                    {
                        this["manamax"] = Convert.ToInt32(this["manamax"]) + Convert.ToInt32(this["debuffmanamax"]);
                        // Remove buff/debuff state
                        this.states.Remove("debuffmanamaxduration");
                        this.states.Remove("debuffmanamax");
                        this.states.Remove("debuffmanamaxcaster");
                        this.SendAlertBad("Your maximum mana returns to normal.\r\n");
                    }

                }
                else if (name == "buffmanamax")
                {
                    // Check if buff/debuff has expired and reset the stat if so
                    if (Lib.GetTime() > Convert.ToInt64(this["buffmanamaxduration"]))
                    {
                        this["manamax"] = Convert.ToInt32(this["manamax"]) - Convert.ToInt32(this["buffmanamax"]);
                        // Remove buff/debuff state
                        this.states.Remove("buffmanamaxduration");
                        this.states.Remove("buffmanamax");
                        this.states.Remove("buffmanamaxcaster");
                        this.SendAlertBad("Your maximum mana returns to normal.\r\n");
                    }

                }
                else if (name == "debuffstaminamax")
                {
                    // Check if buff/debuff has expired and reset the stat if so
                    if (Lib.GetTime() > Convert.ToInt64(this["debuffstaminamaxduration"]))
                    {
                        this["staminamax"] = Convert.ToInt32(this["staminamax"]) + Convert.ToInt32(this["debuffstaminamax"]);
                        // Remove buff/debuff state
                        this.states.Remove("debuffstaminamaxduration");
                        this.states.Remove("debuffstaminamax");
                        this.states.Remove("debuffstaminamaxcaster");
                        this.SendAlertBad("Your maximum stamina returns to normal.\r\n");
                    }

                }
                else if (name == "buffstaminamax")
                {
                    // Check if buff/debuff has expired and reset the stat if so
                    if (Lib.GetTime() > Convert.ToInt64(this["buffstaminamaxduration"]))
                    {
                        this["staminamax"] = Convert.ToInt32(this["staminamax"]) - Convert.ToInt32(this["buffstaminamax"]);
                        // Remove buff/debuff state
                        this.states.Remove("buffstaminamaxduration");
                        this.states.Remove("buffstaminamax");
                        this.states.Remove("buffstaminamaxcaster");
                        this.SendAlertBad("Your maximum stamina returns to normal.\r\n");
                    }

                }
                else if (name == "debuffstrength")
                {
                    // Check if buff/debuff has expired and reset the stat if so
                    if (Lib.GetTime() > Convert.ToInt64(this["debuffstrengthduration"]))
                    {
                        this["strength"] = Convert.ToInt32(this["strength"]) + Convert.ToInt32(this["debuffstrength"]);
                        // Remove buff/debuff state
                        this.states.Remove("debuffstrengthduration");
                        this.states.Remove("debuffstrength");
                        this.states.Remove("debuffstrengthcaster");
                        this.SendAlertBad("Your strength returns to normal.\r\n");
                    }
                }
                else if (name == "buffstrength")
                {
                    // Check if buff/debuff has expired and reset the stat if so
                    if (Lib.GetTime() > Convert.ToInt64(this["buffstrengthduration"]))
                    {
                        this["strength"] = Convert.ToInt32(this["strength"]) - Convert.ToInt32(this["buffstrength"]);
                        // Remove buff/debuff state
                        this.states.Remove("buffstrengthduration");
                        this.states.Remove("buffstrength");
                        this.states.Remove("buffstrengthcaster");
                        this.SendAlertBad("Your strength returns to normal.\r\n");
                    }
                }
                else if (name == "debuffagility")
                {
                    // Check if buff/debuff has expired and reset the stat if so
                    if (Lib.GetTime() > Convert.ToInt64(this["debuffagilityduration"]))
                    {
                        this["agility"] = Convert.ToInt32(this["agility"]) + Convert.ToInt32(this["debuffagility"]);
                        // Remove buff/debuff state
                        this.states.Remove("debuffagilityduration");
                        this.states.Remove("debuffagility");
                        this.states.Remove("debuffagilitycaster");
                        this.SendAlertBad("Your agility returns to normal.\r\n");
                    }
                }
                else if (name == "buffagility")
                {
                    // Check if buff/debuff has expired and reset the stat if so
                    if (Lib.GetTime() > Convert.ToInt64(this["buffagilityduration"]))
                    {
                        this["agility"] = Convert.ToInt32(this["agility"]) - Convert.ToInt32(this["buffagility"]);
                        // Remove buff/debuff state
                        this.states.Remove("buffagilityduration");
                        this.states.Remove("buffagility");
                        this.states.Remove("buffagilitycaster");
                        this.SendAlertBad("Your agility returns to normal.\r\n");
                    }
                }
                else if (name == "debuffintellect")
                {
                    // Check if buff/debuff has expired and reset the stat if so
                    if (Lib.GetTime() > Convert.ToInt64(this["debuffintellectduration"]))
                    {
                        this["intellect"] = Convert.ToInt32(this["intellect"]) + Convert.ToInt32(this["debuffintellect"]);
                        // Remove buff/debuff state
                        this.states.Remove("debuffintellectduration");
                        this.states.Remove("debuffintellect");
                        this.states.Remove("debuffintellectcaster");
                        this.SendAlertBad("Your intellect returns to normal.\r\n");
                    }

                }
                else if (name == "buffintellect")
                {
                    // Check if buff/debuff has expired and reset the stat if so
                    if (Lib.GetTime() > Convert.ToInt64(this["buffintellectduration"]))
                    {
                        this["intellect"] = Convert.ToInt32(this["intellect"]) - Convert.ToInt32(this["buffintellect"]);
                        // Remove buff/debuff state
                        this.states.Remove("buffintellectduration");
                        this.states.Remove("buffintellect");
                        this.states.Remove("buffintellectcaster");
                        this.SendAlertBad("Your intellect returns to normal.\r\n");
                    }

                }
                else if (name == "debuffspirit")
                {
                    // Check if buff/debuff has expired and reset the stat if so
                    if (Lib.GetTime() > Convert.ToInt64(this["debuffspiritduration"]))
                    {
                        this["spirit"] = Convert.ToInt32(this["spirit"]) + Convert.ToInt32(this["debuffspirit"]);
                        // Remove buff/debuff state
                        this.states.Remove("debuffspiritduration");
                        this.states.Remove("debuffspirit");
                        this.states.Remove("debuffspiritcaster");
                        this.SendAlertBad("Your spirit returns to normal.\r\n");
                    }
                }
                else if (name == "buffspirit")
                {
                    // Check if buff/debuff has expired and reset the stat if so
                    if (Lib.GetTime() > Convert.ToInt64(this["buffspiritduration"]))
                    {
                        this["spirit"] = Convert.ToInt32(this["spirit"]) - Convert.ToInt32(this["buffspirit"]);
                        // Remove buff/debuff state
                        this.states.Remove("buffspiritduration");
                        this.states.Remove("buffspirit");
                        this.states.Remove("buffspiritcaster");
                        this.SendAlertBad("Your spirit returns to normal.\r\n");
                    }
                }
            }
        }

        public int GetSkillLevel(string skillname)
        {
            for (int i = this.states.Count - 1; i >= 0; i--)
            {
                System.Collections.IEnumerator names = this.states.Keys.GetEnumerator();
                while (names.MoveNext())
                {
                    string name = (string)names.Current;

                    // found a known spell
                    if (name == skillname)
                    {
                        return Convert.ToInt32(this[name]);
                    }
                }
            }
            // didn't have that skill
            return 0;
        }





        public Actor GetRoom()
        {
            return Lib.GetByID(this["container"].ToString());

        }





        // Add a command word to the user's list
        public void AddActionWord(IAction action)
        {
            foreach (string word in action.Words)
            {
                actionwords.Add(word, action.Name);
            }
        }

        // Will check user-specific commands first, then system commands.
        // Currently only checks system until user-spec commands are implemented.
        public IAction GetActionByWord(string word)
        {
            string actionname = (string)actionwords[word];
            if (actionname != null)
                return (IAction)Lib.Actions[actionname];
            else
                return null;
        }

        // Get a list of command words available to the user
        // Only gives system command until user-specific commands are implemented.
        public string[] GetActionWordList()
        {
            ArrayList actionwordlist = new ArrayList();

            IEnumerator enumerator = actionwords.Values.GetEnumerator();
            while (enumerator.MoveNext())
            {
                IAction action = Lib.GetActionByName((string)enumerator.Current);
                foreach (string word in action.Words)
                {
                    if (!actionwordlist.Contains(word))
                    {
                        actionwordlist.Add(word);
                    }
                }
            }

            return (string[])actionwordlist.ToArray(typeof(string));
        }


        // Add a command word to the user's list
        public void AddCommandWord(Command command)
        {
            foreach (string word in command.Words)
            {
                commandwords.Add(word, command.Name);
            }
        }

        public bool AddCommandWord(Actor actor, string commandname)
        {
            string[] words = commandname.Split(new char[] { ' ' }, 2);

            Remove(actor, words[0]);

            if (words.Length < 2)
            {
                actor.SendError("You must specify a command name and an accesslevel to set it to.\r\n");
                return false;
            }

            Hashtable hashtable = new Hashtable();
            int accesslevel = -1;

            switch (words[1].ToLower())
            {
                case "system":
                    hashtable = Lib.SystemCommandWords;
                    accesslevel = (int)AccessLevel.System;
                    break;
                case "player":
                    hashtable = Lib.PlayerCommandWords;
                    accesslevel = (int)AccessLevel.Player;
                    break;
                case "builder":
                    hashtable = Lib.BuilderCommandWords;
                    accesslevel = (int)AccessLevel.Builder;
                    break;
                case "admin":
                    hashtable = Lib.AdminCommandWords;
                    accesslevel = (int)AccessLevel.Admin;
                    break;
                case "uberadmin":
                    hashtable = Lib.UberAdminCommandWords;
                    accesslevel = (int)AccessLevel.UberAdmin;
                    break;
                default:
                    actor.SendError("Invalid access level specified.\r\n");
                    return false;
            }

            Command command = Lib.GetCommandByName(words[0]);

            if (command == null)
            {
                actor.SendError("Command name not found.\r\n");
                return false;
            }

            Lib.AddCommandWord(hashtable, command);
            Lib.dbService.SystemCommands.AddCommand(accesslevel, words[0]);
            actor.Send("Access level set.\r\n");
            return true;
        }

        private bool Remove(Actor actor, string commandname)
        {
            int accesslevel = GetAccessLevel(commandname);

            //			Hashtable hashtable = new Hashtable();
            ICommand command;

            switch (accesslevel)
            {
                case -1:
                    return false;
                case (int)AccessLevel.System:
                    command = Lib.GetCommandByName(commandname);
                    if (command == null)
                        return false;
                    foreach (string commandword in command.Words)
                    {
                        Lib.SystemCommandWords.Remove(commandword);
                    }
                    break;
                case (int)AccessLevel.Player:
                    command = Lib.GetCommandByName(commandname);
                    if (command == null)
                        return false;
                    foreach (string commandword in command.Words)
                    {
                        Lib.PlayerCommandWords.Remove(commandword);
                    }
                    break;
                case (int)AccessLevel.Builder:
                    command = Lib.GetCommandByName(commandname);
                    if (command == null)
                        return false;
                    foreach (string commandword in command.Words)
                    {
                        Lib.BuilderCommandWords.Remove(commandword);
                    }
                    break;
                case (int)AccessLevel.Admin:
                    command = Lib.GetCommandByName(commandname);
                    if (command == null)
                        return false;
                    foreach (string commandword in command.Words)
                    {
                        Lib.AdminCommandWords.Remove(commandword);
                    }
                    break;
                case (int)AccessLevel.UberAdmin:
                    command = Lib.GetCommandByName(commandname);
                    if (command == null)
                        return false;
                    foreach (string commandword in command.Words)
                    {
                        Lib.UberAdminCommandWords.Remove(commandword);
                    }
                    break;
            }

            Lib.dbService.SystemCommands.DeleteCommand(commandname);
            return true;
        }

        public int GetAccessLevel(string commandname)
        {
            return Lib.dbService.SystemCommands.GetCommandAccessLevel(commandname);
        }

        public bool RemoveCommandWord(Actor actor, string commandname)
        {
            if (Remove(actor, commandname))
            {
                actor.Send("Access level removed.\r\n");
                return true;
            }

            return false;
        }

        //public bool ConvertToBoolean(string convertvalue)
        //{
        //    return Lib.ConvertToBoolean(convertvalue);

        //}

        public void AddBug(string userShortName, string bugText)
        {
            Lib.dbService.MudBugs.AddBug(userShortName, bugText);
        }

        public DataTable GetBugList(bool includeReadBugs)
        {
            return Lib.dbService.MudBugs.GetBugList(includeReadBugs);
        }

        public void MarkBugAsRead(int bugId)
        {
            Lib.dbService.MudBugs.MarkBugAsRead(bugId);
        }

        public void ClearBugs(bool allBugs)
        {
            Lib.dbService.MudBugs.ClearBugs(allBugs);
        }



        public bool ValidatePassword(string password)
        {
            if (password.ToLower() == this["shortname"].ToString().ToLower())
            {
                return false;
            }
            return Lib.Checkvalidpassword.IsMatch(password);
        }

        public IAction GetActionByName(string name)
        {
            return Lib.GetActionByName(name);

        }


        public void Disco(Actor actor, string reason)
        {
            actor["connected"] = false;
            Lib.Disco(actor, reason);
        }

        
        // Add a command word to the user's list
        public void AddCommandWord(Actor actor, string commandname, string arguments, long time, long delay, bool loop)
        {
            Lib.Delayedcommands.Add(actor, commandname, arguments, time, delay, loop);
        }

        // Will check user-specific commands first, then system commands.
        // Currently only checks system until user-spec commands are implemented.
        public Command GetCommandByWord(string word)
        {
            string commandname = (string)commandwords[word];
            if (commandname != null)
                return (Command)Lib.Commands[commandname];
            else
                return Lib.GetCommandByWord(Convert.ToInt32(this["accesslevel"]), word);
        }

        // Get a list of command words available to the user
        // Only gives system command until user-specific commands are implemented.
        public string[] GetCommandWordList()
        {
            ArrayList commandwordlist = new ArrayList();

            IEnumerator enumerator = commandwords.Values.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Command command = Lib.GetCommandByName((string)enumerator.Current);
                foreach (string word in command.Words)
                {
                    if (!commandwordlist.Contains(word))
                    {
                        commandwordlist.Add(word);
                    }
                }
            }

            foreach (string word in Lib.GetCommandWordList(Convert.ToInt32(this["accesslevel"])))
            {
                if (!commandwordlist.Contains(word))
                {
                    commandwordlist.Add(word);
                }
            }

            return (string[])commandwordlist.ToArray(typeof(string));
        }



        public void LoadCommandWordsFromDatabase()
        {
            int type = 0;
            if (this["type"].ToString() == "item")
            {
                type = 2;
            }
            else if (this["type"].ToString() == "user")
            {
                type = 1;
            }
            else
            {
                type = 10;
            }

            string commandName = Lib.dbService.Actor.GetCommand(this["id"].ToString(), type);
            if (commandName != null)
            {
                Command command = Lib.GetCommandByName(commandName);
                if (command != null)
                {
                    AddCommandWord(command);
                }
            }
        }

        /// <summary>
        /// Load all actors into memory from the database
        /// </summary>
        public static ArrayList LoadAll(ref Hashtable loadstats)
        {
            int counter = 0;
            DataTable dt;
            try
            {
                dt = Lib.dbService.Library.GetAllActors();
            }
            catch (Exception ex)
            {
                throw;
            }

            foreach (DataRow datarow in dt.Rows)
            {
                // Create user to populate with data
                Actor actor = new Actor();
                actor["id"] = Convert.ToString(datarow["id"]);

                // Load all actor states
                DataTable table = actor.Loadstate();

                // Set all your default required actor states here
                actor["tradestate"] = "";
                if (actor["type"] == null) actor["type"] = "placeholder";
                if (actor["subtype"] == null) actor["subtype"] = "placeholder";
                if (actor["shortname"] == null) actor["shortname"] = "placeholder";
                actor["shortnameupper"] = Lib.FirstToUpper(actor["shortname"].ToString());
                if (actor["name"] == null) actor["name"] = "placeholder";

                if (actor["type"].ToString() == "user")
                {
                    //Set default color options just in case
                    actor["colorcommandprompt"] = Lib.Ansifyellow;
                    actor["colorcommandtext"] = Lib.Ansifboldyellow;
                    actor["colorerrors"] = Lib.Ansifred;
                    actor["colorexits"] = Lib.Ansifboldwhite;
                    actor["coloritems"] = Lib.Ansifboldgreen;
                    actor["colormessages"] = Lib.Ansifwhite;
                    actor["colormobs"] = Lib.Ansifboldcyan;
                    actor["colorpeople"] = Lib.Ansifboldcyan;
                    actor["colorroomdescr"] = Lib.Ansifwhite;
                    actor["colorroomname"] = Lib.Ansifboldwhite;
                    actor["coloralertgood"] = Lib.Ansifboldgreen;
                    actor["coloralertbad"] = Lib.Ansifboldred;
                    actor["colorsystemmessage"] = Lib.Ansifboldpurple;
                    actor["colorannouncement"] = Lib.Ansifboldyellow;
                    actor["connected"] = false;
                }

                // Add exits to the exits arraylist
                if (actor["xnorth"] != null)
                {
                    actor.Exits.Add("north", actor["xnorth"].ToString());
                }
                if (actor["xnortheast"] != null)
                {
                    actor.Exits.Add("northeast", actor["xnortheast"].ToString());
                }
                if (actor["xeast"] != null)
                {
                    actor.Exits.Add("east", actor["xeast"].ToString());
                }
                if (actor["xsoutheast"] != null)
                {
                    actor.Exits.Add("southeast", actor["xsoutheast"].ToString());
                }
                if (actor["xsouth"] != null)
                {
                    actor.Exits.Add("south", actor["xsouth"].ToString());
                }
                if (actor["xsouthwest"] != null)
                {
                    actor.Exits.Add("southwest", actor["xsouthwest"].ToString());
                }
                if (actor["xwest"] != null)
                {
                    actor.Exits.Add("west", actor["xwest"].ToString());
                }
                if (actor["xnorthwest"] != null)
                {
                    actor.Exits.Add("northwest", actor["xnorthwest"].ToString());
                }
                if (actor["xup"] != null)
                {
                    actor.Exits.Add("up", actor["xup"].ToString());
                }
                if (actor["xdown"] != null)
                {
                    actor.Exits.Add("down", actor["xdown"].ToString());
                }
                if (actor["xin"] != null)
                {
                    actor.Exits.Add("in", actor["xin"].ToString());
                }
                if (actor["xout"] != null)
                {
                    actor.Exits.Add("out", actor["xout"].ToString());
                }

                lock (Lib.actors.SyncRoot)
                {
                    Lib.actors.Add(actor);
                }

                // Get current count of object type
                int loadstat = 0;
                loadstat = Convert.ToInt32(loadstats[actor["type"].ToString()]);
                loadstat += 1;
                loadstats[actor["type"].ToString()] = loadstat;

                actor.LoadActionWordsFromDatabase();
                actor.LoadCommandWordsFromDatabase();

                // Any changes after this point cause this actor to be saved to db on Save command
                actor.oldversion = actor.version;
                counter++;
            }
            return Lib.actors;

        }


        public void LoadSpells()
        {
            // Load spells into user spell list
            System.Collections.IEnumerator names = this.states.Keys.GetEnumerator();
            while (names.MoveNext())
            {
                // Get spell properties
                string statename = (string)names.Current;

                if (statename.StartsWith("spellknown_"))
                {
                    string spellname = statename.Replace("spellknown_", "");
                    Spell spell = Lib.GetSpellByName(spellname);
                    // found a spell the user knows, so add it to the user spell list
                    this.Spells.Add(spell);
                    //Lib.PrintLine("Loaded " + spell["name"] + " into user " + this["name"]);
                }
            }
        }

        public Spell GetSpellByName(string name)
        {
            return Lib.GetSpellByName(name);
        }



        public void LoadActionWordsFromDatabase()
        {
            int type = 0;
            if (this["type"].ToString() == "item")
            {
                type = 2;
            }
            else if (this["type"].ToString() == "user")
            {
                type = 1;
            }
            else
            {
                type = 10;
            }

            string actionName = Lib.dbService.Actor.GetAction(this["id"].ToString(), type);
            if (actionName != null)
            {
                IAction action = Lib.GetActionByName(actionName);
                if (action != null)
                {
                    AddActionWord(action);
                }
            }
        }

        /// <summary>
        /// Sends a message to everybody in the zone
        /// </summary>
        /// <returns>Nothing</returns>
        public void Sayinzone(string message)
        {
            this.Sayinzone(message, this);
        }

        /// <summary>
        /// Sends a message to everybody in a zone, except the person specified
        /// </summary>
        /// <returns>Nothing</returns>
        public void Sayinzone(string message, Actor excludeactor)
        {
            ArrayList excludearray = new ArrayList();
            excludearray.Add(excludeactor);
            this.Sayinzone(message, excludearray);
        }


        /// <summary>
        /// Sends a message to everybody in a zone, except the people specified
        /// </summary>
        /// <returns>Nothing</returns>
        public void Sayinzone(string message, ArrayList excludeactors)
        {
            // Tell others in the room that he ?????
            Actor room = this.GetRoom();
            foreach (Actor tmpuser in Lib.actors)
            {
                Actor tmproom = tmpuser.GetRoom();
                if (tmproom["zone"] == room["zone"] && this["shortname"] != tmpuser["shortname"])
                {
                    for (int actorcounter = excludeactors.Count - 1; actorcounter >= 0; actorcounter--)
                    {
                        Actor excludeactor = (Actor)excludeactors[actorcounter];
                        if (tmpuser["id"].ToString() != excludeactor["id"].ToString())
                        {
                            try
                            {
                                tmpuser.Send(tmpuser["colorannouncement"] + message + "\r\n");
                            }
                            catch
                            {
                                //Must have tried to send a message to someone who went offline just recently.
                                //So ignore the error to avoid a server crash.
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Sends a message to everybody in the zone
        /// </summary>
        /// <returns>Nothing</returns>
        public void Sayinworld(string message)
        {
            this.Sayinworld(message, this);
        }

        /// <summary>
        /// Sends a message to everybody in a zone, except the person specified
        /// </summary>
        /// <returns>Nothing</returns>
        public void Sayinworld(string message, Actor excludeactor)
        {
            ArrayList excludearray = new ArrayList();
            excludearray.Add(excludeactor);
            this.Sayinworld(message, excludearray);
        }

        /// <summary>
        /// Sends a message to everybody in a zone, except the people specified
        /// </summary>
        /// <returns>Nothing</returns>
        public void Sayinworld(string message, ArrayList excludeactors)
        {
            // Tell others in the room that he ?????
            foreach (Actor tmpuser in Lib.actors)
            {
                if (this["shortname"] != tmpuser["shortname"])
                {
                    for (int actorcounter = excludeactors.Count - 1; actorcounter >= 0; actorcounter--)
                    {
                        Actor excludeactor = (Actor)excludeactors[actorcounter];
                        if (tmpuser["id"].ToString() != excludeactor["id"].ToString())
                        {
                            try
                            {
                                tmpuser.Send(tmpuser["colorannouncement"] + message + "\r\n");
                            }
                            catch
                            {
                                //Must have tried to send a message to someone who went offline just recently.
                                //So ignore the error to avoid a server crash.
                            }
                        }
                    }
                }
            }
        }


        //Sends a message to every user within a certain radius of the current user's origin.
        public void SayinRadius(string message, int radius)
        {
            //Going to build sortedlist of every room within a (radius) room distance from the user.
            Actor currentroom = Lib.GetByID(this["container"].ToString());

            SortedList roomslist = new SortedList();

            roomslist.Add(currentroom["id"], "Here");

            for (int radiuscounter = 0; radiuscounter < radius; radiuscounter++)
            {
                for (int roomscounter = 0; roomscounter < roomslist.Count; roomscounter++)
                {
                    string temproomid = (string)roomslist.GetKey(roomscounter);
                    Actor temproom = Lib.GetByID(temproomid);


                    for (int exitscounter = 0; exitscounter < temproom.Exits.Count; exitscounter++)
                    {
                        string currentkey = (string)temproom.Exits.GetKey(exitscounter);
                        string tempid = temproom.Exits[currentkey].ToString();
                        string fromdirection = Lib.Getoppositedir(currentkey);
                        if (roomslist.ContainsKey(temproom.Exits[currentkey]) == false)
                        {
                            roomslist.Add(tempid, fromdirection);
                        }
                    }
                }
            }
            //So we only have to loop through the userlist once, I check if each user's room
            //exists in the userlist, and if so, display the message.

            for (int i = Lib.actors.Count - 1; i >= 0; i--)
            {
                Actor tmpuser = (Actor)Lib.actors[i];
                if (roomslist.ContainsKey((object)tmpuser["container"].ToString()) == true)
                {
                    string fromdirection = (string)roomslist[(object)tmpuser["container"].ToString()];
                    if (this["shortname"] != tmpuser["shortname"])
                    {
                        if (fromdirection == ("Here"))
                        {
                            tmpuser.Send(message + ".\r\n");
                        }
                        else
                        {
                            tmpuser.Send(message + " from " + fromdirection + ".\r\n");
                        }
                    }
                }
            }

        }



        /// <summary>
        /// Send a message to everybody else in the room
        /// </summary>
        /// <returns>Nothing</returns>
        public void Sayinroom(string message)
        {
            this.Sayinroom(message, this);
        }

        /// <summary>
        /// Sends a message to everybody in a room, except the person specified
        /// </summary>
        /// <returns>Nothing</returns>
        public void Sayinroom(string message, Actor excludeactor)
        {
            ArrayList excludearray = new ArrayList();
            excludearray.Add(excludeactor);
            this.Sayinroom(message, excludearray);
        }

   

        /// <summary>
        /// Sends a message to everybody in a container/room, except the people specified
        /// </summary>
        /// <returns>Nothing</returns>
        public void Sayinroom(string message, ArrayList excludeactors)
        {
            // Get person's container and speak to everything in it.
            Actor container = this.GetContainer();
            if (container == null) return;

            //ArrayList contents = container.GetContents();
            ArrayList contents = container.GetUsers();
            bool chk = false;

            // Tell others in this container/room something
            for (int tmpactorcounter = contents.Count - 1; tmpactorcounter >= 0; tmpactorcounter--)
            {
                Actor tmpuser = (Actor)contents[tmpactorcounter];
                for (int actorcounter = excludeactors.Count - 1; actorcounter >= 0; actorcounter--)
                {
                    Actor excludeactor = (Actor)excludeactors[actorcounter];
                    if (tmpuser["id"].ToString() == excludeactor["id"].ToString()) chk = true;
                }
                if (!chk)
                {
                    try
                    {
                        if (tmpuser["type"].ToString() == "user")
                        {
                            tmpuser.Send(message + "\r\n");
                            chk = false;
                        }
                    }
                    catch
                    {
                        //May have tried to send a message to someone who went offline just recently.
                        //So ignore the error to avoid a server crash.
                    }
                }

            }
        }

        /// <summary>
        /// Function that unequips a user item.
        /// </summary>
        /// <returns>Success or failure as a boolean</returns>
        public bool UnEquip(Actor item)
        {
            if (!Lib.ConvertToBoolean(item["equipable"]) || !Lib.ConvertToBoolean(item["equipped"]))
            {
                return false;
            }
            item["equipped"] = false;
            switch (item["equipslot"].ToString())
            {
                case "head":
                    this["wearhead"] = "";
                    break;
                case "neck":
                    this["wearneck"] = "";
                    break;
                case "shoulders":
                    this["wearshoulders"] = "";
                    break;
                case "back":
                    this["wearback"] = "";
                    break;
                case "arms":
                    this["weararms"] = "";
                    break;
                case "wrists":
                    this["wearwrists"] = "";
                    break;
                case "hands":
                    this["wearhands"] = "";
                    break;
                case "rightring":
                    this["wearrightring"] = "";
                    break;
                case "leftring":
                    this["wearleftring"] = "";
                    break;
                case "chest":
                    this["wearchest"] = "";
                    break;
                case "waist":
                    this["wearwaist"] = "";
                    break;
                case "legs":
                    this["wearlegs"] = "";
                    break;
                case "feet":
                    this["wearfeet"] = "";
                    break;
                case "weapon1":
                    this["wearweapon1"] = "";
                    break;
                case "weapon2":
                    this["wearweapon2"] = "";
                    break;
            }
            // Persist these equipment changes into the database
            item.Save();
            return true;
        }

        /// <summary>
        /// Function that equips a user item.
        /// </summary>
        /// <returns>Success or failure as a boolean</returns>
        public bool Equip(Actor item)
        {
            // Make sure it's an equippable item and not already equipped
            if (!Lib.ConvertToBoolean(item["equipable"]) || Lib.ConvertToBoolean(item["equipped"]))
            {
                return false;
            }
            // Make sure nothing else is already equipped there
            foreach (Actor scanitem in this.GetContents())
            {
                if (scanitem["equipslot"] == item["equipslot"])
                {
                    
                    // Item cannot already be equipped
                    if (Lib.ConvertToBoolean(scanitem["equipped"])) return false;
                }
            }
            item["equipped"] = true;
            switch (item["equipslot"].ToString())
            {
                case "head":
                    this["wearhead"] = item["id"];
                    break;
                case "neck":
                    this["wearneck"] = item["id"];
                    break;
                case "shoulders":
                    this["wearshoulders"] = item["id"];
                    break;
                case "back":
                    this["wearback"] = item["id"];
                    break;
                case "arms":
                    this["weararms"] = item["id"];
                    break;
                case "wrists":
                    this["wearwrists"] = item["id"];
                    break;
                case "hands":
                    this["wearhands"] = item["id"];
                    break;
                case "rightring":
                    this["wearrightring"] = item["id"];
                    break;
                case "leftring":
                    this["wearleftring"] = item["id"];
                    break;
                case "chest":
                    this["wearchest"] = item["id"];
                    break;
                case "waist":
                    this["wearwaist"] = item["id"];
                    break;
                case "legs":
                    this["wearlegs"] = item["id"];
                    break;
                case "feet":
                    this["wearfeet"] = item["id"];
                    break;
                case "weapon1":
                    this["wearweapon1"] = item["id"];
                    break;
                case "weapon2":
                    this["wearweapon2"] = item["id"];
                    break;
            }
            // Persist these equipment changes into the database
            item.Save();
            return true;

        }


      

        // The following are properties that reset when a user logs in/out
        // or simply are not stored in the database.

        //public string Shortnameupper // return the name with the first character uppercase for sentences and such
        //{
        //    get
        //    {
        //        return this["shortname"].ToString().Substring(0, 1).ToUpper() + this["shortname"].ToString().Substring(1, this["shortname"].ToString().Length - 1);
        //    }
        //}

        public string GetNameUpper() // return the name with the first character uppercase for sentences and such
        {
            if (this["name"].ToString().Length<1)
            {
                return this["name"].ToString();
            }

            return this["name"].ToString().Substring(0, 1).ToUpper() + this["name"].ToString().Substring(1, this["name"].ToString().Length - 1);
        }

        public string GetNamelower() // return the full name in lower case
        {
             if (this["name"].ToString().Length<1)
            {
                return this["name"].ToString();
            }

            return this["name"].ToString().ToLower();
        }

        /// <summary>
        /// Return the nameprefix and name of an actor.
        /// </summary>
        /// <returns></returns>
        public string GetNameFull()
        {
            if (this["nameprefix"] != null)
            {
                if (this["nameprefix"].ToString() != "")
                {
                    return this["nameprefix"] + " " + this["name"].ToString();
                }
            }

            return this["name"].ToString();
        }

        /// <summary>
        /// Return the nameprefix and name of an actor witht he prefix in upper case for starting sentences.
        /// </summary>
        /// <returns></returns>
        public string GetNameFullUpper()
        {
            if (this["nameprefix"] != null)
            {
                if (this["nameprefix"].ToString() != "")
                {
                    return this["nameprefix"].ToString().Substring(0, 1).ToUpper() + this["nameprefix"].ToString().Substring(1) + " " + this["name"].ToString();
                }
            }
            return this["name"].ToString();
        }

       

        public void Additem(Actor item)
        {
            items.Add(item);
            item["container"] = this["id"];
            item["containertype"] = this["type"].ToString();
        }

        public void Removeitem(Actor item)
        {
            items.Remove(item);
            item["container"] = "";
            item["containertype"] = "";
            this.UnEquip(item);
        }



        public Actor GetItemAtIndex(int index)
        {
            return (Actor)items[index];
        }

        // Return an object that is contained within this object.
        public Actor GetItemById(string id)
        {
            for (int i = this.items.Count - 1; i >= 0; i--)
            {
                Actor item = (Actor)items[i];
                if (item["id"].ToString() == id)
                {
                    return item;
                }
            }
            return null;
        }

        // Return an item specified by name.
        public Actor GetItemByName(string itemname)
        {
            // No position number provided means return the first matching item.
            return GetItemByName(itemname, 1);
        }

        // Return an item specified by name and by position number.
        public Actor GetItemByName(string itemname, int itemnumber)
        {
            // No position number provided means return the first matching item.
            return GetItemByName(itemname, itemnumber, 0);
        }

        // Return an item specified by name and by position number.
        public Actor GetItemByName(string itemname, int itemnumber, int itemquantity)
        {
            itemname = itemname.ToLower();
            if (itemname == "me" || itemname == "self")
            {
                return this;
            }
            return Lib.GetItemByName(this.GetContents(), itemname, itemnumber, itemquantity);
        }

        // Return an item specified by name and by position number.
        public Actor GetItemByName(ArrayList list, string itemname, int itemnumber, int itemquantity)
        {
            itemname = itemname.ToLower();
            if (itemname == "me" || itemname == "self")
            {
                return this;
            }
            return Lib.GetItemByName(list, itemname, itemnumber, itemquantity);
        }

        //public string ServerVersion
        //{
        //    get { return Lib.Serverversion; }
        //}

        public string GetOppositeDirection(string direction)
        {
            return Lib.Getoppositedir(direction);
        }


        public void Sanitize(ref string arguments)
        {
            TigerMUD.DatabaseLib.DataCleaning.Sanitize(ref arguments);
        }

        public string Sanitize(string text)
        {
            return TigerMUD.DatabaseLib.DataCleaning.Sanitize(text);
        }


        public void AddBug(Actor actor, string arguments)
        {
            Lib.dbService.MudBugs.AddBug(actor["shortname"].ToString(), arguments);
        }
        public bool Exists(string userid)
        {
            return Lib.Exists(userid);
        }

        public void AddFriend(string shortName, string friendName)
        {
            Lib.dbService.Actor.AddFriend(shortName, friendName);
        }

        public void AddFriendWithAuthorisation(string shortName, string friendName)
        {
            Lib.dbService.Actor.AddFriendWithAuthorisation(shortName, friendName);
        }

        public bool IsFriendInList(string shortName, string friendName)
        {
            return Lib.dbService.Actor.IsFriendInList(shortName, friendName);
        }

        public bool IsFriendPendingAuthorisation(string shortName, string friendName)
        {
            return Lib.dbService.Actor.IsFriendInList(shortName, friendName);
        }

        public void AuthoriseFriend(string shortName, string requestorName)
        {
            Lib.dbService.Actor.AuthoriseFriend(shortName, requestorName);
        }

        public void RemoveFriend(string shortName, string friendName)
        {
            Lib.dbService.Actor.RemoveFriend(shortName, friendName);
        }

        public void RejectFriendRequest(string shortName, string requestorName)
        {
            Lib.dbService.Actor.RejectFriendRequest(shortName, requestorName);
        }

        public DataTable GetPendingAuthorisationRequests(string shortName)
        {
            return Lib.dbService.Actor.GetPendingAuthorisationRequests(shortName);
        }

        public DataTable GetFriendsList(string shortName)
        {
            return Lib.dbService.Actor.GetFriendsList(shortName);
        }

        public bool IsActorWaitingForAuthorisation(string shortName, string requestorName)
        {
            return Lib.dbService.Actor.IsActorWaitingForAuthorisation(shortName, requestorName);
        }

        public string PaddedLineItem(string item, string item2, char padchar, int width)
        {
            return Lib.PaddedLineItem(item, item2, padchar, width);
        }

        public string PaddedLineItem(string item, string item2, string item3, char padchar, int width)
        {
            return Lib.PaddedLineItem(item, item2, item3, padchar, width);

        }

      
        public class Comparer : IComparer
        {
            CaseInsensitiveComparer cic = new CaseInsensitiveComparer();
            Actor newx = null;
            Actor newy = null;
            int result = 0;

            int IComparer.Compare(Object x, Object y)
            {
                newx = (Actor)x;
                newy = (Actor)y;
                result = cic.Compare(newx["id"], newy["id"]);
                return result;
            }
        }

        /// <summary>
        /// Return the contents of an actor
        /// </summary>
        /// <returns></returns>
        public ArrayList GetContents()
        {
            ArrayList resultlist = new ArrayList();

            foreach (Actor actor in this.items)
            {
                // Exclude non-connected users
                if (actor["type"].ToString() != "user" || Lib.ConvertToBoolean(actor["connected"]))
                {
                    resultlist.Add(actor);
                }
            }
            resultlist.Sort(new Comparer());
            return resultlist;

        }
        /// <summary>
        /// Return the contents of an actor excluding a certain actor
        /// </summary>
        /// <returns></returns>
        public ArrayList GetContents(Actor excludeactor)
        {
            ArrayList resultlist = new ArrayList();

            foreach (Actor actor in this.items)
            {
                // Exclude non-connected users
                if (actor != excludeactor)
                {
                    if (actor["type"].ToString() != "user" || Lib.ConvertToBoolean(actor["connected"]))
                    {
                        resultlist.Add(actor);
                    }
                }
            }
            resultlist.Sort(new Comparer());
            return resultlist;

        }

        /// <summary>
        /// Return the contents of an actor if they match a certain type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ArrayList GetContents(string type)
        {
            ArrayList resultlist = new ArrayList();

            foreach (Actor actor in this.items)
            {
                // Exclude non-connected users
                if (actor["type"].ToString() != "user" || Lib.ConvertToBoolean(actor["connected"]))
                {
                    if (actor["type"].ToString() == type)
                        resultlist.Add(actor);
                }
            }
            return resultlist;
        }

        public ArrayList GetContents(string type, string subtype)
        {
            ArrayList resultlist = new ArrayList();

            foreach (Actor actor in this.items)
            {
                // Exclude non-connected users
                if (actor["type"].ToString() != "user" || Lib.ConvertToBoolean(actor["connected"]))
                {
                    if (actor["type"].ToString() == type && actor["subtype"].ToString() == subtype)
                        resultlist.Add(actor);
                }
            }
            return resultlist;
        }

        public ArrayList GetContents(string type, Actor excludeactor)
        {
            ArrayList resultlist = new ArrayList();

            foreach (Actor actor in this.items)
            {
                // Exclude non-connected users
                if (actor["type"].ToString() != "user" || Lib.ConvertToBoolean(actor["connected"]))
                {
                     // Exclude non-connected users
                    if (actor != excludeactor)
                    {
                        if (actor["type"].ToString()==type)
                            resultlist.Add(actor);
                    }
                }
            }
            return resultlist;
        }

        /// <summary>
        /// Return the contents of an actor if they match a certain type and subtype, considering exclusions.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ArrayList GetContents(string type, string subtype, Actor excludeactor)
        {
            ArrayList resultlist = new ArrayList();

            foreach (Actor actor in this.items)
            {
                // Exclude non-connected users
                if (actor["type"].ToString() != "user" || Lib.ConvertToBoolean(actor["connected"]))
                {
                    // Exclude non-connected users
                    if (actor != excludeactor)
                    {
                        if (actor["type"].ToString() == type && actor["subtype"].ToString() == subtype)
                            resultlist.Add(actor);
                    }
                }
            }
            return resultlist;
        }

        public Actor GetContainer()
        {
            return Lib.GetByID(this["container"].ToString());
        }

        public SortedList Exits
        {
            get { return exits; }
            set { exits = value; }
        }

      
        public ArrayList GetUsers()
        {
            ArrayList actors = new ArrayList();
            for (int i = this.items.Count - 1; i >= 0; i--)
            {
                Actor actor = (Actor)this.items[i];
                if (actor["type"].ToString() == "user")
                {
                    actors.Add(actor);
                }
            }
            return actors;
        }



        public ArrayList GetUsers(Actor excludeactor)
        {
            ArrayList actors = new ArrayList();
            for (int i = this.items.Count - 1; i >= 0; i--)
            {
                Actor actor = (Actor)this.items[i];
                if (actor["type"].ToString() == "user" && actor["id"] != excludeactor["id"])
                {
                    actors.Add(actor);
                }
            }
            return actors;
        }

        public Actor GetByName(string name)
        {

            for (int i = this.items.Count - 1; i >= 0; i--)
            {
                Actor user = (Actor)this.items[i];
                if (user["shortname"].ToString() == name.ToLower() || user["name"].ToString() == name.ToLower())
                {
                    return user;
                }
            }
            return null;
        }

     
        // Return a mob specified by name and by position number.
        public Actor GetMobByName(string mobname, int mobnumber)
        {
            mobname = mobname.ToLower();
            if (mobnumber < 1) mobnumber = 1;
            int mobcounter = 1;

            // Check for item in inventory
            foreach (Actor mob in this.GetMobs())
            {
                if (mob != null)
                {
                    if (mob["shortname"].ToString() == mobname || mob.GetNamelower() == mobname)
                    {
                        if (mobcounter == mobnumber)
                        {
                            return mob;
                        }
                        mobcounter++;
                    }
                }
            }
            // Didn't find what we were looking for.
            return null;
        }

        // Return a mob specified by name and by position number.
        public Actor GetMobByName(string mobname)
        {
            return this.GetMobByName(mobname, 1);
        }



        public ArrayList GetMobs()
        {
            ArrayList mobs = new ArrayList();
            lock (this.items.SyncRoot)
            {
                for (int i = this.items.Count - 1; i >= 0; i--)
                {
                    Actor actor = (Actor)this.items[i];
                    if (actor["type"].ToString() == "mob")
                    {
                        mobs.Add(actor);
                    }
                }
            }
            return mobs;
        }



        public void MoveToRoom(string roomid, string direction, bool quiet)
        {
          
            // Get current room
            Actor room = Lib.GetByID(this["container"].ToString());
            // Get destination room
            Actor destroom = Lib.GetByID(roomid);
            
            // Catch error
            if (room == null || destroom==null)
            {
                return;
            }

            if (!quiet)
            {
                // First tell users this mob has left the room
                this.Sayinroom(this.GetNameUpper() + " left going " + direction + ".");
            }

            // Remove actor from current room actor list
            room.items.Remove(this);

            // Move the mob
            this["container"] = destroom["id"].ToString();
            this["containertype"] = destroom["type"].ToString();

            // Add actor to new room actor list
            destroom.items.Add(this);

            if (!quiet)
            {
                // Tell users this mob entered the new room
                this.Sayinroom(this.GetNameUpper() + " arrived from " + Lib.Getoppositedir(direction) + ".");
            }

            //Currently we don't persist a mob's move to the db.

        }

        // Reversed comparer for sorting largest item on top
        //public class TargetComparer : IComparer
        //{
        //    int IComparer.Compare(Object x, Object y)
        //    {
        //        return (new CaseInsensitiveComparer()).Compare(((HateTarget)y).hate, ((HateTarget)x).hate);
        //    }
        //}

        public class TargetComparer : IComparer
        {
            CaseInsensitiveComparer cic = new CaseInsensitiveComparer();
            HateTarget newx;
            HateTarget newy;
            int result = 0;

            int IComparer.Compare(Object x, Object y)
            {
                newx = (HateTarget)x;
                newy = (HateTarget)y;
                result = cic.Compare(newy.hate, newx.hate);
                return result;
            }
        }


        /// <summary>
        /// Table of targets the mob is in combat with, sorted descending by hate level.
        /// </summary>
        public class HateTable
        {
            protected ArrayList targets = new ArrayList();

            public HateTable()
            {
            }

            public virtual bool IncrementHate(HateTarget target, int hateamount)
            {
                // Find the target
                lock (targets.SyncRoot)
                {
                    foreach (HateTarget chktarget in targets)
                    {
                        if (chktarget.actor == target.actor)
                        {
                            target = chktarget;
                            Remove(target);
                            target.hate += hateamount;
                            Add(target);
                            return true;
                        }
                    }
                }
                return false;

            }


            public virtual bool Exists(HateTarget target)
            {
                lock (targets.SyncRoot)
                {
                    foreach (HateTarget chktarget in targets)
                    {
                        if (chktarget.actor == target.actor)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public virtual void Add(Actor actor, int hate)
            {
                lock (targets.SyncRoot)
                {
                    HateTarget hatetarget;
                    hatetarget.actor = actor;
                    hatetarget.hate = hate;
                    if (!this.Exists(hatetarget))
                    {
                        Add(hatetarget);
                    }
                }
            }

            public virtual void Add(HateTarget target)
            {
                lock (targets.SyncRoot)
                {
                    targets.Add(target);
                    targets.Sort(new TargetComparer());
                }
            }

            public void Remove(int index)
            {
                if (index < targets.Count)
                {
                    targets.Remove(targets[index]);
                }
            }

            public void Remove(HateTarget target)
            {
                lock (targets.SyncRoot)
                {
                    targets.Remove(target);
                }
            }

            public HateTarget Pop()
            {
                lock (targets.SyncRoot)
                {
                    HateTarget hatetarget = (HateTarget)targets[0];
                    Remove(0);
                    return hatetarget;
                }

            }

            public HateTarget First
            {
                get
                {
                    lock (targets.SyncRoot)
                    {
                        return (HateTarget)targets[0];
                    }
                }
            }

            public int Count
            {
                get
                {

                    return targets.Count;
                }
            }
        }


        public void Die(Actor actor)
        {
            if (this["type"].ToString() == "user")
                UserDie(actor);
            else
                MobDie(actor);

            return;
        }



        /// <summary>
        /// Actor die routine
        /// </summary>
        /// <param name="actor"></param>
        public void MobDie(Actor actor)
        {
            this.ClearTemporaryEffects();

            // actor is the killer of this mob
            // clear hate table
            while (this.hatetable.Count > 0)
            {
                this.hatetable.Pop();
            }
            this["deathticks"] = Lib.GetTime();
            this.Sayinroom(this["name"] + " falls dead!");

            if (actor != null)
            {
                actor.Send("You killed " + this["name"] + "!\r\n");
                this.Sayinroom(actor["name"] + " killed " + this["name"] + "!", actor);
            }
            else
            {
                this.Sayinroom("Someone killed " + this["name"] + "!");

            }


            this["name"] = "Corpse of " + this["name"];
            this.Save();
        }



        public void AddHate(Actor enemy, int amount)
        {
            Actor.HateTarget hatetarget;
            // Don't hate corpses!
            if (Convert.ToInt32(enemy["health"]) <= 0)
            {
                return;
            }
            hatetarget.actor = enemy;
            hatetarget.hate = 1;
            if (!hatetable.Exists(hatetarget))
            {
                if (hatetarget.actor["type"].ToString() == "user")
                {
                    hatetarget.actor.Sayinroom(this["name"] + " attacks " + hatetarget.actor["name"] + "!", hatetarget.actor);
                }
                else
                    hatetarget.actor.Sayinroom(this["name"] + " attacks " + hatetarget.actor["name"] + "!");

                this.hatetable.Add(hatetarget);
            }
            else
            {
                // Add to hate each cycle that the enemy is the mob's target
                // This ensures that if no one hits the mob that it sticks to one
                // target.
                this.hatetable.IncrementHate(hatetarget, 1);
            }
        }

        public bool Combat()
        {
            if (this["type"].ToString() == "user")
            {
                // Fix for bug where disco users still attack
                if (Lib.ConvertToBoolean(this["connected"]))
                {
                    UserCombat();
                }
            }
            else
                MobCombat();

            return true;

        }


        /// <summary>
        /// Actor combat
        /// </summary>
        /// <returns></returns>
        public bool MobCombat()
        {
            // Is mob aggressive?
            // Aggression zero mobs never attack even when attacked
            if (Convert.ToInt32(this["aggression"]) == 0)
            {
                return false;
            }
           
            // Aggression level 1 means mob only defends itself when attacked
            if (Convert.ToInt32(this["aggression"]) == 1 && this.hatetable.Count < 1)
            {
                return false;
            }

            // Is mob alive?
            if (Convert.ToInt32(this["health"]) < 1)
            {
                return false;
            }
            
            Actor room = null;

            // If so, check if there's a potential target in the room
            room = this.GetRoom();
            if (room.GetUsers().Count < 1)
            {
                // Nobody to attack
                return false;
            }
            // Add users in the room to the hatelist
            foreach (Actor user in room.GetUsers())
            {
                if (user["connected"] != null)
                {
                    // Make sure user is connected first
                    if (Lib.ConvertToBoolean(user["connected"]))
                    {
                        this.AddHate(user, 1);
                    }
                }
            }

            // Exit if mob is still not in combat
            if (this.hatetable.Count < 1)
            {
                return false;
            }

            Actor weapon1 = this.GetWeaponPrimary();
            Actor weapon2 = this.GetWeaponSecondary();
            if (weapon1 == null && weapon2 == null)
            {
                // No combat because the mob has no weapons
                return false;
            }
            long weaponspeedmodifier = Convert.ToInt32(((Convert.ToInt32(this["agility"]) * .6) / 100) - 1);
            long weapon1effectivespeed = 0;
            long weapon1effectivespeedticks = 0;
            long weapon2effectivespeed = 0;
            long weapon2effectivespeedticks = 0;
            long slowestweaponticks = 0;
            long weapon1dmg = 0;
            long weapon2dmg = 0;
            bool weapon1hit = false;
            bool weapon2hit = false;
            if (weapon1 != null)
            {
                weapon1effectivespeed = Convert.ToInt32(weapon1["speed"]) - (Convert.ToInt32(weapon1["speed"]) * weaponspeedmodifier);
                if (weapon1effectivespeed < 1) weapon1effectivespeed = 1;
                weapon1effectivespeedticks = weapon1effectivespeed * 12;
            }
            if (weapon2 != null)
            {
                weapon2effectivespeed = Convert.ToInt32(weapon2["speed"]) - (Convert.ToInt32(weapon2["speed"]) * weaponspeedmodifier);
                if (weapon2effectivespeed < 1) weapon2effectivespeed = 1;
                weapon2effectivespeedticks = weapon2effectivespeed * 12;
            }
            // Determine slowest weapon and use it for timing
            slowestweaponticks = Math.Max(weapon1effectivespeedticks, weapon2effectivespeedticks);

            // Check if time to strike for each weapon
            if (Lib.GetTime() > (Convert.ToInt64(this["lastattack"]) + slowestweaponticks))
            {
                this["lastattack"] = Lib.GetTime();
                // Look at hate list and get top target
                Actor.HateTarget hatetarget = this.hatetable.First;
                Actor targetuser = (Actor)hatetarget.actor;
                Randomizer rand = new Randomizer();
                weapon1hit = this.CheckHit(hatetarget);
                // If either weapon hit
                if (weapon1hit || weapon2hit)
                {
                    if (weapon1 != null)
                    {
                        // Calculate damage done
                        weapon1dmg = rand.getrandomnumber(Convert.ToInt32(weapon1["damagemin"]), Convert.ToInt32(weapon1["damagemax"]));
                        // If target is still in the room
                        if (hatetarget.actor["container"].ToString() == this["container"].ToString())
                        {
                            hatetarget.actor.SendError(this["shortnameupper"] + "'s " + weapon1["name"] + " hits you! ");

                            if (hatetarget.actor["type"].ToString() == "user")
                            {
                                hatetarget.actor.Sayinroom(this["shortnameupper"] + "'s " + weapon1["name"] + " hits " + hatetarget.actor["name"] + "!", hatetarget.actor);
                            }
                            else
                                hatetarget.actor.Sayinroom(this["shortnameupper"] + "'s " + weapon1["name"] + " hits " + hatetarget.actor["name"] + "!");

                            hatetarget.actor.RemoveHealth(this, Convert.ToInt32(weapon1dmg), 0);
                            if (Convert.ToInt32(hatetarget.actor["health"]) < 1)
                            {
                                this.hatetable.Pop();
                                return false;
                            }
                        }
                        else
                        {
                            this.hatetable.Pop();
                            return false;
                        }
                    }
                    if (weapon2 != null)
                    {
                        // Calculate damage done
                        weapon2dmg = rand.getrandomnumber(Convert.ToInt32(weapon2["damagemin"]), Convert.ToInt32(weapon2["damagemax"]));
                        // If target is still in the room
                        if (hatetarget.actor["container"].ToString() == this["container"].ToString())
                        {
                            hatetarget.actor.SendError(this["shortnameupper"] + "'s " + weapon2["name"] + " hits you! ");
                            if (hatetarget.actor["type"].ToString() == "user")
                            {
                                hatetarget.actor.Sayinroom(this["shortnameupper"] + "'s " + weapon2["name"] + " hits " + hatetarget.actor["name"] + "!", hatetarget.actor);
                            }
                            else
                                hatetarget.actor.Sayinroom(this["shortnameupper"] + "'s " + weapon2["name"] + " hits " + hatetarget.actor["name"] + "!");
                            hatetarget.actor.RemoveHealth(this, Convert.ToInt32(weapon2dmg), 0);
                            if (Convert.ToInt32(hatetarget.actor["health"]) < 1)
                            {
                                this.hatetable.Pop();
                                return false;
                            }
                        }
                        else
                        {
                            this.hatetable.Pop();
                            return false;
                        }
                    }
                }
                else
                {
                    hatetarget.actor.Send(this["shortnameupper"] + "'s " + weapon1["name"] + " misses you!\r\n");
                    if (hatetarget.actor["type"].ToString() == "user")
                    {
                        this.Sayinroom(this["shortnameupper"] + "'s " + weapon1["name"] + " misses " + hatetarget.actor["name"] + "!", hatetarget.actor);
                    }
                    else
                        this.Sayinroom(this["shortnameupper"] + "'s " + weapon1["name"] + " misses " + hatetarget.actor["name"] + "!");
                }
                if (Convert.ToInt32(hatetarget.actor["health"]) < 1 || hatetarget.actor["container"].ToString() != this["container"].ToString())
                {
                    this.hatetable.Pop();
                    return false;
                }
            }
            return true;
        }



        public bool CheckHit(HateTarget target)
        {
            Randomizer rand = new Randomizer();
            // Calculate chance to hit
            if (Convert.ToInt32(this["agility"]) > Convert.ToInt32(target.actor["agility"]))
            {
                // Attacker agility beats target
                long hit = rand.getrandomnumber(1, Convert.ToInt32(this["agility"]));
                if (hit < Convert.ToInt32(target.actor["agility"]))
                {
                    // HIT!
                    return true;
                }
            }
            if (Convert.ToInt32(this["agility"]) < Convert.ToInt32(target.actor["agility"]))
            {
                // Target agility beats attacker
                long hit = rand.getrandomnumber(1, Convert.ToInt32(target.actor["agility"]));
                if (hit < Convert.ToInt32(this["agility"]))
                {
                    // HIT!
                    return true;
                }
            }
            if (Convert.ToInt32(this["agility"]) == Convert.ToInt32(target.actor["agility"]))
            {
                // Even agility match
                long hit = rand.getrandomnumber(1, 100);
                if (hit <= 50)
                {
                    // HIT!
                    return true;
                }
            }
            return false;
        }



        public void Respawn()
        {
            Actor room = Lib.GetByID(this["container"].ToString());
            // Mobs just respawn where they died for now
            Actor respawnlocation = room;

            // Once code is added to locate respawn rooms, these lines will be useful
            // Remove user from current room
            room.Removeitem(this);
            // Add user to new room
            respawnlocation.Additem(this);



            this["name"] = Convert.ToString(this["name"]).Replace("Corpse of ", "");
            this["health"] = Convert.ToInt32(this["healthmax"]);
            this["stamina"] = Convert.ToInt32(this["staminamax"]);
            this["mana"] = Convert.ToInt32(this["manamax"]);
            this.Save();
            this.Sayinroom(this["name"] + " appears, having been reanimated from death!");
        }

        /// <summary>
        /// Displays the contents of this Actor instance to the Actor provided as a parameter.
        /// </summary>
        /// <param name="user"></param>
        public void ShowContents(Actor user)
        {
            bool itemfound = false;
            string message;

            // Show contents of non-users and non-mobs like a box would, meaning everything.
            if (this["type"].ToString() != "user" && this["type"].ToString() != "mob")
            {
                message = "The " + this["name"] + " contains:\r\n";
                foreach (Actor item in this.GetContents())
                {
                    itemfound = true;
                    if (Convert.ToInt32(item["quantity"]) > 1)
                    {
                        message += item["quantity"] + " " + item["name"] + "s";
                    }
                    else
                    {
                        message += item.GetNameFull().Substring(0, 1).ToUpper() + item.GetNameFull().Substring(1);
                    }
                    if (Lib.ConvertToBoolean(this["equipped"]))
                    {
                        message += " (Equipped)";
                    }
                    message += "\r\n";
                }
                if (itemfound == false)
                {
                    message += "Nothing";
                }
                user.Send(message + "\r\n");
            }
            else
            {
                // Looking at a user or mob
                // Users and mobs only show their contents that are equipped.
                message = this.GetNameFull() + " is wearing/wielding:\r\n";
                foreach (Actor item in this.GetContents())
                {
                    if (Lib.ConvertToBoolean(item["equipped"]))
                    {
                        itemfound = true;
                        if (Convert.ToInt32(item["quantity"]) > 1)
                        {
                            message += item["quantity"] + " " + item["name"] + "s";
                        }
                        else
                        {
                            message += item.GetNameFullUpper();
                        }
                    }
                }
                if (itemfound == false)
                {
                    message += "Nothing";
                }
                user.Send(message + "\r\n");
            }

        }


        public string GetNamePrefixUpper() // return the name prefix (a or an) with the first character uppercase for sentences and such
        {
            if (this["nameprefix"] == null) return "";
            if (this["nameprefix"].ToString().Length < 1) return "";
            return this["nameprefix"].ToString().Substring(0, 1).ToUpper() + this["nameprefix"].ToString().Substring(1, this["nameprefix"].ToString().Length - 1);
        }


        /// <summary>
        /// Use this when you want to Destroy an item from the game.
        /// The item is deleted from both memory and database. It's gone forever.
        /// </summary>
        public void Destroy()
        {
            // Remove item from it's container first
            Actor container = this.GetContainer();
            container.Removeitem(this);

            // Now delete the item from memory
            lock (Lib.actors.SyncRoot)
            {
                Lib.actors.Remove(this);
            }
            // Delete the item's row from the db
            try
            {
                Lib.dbService.Actor.DeleteActor(this["id"].ToString());
            }
            catch (Exception ex)
            {
                Lib.PrintLine("Deleting item from the database failed. The error was: " + ex.Message + ex.StackTrace);
                Lib.PrintLine("The item that failed was: " + this["id"].ToString());
            }
        }

    }
}
