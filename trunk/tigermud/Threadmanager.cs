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
using System.Data.Odbc;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;

using TigerMUD.CommsLib;

namespace TigerMUD
{
    // All MUD threads are launched and controlled from this class
    public class Threadmanager
    {
        private Thread timerthread;
        private string path;
        private IConnectionListener tcpListener;
        //private IConnectionListener msnListener;
        string errors = null;

        public Threadmanager()
        {
            path = AppDomain.CurrentDomain.BaseDirectory + @"tigermud.xml";
            tcpListener = null;
            //msnListener = null;
        }

        #region Startup Routine
        /// <summary>
        /// Starts the MUD engine
        /// </summary>
        /// <param name="isService">True if the MUD engine is running as a service, false otherwise.</param>
        public void Start(bool isService)
        {
            // Set all the folder paths

            IPHostEntry iphostentry = Dns.Resolve(Dns.GetHostName());
            Lib.PrintLine("TigerMUD version " + Lib.Serverversion + " started.");

            // GPL Compliance
            Lib.PrintLine("TigerMUD version " + Lib.Serverversion + ", Copyright (c) 2004 Adam Miller et al.");
            Lib.PrintLine("TigerMUD comes with ABSOLUTELY NO WARRANTY; for details see tigermud_license.txt");

            Lib.PrintLine("TigerMUD Started.");
            Lib.Print("Engine is running as a:\t\t");
            if (isService)
            {
                Lib.PrintLine("Service");
            }
            else
            {
                Lib.PrintLine("Console Application");
            }
            Lib.PrintLine("Server name: \t\t\t" + Dns.GetHostName());
            Lib.PrintLine("IP address: \t\t\t" + iphostentry.AddressList[0].ToString());
            Lib.PrintLine("Port number: \t\t\t" + Lib.Serverinfo.ServerPort);
            Lib.PrintLine("Database servername: \t\t" + Lib.Serverinfo.DatabaseServer);
            Lib.PrintLine("Database name: \t\t\t" + Lib.Serverinfo.DatabaseName);
            Lib.Print("Database type: \t\t\t");
            if (Lib.Serverinfo.DatabaseType.ToLower() == "mssql")
            {
                Lib.PrintLine("Microsoft SQL Server");
            }
            else if (Lib.Serverinfo.DatabaseType.ToLower() == "mysql")
            {
                Lib.PrintLine("MySQL Server");
            }
            else if (Lib.Serverinfo.DatabaseType.ToLower() == "msaccess")
            {
                Lib.PrintLine("Microsoft Access");
            }
            else
            {
                Lib.PrintLine("*** INVALID OPTION *** Check tigermud.xml for an error.");
                throw new Exception("*** INVALID OPTION *** Check tigermud.xml for an error.");
            }

            // SQL statement logging
            Lib.Print("Dump SQL statements to console:\t");
            if (Lib.Serverinfo.ShowSQLStatements)
            {
                Lib.PrintLine("Yes");
            }
            else
            {
                Lib.PrintLine("No");
            }

            Lib.PrintLine("Welcome screen file: \t\t" + Lib.Serverinfo.Welcomescreen);
            Lib.PrintLine("Idle disconnect (minutes): \t" + Lib.Serverinfo.Idledisconnect / 600);
            Lib.Print("Email activation for accounts: \t");
            if (Lib.Serverinfo.EmailActivation == "true")
            {
                Lib.PrintLine("ON");
            }
            else
            {
                Lib.PrintLine("OFF");
            }
            // Profanity Filter
            Lib.Print("Profanity filter is: \t\t");
            if (ProfanityFilter.EnableFilter)
            {
                Lib.PrintLine("Enabled");
            }
            else
            {
                Lib.PrintLine("Disabled");
            }

            Lib.PrintLine("SMTP server name: \t\t" + Lib.Serverinfo.SmtpServer);
            Lib.Print("Server console output to: \t");

            if (Lib.Serverinfo.ServerMode == "service")
            {
                Lib.PrintLine("TigerMUD.log");
            }
            else
            {
                Lib.PrintLine("Console");
                Lib.PrintLine("Also sending console output to: " + Path.GetFullPath(Path.Combine(Lib.PathtoRoot, Lib.serverLogFileName)));
            }
            Lib.PrintLine();

            InitDatabase();
            InitStoryDatabase();
            Lib.LoadServerState();
            Lib.PrintLine("Loaded server state table.");

            // If Gametime doesn't exist, add it.
            if (Lib.ServerState["serverticks"] == null)
            {
                Lib.ServerState["serverticks"] = 0;
            }
            // If Gametime doesn't exist, add it.
            if (Lib.ServerState["lastserverticks"] == null)
            {
                Lib.ServerState["lastserverticks"] = 0;
            }

            // Scheduled tasks loaded here.
            // Each of these tasks must add themselves back into the Delayedcommands array at the end
            // of their run, or they won't repeat, meaning that they only run once.
            // These are all special system commands that are not available to users,
            // because they are not added to the mudcommands db table.
            // WARNING: You can easily create thread deadlocks if you set any of these
            // tasks to run too fast (duration between events too short). Many of them lock resources,
            // and any other threads needing those resources might never get access if your task 
            // keeps them locked by running too fast. This is easily to repro when you put no delay in a task.

            // About Game Time
            // 1 second in real time is 12 seconds game time. A server tick is 1 tenth (1/10) of a real second.
            // So a handy reference for measuring game hours, days, weeks, etc. is below.
            // delay in tenths of seconds, so 50 = 5 seconds.

            Lib.Delayedcommands.Add(new Actor(), "timer_Gametime", "", Lib.GetTime(), 10, true);
            Lib.Delayedcommands.Add(new Actor(), "timer_movesunmoon", "", Lib.GetTime(), 200, true);
            Lib.Delayedcommands.Add(new Actor(), "timer_idledisconnect", "", Lib.GetTime(), 350, true);
            Lib.Delayedcommands.Add(new Actor(), "timer_movemobs", "", Lib.GetTime(), 450, true);
            Lib.Delayedcommands.Add(new Actor(), "timer_regencycle", "", Lib.GetTime(), 30, true);
            Lib.Delayedcommands.Add(new Actor(), "timer_npccombat", "", Lib.GetTime(), 10, true);
            Lib.Delayedcommands.Add(new Actor(), "timer_playercombat", "", Lib.GetTime(), 10, true);
            Lib.Delayedcommands.Add(new Actor(), "timer_cyclestates", "", Lib.GetTime(), 10, true);
            Lib.Delayedcommands.Add(new Actor(), "timer_growplants", "", Lib.GetTime(), 10, true);

            //Lib.PrintLine("Delayed commands loaded: \r\n" + Lib.Delayedcommands.Display());

            Lib.Gametime = Lib.Gametime.AddMinutes(Lib.GetTime() / 50);
            try
            {
                InitScriptCompiler();
                errors = InitCommands();
                if (errors != "")
                {
                    Lib.PrintLine(errors);
                }
                Lib.PrintLine("Loaded " + Lib.Commands.Count + " commands.");
                LoadAllCommandFromDatabase();
            }
            catch (Exception ex)
            {
                Lib.PrintLine(DateTime.Now + " EXCEPTION in Threadmanager.Start (Loading commands): " + ex.Message + ex.StackTrace);
            }

            try
            {
                // Reset error display
                errors = "";
                errors = InitActions();
                if (errors != "")
                {
                    Lib.PrintLine(errors);
                }
                Lib.PrintLine("Loaded " + Lib.Actions.Count + " actions.");
            }
            catch (Exception ex)
            {
                Lib.PrintLine(DateTime.Now + " EXCEPTION in Threadmanager.Start (Loading actions): " + ex.Message + ex.StackTrace);
            }


            Hashtable loadstats = new Hashtable();
            // Load all actors
            try
            {
                Lib.PrintLine("Loading actors...");
                Lib.actors = Actor.LoadAll(ref loadstats);
            }
            catch (Exception ex)
            {
                throw new Exception("SERVER CRASH. Cannot load actors from database. Fatal error. The error was:\r\n" + ex.Message + ex.StackTrace);
            }

            // Cycle through the hashtable to display number of actors loaded by type
            System.Collections.IEnumerator names = loadstats.Keys.GetEnumerator();
            while (names.MoveNext())
            {
                string name = (string)names.Current;
                Lib.PrintLine("Loaded " + loadstats[name] + " " + name + "s.");
            }

            // Reset counter
            loadstats.Clear();

            Lib.PrintLine("Loading spells...");

            // Load spells from the database and provide hashtable for counting all types loaded
            try
            {
                Lib.spells = Spell.LoadAll(ref loadstats);
            }
            catch (Exception ex)
            {
                throw new Exception("SERVER CRASH. Cannot load spells from database. Fatal error. The error was:\r\n" + ex.Message + ex.StackTrace);
            }

            // Cycle through the hashtable to display number of actors loaded by type
            names = loadstats.Keys.GetEnumerator();
            while (names.MoveNext())
            {
                string name = (string)names.Current;
                Lib.PrintLine("Loaded " + loadstats[name] + " " + name + "s.");
            }

            // Assign spells to actors who know them
            foreach (Actor actor in Lib.actors)
            {
                actor.LoadSpells();
            }

            // Put all items into their containers
            Lib.PopulateContainers(Lib.actors);
            Lib.PrintLine("Populated all containers.");

            // Load markets up (Each market you define must appear here)
            Lib.market.LoadMarket();
            Lib.PrintLine("Loaded the market.");

            Timer timer = new Timer();
            timer.Heartbeat += new Heartbeateventhandler(timer_Heartbeat);
            timer.Heartbeat += new Heartbeateventhandler(DoTimedCommands);
            timerthread = new Thread(new ThreadStart(timer.clock));
            timerthread.Start();

            // Load welcome screen
            Lib.Print("Loading welcome screen...");
            string path = Path.GetFullPath(Path.Combine(Lib.PathtoRoot, Lib.Serverinfo.Welcomescreen));
            Lib.PrintLine(path);
            try
            {
                // try current directory first
                Lib.Welcomescreen = Lib.Readfile(path);
            }
            catch (Exception ex)
            {
                Lib.PrintLine(DateTime.Now + " EXCEPTION in Threadmanager.Start (Welcome screen): " + ex.Message + ex.StackTrace);
                System.Exception exception = new Exception("Cannot find welcome screen file. Server load failed.");
                throw exception;
            }
            Lib.Welcomescreen += Lib.Ansifboldwhite + "TigerMUD codebase version " + Lib.Serverversion + "\r\n\r\n";
            Lib.PrintLine("Succeeded.");

            // Create a new connection handler
            if (InitConnectionHandlers())
            {
                Lib.PrintLine();
                Lib.PrintLine("Ready for connections...");
            }
        }

