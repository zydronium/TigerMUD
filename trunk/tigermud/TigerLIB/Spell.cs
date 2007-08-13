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

namespace TigerMUD
{
    /// <summary>
    /// This class defines spells.
    /// </summary>
    public class Spell
    {
        

        /// <summary>
        /// This is the state table for code expansion of spells and home for any
        /// hashtables that are part of the class.
        /// </summary>
        //public Hashtable State = new Hashtable();

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

        /// <summary>
        /// Class constructor.
        /// </summary>
        public Spell()
        {
            this["id"] = System.Guid.NewGuid().ToString();
        }


        /// <summary>
        /// Function that loads spells from the mudspells database table.
        /// </summary>
        /// <returns>An ArrayList of spells from the database</returns>
        public static ArrayList LoadAll()
        {
            int counter = 0;
            try
            {
                DataTable dt = Lib.dbService.Library.GetAllSpells();

                foreach (DataRow datarow in dt.Rows)
                {
                    Spell spell = new Spell();
                    spell["id"] = System.Guid.NewGuid();
                    spell.Loadstate();

                    lock (Lib.spells.SyncRoot)
                    {
                        Lib.spells.Add(spell);
                    }

                    // Any changes after this point cause this spell to be saved to db on Save command
                    spell.oldversion = spell.version;

                    counter++;
                }
            }
            catch (Exception ex)
            {
                Lib.log.Add("Loadspellsfromdb function", "EXCEPTION " + ex.Message + ex.StackTrace);
            }
            return Lib.spells;
        }

        public bool Stateexists(string Name)
        {
            return Lib.dbService.Spell.StateExists(this["id"].ToString(), Name);
        }