        private bool InitConnectionHandlers()
        {
            Lib.PrintLine("Starting connection handlers...");
            bool anyActiveListeners = false;

            if (Lib.Serverinfo.TcpSocketEnabled)
            {
                tcpListener = ConnectionListenerFactory.CreateTcpConnectionListener(Lib.Serverinfo.TcpSocketPort);
                tcpListener.Start();
                Lib.PrintLine("Started TcpListener on port " + Lib.Serverinfo.TcpSocketPort + ".");
                anyActiveListeners = true;
            }

            //if (Lib.Serverinfo.MsnSocketEnabled)
            //{
            //    msnListener = ConnectionListenerFactory.CreateMsnConnectionListener(Lib.Serverinfo.MsnSocketSignonEmail,
            //        Lib.Serverinfo.MsnSocketPassword);
            //    msnListener.Start();
            //    Lib.PrintLine("Started MsnListener with login " + Lib.Serverinfo.MsnSocketSignonEmail + ".");
            //    anyActiveListeners = true;
            //}

            if (!anyActiveListeners)
            {
                Lib.PrintLine("Fatal Error! No active connection listeners. Please enable one in TigerMUD.xml");
            }

            return anyActiveListeners;
        }

        private void InitDatabase()
        {
            // Lib.database = new Database(Lib.Serverinfo);
            Lib.Conn.ConnectionString = Lib.Getconnstring();
            Lib.Dbcommand.Connection = Lib.Conn;
            Lib.Conn.Open();

            // Initialise the DbService
            Lib.dbService = new TigerMUD.DatabaseLib.DbService(Lib.dbConnectionString);
            // Set the Log Statements option
            Lib.dbService.LogStatements = Lib.Serverinfo.ShowSQLStatements;
        }

        // for Story manager
        private void InitStoryDatabase()
        {
            //string filename = Lib.PathtoRoot + "story.mdb";
            //new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filename);

            Lib.StoryConn.ConnectionString = Lib.GetStoryconnstring();                 
            Lib.StoryDbCommand.Connection = Lib.StoryConn;
            Lib.StoryConn.Open();

            // Transfer this somewhere. I dunno where yet. So far I will compile all database calls to two functions
            // At InitStoryDatabase and at CommitStoryDatabase
            Lib.StoryDbCommand.CommandText = "SELECT * FROM relationships";
            OdbcDataReader aReader = Lib.StoryDbCommand.ExecuteReader();
            
            // Load Actors
            RelationshipType r;
            Actor a1;
            Actor a2;
            Relationship NewRel;

            while (aReader.Read())
            {
                
                 //name of actor
                a1 = (Actor)Lib.GetActionByName(aReader.GetString(1).ToString());
                
                //acquaintance of actor
                a2 = (Actor)Lib.GetActionByName(aReader.GetString(2).ToString());
                //their relationship

                r = (RelationshipType)aReader.GetInt16(3);

                NewRel = new Relationship(a1, a2, r);
                Lib.RelationshipList.Add(NewRel);
            }

                //close the reader 
                aReader.Close();
        }