        /// <summary>
        /// Load and set proper data type for actor states. You do not have to use the DataTable that gets returned.
        /// </summary>
        public DataTable Loadstate()
        {
            DataTable dt = Lib.dbService.Spell.LoadState(this["id"].ToString());
            if (dt == null)
            {
                return null;
            }
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow datarow in dt.Rows)
                {
                    // Get states from the db
                    string name = (string)datarow["name"];
                    string setting = (string)datarow["setting"];
                    string datatype = (string)datarow["datatype"];

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

                    if (name.StartsWith("castermod"))
                    {
                        // Strip the "castermod" prefix and store the name as whatever text follows it,
                        // like the word health for example. Turn "castermodhealth" into "health".
                        Castermod.Add(name.Replace("castermod", ""), setting);
                    }
                    if (name.StartsWith("targetmod"))
                    {
                        // Strip the "targetmod" prefix and store the name as whatever text follows it,
                        // like the word health for example. Turn "targetmodhealth" into "health".
                        Targetmod.Add(name.Replace("targetmod", ""), setting);
                    }
                    if (name.StartsWith("reagentsconsumable"))
                    {
                        // Add the consumable reagents for this spell.
                        ReagentsConsumable.Add(name.Replace("reagentsconsumable", ""), setting);
                    }
                    if (name.StartsWith("reagentsstatic"))
                    {
                        // Add the static reagents for this spell.
                        ReagentsStatic.Add(name.Replace("reagentsstatic", ""), setting);
                    }
                }
            }
            return dt;
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
                dt = Lib.dbService.Library.GetAllSpells();
            }
            catch (Exception ex)
            {
                throw;
            }

            foreach (DataRow datarow in dt.Rows)
            {
                // Create user to populate with data
                Spell spell = new Spell();
                spell["id"] = Convert.ToString(datarow["id"]);

                // Load all actor states
                DataTable table = spell.Loadstate();

                lock (Lib.spells.SyncRoot)
                {
                    Lib.spells.Add(spell);
                }

                // Get current count of object type
                int loadstat = 0;
                loadstat = Convert.ToInt32(loadstats[spell["type"].ToString()]);
                loadstat += 1;
                loadstats[spell["type"].ToString()] = loadstat;

                //actor.LoadActionWordsFromDatabase();
                //actor.LoadCommandWordsFromDatabase();

                spell.OldVersion = spell.Version;

                counter++;
            }
            return Lib.spells;

        }

        /// <summary>
        /// Checks if a user meets the requirements to cast this spell.
        /// </summary>
        /// <param name="spellname"></param>
        /// <param name="user"></param>
        /// <returns>The spell</returns>
        public bool CanCast(Actor user)
        {
            // Is the caster alive?
            if (!user.Alive())
            {
                user.SendError("You cannot cast while dead.\r\n");
                return false;
            }

            // Did they not specify a target? Make sure they have a current target then.
            if (user["target"].ToString() == "")
            {
                user.SendError("You have no target to cast on.\r\n");
                return false;
            }

            // spell exists, but do they know the spell?
            bool spellcheck = user.IsKnownSpell(this);
            if (!spellcheck)
            {
                user.SendError("You don't know the spell '" + this["name"] + "'.\r\n");
                return false;
            }

            // Have the skill to cast this?
            bool skillcheck = user.HasSkill("skillknown_" + this["skillname"].ToString());
            if (!skillcheck)
            {
                user.SendError("You don't have the " + this["skillname"] + " skill needed to cast this spell.\r\n");
                return false;
            }
            // Have minimum skill level to cast?
            int skilllevelcheck = user.GetSkillLevel("skillknown_" + this["skillname"]);
            if (Convert.ToInt32(this["skillmin"]) > skilllevelcheck)
            {
                user.SendError("You don't have the minimum " + this["skillname"] + " skill level to cast this spell.\r\n");
                return false;
            }
            // Enough mana to cast?
            if ((Convert.ToInt32(user["mana"]) - Convert.ToInt32(this["manacost"].ToString())) < 0)
            {
                user.SendError("You do not have enough mana to cast this spell.\r\n");
                return false;
            }

            // All checks passed!
            return true;
        }

        public bool BeginCast(Actor caster)
        {
            // Check if a spell is pending
            if (caster["spellpending"].ToString() != "")
            {
                caster.SendAlertBad("You are already casting " + Spell.GetByID(caster["spellpending"].ToString())["name"] + ".\r\n" + caster["colorcommandtext"]);
                return false;
            }

            // Check if we are in spell cooldown
            if (Lib.GetTime() < Convert.ToInt64(caster["spellcooldownticks"]))
            {
                caster.SendAlertBad("You are still recovering from the last spell.\r\n" + caster["colorcommandtext"]);
                return false;
            }

            // Set spell being cast and windup
            caster["spellpending"] = this["id"];
            caster["spellwindupticks"] = Lib.GetTime() + Convert.ToInt32(this["windup"]);
            caster.SendAlertGood("You begin casting " + this["name"] + ".\r\n");
            return false;

        }

        public bool Cast(Actor caster, Actor target)
        {
            if (target == null)
            {
                return false;
            }

            if (caster["spellpending"].ToString() == "")
            {
                return false;
            }

            int amount = 0;
            // Freeze user for the duration of the spell windup time
            caster["spellwindupticks"] = Lib.GetTime() + Convert.ToInt32(this["windup"]);

            // All checks pass so cast spell
            caster.Sayinroom(caster["name"] + " " + this["displayroom"]);
            if (target["id"].ToString() == caster["id"].ToString())
            {
                caster.SendAlertGood("You cast " + this["name"] + " on yourself.\r\n");
                target.SendAlertGood("Your " + this["displaytarget"] + "\r\n");

            }
            else
            {
                caster.SendAlertGood("You cast " + this["name"] + " on " + target["name"] + ".\r\n");
                target.SendAlertGood(caster["name"] + "'s " + this["displaytarget"] + "\r\n");
            }

            // These are helpful spells and will not start combat
            if (this["type"].ToString() == "buff")
            {
                // Get stats that this spell affects
                System.Collections.IEnumerator names = this.states.Keys.GetEnumerator();
                while (names.MoveNext())
                {
                    string name = (string)names.Current;
                    switch (name)
                    {
                        case "health":
                            amount = Convert.ToInt32(this[name]);
                            target.AddHealth(caster, amount, Convert.ToInt32(this["duration"]));
                            break;
                        case "mana":
                            amount = Convert.ToInt32(this[name]);
                            target.AddMana(caster, amount, Convert.ToInt32(this["duration"]));
                            break;
                        case "stamina":
                            amount = Convert.ToInt32(this[name]);
                            target.AddStamina(caster, amount, Convert.ToInt32(this["duration"]));
                            break;
                        case "healthmax":
                            amount = Convert.ToInt32(this[name]);
                            target.AddHealthMax(caster, amount, Convert.ToInt32(this["duration"]));
                            break;
                        case "manamax":
                            amount = Convert.ToInt32(this[name]);
                            target.AddManaMax(caster, amount, Convert.ToInt32(this["duration"]));
                            break;
                        case "staminamax":
                            amount = Convert.ToInt32(this[name]);
                            target.AddStaminaMax(caster, amount, Convert.ToInt32(this["duration"]));
                            break;
                        case "strength":
                            amount = Convert.ToInt32(this[name]);
                            target.AddStrength(caster, amount, Convert.ToInt32(this["duration"]));
                            break;
                        case "intellect":
                            amount = Convert.ToInt32(this[name]);
                            target.AddIntellect(caster, amount, Convert.ToInt32(this["duration"]));
                            break;
                        case "spirit":
                            amount = Convert.ToInt32(this[name]);
                            target.AddSpirit(caster, amount, Convert.ToInt32(this["duration"]));
                            break;
                        case "agility":
                            amount = Convert.ToInt32(this[name]);
                            target.AddAgility(caster, amount, Convert.ToInt32(this["duration"]));
                            break;
                    }
                }
            }
            // Handle debuff spells (these are offensive spells that will start combat)
            if (this["type"].ToString() == "debuff")
            {
                // get stats that this spell modifies
                System.Collections.IEnumerator names = this.states.Keys.GetEnumerator();
                while (names.MoveNext())
                {
                    string name = (string)names.Current;
                    amount = 0;

                    // Add caster to the hatetable if target is a mob (only mobs have hate tables)
                    if (target["type"].ToString() == "mob")
                    {
                        Actor targetmob = target;
                        targetmob.AddHate(caster, amount);
                    }

                    switch (name)
                    {
                        case "health":
                            amount = Convert.ToInt32(this[name]);
                            target.RemoveHealth(caster, amount, Convert.ToInt32(this["duration"]));
                            break;
                        case "mana":
                            amount = Convert.ToInt32(this[name]);
                            target.RemoveMana(caster, amount, Convert.ToInt32(this["duration"]));
                            break;
                        case "stamina":
                            amount = Convert.ToInt32(this[name]);
                            target.RemoveStamina(caster, amount, Convert.ToInt32(this["duration"]));
                            break;
                        case "healthmax":
                            amount = Convert.ToInt32(this[name]);
                            target.RemoveHealthMax(caster, amount, Convert.ToInt32(this["duration"]));
                            break;
                        case "manamax":
                            amount = Convert.ToInt32(this[name]);
                            target.RemoveManaMax(caster, amount, Convert.ToInt32(this["duration"]));
                            break;
                        case "staminamax":
                            amount = Convert.ToInt32(this[name]);
                            target.RemoveStaminaMax(caster, amount, Convert.ToInt32(this["duration"]));
                            break;
                        case "strength":
                            amount = Convert.ToInt32(this[name]);
                            target.RemoveStrength(caster, amount, Convert.ToInt32(this["duration"]));
                            break;
                        case "intellect":
                            amount = Convert.ToInt32(this[name]);
                            target.RemoveIntellect(caster, amount, Convert.ToInt32(this["duration"]));
                            break;
                        case "spirit":
                            amount = Convert.ToInt32(this[name]);
                            target.RemoveSpirit(caster, amount, Convert.ToInt32(this["duration"]));
                            break;
                        case "agility":
                            amount = Convert.ToInt32(this[name]);
                            target.RemoveAgility(caster, amount, Convert.ToInt32(this["duration"]));
                            break;
                    }
                }
            }

            // Deal with spells that damage over time, like a poison
            if (this["type"].ToString() == "poison")
            {

                if (this["health"] != null)
                {
                    target["healthpoisonduration"] = Convert.ToInt32(this["duration"]);
                    target["healthpoisonendticks"] = Lib.GetTime() + Convert.ToInt32(this["duration"]);
                    target["healthpoisondamage"] = Convert.ToInt32(this["health"]);
                    target["healthpoisonfrequency"] = Convert.ToInt32(this["frequency"]);
                    target["healthpoisonlastticks"] = Lib.GetTime();
                    target["healthpoisoncaster"] = caster["id"].ToString();
                }

                if (this["stamina"] != null)
                {
                    target["staminapoisonduration"] = Convert.ToInt32(this["duration"]);
                    target["staminapoisonendticks"] = Lib.GetTime() + Convert.ToInt32(this["duration"]);
                    target["staminapoisondamage"] = Convert.ToInt32(this["stamina"]);
                    target["staminapoisonfrequency"] = Convert.ToInt32(this["frequency"]);
                    target["staminapoisonlastticks"] = Lib.GetTime();
                    target["staminapoisoncaster"] = caster["id"].ToString();
                }

                if (this["mana"] != null)
                {
                    target["manapoisonduration"] = Convert.ToInt32(this["duration"]);
                    target["manapoisonendticks"] = Lib.GetTime() + Convert.ToInt32(this["duration"]);
                    target["manapoisondamage"] = Convert.ToInt32(this["mana"]);
                    target["manapoisonfrequency"] = Convert.ToInt32(this["frequency"]);
                    target["manapoisonlastticks"] = Lib.GetTime();
                    target["manapoisoncaster"] = caster["id"].ToString();
                }
            }

            // Deduct the spell mana cost from the caster
            caster["mana"] = Convert.ToInt32(caster["mana"]) - Convert.ToInt32(this["manacost"]);

            // Set spell cooldown
            caster["spellpending"] = "";
            caster["spellcooldownticks"] = Lib.GetTime() + Convert.ToInt32(this["cooldown"]);
            return true;
        }

        /// <summary>
        /// Save all of this actor's states to the database.
        /// </summary>
        public void Savestates()
        {
            // Don't save a spell if it hasn't changed since being loaded
            if (this.Version==this.OldVersion)
            {
                return;
            }

            // Set any values you want to reset before saving the spell

            // Important performance step here.
            // Don't save the states unless they've changed.
            // This is especially important on MS Access databases
            // since writes are hundreds of times slower than reads.

            Hashtable testht = new Hashtable();
            DataTable testdt = Lib.dbService.Spell.LoadState(this["id"].ToString());

            // Get states from db and load into test hashtable
            foreach (DataRow datarow in testdt.Rows)
            {
                // Get states from the db
                string name = (string)datarow["name"];
                string setting = (string)datarow["setting"];
                string datatype = (string)datarow["datatype"];

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

            //System.Collections.IEnumerator names = states.Keys.GetEnumerator();


            // TODO occasional Exceptions here about collection "State" was modified
            while (names.MoveNext())
            {
                string name = (string)names.Current;

                // Nulls can happen when a new state is added but not saved yet
                if (testht[name] != null)
                {
                    if (this[name].ToString() != testht[name].ToString())
                    {
                        Lib.dbService.Spell.SaveSpellState(this,
                            name);
                    }
                }
                else
                {
                    // Detected a new state, so add it to the database
                    Lib.dbService.Spell.SaveSpellState(this,
                            name);
                }
            }
        }

        public void Savestate()
        {
            System.Collections.IEnumerator names = states.Keys.GetEnumerator();
            while (names.MoveNext())
            {
                string name = (string)names.Current;
                Lib.dbService.Spell.SaveSpellState(this,
                    name);
            }
        }


        /// <summary>
        /// Completely save all of an spell's data
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
                Lib.PrintLine("Saving spell to the database failed. The error was: " + ex.Message + ex.StackTrace);
                throw ex;
            }
        }


        /// <summary>
        /// Some spells use one or more reagents that are consumed when you cast the spell.
        /// The name is the reagent and the value is the number of units consumed.
        /// </summary>
        public Hashtable ReagentsConsumable = new Hashtable();

        /// <summary>
        /// Some spells use one or more reagents that must be in possession but are not consumed when you cast the spell.
        /// The name is the reagent and the value is the number of units required.
        /// </summary>
        public Hashtable ReagentsStatic = new Hashtable();

        /// <summary>
        /// Returns a spell that has the name given.
        /// </summary>
        public static Spell GetByID(string id)
        {
            for (int i = Lib.spells.Count - 1; i >= 0; i--)
            {
                Spell scanspell = (Spell)Lib.spells[i];
                if (id == scanspell["id"].ToString())
                {
                    return scanspell;
                }
            }
            // No spell exists with that name
            return null;
        }

        /// <summary>
        /// Some spells have a modifier to the stats of the target.
        /// Name is the name of the stat preceded by "targetmod" followed by the name of the stat
        /// that the target gets a modifier to. For example: targetmodhealth
        /// Next is the setting which is the numeric modifier amount for that stat (positive or negative).
        /// For example: targetmodhealth -100
        /// This means the only effect to the target is this spell removes 100 of the target's health.
        /// So this is essentially a combat spell.
        /// There can be stacks of modifiers depending on the spell and what it does.
        /// </summary>
        protected Hashtable Targetmod = new Hashtable();

        /// <summary>
        /// Returns a boolean to tell if there are any target modifiers (there almost always are). 
        /// </summary>
        public bool IsTargetMod()
        {
            if (Targetmod.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Returns the arraylist of bonuses that the target of the spell gets.
        /// </summary>
        public Hashtable GetTargetMod()
        {
            return Targetmod;
        }

        /// <summary>
        /// Some spells have a modifier to the stats of the caster.
        /// XXXName is the name of the stat preceded by "castermod" followed by the name of the stat
        /// that the caster gets a modifier to
        /// Next is the setting which is the numeric modifier amount for that stat (positive or negative).
        /// For example: castermodmana -100
        /// This means the only effect to the caster is this spell uses 100 mana.
        /// There can be stacks of modifiers depending on the spell and what it does.
        /// </summary>
        protected Hashtable Castermod = new Hashtable();

        /// <summary>
        /// Returns a boolean to tell if there are any caster modifiers (most common is decrease mana). 
        /// </summary>
        public bool IsCasterMod()
        {
            if (Castermod.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Returns the arraylist of stat modifiers that the caster of the spell gets. 
        /// </summary>
        public Hashtable GetCasterMod()
        {
            return Castermod;
        }

     
    }
}