        private void CommitStoryDatabase()
        {
            OdbcDataReader aReader;

            // TODO: Find a more efficient way of doing this
            foreach (Relationship rel in Lib.RelationshipList)
            { 
                //check if relationship is existing
                Lib.StoryDbCommand.CommandText = "SELECT * FROM relationships WHERE name = '"+ rel.actor1["name"] + "' AND acquaintance = '" + rel.actor2["name"] + "'";

                aReader = Lib.StoryDbCommand.ExecuteReader();
                if(aReader.HasRows)
                {
                    //aReader.Read();
                    //if(rel.relation != aReader.GetInt16(3))
                        Lib.StoryDbCommand.CommandText = "UPDATE relationships SET relationship = " + rel.relation + " WHERE name = '" + rel.actor1["name"] + "' AND acquaintance = '" + rel.actor2["name"] + "'";

                }
            }
        }

        // Add each global command to the list in this method
        // Non-global commands will be handled slightly differently when they are
        // implemented.
        private string InitCommands()
        {
            string errors = "";
            string errorstrings = "";
            Object[] Objects = new Object[Lib.MaxGameCommands];
            Command[] Commands = new Command[Lib.MaxGameCommands];

            Lib.PrintLine("Loading commands...");

            //Load Commands from existing DLLs
            //DirectoryInfo toplevel=Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory);
            //DirectoryInfo toplevel = new DirectoryInfo(Lib.PathtoRoot);

            ArrayList files = Lib.GetFilesRecursive(Lib.PathtoRoot, "*commands.dll");
            if (files.Count > 0)
            {
                foreach (string file in files)
                {
                    Lib.LoadPlugin(file);
                }
            }

            // Load commands from source files that are not yet compiled
            files = Lib.GetFilesRecursive(Lib.PathtoRootScriptsandPlugins, "*.tmc.cs");
            if (files.Count > 0)
            {
                try
                {
                    Objects = Lib.Compiler.GetObjectsFromFiles(Lib.ConvertToStringArray(files), out errors);
                    errorstrings += errors;
                }
                catch
                {
                    return errorstrings;
                    //throw new Exception(ex.Message + ex.StackTrace);
                }
                if (Objects != null)
                {
                    // If this comes back equal to it's init length, then nothing was entered into the array
                    if (Objects.Length != Lib.MaxGameActions)
                    {
                        foreach (Object Command in Objects)
                        {
                            Lib.AddCommand((TigerMUD.Command)Command);
                        }
                    }
                }
            }
            return errorstrings;
        }

        private void LoadAllCommandFromDatabase()
        {
            LoadCommandWordsFromDatabase((int)AccessLevel.System);
            LoadCommandWordsFromDatabase((int)AccessLevel.Player);
            LoadCommandWordsFromDatabase((int)AccessLevel.Builder);
            LoadCommandWordsFromDatabase((int)AccessLevel.Admin);
            LoadCommandWordsFromDatabase((int)AccessLevel.UberAdmin);
        }

        private void LoadCommandWordsFromDatabase(int accesslevel)
        {
            DataTable dt = Lib.dbService.ThreadManager.GetAllCommands(accesslevel);
            foreach (DataRow row in dt.Rows)
            {
                ICommand command = Lib.GetCommandByName(row[0].ToString());

                if (command != null)
                {
                    switch (accesslevel)
                    {
                        case (int)AccessLevel.System:
                            Lib.AddCommandWord(Lib.SystemCommandWords, command);
                            break;
                        case (int)AccessLevel.Player:
                            Lib.AddCommandWord(Lib.PlayerCommandWords, command);
                            break;
                        case (int)AccessLevel.Builder:
                            Lib.AddCommandWord(Lib.BuilderCommandWords, command);
                            break;
                        case (int)AccessLevel.Admin:
                            Lib.AddCommandWord(Lib.AdminCommandWords, command);
                            break;
                        case (int)AccessLevel.UberAdmin:
                            Lib.AddCommandWord(Lib.UberAdminCommandWords, command);
                            break;
                    }
                }
            }
        }

        private string InitActions()
        {
            string errors = "";
            string errorstrings = "";
            Object[] Objects = new Object[Lib.MaxGameActions];

            Lib.PrintLine("Loading actions...");

            //Load Commands from plugins
            //DirectoryInfo toplevel = new DirectoryInfo(Lib.PathtoRoot);
            ArrayList files = Lib.GetFilesRecursive(Lib.PathtoRoot, "*actions.dll");
            if (files.Count > 0)
            {
                foreach (string file in files)
                {
                    Lib.LoadPlugin(file);
                }
            }
            files = Lib.GetFilesRecursive(Lib.PathtoRootScriptsandPlugins, "*.tma.cs");
            if (files.Count > 0)
            {
                try
                {
                    Objects = Lib.Compiler.GetObjectsFromFiles(Lib.ConvertToStringArray(files), out errors);
                    errorstrings += errors;
                }
                catch
                {
                    return errorstrings;
                    //throw new Exception(ex.Message + ex.StackTrace);
                }
                if (Objects != null)
                {
                    // If this comes back equal to it's init length, then nothing was entered into the array
                    if (Objects.Length != Lib.MaxGameActions)
                    {
                        foreach (Object ActionObject in Objects)
                        {
                            Lib.AddAction((IAction)ActionObject);
                        }
                    }
                }
            }
            return errorstrings;
        }

        private void InitScriptCompiler()
        {
            try
            {
                //AJ: Get the right directory
                string dir = Lib.PathtoRoot;
                //AJ: Create the new AppDomain
                Lib.ScriptAppDomain = AppDomain.CurrentDomain;
                //AJ: Create the Remote Loader in the new AppDomain and retrive a proxy copy of it
                //Console.WriteLine("trying: " + Path.Combine(Lib.PathtoRoot, "TigerLoaderLib.dll"));
                //Tiger.Loader.Lib.RemoteLoader remoteLoader = (Tiger.Loader.Lib.RemoteLoader)Lib.ScriptAppDomain.CreateInstanceFromAndUnwrap(Path.Combine(Lib.PathtoRoot, "TigerLoaderLib.dll"), "Tiger.Loader.Lib.RemoteLoader");

                Tiger.Loader.Lib.RemoteLoader remoteLoader = (Tiger.Loader.Lib.RemoteLoader)Lib.ScriptAppDomain.CreateInstanceFromAndUnwrap(Path.Combine(Lib.PathtoRoot, "TigerLoaderLib.dll"), "Tiger.Loader.Lib.RemoteLoader");

                //AJ: Use the Remote Loader to load the SubApp and return a proxy copy of it
                //this.app							= (ISubApp_V1) this.remoteLoader.StartSubApp(this.directory, this.filename, this.objectName);

                Lib.Compiler = (TigerMUD.IScriptCompiler)remoteLoader.LoadObject(dir, "TigerMUDScriptsAndPlugins.dll", "TigerMUD.ScriptCompiler");
                Lib.PrintLine("Loaded ScriptCompiler from " + dir);
            }
            catch (Exception ex)
            {
                Lib.PrintLine(DateTime.Now + " EXCEPTION in InitScriptCompiler: " + ex.Message + ex.StackTrace);
            }
        }

        private void DestroyCompiler()
        {
            Lib.Compiler = null;
            //AppDomain.Unload(Lib.ScriptAppDomain);
        }

        private void DestroyActions()
        {
            if (Lib.Actions != null)
            {
                Lib.Actions.Clear();
            }
        }

        private void DestroyCommands()
        {
            if (Lib.Commands != null)
            {
                Lib.Commands.Clear();
            }
        }

        public string ReloadScripts()
        {
            DestroyCommands();
            DestroyActions();
            DestroyCompiler();
            InitScriptCompiler();
            InitCommands();
            string errors = InitActions();
            return errors;
        }



        #endregion

        #region Timer Event Handlers
        private bool timer_Heartbeat(object sender, EventArgs e)
        {
            return true;
            // Lib.PrintLine("Thump");
        }

        private bool DoTimedCommands(object sender, EventArgs e)
        {
            while (Convert.ToInt64(Lib.Delayedcommands.First.time + Lib.Delayedcommands.First.delay) <= Lib.GetTime())
            {
                DelayedCommand dc = Lib.Delayedcommands.First;
                ICommand command = Lib.GetCommandByName(dc.command);
                if (command == null)
                {
                    Lib.PrintLine(dc.command + " command not found.  Please check to make sure TMC files are all installed.");
                    Lib.Delayedcommands.Remove(dc);
                    continue;
                }

                try
                {

                    command.DoCommand(dc.actor, dc.command, dc.arguments.Trim());

                    // Only show prompt after running a command if the actor is a real user
                    if (dc.actor["type"].ToString() == "user")
                    {
                        dc.actor.Showprompt();
                    }
                }
                catch (ThreadAbortException ex)
                {
                    //Thread is most likely being aborted due to server shutdown
                    return false;
                }
                catch (Exception ex)
                {
                    if (command != null) Lib.PrintLine(DateTime.Now + " EXCEPTION running command " + command.Name + ": " + ex.Message + ex.StackTrace);
                    else Lib.PrintLine(DateTime.Now + " EXCEPTION running command " + dc.command + ": " + ex.Message + ex.StackTrace);
                    return false;
                }

                Lib.Delayedcommands.Remove(0);
                if (dc.loop)
                {
                    Lib.Delayedcommands.Add(dc.actor, dc.command, dc.arguments, Lib.GetTime(), dc.delay, dc.loop);
                }
            }
            return true;
        }
        #endregion


        /// <summary>
        /// Stops the MUD engine.
        /// 
        /// From RFE 1063516:
        /// 1. Block any new connections.
        /// 2. Kick all active connections (which causes them to
        ///    save to db automatically)
        /// 3. Backup all items, mobs, and rooms to db.
        /// 4. Save server variables to db, like clock/calendar time.
        /// 5. Exit
        /// </summary>
        public void Stop()
        {
            Lib.PrintLine("Stopping the MUD Server...");

            // Stop the Client Listener
            Lib.PrintLine("Blocking new connections.");
            Lib.PrintLine("Kicking " + tcpListener.ClientCount + " active connections.");
            tcpListener.Stop();
            int counter = 0;
            while (tcpListener.Active)
            {
                Thread.Sleep(100);
                if (counter++ > 30)
                {
                    throw new System.TimeoutException("Could not stop tcpListener in time");
                }
            }

            // Abort system threads
            Lib.PrintLine("Kill system threads.");
            counter = 0;
            timerthread.Abort();
            while (timerthread.IsAlive)
            {
                Thread.Sleep(100);
                if (counter++ > 30)
                {
                    throw new System.TimeoutException("Could not stop timerThread in time");
                }
            }

            // Back up all actors
            lock (Lib.actors.SyncRoot)
            {
                Lib.PrintLine("Backup " + Lib.actors.Count + " actors.");
                foreach (Actor item in Lib.actors)
                {
                    if (item != null)
                    {
                        item.Save();
                    }
                }
            }

            // Back up all spells
            lock (Lib.spells.SyncRoot)
            {
                Lib.PrintLine("Backup " + Lib.spells.Count + " spells.");
                foreach (Spell spell in Lib.spells)
                {
                    if (spell != null)
                    {
                        spell.Save();
                    }
                }
            }

            // Empty system containers
            Lib.PrintLine("Empty system containers.");
            Lib.actors.Clear();
            Lib.Conn.Close();
            Lib.StoryConn.Close();
            CommitStoryDatabase();
            Lib.SystemCommandWords.Clear();
            Lib.PlayerCommandWords.Clear();
            Lib.BuilderCommandWords.Clear();
            Lib.AdminCommandWords.Clear();
            Lib.UberAdminCommandWords.Clear();
            Lib.Actions.Clear();
            Lib.Commands.Clear();
            Lib.spells.Clear();

            Lib.PrintLine("Save server state.");
            Lib.SaveServerState();

            Lib.PrintLine("Shutdown is complete.");
        }
    }
}
