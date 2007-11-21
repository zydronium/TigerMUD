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
using System.Data;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Data.Odbc;
using System.Net;
using System.Security.Cryptography; // Needed for a descent random number generator.
using System.Threading;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions; // We use some RegEx's to validate username, password, and email names, so we need this.
using System.Web.Mail;
using TigerMUD.DatabaseLib;
using TigerMUD.CommsLib;


namespace TigerMUD
{

    public struct ServerInfo
    {
        public string DatabaseType;
        public string DatabaseServer;// servername to sqlservername for better meaning -- Drevlan 09.27.04
        // for SQL server, put the instance in if you named it, like if you named your
        // SQL instance MUD, then use <MACHINENAME>\\MUD --Zarius
        public string DatabaseUserID; // MySQL db username
        public string DatabasePassword; // MySQL db password
        public string DatabaseName;
        public int ServerPort;
        public string Welcomescreen;
        public float Idledisconnect;
        public string SmtpServer;
        public string EmailActivation;
        public string ServerMode;
        public bool ShowSQLStatements;

        // Client Sockets 
        public bool TcpSocketEnabled;
        public int TcpSocketPort;

        public bool MsnSocketEnabled;
        public string MsnSocketSignonEmail;
        public string MsnSocketPassword;
    }


    /// <summary>
    /// Defines the possible access levels under which users run commands.
    /// </summary>
    public enum AccessLevel
    {
        System = 0,
        Mob = 1,
        Item = 2,
        Player = 10,
        Builder = 20,
        Admin = 50,
        UberAdmin = 99
    }

    /// <summary>
    /// This is a library class containing many useful routines not specific to a class.
    /// It also contains all the static variables shared between threads.
    /// </summary>
    public class Lib
    {
        public static DbService dbService; // Abstracts all database functions
        public static string dbConnectionString; // The database connection string
        public static ArrayList users = new ArrayList(); // List of Connected users
        public static string Serverversion = "1.9";
        public static string Creditsdev = "Adam Miller, William Crawford, Luke Venediger, Doug Miller, Jeff Bosche, Brian Newton, Andrew Conrad, Andrew Jump, John Ingram, David Kolln";
        public static string Creditsother = "Anna Steverson";
        public static int connections = 0;
        public static Mudlog log = new Mudlog();
        public static int Discotimerwarn = 1; // disconnect warning timer in minutes (not implemented yet)
        public static int Discotimerdisco = 2; // disconnect timer in minutes (not implemented yet)

        public static ArrayList actors = new ArrayList(); // List of existing items
        public static ArrayList spells = new ArrayList(); // Array of spells
        public static ArrayList plants = new ArrayList();//Array of plants

        public static Market market = new Market("1", "The Modern Trader Market"); // Define market #1

        public static Randomizer rand = new Randomizer();
        public static Actor item = new Actor();
        public static Actor room = new Actor();

        // "\x1B" is the ESC character
        public static string Ansireset = "\x1B" + "[0m";  // reset. clears all colors and styles (to white on black)
        public static string Ansiinverseon = "\x1B" + "[7m"; // inverse on; reverses foreground + background colors
        public static string Ansiinverseoff = "\x1B" + "[27m"; // inverse off
        public static string Ansifblack = "\x1B" + "[0;30m"; // set foreground color to black
        public static string Ansifred = "\x1B" + "[0;31m"; // set foreground color to red
        public static string Ansifgreen = "\x1B" + "[0;32m"; //  set foreground color to green
        public static string Ansifyellow = "\x1B" + "[0;33m"; // set foreground color to yellow
        public static string Ansifblue = "\x1B" + "[0;34m"; //  set foreground color to blue
        public static string Ansifpurple = "\x1B" + "[0;35m"; // set foreground color to purple
        public static string Ansifcyan = "\x1B" + "[0;36m"; // set foreground color to cyan
        public static string Ansifwhite = "\x1B" + "[0;37m"; // set foreground color to white
        public static string Ansifboldred = "\x1B" + "[1;31m"; // set foreground color to bold red
        public static string Ansifboldgreen = "\x1B" + "[1;32m"; //  set foreground color to bold green
        public static string Ansifboldyellow = "\x1B" + "[1;33m"; // set foreground color to bold yellow
        public static string Ansifboldblue = "\x1B" + "[1;34m"; //  set foreground color to bold blue
        public static string Ansifboldpurple = "\x1B" + "[1;35m"; // set foreground color to bold purple
        public static string Ansifboldcyan = "\x1B" + "[1;36m"; // set foreground color to bold cyan
        public static string Ansifboldwhite = "\x1B" + "[1;37m"; // set foreground color to bold white
        public static string Ansibblack = "\x1B" + "[40m"; // set background color to black
        public static string Ansibred = "\x1B" + "[41m"; // set background color to red
        public static string Ansibgreen = "\x1B" + "[42m"; // set background color to green
        public static string Ansibyellow = "\x1B" + "[43m"; // set background color to yellow
        public static string Ansibblue = "\x1B" + "[44m"; //  set background color to blue
        public static string Ansibpurple = "\x1B" + "[45m"; // set background color to purple
        public static string Ansibcyan = "\x1B" + "[46m"; // set background color to cyan
        public static string Ansibwhite = "\x1B" + "[47m"; // set background color to white
        public static string Ansibdefault = "\x1B" + "[49m"; // set background color to default (black)
        // Creating a Regex is expensive, so make it global -- William 10.17.2004
        public static Regex Checkvalidusername = new Regex("(^[a-zA-Z]{1})(\\w){1,14}$"); // Changed to verify length as well as characters -- William 10.16.2004
        public static Regex Emailregex = new Regex("(?<user>[^@]+)@(?<host>.+)");
        public static Regex Checkvalidpassword = new Regex("^\\w{5,15}$"); // Changed to verify length
        public static Regex Lettersonly = new Regex("^[a-zA-Z]+$"); // Contains only letters
        public static Regex Numbersonly = new Regex("^\\d+$"); // Contains only numbers
        public static Regex LettersNumbersonly = new Regex("^\\w+$"); // Contains only letters, numbers
        public static Regex Lettersspacesonly = new Regex("^[a-zA-Z ]+$"); // Contains only letters, spaces
        public static Regex Sentencecharsonly = new Regex("^[\\w,!\\?\\.,+-/\\\\()\\s]+$"); // Contains only characters that would be in a sentence.
        public static ServerInfo Serverinfo;
        public static OdbcConnection Conn = new OdbcConnection();
        public static OdbcCommand Dbcommand = new OdbcCommand();
        // MUD clock starts at January 1st, Year 0001 at midnight.
        public static System.DateTime Gametime = new System.DateTime(1, 1, 1, 0, 0, 0);
        // CommandWords contains the words the user will type and the name of the 
        // command to call for them.  Commands contains the names of the commands
        // and the actual Command class.  This seems overly complicated, but it's
        // necessary to be able to dynamically change commands.
        public static Hashtable SystemCommandWords = new Hashtable();
        public static Hashtable PlayerCommandWords = new Hashtable();
        public static Hashtable BuilderCommandWords = new Hashtable();
        public static Hashtable AdminCommandWords = new Hashtable();
        public static Hashtable UberAdminCommandWords = new Hashtable();
        public static Hashtable Commands = new Hashtable();
        public static Hashtable Actions = new Hashtable();
        public static Hashtable ServerState = Hashtable.Synchronized(new Hashtable());

        //AJ: Old Code - Changed these to interfaces
        //public static TigerMUD.ScriptCompiler Compiler;
        //public static TigerMUD.PluginLoader PluginLoader;
        //AJ: END Old Code

        public static TigerMUD.IScriptCompiler Compiler;
        public static TigerMUD.IPluginLoader PluginLoader;

        public static AppDomain ScriptAppDomain;
        public static DelayedCommands Delayedcommands = new DelayedCommands();
        public static EmailPassword sendemailpassword = new EmailPassword();

        public static float Timeratio = 12; // How many game seconds pass for each real second
        public static float moontransit_secs = 2124199; // moon orbits earth in real seconds

        public static float moontransit_ticks = 147514; // to get this, take real seconds/12*.833333
        public static float moonquadrant_ticks = 147514 / 8;
        // Ratio of realtime versus game time seconds
        public static float Gamesecondperrealsecond = 12;
        // How many ticks per game second
        public static float Tickspergamesecond = .833333F;
        public static string moonphase = "new";
        public static string Welcomescreen = "";
        public static int ShoutRadius = 3; //How far players can shout
        // Local file for server console output, for running tigermud as a service
        public static string serverLogFileName = "TigerMUD.log";
        // Player command logger
        public static string commandLogFileName = "TigerMUDCommand.log";

        public static int MaxGameCommands = 2000;
        public static int MaxGameActions = 2000;
        public static int MaxStatesPerActor = 2000;

        public static string AdminEmail = "admin@tigermud.com";

        public static string PathtoDebugorRelease = null;

        public static string PathtoRoot = null;
        public static string PathtoRootAssemblies = null;
        public static string PathtoRootRemoteConsole = null;
        public static string PathtoRootRemoteConsoleAssemblies = null;
        public static string PathtoRootScriptsandPlugins = null;
        public static string PathtoRootScriptsandPluginsAssemblies = null;
        public static string PathtoRootTigerLoaderLib = null;
        public static string PathtoRootTigerLoaderLibAssemblies = null;

        //public static string PathtoStartup = null;

        public static StreamWriter serverlogwriter = null;
        public static StreamWriter commandlogwriter = null;


        public static Actor GetActorFromArguments(Actor actor, string arguments)
        {
            Actor room = Lib.GetByID(actor["container"].ToString());
            Actor item = new Actor();
            int containernumericprefix = 0;
            string itemname = "";
            int itemnumericprefix = 0;
            int itemquantity = 0;

            if (arguments.Length < 1)
            {
                actor.SendError("You must specify something to take action upon.\r\n");
                return null;
            }
            string txt;
            int numericprefix = 0; // Numeric portion of the text converted to integer
            ArrayList words = Lib.GetWords(arguments);
            if (words.Count < 1)
            {
                actor.SendError("You must specify something to take action upon.\r\n");
                return null;
            }
            // If words.count>1 then item name is all the words
            if (words.Count > 1)
            {
                txt = arguments;
            }
            else
            {
                // if words.count<2 then target is first word
                txt = (string)words[0];
            }
            txt = Lib.SplitItemNameFromNumericPrefix(txt, ref numericprefix);
            if (txt == null)
            {
                actor.SendError("The object you specified was incorrect.\r\n");
                return null;
            }

            // Get the container name from the word array if there is one.
            string containername = Lib.GetContainerName(words);

            // Code for when container is involved
            if (containername != null)
            {
                containername = Lib.SplitItemNameFromNumericPrefix(containername, ref containernumericprefix);
                Actor container = actor.GetItemByName(containername, containernumericprefix);
                if (container == null)
                {
                    container = room.GetItemByName(containername, containernumericprefix);
                }
                if (container == null)
                {
                    actor.SendError("You don't have a container named '" + containername + "'.\r\n");
                    return null;
                }
                // Cannot examine items in another player's inventory
                if (container["type"].ToString() == "user" || container["type"].ToString() == "mob")
                {
                    actor.SendError("You cannot do things to items in another person's inventory.\r\n");
                    return null;
                }
                item = container.GetItemByName(itemname, itemnumericprefix, itemquantity);
                if (item == null)
                {
                    actor.SendError("The item is not in the container you specified.\r\n");
                    return null;
                }
                room = container;
            }

            // Check for actor in inventory
            item = actor.GetItemByName(txt, numericprefix);
            if (item != null)
            {
                return item;
            }

            // Check for actor in room
            item = room.GetItemByName(txt, numericprefix);
            if (item != null)
            {
                return item;
            }

            actor.SendError("Could not find what you specified.\r\n");
            return null;
        }


        /// <summary>
        /// Function that makes it easier to switch between databases
        /// dbtype variable is in public section of Lib. It reads the database
        /// configuration values from tigermud.xml and custom creates a connection string
        /// for the specified database type.
        /// </summary>
        /// <returns>sql Connect string</returns>
        public static string Getconnstring()
        {
            string sqlstr;
            if (Serverinfo.DatabaseType.ToLower() == "mssql")
            {
                sqlstr = "Driver={SQL Server};Server=" + Serverinfo.DatabaseServer + ";Trusted_Connection=yes;Database=" + Serverinfo.DatabaseName + ";";
            }
            else if (Serverinfo.DatabaseType.ToLower() == "mysql")
            {
                sqlstr = "DRIVER={MySQL ODBC 3.51 Driver};" +
                    "SERVER=" + Serverinfo.DatabaseServer + ";" +
                    "DATABASE=" + Serverinfo.DatabaseName + ";" +
                    "UID=" + Serverinfo.DatabaseUserID + ";" +
                    "PASSWORD=" + Serverinfo.DatabasePassword + ";" +
                    "OPTION=3";
            }
            else if (Serverinfo.DatabaseType.ToLower() == "msaccess")
            {
                //string path = AppDomain.CurrentDomain.BaseDirectory + @"..\..\" + Serverinfo.DatabaseName + ".mdb";
                string path = Path.GetFullPath(Path.Combine(Lib.PathtoRoot, Serverinfo.DatabaseName + ".mdb"));
                //string path=@"..\..\" + Serverinfo.DatabaseName + ".mdb";
                Lib.Print("Connecting to database at: " + path + "...");
                try
                {
                    sqlstr = @"Driver={Microsoft Access Driver (*.mdb)};" +
                        @"Dbq=" + path + ";" +
                        @"Uid=Admin;" +
                        @"Pwd=";
                    Lib.PrintLine("Succeeded.");
                }
                catch
                {
                    Lib.PrintLine("Failed.");
                    throw new Exception("Could not find TigerMUD database file.");
                }
            }
            else
            {
                string errorstring = "Error in TigerMUD.xml file. The database type option is set to an invalid string.";
                Lib.PrintLine(errorstring);
                throw new Exception(errorstring);
            }

            // Set the connection string property
            dbConnectionString = sqlstr;

            return sqlstr;
        }

        public static String[] ConvertToStringArray(ArrayList input)
        {
            // Allocate the memory for the array.
            String[] output = new String[input.Count];
            //Then copy to the array, like this.
            // Copy the elements to the array.
            input.CopyTo(output);
            return output;
        }



        public static void LoadPlugin(string filename)
        {
            //Old way - doesn't load into ScriptAppDomain
            System.Reflection.Assembly commandAssembly = System.Reflection.Assembly.LoadFrom(filename);

            Type[] types = commandAssembly.GetTypes();

            foreach (Type type in types)
            {

                if (type.Namespace == "TigerMUD")
                {
                    if (type.GetInterface("IAction") != null)
                        Lib.AddAction((Action)Activator.CreateInstance(type));

                    if (type.GetInterface("ICommand") != null)
                        Lib.AddCommand((Command)Activator.CreateInstance(type));
                    // DEBUG
                    //Console.WriteLine("Adding " + type.Name);
                }
            }
        }

        public static bool ValidateEmail(string email)
        {
            return Lib.Emailregex.IsMatch(email);
        }

        /// <summary>
        /// Returns filenames of every file in and below the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ArrayList GetFilesRecursive(string path, string mask)
        {
            ArrayList list = new ArrayList();
            foreach (string d in Directory.GetDirectories(path))
            {
                DirectoryInfo di = new DirectoryInfo(d);
                foreach (string f in Directory.GetFiles(di.FullName, mask))
                {
                    FileInfo fi = new FileInfo(f);
                    // Skip all DLLs in the obj paths
                    if (!fi.FullName.Contains(@"\obj\"))
                    {
                        list.Add(fi.FullName);
                    }
                }
                list.AddRange(GetFilesRecursive(di.FullName, mask));
            }
            return list;
        }



        /// <summary>
        /// Saves the server state variables to the mudstate database table.
        /// </summary>
        /// <returns>Nothing</returns>
        public static void SaveServerState()
        {
            //lock (Lib.ServerState.SyncRoot)
            //{
            //    System.Collections.IEnumerator names = Lib.ServerState.Keys.GetEnumerator();
            //    while (names.MoveNext())
            //    {
            //        string name = (string)names.Current;
            //        Lib.dbService.Library.SaveServerState(name, Lib.ServerState[name].ToString());
            //    }
            //    Lib.ServerState["lastserverticks"] = Lib.GetTime();
            //}
            lock (Lib.ServerState.SyncRoot)
            {
                foreach (string name in Lib.ServerState.Keys)
                {
                    Lib.dbService.Library.SaveServerState(name, Lib.ServerState[name].ToString());
                }
                Lib.ServerState["lastserverticks"] = Lib.GetTime();
            }
        }

        public static int GameTimeGetHour()
        {
            return Lib.Gametime.Hour;
        }


        /// <summary>
        /// Gets special locations such as graveyards and new user spawn locations.
        /// Also has logic to handle when such locations have not been defined.
        /// </summary>
        /// <param name="subtype"></param>
        /// <param name="zone"></param>
        /// <returns></returns>
        public static Actor GetSpecialLocation(string subtype, string zone)
        {
            // This function gets this zone's special loc where players resurrect when they die.
            lock (Lib.actors.SyncRoot)
            {
                foreach (Actor room in Lib.actors)
                {
                    if (room != null)
                    {
                        if (room["type"].ToString() == "room" && room["subtype"].ToString() == subtype && room["zone"].ToString() == zone)
                        {
                            return room;
                        }
                    }
                }
            }
            // could not find a special loc for this zone
            // so use the first special loc we find
            lock (Lib.actors.SyncRoot)
            {
                foreach (Actor room in Lib.actors)
                {
                    if (room != null)
                    {
                        if (room["type"].ToString() == "room" && room["subtype"].ToString() == subtype)
                        {
                            return room;
                        }
                    }
                }
            }
            // Could not find ANY special locs, so just resurrect in the first room of this zone.
            lock (Lib.actors.SyncRoot)
            {
                foreach (Actor room in Lib.actors)
                {
                    if (room != null)
                    {
                        if (room["type"].ToString() == "room" && room["zone"].ToString() == zone)
                        {
                            return room;
                        }
                    }
                }
            }
            // Everything failed, so resurrect in the first room of the MUD period.
            lock (Lib.actors.SyncRoot)
            {
                foreach (Actor room in Lib.actors)
                {
                    if (room != null)
                    {
                        if (room["type"].ToString() == "room")
                        {
                            return room;
                        }
                    }
                }
            }
            return null;
        }

        public static bool Print(string message)
        {
            //if (Lib.Serverinfo.ServerMode == "service")
            //{
            //    // This using statement will close the writer
            //    // once it exits the code block
            //    using (StreamWriter writer = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\" + logFileName, true))
            //    {
            //        writer.AutoFlush = true;
            //        writer.Write(message);
            //    }
            //    return true;
            //}
            //else
            //{
            //serverlogwriter.AutoFlush = true;
            serverlogwriter.WriteLine(message);
            if (Lib.Serverinfo.ServerMode != "service")
            {
                Console.Write(message);
            }
            return true;
            //}
        }

        public static bool Print()
        {
            Lib.Print("\r");
            return true;
        }

        public static bool PrintLine()
        {
            Lib.PrintLine("\r");
            return true;
        }

        public static bool PrintLine(string message)
        {
            //if (Lib.Serverinfo.ServerMode=="service")
            //{
            // This using statement will close the writer
            // once it exits the code block
            //writer = new StreamWriter(Path.GetFullPath(Path.Combine(Lib.PathtoRoot, logFileName)), true);
            //{

            serverlogwriter.WriteLine(message);
            commandlogwriter.WriteLine(message);
            //}
            //return true;
            //}
            //else
            //{
            if (Lib.Serverinfo.ServerMode != "service")
            {
                Console.WriteLine(message);
            }
            return true;
            //}
        }


        //public static string GetIdFromName(string name)
        //{
        //    return Lib.GetIdFromName(name);
        //}



        /// <summary>
        /// Function that reads a text file line by line.
        /// </summary>
        /// <returns>The entire text file as one string</returns>
        public static string Readfile(string path)
        {
            StreamReader sr = new StreamReader(path);
            string line = "";
            string fulltext = "";
            while (line != null)
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    fulltext += line + "\r\n";
                }
            }
            sr.Close();
            return fulltext;
        }

        /// <summary>
        /// Dumps the contents of a DataTable to a multi-line string.
        /// </summary>
        /// <param name="table">The DataTable holding the data.</param>
        /// <param name="includeColumnNames">If true, then the first line will contain the column names.</param>
        /// <param name="columnDelimiter">The delimiting character to use.</param>
        /// <returns>A multiline string.</returns>
        public static string DumpTableToString(DataTable table, bool includeColumnNames, string columnDelimiter)
        {
            StringBuilder outputString = new StringBuilder();
            string endOfLine = "\r\n";

            // Do we need to include the column names?
            if (includeColumnNames)
            {
                foreach (DataColumn column in table.Columns)
                {
                    outputString.Append(column.ColumnName + columnDelimiter);
                }

                // Remove the last delimiter
                outputString.Remove(outputString.Length - columnDelimiter.Length, columnDelimiter.Length);
                // Add the EOL string
                outputString.Append(endOfLine);
            }

            // Dump the contents of the table
            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn column in table.Columns)
                {
                    outputString.Append(row[column].ToString() + columnDelimiter);
                }
                // Remove the last delimiter
                outputString.Remove(outputString.Length - columnDelimiter.Length, columnDelimiter.Length);
                // Add the EOL string
                outputString.Append(endOfLine);
            }

            // return the results
            return outputString.ToString();
        }

        /// <summary>
        /// Function that takes a text direction and determines the opposite direction.
        /// </summary>
        /// <returns>The opposite direction as a string</returns>
        public static string Getoppositedir(string direction)
        {
            // Remove the words "to the" if they exist in direction
            StringBuilder sb = new StringBuilder(direction);
            // Pull all the occurances of "to the " out if they exist
            sb.Replace("to the ", null);
            // Remove all spaces if they exist
            sb.Replace(" ", null);
            direction = sb.ToString();

            switch (direction)
            {
                case "north":
                    return "the south";
                case "northeast":
                    return "the southwest";
                case "east":
                    return "the west";
                case "southeast":
                    return "the northwest";
                case "south":
                    return "the north";
                case "southwest":
                    return "the northeast";
                case "west":
                    return "the east";
                case "northwest":
                    return "the southeast";
                case "up":
                    return "below";
                case "upwards":
                    return "below";
                case "down":
                    return "above";
                case "downwards":
                    return "above";
                case "in":
                    return "outside";
                case "inside":
                    return "outside";
                case "out":
                    return "inside";
                case "outside":
                    return "inside";
                default:
                    return "nowhere";
            }
        }

        /// <summary>
        /// Function that takes a whole string and tells you if the string is all numeric or not.
        /// </summary>
        /// <returns>The result as a boolean</returns>
        public static bool IsNumeric(string txt)
        {
            // Longs are at most 19 chars
            //if (txt.Length > 19) return false;

            for (int counter = 0; counter < txt.Length; counter++)
            {
                if (!Char.IsNumber(txt, counter))
                {
                    return false;
                }
            }
            return true;
        }

        //public static DataTable GetUserInfo(string shortName)
        //{
        //    return Lib.dbService.EmailPassword.GetUserInfo(shortName);
        //}

        //public static void UpdateUserPassword(string shortName, string password)
        //{
        //    Lib.dbService.EmailPassword.UpdateUserPassword(shortName, password);
        //}

        public static string DeleteServerLog()
        {
            return Lib.log.Delete();
        }

        public static void ShowMarket(Actor actor)
        {
            Lib.market.ShowWelcomeScreen(actor);
        }

        public static void AddActorToWorld(Actor actor)
        {
            actor.Save();
            return;

            //lock (Lib.items.SyncRoot)
            //{
            //    Lib.items.Add(actor);
            //}
            //return;
        }

        public static string Encrypt(string text)
        {
            return Lib.EncryptPassword(text);
        }

        //public static string GetServerState(string name)
        //{
        //    return Convert.ToString(Lib.ServerState[name]);
        //}


        public static string SetServerState(string name)
        {
            return Convert.ToString(Lib.ServerState[name]);
        }

        public static void SetServerState(string name, string setvalue)
        {
            Lib.ServerState[name] = setvalue;
        }

        public static void SetServerState(string name, int setvalue)
        {
            string newvalue = Convert.ToString(setvalue);
            Lib.ServerState[name] = newvalue;
        }

        public static void SetServerState(string name, bool setvalue)
        {
            string newvalue = Convert.ToString(setvalue);
            Lib.ServerState[name] = newvalue;
        }

        public static void SetServerState(string name, long setvalue)
        {
            string newvalue = Convert.ToString(setvalue);
            Lib.ServerState[name] = newvalue;
        }

        public static void SetServerState(string name, double setvalue)
        {
            string newvalue = Convert.ToString(setvalue);
            Lib.ServerState[name] = newvalue;
        }

        public static void SetServerState(string name, decimal setvalue)
        {
            string newvalue = Convert.ToString(setvalue);
            Lib.ServerState[name] = newvalue;
        }

        public static void SetServerState(string name, char setvalue)
        {
            string newvalue = Convert.ToString(setvalue);
            Lib.ServerState[name] = newvalue;
        }

        public static void SetServerState(string name, float setvalue)
        {
            string newvalue = Convert.ToString(setvalue);
            Lib.ServerState[name] = newvalue;
        }

        /// <summary>
        /// Function that takes item names with numeric prefixes and separates the number from the text.
        /// Note that you provide a variable to contain the numeric portion as a "by ref" parameter.
        /// </summary>
        /// <returns>The item name as a string</returns>
        public static string SplitItemNameFromNumericPrefix(string playercommand, ref int numericprefix)
        {
            // Thanks Kender!
            numericprefix = 0;
            // Catch errors here
            if (playercommand == null) return "";
            if (playercommand == "") return playercommand;

            if (!Char.IsNumber(playercommand, 0))
            {	//if the command does not start with a number return it immediately
                return playercommand;
            }
            int counter = 0;
            StringBuilder number = new StringBuilder();
            while (Char.IsNumber(playercommand, counter))
            {
                number.Append(playercommand.Substring(counter++, 1));
            }
            // Convert numeric portion to integer for looping
            try
            {
                numericprefix = Convert.ToInt32(number.ToString());
            }
            catch (Exception ex)
            {
                // Catch if user enters a number larger than an Integer type can handle. We then assume that it is 'not a number'
                Lib.log.Add("processcommand function", DateTime.Now + " EXCEPTION " + ex.Message + ex.StackTrace + " - user entered a really big number for looking at an item by number, treating it as string");
                return playercommand;
            }
            //extract the non-numeric part of the string and trim leading and trailing spaces
            return playercommand.Substring(counter, playercommand.Length - counter).Trim();
        }

        private static bool serverstarted = false;

        public static bool ServerStarted
        {
            get { return serverstarted; }
            set { serverstarted = value; }
        }

        /// <summary>
        /// Demonstrates writing an XML file like TigerMUD.xml
        /// </summary>
        /// <returns>Nothing</returns>
        public static void Createxmldoc(string filename)
        {
            // Create a new file in C:\\ dir
            XmlTextWriter textWriter = new XmlTextWriter(filename, null);
            textWriter.Formatting = Formatting.Indented;
            // Opens the document 
            textWriter.WriteStartDocument();
            // Write comments
            textWriter.WriteComment("TigerMUD Server Configuration File");
            // Write first element
            textWriter.WriteStartElement("TigerMUD");

            // Start Database Elements
            textWriter.WriteStartElement("Database", "");

            textWriter.WriteStartElement("Type", "");
            textWriter.WriteString("mysql");
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("DBServer", "");
            textWriter.WriteString("localhost");
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("UserID", "");
            textWriter.WriteString("username");
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("Password", "");
            textWriter.WriteString("password");
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("DatabaseName", "");
            textWriter.WriteString("tigermud");
            textWriter.WriteEndElement();

            textWriter.WriteEndElement(); // End Database Elements

            //Start Server Elements
            textWriter.WriteStartElement("Server", "");

            textWriter.WriteStartElement("Port", "");
            textWriter.WriteString("8000");
            textWriter.WriteEndElement();

            textWriter.WriteEndElement(); // End Server Elements

            textWriter.WriteEndElement(); // Closes root element
            // Ends the document.
            textWriter.WriteEndDocument();
            textWriter.Close();
        }

        /// <summary>
        /// Function that reads the TigerMUD.xml server config file and loads the values.
        /// </summary>
        /// <returns>A ServerInfo structure loaded with the server configuration values</returns>
        public static ServerInfo Readxmldoc(string filename)
        {
            ServerInfo tempInfo = new ServerInfo();
            //Load the reader with the XML file.
            XmlTextReader xmlReader = new XmlTextReader(filename);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlReader);
            xmlReader.Close(); // Finished, so release the file

            // Database Information
            XmlNode xmlNode = xmlDoc.SelectSingleNode(@"/TigerMUD/Database");
            foreach (XmlNode node in xmlNode.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Type":
                        tempInfo.DatabaseType = node.InnerText;
                        break;
                    case "DBServer":
                        tempInfo.DatabaseServer = node.InnerText;
                        break;
                    case "UserID":
                        tempInfo.DatabaseUserID = node.InnerText;
                        break;
                    case "Password":
                        tempInfo.DatabasePassword = node.InnerText;
                        break;
                    case "DatabaseName":
                        tempInfo.DatabaseName = node.InnerText;
                        break;
                    case "ShowSQLStatements":
                        tempInfo.ShowSQLStatements = bool.Parse(node.InnerText);
                        break;
                }
            }

            //Server Information
            xmlNode = xmlDoc.SelectSingleNode(@"/TigerMUD/Server");
            foreach (XmlNode node in xmlNode.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Port":
                        tempInfo.ServerPort = Convert.ToInt32(node.InnerText);
                        break;
                    case "Welcomescreen":
                        tempInfo.Welcomescreen = node.InnerText;
                        Console.WriteLine(node.InnerText);
                        Console.WriteLine(node.Value);
                        break;
                    case "Idledisconnect":
                        tempInfo.Idledisconnect = (Convert.ToInt64(node.InnerText)) * 600; // The x600 turns this number into ticks
                        break;
                    case "SmtpServer":
                        tempInfo.SmtpServer = node.InnerText;
                        break;
                    case "EmailActivation":
                        tempInfo.EmailActivation = node.InnerText;
                        break;
                    case "ServerMode":
                        tempInfo.ServerMode = node.InnerText;
                        break;
                }
            }

            // Client Sockets : Tcp
            xmlNode = xmlDoc.SelectSingleNode(@"/TigerMUD/ClientSockets/TcpSocket");
            foreach (XmlNode node in xmlNode.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Enabled":
                        tempInfo.TcpSocketEnabled = bool.Parse(node.InnerText);
                        break;
                    case "Port":
                        tempInfo.TcpSocketPort = int.Parse(node.InnerText);
                        break;
                }
            }

            // Client Sockets : Msn
            xmlNode = xmlDoc.SelectSingleNode(@"/TigerMUD/ClientSockets/MsnSocket");
            foreach (XmlNode node in xmlNode.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Enabled":
                        tempInfo.MsnSocketEnabled = bool.Parse(node.InnerText);
                        break;
                    case "SignonEmail":
                        tempInfo.MsnSocketSignonEmail = node.InnerText;
                        break;
                    case "Password":
                        tempInfo.MsnSocketPassword = node.InnerText;
                        break;
                }
            }

            // Profanity Filter - initialise the filter
            xmlNode = xmlDoc.SelectSingleNode(@"/TigerMUD/ProfanityFilter");
            foreach (XmlNode node in xmlNode.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Enabled":
                        ProfanityFilter.EnableFilter = bool.Parse(node.InnerText);
                        break;
                    case "SendPlayerMessage":
                        ProfanityFilter.SendMessageToPlayer = bool.Parse(node.InnerText);
                        break;
                    case "PlayerMessage":
                        ProfanityFilter.PlayerMessage = node.InnerText;
                        break;
                }
            }

            return tempInfo;
        }

        /// <summary>
        /// Function that sends a message to a given user socket.
        /// </summary>
        /// <remarks>
        /// This barebones send function is only used when we don't have the user object instance, which
        /// occurs at login before user authentication.
        /// </remarks>
        /// <returns>A boolean to indicate success(true) or error(false).</returns>
        public static bool Sendsimple(IUserSocket ns, string message)
        {
            ns.Send(message);
            return true;
        }

        /// <summary>
        /// Converts color codes to the ANSI equivalent
        /// Written by Jeff Boschee
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Colorize(string value)
        {
            string ccode = "#";
            string bgcode = "^";

            //Bold Colors
            value = value.Replace(ccode + "R", "[1m[31m"); //red
            value = value.Replace(ccode + "G", "[1m[32m"); //green
            value = value.Replace(ccode + "Y", "[1m[33m"); //yellow
            value = value.Replace(ccode + "B", "[1m[34m"); //blue
            value = value.Replace(ccode + "M", "[1m[35m"); //magenta
            value = value.Replace(ccode + "C", "[1m[36m"); //cyan
            value = value.Replace(ccode + "W", "[1m[37m"); //white

            //Dim Colors
            value = value.Replace(ccode + "D", "[0m[30m"); //black
            value = value.Replace(ccode + "r", "[0m[31m"); //red
            value = value.Replace(ccode + "g", "[0m[32m"); //green
            value = value.Replace(ccode + "y", "[0m[33m"); //orange
            value = value.Replace(ccode + "b", "[0m[34m"); //blue
            value = value.Replace(ccode + "m", "[0m[35m"); //purple
            value = value.Replace(ccode + "c", "[0m[36m"); //cyan
            value = value.Replace(ccode + "w", "[0m[37m"); //grey

            //Misc
            value = value.Replace(ccode + "f", "[5m"); //flash
            value = value.Replace(ccode + "n", "[0m"); //normal
            value = value.Replace(ccode + "z", "[7m"); //inverse
            value = value.Replace(ccode + "i", "[3m"); //italics
            value = value.Replace(ccode + "u", "[4m"); //Underline

            //backgrounds
            value = value.Replace(bgcode + "r", "[41m"); //red
            value = value.Replace(bgcode + "g", "[42m"); //green
            value = value.Replace(bgcode + "y", "[43m"); //yellow
            value = value.Replace(bgcode + "b", "[44m"); //blue
            value = value.Replace(bgcode + "m", "[45m"); //purple
            value = value.Replace(bgcode + "c", "[46m"); //cyan
            value = value.Replace(bgcode + "w", "[47m"); //grey

            return value;
        }

        /// <summary>
        /// Function that inserts carriage returns into a string where lines exceed
        /// a given screen width.
        /// </summary>
        /// <returns>The new string with added carriage returns</returns>
        public static string WordWrap(string message, int maxcharacters)
        {
            if (message.Length >= maxcharacters)		// Only activate when needed
            {
                StringBuilder messageBuilder = new StringBuilder();	// Used to build our return message
                message = message.Replace("\r\n", "\n");
                message = message.Replace("\r", "\n");
                string[] lines = message.Split('\n');	// Split the message into lines
                // Process each line in the message
                foreach (string line in lines)
                {
                    if (line.Length == 0)	//Insert a blank line
                    {
                        messageBuilder.Append("\r\n");
                    }
                    else // Line is longer than zero
                    {
                        ArrayList words = new ArrayList(line.Split(' '));	// Split each line into words
                        StringBuilder lineBuilder = new StringBuilder();	// Used to build a line
                        int lineLength = 0;
                        // Process each word in the line
                        for (int i = 0; i < words.Count; i++)
                        {
                            string word = words[i].ToString();	// Only used to calculate length, not outputted
                            if (word.Length == 0)	// Empty words are caused by multiple consecutive spaces on the line, output a space
                            {
                                lineBuilder.Append(" ");
                                lineLength++;
                            }
                            else // word is longer than zero
                            {
                                word = word.Replace("\t", "        ");	// Replace tabs with 8 spaces (is this dependent on client?)
                                // Strip out ANSI control codes for length calculation
                                while (word.IndexOf("\x1B") != -1)
                                {
                                    int ansiStart = word.IndexOf("\x1B");
                                    int ansiEnd = word.IndexOf("m", ansiStart) + 1;
                                    if (ansiEnd == 0)	// Avoid endless loop if \x1B is not followed by 'm'
                                    {
                                        break; // Escape the while loop
                                    }
                                    word = word.Remove(ansiStart, ansiEnd - ansiStart);
                                }// End ANSI strip loop
                                // Break the word up into pieces if it is too long
                                if (word.Length >= maxcharacters)
                                {
                                    words.Insert(i + 1, words[i].ToString().Substring(maxcharacters - lineLength));	// Insert a new word consisting of everything that did not fit on this line into the arraylist for processing in the next iteration of the loop
                                    lineBuilder.AppendFormat("{0}\r\n", words[i].ToString().Substring(0, maxcharacters - lineLength));	//Output the first piece of the long word
                                    lineLength = 0;
                                }
                                else // Word is of valid length
                                {
                                    if (lineLength + word.Length >= maxcharacters) // Check if we are exceeding the maximum line length
                                    {
                                        lineBuilder.Append("\r\n");	// Start a new row if we are
                                        lineLength = 0;
                                    }
                                    lineBuilder.AppendFormat("{0} ", words[i]);	// Append the word to our line, followed by a space
                                    lineLength += word.Length + 1;
                                }
                            }
                        }// End words loop
                        lineBuilder.Remove(lineBuilder.Length - 1, 1);	// Remove the last space
                        messageBuilder.AppendFormat("{0}\r\n", lineBuilder.ToString());	// Append our line to the message, follwed by a \r\n
                    }
                }// End lines loop
                messageBuilder.Remove(messageBuilder.Length - 2, 2);	// Remove the last \r\n
                message = messageBuilder.ToString();	// Our message is complete
            }
            return message;
        }



        public static string More(string message, ref string Morebuffer, int Screenwidth)
        {
            // The MORE functionality here
            int linesperpage = 22; // testing shows that this set to 21 actually varies between 15 and 20 or so.
            int linecount = 1; // There is always at least 1 line to display
            string txt = "";
            string messagetosend = "";
            int charcount = 1;
            for (int position2 = 0; position2 <= message.Length - 1; position2++)
            {
                // Count real message length minus all the ANSI codes and tabs and non-displaying characters.
                // Found ANSI code, don't wanna count it's characters.
                if (message.Substring(position2, 1) == "\x1B")
                {
                    // found ansi code, shoot forward to the end of it at the 'm'
                    while (message.Substring(position2, 1) != "m")
                    {
                        position2++;
                    }
                    position2++; // extra bump to get past the 'm'
                }
                // found tab character
                else if (message.Substring(position2, 1) == "\t")
                {
                    position2++; // position advances once because tab exists in the string as one char
                    charcount += 3; // but count 3 because it actually prints three chars 
                }
                else if (message.Substring(position2, 1) == "\r" || message.Substring(position2, 1) == "\n")
                {
                    charcount = 0;
                    position2++;
                    linecount++;
                }
                else
                {
                    charcount++;
                    position2++;
                }
                // Ok we're at character this["screenwidth"]+1, so that means we need to count a new line
                if (charcount >= Screenwidth)
                {
                    charcount = 0;
                    linecount++;
                }
                // if the text to send is more than 'linesperpage', then invoke the MORE functionality
                if (linecount >= linesperpage)
                {
                    // Display text up to this point
                    messagetosend = message.Substring(0, position2);
                    // Store the rest in the user's buffer, retrieveable through the MORE command
                    // Catch any trailing carriage return, because we won't want them at the beginning of the Morebuffer
                    if (message.Substring(position2, 1) == "\r" || message.Substring(position2, 1) == "\n")
                    {
                        Morebuffer = message.Substring(position2 + 1, (message.Length) - (position2 + 1));
                    }
                    else
                    {
                        Morebuffer = message.Substring(position2, message.Length - position2);
                    }
                    // Retrieve text from the more buffer to create the 'remaining line count'
                    txt = Morebuffer;
                    // Count the number of lines in variable txt
                    charcount = 1;
                    linecount = 0;
                    for (int counter2 = 0; counter2 < txt.Length - 1; counter2++)
                    {
                        if (txt.Substring(counter2, 1) == "\r")
                        {
                            linecount++;
                            charcount = 1;
                        }
                        if (charcount == Screenwidth + 1)
                        {
                            linecount++;
                            charcount = 1;
                        }
                        charcount++;
                        if (linecount == 0)
                        {
                            linecount = 1;
                        }
                    }
                    messagetosend = messagetosend + "\r\n<--- Hit Enter to see the next page of text. (" + Math.Round((decimal)linecount / (decimal)linesperpage, 1) + " remaining pages.) --->\r\n";
                    message = messagetosend;
                    return message;
                }
            }
            return message;
        }


        public static int CountReturns(string message)
        {
            // Attempt to skip this expensive code if the message is less than one full page
            int counter = 0;
            // Count carriage returns
            for (int pos = 0; pos <= message.Length - 1; pos++)
            {
                if (message.Substring(pos, 1) == "\n")
                {
                    // Count the CR
                    counter++;
                }
            }
            return counter;
        }

        public static ArrayList GetAllOfType(string type)
        {
            type = type.ToLower();
            ArrayList actors = new ArrayList();
            foreach (Actor actor in Lib.actors)
            {
                if (actor["type"].ToString() == type)
                {
                    actors.Add(actor);
                }
            }
            return actors;
        }




        public static string FormatContentsIntoSentence(ArrayList roomitems)
        {
            string txt = "";

            // Show items in the room
            // This logic shows items in three formats like this:
            // One item: A sword is here.
            // Two items: A sword and a bucket are here.
            // Three or more items: A sword, a bucket, and a knife are here.

            int roomitemscount = roomitems.Count;

            if (roomitemscount < 1)
            {
                return "";
            }

            if (roomitemscount == 1)
            {
                //item = room.GetItemAtIndex(0);
                item = (Actor)roomitems[0];
                if (Convert.ToInt32(item["quantity"]) > 1)
                {
                    txt = item["quantity"] + " " + item["name"] + "s are here.";
                }
                else
                {
                    txt = item.GetNameFull().Substring(0, 1).ToUpper() + item.GetNameFull().Substring(1) + " is here.";

                }
            }
            if (roomitemscount == 2)
            {
                //item = room.GetItemAtIndex(0);
                item = (Actor)roomitems[0];
                if (Convert.ToInt32(item["quantity"]) > 1)
                {
                    txt = item["quantity"] + " " + item["name"] + "s and ";
                }
                else
                {
                    txt = item.GetNameFull().Substring(0, 1).ToUpper() + item.GetNameFull().Substring(1) + " and ";

                }
                //item = room.GetItemAtIndex(1);
                item = (Actor)roomitems[1];
                if (Convert.ToInt32(item["quantity"]) > 1)
                {
                    txt += item["quantity"] + " " + item["name"] + "s are here.";
                }
                else
                {
                    txt += item.GetNameFull() + " are here.";

                }

            }
            if (roomitemscount > 2)
            {
                item = (Actor)roomitems[0];
                if (Convert.ToInt32(item["quantity"]) > 1)
                {
                    txt = item["quantity"] + " " + item["name"] + "s";
                }
                else
                {
                    txt = item.GetNameFull().Substring(0, 1).ToUpper() + item.GetNameFull().Substring(1);

                }
                for (int tmpcounter = 1; tmpcounter <= roomitems.Count - 1; tmpcounter++)
                {
                    item = (Actor)roomitems[tmpcounter];
                    if (tmpcounter == roomitems.Count - 1)
                    {
                        if (Convert.ToInt32(item["quantity"]) > 1)
                        {
                            txt = txt + ", and " + item["quantity"] + " " + item["name"] + "s are here.";
                        }
                        else
                        {
                            txt = txt + ", and " + item.GetNameFull() + " are here.";

                        }
                    }
                    else
                    {
                        if (Convert.ToInt32(item["quantity"]) > 1)
                        {
                            txt = txt + ", " + item["quantity"] + " " + item["name"] + "s";
                        }
                        else
                        {
                            txt = txt + ", " + item.GetNameFull();

                        }
                    }
                }
            }
            return txt;
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

        public class HelpComparer : IComparer
        {
            CaseInsensitiveComparer cic = new CaseInsensitiveComparer();
            string newx = null;
            string newy = null;
            int result = 0;

            int IComparer.Compare(Object x, Object y)
            {
                newx = (string)x;
                newy = (string)y;
                result = cic.Compare(newx, newy);
                return result;
            }
        }


        public static ArrayList GetWorldItems()
        {
            Lib.actors.Sort(new Comparer());
            return Lib.actors;
        }

        public static void AddServerLogEntry(Actor actor, string logentry)
        {
            Lib.log.Add(actor, logentry);
        }

        public static void GameTimeAddMinutes(double minutes)
        {
            Lib.Gametime = Lib.Gametime.AddMinutes(minutes);
        }

        public static float ServerIdleDisconnect
        {
            get
            {
                return Lib.Serverinfo.Idledisconnect;
            }
        }

        public static void AddDelayedCommand(Actor actor, string commandname, string arguments, long time, long delay, bool loop)
        {
            Lib.Delayedcommands.Add(actor, commandname, arguments, time, delay, loop);
        }


        /// <summary>
        /// Function that sends users a given message that requires a yes/no response
        /// and determines how the user answered it.
        /// </summary>
        /// <returns>A boolean that tells how the user responded yes or no.</returns>
        public static bool YesNo(Actor user, string message)
        {
            string Yesnostring = "";

            user.Send(message);
            try
            {
                // if user already disco, then socket is gone and need to catch the exception
                Yesnostring = user.UserSocket.GetResponse();
            }
            catch
            {

            }

            if (Yesnostring == "y" || Yesnostring == "yes")
                return true;
            return false;
        }

        public static bool Exists(string userid)
        {
            return Lib.dbService.Actor.Exists(userid);
        }

        // Tell outdoor users in the whole world that something happened in the sky
        public static void Sayskyevents(string message)
        {
            for (int i = Lib.actors.Count - 1; i >= 0; i--)
            {
                Actor tmpactor = (Actor)Lib.actors[i];
                if (tmpactor["type"].ToString() == "user" || tmpactor["type"].ToString() == "mob")
                {
                    Actor container = tmpactor.GetContainer();
                    if (container != null)
                    {
                        if (container["subtype"].ToString() == "outdoor" || Convert.ToString(container["subtype"]) == "outdoor")
                        {
                            tmpactor.Send(message + "\r\n");
                        }
                    }
                }
            }

        }

        public static bool PopulateContainers(ArrayList actors)
        {
            Actor container;
            foreach (Actor actor in actors)
            {
                if (actor["container"].ToString() != null && actor["containertype"].ToString() != null)
                {
                    // Get the container for this actor
                    container = actor.GetContainer();
                    // Put actor into its container
                    if (container != null)
                    {
                        if (container["id"].ToString() == actor["id"].ToString())
                        {
                            // Found object that is inside itself!
                            actor.Orphan();
                        }
                        else
                        {
                            container.Additem(actor);
                            // Set version number
                            actor.OldVersion = actor.Version;
                        }
                    }
                    else
                    {
                        if (actor["type"].ToString() != "room")
                        {
                            // Found object with null container, which should never happen.
                            actor.Orphan();
                        }
                    }
                }
            }
            return true;
        }




        /// <summary>
        /// Get an object given it's id string
        /// </summary>
        /// <param name="idobj"></param>
        /// <returns>The object or returns null if not found</returns>
        public static Actor GetByID(object idobj)
        {
            Actor user = null;
            if (idobj == null)
            {
                return null;
            }

            string id = idobj.ToString();

            lock (Lib.actors.SyncRoot)
            {
                for (int i = Lib.actors.Count - 1; i >= 0; i--)
                {
                    user = (Actor)Lib.actors[i];
                    if (user["id"].ToString() == id)
                    {
                        return user;
                    }
                }
            }
            // Returns null if nothing was found with the given ID
            return null;
        }

        public static int GetConnectedActorCount()
        {
            int counter = 0;
            foreach (Actor actor in Lib.actors)
            {
                object actorConnected = actor["connected"];
                if (actorConnected != null)
                {
                    if (Lib.ConvertToBoolean(actorConnected) == true)
                    {
                        counter++;
                    }
                }
            }
            return counter;
        }

        public static void Disco(Actor actor, string reason)
        {
            Disco(actor.UserSocket, reason);
        }


        /// <summary>
        /// Turn first letter of a string into upper case.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string FirstToUpper(string text)
        {
            if (text.Length > 1)
            {
                return text.Substring(0, 1).ToUpper() + text.Remove(0, 1);
            }
            else
                return text;
        }


        /// <summary>
        /// Function that disconnects a given socket from the server.
        /// </summary>
        /// <returns>Nothing</returns>
        public static void Disco(IUserSocket userSocket, string reason)
        {
            // Only save a socket that is still connected.
            if (userSocket != null)
            {
                if (userSocket.Connected)
                {
                    // Remove disconnected user from users arraylist
                    lock (Lib.actors.SyncRoot)
                    {
                        foreach (Actor tmpuser in Lib.actors)
                        {
                            // Skip users that are not already connected
                            if (tmpuser.UserSocket != null)
                            {
                                // disconnected users have a socket handle of -1
                                if (tmpuser.UserSocket.UniqueId == userSocket.UniqueId &&
                                    userSocket.Connected)
                                {
                                    // Reset any pending trade flags and combat
                                    tmpuser["tradestate"] = "";
                                    tmpuser.Clearbufferitems();
                                    tmpuser.Tradetargetuser = null;
                                    tmpuser["target"] = "";

                                    // Mark this user as disconnected
                                    tmpuser["connected"] = false;

                                    // Save user settings
                                    if (tmpuser["name"].ToString().Trim() != String.Empty)
                                    {
                                        tmpuser.Save();
                                    }

                                    // Inform users in the room that this person disappeared
                                    foreach (Actor user in tmpuser.GetContainer().GetContents())
                                    {
                                        if (user != null)
                                        {
                                            if (user["type"].ToString() == "user")
                                            {
                                                if (Lib.ConvertToBoolean(user["connected"])) user.Send(tmpuser.GetNameFull() + " disappears.\r\n");
                                            }
                                        }
                                    }

                                    //Lib.items.Remove(tmpuser);
                                    tmpuser["connected"] = false;

                                    // Kill this user thread
                                    break;
                                }
                            }
                        }
                    }
                }
                // Reduce the connection count.
                Interlocked.Decrement(ref Lib.connections);

                // Close the socket
                userSocket.Close();

                // Log a message
                Lib.log.Add(userSocket, null, "DISCONNECT (" + userSocket.ClientEndpointId + ") " + reason + ". Online:" + Lib.connections);
            }

        }

        /// <summary>
        /// Function that determines what item is equipped in a given wear slot.
        /// </summary>
        /// <returns>The item name as a string or null if nothing exists in that slot</returns>
        public static string Findequipslot(Actor user, string equipslot)
        {
            Actor item;
            for (int tmpcounter = user.GetContents().Count - 1; tmpcounter >= 0; tmpcounter--)
            {
                item = user.GetItemAtIndex(tmpcounter);
                if (item != null)
                {
                    // Compare this item's slot with what user may already have in that slot
                    if (item["equipslot"].ToString() == equipslot && Lib.ConvertToBoolean(item["equipped"]) == true)
                    {
                        return item["name"].ToString();
                    }
                }
            }
            // found nothing in that slot.
            return null;
        }

        public static string GetResponse(IUserSocket userSocket)
        {
            return userSocket.GetResponse();
        }

        public static string GetResponse(Actor actor)
        {
            return actor.userSocket.GetResponse();
        }

        public static bool SendMessage(string M_Sender, string M_Receiver, string M_Subject, string M_Body)
        {
            return Lib.dbService.InternalMail.SendMessage(M_Sender, M_Receiver, M_Subject, M_Body);
        }

        public static DataTable GetUnreadMail(string username)
        {
            return Lib.dbService.InternalMail.GetAllUnreadMessages(username);
        }

        public static void MarkAsRead(int mailID)
        {
            Lib.dbService.InternalMail.MarkAsRead(mailID);
        }

        public static DataTable GetAllMail(string username)
        {
            return Lib.dbService.InternalMail.GetAllMessages(username);
        }

        public static DataTable GetSentMail(string username)
        {
            return Lib.dbService.InternalMail.GetAllSentMessages(username);
        }


        /// <summary>
        /// Takes a username and checks if they are online.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>the Actor if online, otherwise null.</returns>
        public static Actor Checkonline(string username)
        {
            // Check if a user is online and returns a reference to the user if so,
            // otherwise returns null.
            lock (Lib.actors.SyncRoot)
            {
                foreach (Actor tmpuser in Lib.actors)
                {
                    if (tmpuser["shortname"].ToString() == username)
                    {
                        if (Lib.ConvertToBoolean(tmpuser["connected"]))
                        {
                            return tmpuser;
                        }
                    }
                }
                // didnt find the user
                return null;
            }
        }


        /// <summary>
        /// Function that prompts the user for username and password.
        /// </summary>
        /// <returns>The username string, null if the user disconnects during login, </returns>
        public static string Authenticate(IUserSocket userSocket, Commandprocessor cp)
        {
            bool result = false;
            string username;
            string userpassword;
            int kickcounter = 0; // Count bad text entry attempts
            int ubercounter = 0;
            bool isnewuser = false;

            do
            {
                kickcounter++;
                ubercounter++;
                do
                {
                    try
                    {
                        // if user already disco, then socket is gone and need to catch the exception
                        userSocket.Send(Lib.Ansifwhite + "Enter your character's name (or type 'new'): ");
                        username = userSocket.GetResponse().ToLower();
                    }
                    catch
                    {
                        return null;
                    }
                    if (username == null)
                    {
                        // encountered some socket error
                        return null;
                    }
                    
                    //// Echo received bytes for debugging
                    //byte[] bytearray;
                    //System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                    //bytearray = encoding.GetBytes(username);
                    //foreach (byte b in bytearray)
                    //{
                    //    userSocket.Send(Convert.ToInt32(b).ToString() + " ");
                    //}

                    // Enforce name rules
                    if ((!(Checkvalidusername.IsMatch(username) && username.Length > 2 && username.Length < 16)))
                    {
                        kickcounter++;
                        userSocket.SendLine(Lib.Ansifred + "Incorrect. Length must be 3-15, begin with a letter, and contain only alphanumeric characters.");
                        // Kick user if they enter too many incorrectly formatted usernames to prevent DoS attacks
                        if (kickcounter > 6)
                        {
                            return null;
                        }
                    }
                } while ((!(Checkvalidusername.IsMatch(username) && username.Length > 2 && username.Length < 16)));

                if (username == null)
                {
                    // encountered some socket error
                    return null;
                }
                if (username == "new")
                {
                    username = Lib.Createnewuser(userSocket, cp);
                    if (username == null)
                    {
                        try
                        {
                            // If user is already disco, then this might fail so catch exception.
                            userSocket.SendLine(Lib.Ansibred + "Too many bad inputs. disconnecting.");
                            //Lib.Disco(ns);
                        }
                        catch
                        {
                            return null;
                        }
                        return null;
                    }
                    isnewuser = true;
                    result = true;
                    break;
                }
                if (kickcounter > 6)
                {
                    return null;
                }

                if (!isnewuser)
                {
                    try
                    {
                        // If user disco during this prompt, the socket it gone and we need to catch this exception.
                        userSocket.Send(Lib.Ansifwhite + "Password: ");
                        userpassword = userSocket.GetResponse();
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                    if (userpassword == null)
                    {
                        return null;
                    }
                    else if (userpassword != "forgot")
                    {
                        userpassword = EncryptPassword(userpassword);
                        result = Lib.ValidateUsernamePassword(username.ToLower(), userpassword);
                        if (!result)
                        {
                            userSocket.SendLine(Lib.Ansifred + "Incorrect username or password. Type the password 'forgot' and we will email it to you at your registered email address.");
                        }
                        // Ensure this user is not already logged in
                        Actor user = new Actor();
                        if (Lib.Checkonline(username) != null)
                        {
                            userSocket.SendLine(Lib.Ansifred + "That user has already logged in. You cannot login twice.");
                            result = false;
                        }
                    }
                    else
                    {
                        EmailPassword emailpassword = new EmailPassword(userSocket, username);
                        emailpassword.CheckEmailPassword();
                        result = false;
                    }
                }
            } while ((result == false) && (ubercounter < 5));
            // Login failed, bail out here
            if (result == false)
            {
                return null;
            }

            //email activation. In Tigermud.xml, set <emailactivation> to "true" to turn on
            TigerMUD.EmailUserActivation o_EmailActivation;
            if (Serverinfo.EmailActivation == "true")
            {
                o_EmailActivation = new EmailUserActivation(userSocket, username);
                if (!o_EmailActivation.CheckEmailActivation())
                {
                    return null;
                }
            }

            // At this point the login has succeeded.
            if (cp.processcommand(userSocket, "", username, true))
            {
                // Login succeeded, return the username
                return username;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Takes an array of commands and determines which is the container name.
        /// </summary>
        /// <returns>The container name</returns>
        public static string GetContainerName(ArrayList words)
        {
            string containername = "";
            for (int i = 0; i < words.Count - 1; i++)
            {
                string test = (string)words[i];
                if ((string)words[i] == "from" || (string)words[i] == "in" || (string)words[i] == "into" || (string)words[i] == "to")
                {
                    i++;
                    // The rest of the words are the container name
                    while (i <= words.Count - 1)
                    {
                        containername += (string)words[i] + " ";
                        i++;
                    }
                }
            }
            containername = containername.Trim();
            if (containername == "" || containername == null) return null;
            else
                return containername;
        }




        // Return an item specified by name and by position number.
        public static Actor GetItemByName(ArrayList items, string itemname, int itemnumber, int itemquantity)
        {
            itemname = itemname.ToLower();

            if (itemnumber < 1) itemnumber = 1;
            int itemcounter = 1;
            if (itemquantity < 1) itemquantity = 1;

            // Bail out if we receive a blank item name
            if (itemname == null)
            {
                return null;
            }
            if (itemname == "")
            {
                return null;
            }

            // This is slightly less efficient, but gives preference to finding players over other things.
            // This prevents Adam's pet or Adam's sword from being returned before Adam himself
            // when using the command 'look adam'.
            foreach (Actor item in items)
            {
                if (item["name"].ToString().ToLower() == itemname || item["shortname"].ToString().ToLower() == itemname)
                {
                    if (item["type"].ToString() == "user" && itemcounter == itemnumber)
                    {
                        return item;
                    }
                    itemcounter++;
                }

            }

            itemcounter = 1;

            // Didn't find it, so search again for matching things other than users
            foreach (Actor item in items)
            {
                if (item != null)
                {
                    if (item["name"].ToString() == itemname || item["name"].ToString() + "s" == itemname || item["shortname"].ToString() == itemname || item["shortname"] + "s" == itemname)
                    {

                        // For stacks, ignore any that are smaller than the quantity the user specified
                        if (item["subtype"].ToString().StartsWith("stack") && (itemcounter == itemnumber) && Convert.ToInt32(item["quantity"]) >= itemquantity)
                        {
                            return item;
                        }
                        // When user specified no item position number, then keep cycling through the stacks
                        // looking for one that big enough to match (or exceed) the quantity the user specified.
                        // We do this because specifying no position number defaults to 1. Sometimes the stack you want isn't
                        // the first stack, so need to keep going.
                        else if (item["subtype"].ToString().StartsWith("stack") && (itemnumber == 1) && Convert.ToInt32(item["quantity"]) >= itemquantity)
                        {
                            return item;
                        }
                        // Itemquantity=1 means there was no quantity specified or they specified just 1
                        else if (itemcounter == itemnumber && itemquantity == 1)
                        {
                            return item;
                        }
                        itemcounter++;
                    }
                }
            }

            foreach (Actor item in items)
            {
                // Attempt to match the last word of the item with user text.
                // For example, user typed 'sword' for Big Ugly Jello Sword.
                ArrayList words = Lib.GetWords(item["name"].ToString());
                string lastword = (string)words[words.Count - 1];

                if (lastword.ToLower() == itemname)
                {
                    return item;
                }
            }

            foreach (Actor item in items)
            {
                // Last ditch attempt to find item using the first few letters of the item name
                // For example, user types 'big monk' instead of Big Monkey of Eternal Doom.

                // We do a new loop here to prevent 'get bag' from returning 'baggy pants' first 
                // when there may actually be a bag in the room.
                if ((item["name"]).ToString().ToLower().StartsWith(itemname))
                {
                    return item;
                }
            }

            // Didn't find what we were looking for.
            return null;
        }

        /// <summary>
        /// Returns the description of the current state of the sun.
        /// </summary>
        /// <returns></returns>
        public static string SunView()
        {
            string sunmessage = "";
            // moonquadrant<4 means it is in the visible sky
            if (Convert.ToInt32(Lib.ServerState["sunquadrant"]) < 4)
            {
                switch (Convert.ToInt32(Lib.ServerState["sunquadrant"]))
                {
                    case 0:
                        sunmessage = "low on the horizon, having risen just earlier.";
                        break;
                    case 1:
                        sunmessage = "rising towards its high point in the sky.";
                        break;
                    case 2:
                        sunmessage = "moving past its high noon position and heading into the afternoon.";
                        break;
                    case 3:
                        sunmessage = "getting lower on the horizon.";
                        break;
                }
                return "The sun is " + sunmessage + "\r\n";
            }
            else
            {
                return "The sun is not currently in the sky.\r\n";
            }
        }

        /// <summary>
        /// Returns the description of the current state of the moon.
        /// </summary>
        /// <returns></returns>
        public static string MoonView()
        {
            // moonquadrant<4 means it is in the visible sky
            if (Convert.ToInt32(Lib.ServerState["moonquadrant"]) < 4)
            {
                return "It is a " + Lib.ServerState["moonphase"] + " moon.\r\n";
            }
            else
            {
                return "The moon is not currently in the sky.\r\n";
            }
        }




        /// <summary>
        /// Takes array of player command words and determines the item name, quantity, numeric prefix,
        /// and if player specified 'all' for item name.
        /// </summary>
        /// <param name="words"></param>
        /// <param name="numericprefix"></param>
        /// <param name="itemquantity"></param>
        /// <param name="all"></param>
        /// <returns></returns>
        public static string GetItemNamePrefixAndQuantity(ArrayList words, ref int numericprefix, ref int itemquantity, ref bool all)
        {
            int lastwordpositionnumber = 0;
            string tmpnumericprefix = "";
            string itemname = "";

            //First check if a guid was provided instead of a regular item name
            if (Convert.ToString(words[0]).IndexOf("-") > 0 && Convert.ToString(words[0]).Length >= 35)
            {
                // itemname is just a guid so return it as-is
                return Convert.ToString(words[0]);
            }

            // Step through each word and find the end of the item name
            // that is marked by the kewyword "from" or by simply reaching the end.
            for (int i = 0; i <= words.Count - 1; i++)
            {
                if ((string)words[i] == "from" || (string)words[i] == "in" || (string)words[i] == "into" || (string)words[i] == "to")
                {
                    // We've reached the end of the item name indicated by reaching the "from" keyword,
                    // for example "gleaming sword from backpack".
                    // Return all the words before "from" as the item name.
                    i--;
                    lastwordpositionnumber = i;
                    break;
                }
                lastwordpositionnumber = i;
            }

            // Now variable "lastwordpositionnumber" contains the number of the last word of the item name.
            // Now cycle again through the words, but only up to i.

            for (int j = 0; j <= lastwordpositionnumber; j++)
            {
                // The first word might actually be a number indicating a quantity if items
                // like in the case with stacks.

                // Is the first word completely a number?
                if (Lib.IsNumeric((string)words[j]))
                {
                    try
                    {
                        // Then this word is our quantity.
                        itemquantity = Convert.ToInt32(words[j]);
                    }
                    catch
                    {
                        // catch exceedingly long numerics
                        itemquantity = 1;
                    }

                }
                // Does the first word specify "all"?
                else if ((string)words[j] == "all")
                {
                    // Then set the all flag;
                    all = true;
                }
                // Does the first word begin with numbers, but isn't all numeric?
                else if (Lib.IsNumeric(((string)words[j]).Substring(0, 1)))
                {

                    // then we need to split the numeric from the alphabetic portion
                    // and this becomes our position number. Like when someone specifies
                    // "get 3sword". That means get the third sword. 
                    int k = 0;
                    while (Lib.IsNumeric(((string)words[j]).Substring(k, 1)))
                    {
                        tmpnumericprefix += ((string)words[j]).Substring(k, 1);
                        k++;
                    }
                    // Captured the whole set of numbers and convert to Int32.
                    numericprefix = Convert.ToInt32(tmpnumericprefix);
                    // Strip the numbers off the word
                    itemname = ((string)words[j]).Substring(k);
                }
                // If nothing else, then we just have the first word of the item name.
                // Keep cycling and get teh rest of the words (if any).
                else
                {
                    itemname += (string)words[j] + " ";
                }
            }
            // Quantity zero means user didn't specify a quantity. Set to default which is 1.
            if (itemquantity == 0) itemquantity = 1;
            // Remove any trailing spaces from the itemname we captured.
            itemname = itemname.Trim();
            return itemname;
        }

        /// <summary>
        /// Function that verifies a username and password.
        /// </summary>
        /// <returns>A boolean for success(true) and failure(false)</returns>
        public static bool ValidateUsernamePassword(string username, string password)
        {
            Actor actor = Lib.GetByName(username);
            if (actor != null)
            {
                if (actor["userpassword"].ToString() == password)
                {
                    return true;
                }
                else
                    return false;

            }
            return false;

            //return Lib.dbService.Library.IsUserLoginValid(username, password);
        }

        public static Actor GetByName(string name)
        {

            for (int i = Lib.actors.Count - 1; i >= 0; i--)
            {
                Actor user = (Actor)Lib.actors[i];
                if (user["shortname"].ToString() == name.ToLower() || user["name"].ToString() == name.ToLower())
                {
                    return user;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns a spell that has the name given.
        /// </summary>
        public static Spell GetSpellByName(string name)
        {
            for (int i = Lib.spells.Count - 1; i >= 0; i--)
            {
                Spell scanspell = (Spell)Lib.spells[i];
                if (name.ToLower() == scanspell["name"].ToString().ToLower() || name.ToLower() == scanspell["shortname"].ToString().ToLower())
                {
                    return scanspell;
                }
            }
            // No spell exists with that name
            return null;
        }

        /// <summary>
        /// Creates a new user and saves it to the database.
        /// </summary>
        /// <returns>The new username string or null on an error</returns>
        public static string Createnewuser(IUserSocket userSocket, Commandprocessor cp)
        {
            int kickcounter = 0; // Count bad text entry attempts
            string newusername;
            string newpassword;

            // the compiler always warns about this usernamealreadyexists variable 
            // not being used. Must be a bug in the compiler.
            bool usernamealreadyexists = false;
            string Yesno;

            do
            {
                do
                {
                    try
                    {
                        // If user disco during this prompt, the socket it gone and we need to catch this exception.
                        userSocket.Send(Lib.Ansifwhite + "Enter the name by which you want to be known: ");
                        newusername = userSocket.GetResponse().ToLower();
                        userSocket.Send(Lib.Ansifwhite + "You entered '" + newusername + "'. Is this correct? (y/n): ");
                        Yesno = userSocket.GetResponse().ToLower();
                    }
                    catch
                    {
                        return null;
                    }
                    kickcounter++;
                    // Kick user if they enter too many incorrectly formatted usernames to prevent DoS attacks
                    if (kickcounter > 9)
                    {
                        return null;
                    }
                } while ((Yesno != "y") && (Yesno != "yes"));

                // Enforce username character rules
                if (!Checkvalidusername.IsMatch(newusername))
                {
                    kickcounter++;
                    userSocket.SendLine(Lib.Ansifred + "Incorrect. Length must be 3-15, begin with a letter, and contain only alphanumeric characters.");

                    // Kick user if they enter too many incorrectly formatted usernames to prevent DoS attacks
                    if (kickcounter > 9)
                    {
                        return null;
                    }
                }

                // Check if the username already exists
                usernamealreadyexists = Lib.Exists(newusername);
                if (usernamealreadyexists)
                {
                    kickcounter++;
                    userSocket.SendLine(Lib.Ansifred + "This name is already in use. Please try again.");

                    // Kick user if they enter too many incorrectly formatted usernames to prevent DoS attacks
                    if (kickcounter > 9)
                    {
                        return null;
                    }
                }
            } while ((!Checkvalidusername.IsMatch(newusername)) || usernamealreadyexists);
            // Passed new username rules check
            // Get new password
            kickcounter = 0;
            do
            {
                do
                {
                    try
                    {
                        // If user disco during this prompt, the socket it gone and we need to catch this exception.
                        userSocket.Send(Lib.Ansifwhite + "Enter the password you want for this character: ");
                        newpassword = userSocket.GetResponse();
                        userSocket.Send(Lib.Ansifwhite + "You entered '" + newpassword + "'. Is this correct? (y/n): ");
                        Yesno = userSocket.GetResponse().ToLower();
                    }
                    catch
                    {
                        return null;
                    }
                } while (Yesno != "y" && Yesno != "yes");
                if (!Checkvalidpassword.IsMatch(newpassword) || newpassword.ToLower() == newusername.ToLower())
                {
                    kickcounter++;
                    userSocket.SendLine(Lib.Ansifred + "Invalid Password.  The password cannot match your username, length must be 5-15 characters, and only contain alphanumeric characters.");
                    // Kick user if they enter too many incorrectly formatted passwords to prevent DoS attacks
                    if (kickcounter > 6)
                    {
                        return null;
                    }
                }
            } while (!Checkvalidpassword.IsMatch(newpassword) || newpassword.ToLower() == newusername.ToLower());
            // Passed the password rules check
            //Encrypt the password before sending to DB
            newpassword = EncryptPassword(newpassword);
            // Create this new user in the DB
            try
            {
                Actor spawnlocation = Lib.GetSpecialLocation("new user spawn", "");

                Actor tempuser = new Actor(newusername, userSocket);
                tempuser["shortname"] = newusername;
                tempuser["userpassword"] = newpassword;
                tempuser["shortnameupper"] = Lib.FirstToUpper(tempuser["shortname"].ToString());

                tempuser["container"] = spawnlocation["id"];
                tempuser["containertype"] = spawnlocation["type"].ToString();

                tempuser["descr"] = "looks like a new player.";
                tempuser["name"] = tempuser["shortnameupper"] + " the New";
                tempuser["gender"] = "male"; // for now all males until we add code to let new user choose
                tempuser["cash"] = 100;
                tempuser["health"] = 100;
                tempuser["mana"] = 100;
                tempuser["stamina"] = 100;
                tempuser["lastmessageticks"] = Lib.GetTime();

                tempuser["colorcommandprompt"] = Lib.Ansifyellow;
                tempuser["colorcommandtext"] = Lib.Ansifboldyellow;
                tempuser["colorerrors"] = Lib.Ansifred;
                tempuser["colorexits"] = Lib.Ansifboldwhite;
                tempuser["coloritems"] = Lib.Ansifboldgreen;
                tempuser["colormessages"] = Lib.Ansifwhite;
                tempuser["colormobs"] = Lib.Ansifboldcyan;
                tempuser["colorpeople"] = Lib.Ansifboldcyan;
                tempuser["colorroomdescr"] = Lib.Ansifwhite;
                tempuser["colorroomname"] = Lib.Ansifboldwhite;
                tempuser["coloralertgood"] = Lib.Ansifboldgreen;
                tempuser["coloralertbad"] = Lib.Ansifboldred;
                tempuser["colorsystemmessage"] = Lib.Ansifboldpurple;
                tempuser["colorannouncement"] = Lib.Ansifboldyellow;
                tempuser["connected"] = false;
                tempuser["screenwidth"] = 80;
                tempuser["wordwrap"] = true;
                tempuser["more"] = true;
                tempuser["kudostogive"] = 0;
                tempuser["reputation"] = 0;
                tempuser["experience"] = 0;
                tempuser["userlevel"] = 1; // all new users start at level 1
                tempuser["played"] = 0;
                tempuser["health"] = 1;
                tempuser["stamina"] = 2;
                tempuser["mana"] = 3;
                tempuser["strength"] = 10;
                tempuser["agility"] = 20;
                tempuser["intellect"] = 30;
                tempuser["spirit"] = 40;
                tempuser["defaultsay"] = "say";
                tempuser["healthmax"] = 100;
                tempuser["staminamax"] = 50;
                tempuser["manamax"] = 200;
                tempuser["lastlogindate"] = DateTime.Now;
                tempuser["lastloginip"] = "nowhere";

                // Add Equipment Slots
                tempuser["wearhead"] = "";
                tempuser["wearneck"] = "";
                tempuser["wearshoulders"] = "";
                tempuser["wearback"] = "";
                tempuser["weararms"] = "";
                tempuser["wearwrists"] = "";
                tempuser["wearhands"] = "";
                tempuser["wearrightring"] = "";
                tempuser["wearleftring"] = "";
                tempuser["wearchest"] = "";
                tempuser["wearwaist"] = "";
                tempuser["wearlegs"] = "";
                tempuser["wearfeet"] = "";
                tempuser["wearweapon1"] = "";
                tempuser["wearweapon2"] = "";

                tempuser["spellpending"] = "";
                tempuser["morebuffer"] = "";
                tempuser["zone"] = spawnlocation["zone"].ToString();

                // Add starter spells
                tempuser["spellknown_heal1"] = 550;
                tempuser["spellknown_poison1"] = 800;
                tempuser["skillknown_healing"] = 1000;
                tempuser["skillknown_poison"] = 1000;
                tempuser["skillknown_melee"] = 1000;
                tempuser["skillknown_swords"] = 1000;

                tempuser["accesslevel"] = 10;

                // Have to dot hsi because just added spells to user
                tempuser.LoadSpells();

                // Save new user to db
                tempuser.Save();

                // Add new user to memory
                Lib.actors.Add(tempuser);

                //Add new user to their container
                Actor container = tempuser.GetContainer();
                container.Additem(tempuser);
            }
            catch (Exception ex)
            {
                Lib.log.Add("Lib.Createnewuser function", DateTime.Now + " EXCEPTION " + ex.Message + ex.StackTrace);
            }
            userSocket.SendLine(Lib.Ansifwhite + newusername + " was created successfully.");
            userSocket.SendLine("");
            return newusername;
        }

        /// <summary>
        /// Encrypts user password
        /// </summary>
        /// <returns> The Encrypted Password.</returns>
        public static string EncryptPassword(string clearPassword)
        {
            string textToHash = clearPassword;
            byte[] byteRep = UnicodeEncoding.UTF8.GetBytes(textToHash);
            byte[] hashedTextInBytes = null;
            MD5CryptoServiceProvider mymd5 = new MD5CryptoServiceProvider();
            hashedTextInBytes = mymd5.ComputeHash(byteRep);
            string hashedText = Convert.ToBase64String(hashedTextInBytes);
            return hashedText;
        }
        /// <summary>
        /// Finds each word in a command string from the user.
        /// </summary>
        /// <returns>An arraylist with each word cleanly separated</returns>
        public static ArrayList GetWords(string playercommand)
        {
            ArrayList words = new ArrayList();
            foreach (string word in playercommand.Split(' '))
            {
                words.Add(word.Trim());
            }
            if (words != null)
            {
                // Ensure that we don't return count=1 when there isn't anything in the array
                if ((string)words[0] == "")
                {
                    words.Clear();
                    return words;
                }
            }
            return words;
        }


        /// <summary>
        /// Adds command words to the command list.  Each Command may have more than 1 word,
        /// so this adds them all.
        /// </summary>
        /// <returns>An arraylist with each word cleanly separated</returns>
        public static void AddCommandWord(Hashtable hashtable, Command command)
        {
            foreach (string word in command.Words)
            {
                if (hashtable[word] != null)
                {
                    //Lib.PrintLine(word + " already exists.  Not adding.");
                    continue;
                }
                hashtable.Add(word, command.Name);
            }
        }

        /// <summary>
        /// Deletes a command word from all the command word hashtables
        /// </summary>
        /// <param name="command"></param>
        public static void DeleteCommandWord(Command command)
        {
            DeleteCommandWordFromHashtable(Lib.Commands, command);
            DeleteCommandWordFromHashtable(Lib.PlayerCommandWords, command);
            DeleteCommandWordFromHashtable(Lib.BuilderCommandWords, command);
            DeleteCommandWordFromHashtable(Lib.AdminCommandWords, command);
            DeleteCommandWordFromHashtable(Lib.UberAdminCommandWords, command);
        }

        /// <summary>
        /// Deletes a command word from a specific command word hashtable
        /// </summary>
        /// <param name="hashtable"></param>
        /// <param name="command"></param>
        public static void DeleteCommandWordFromHashtable(Hashtable hashtable, Command command)
        {
            foreach (string word in command.Words)
            {
                // Does word already exist?
                if (hashtable[command.Name] != null)
                {
                    // Then delete it.
                    hashtable.Remove(command.Name);
                }
            }
        }

        /// <summary>
        /// Adds commands to the global command list.
        /// </summary>
        /// <returns>Returns Command to be able to chain with AddCommandWord</returns>
        public static Command AddCommand(Command command)
        {
            if (Commands[command.Name] != null)
            {
                // Delete any old versions
                //Lib.DeleteCommandWordFromHashtable(Commands,command);
                //Lib.PrintLine(command["name"] + " already exists.  Not adding.");
                return (Command)Commands[command.Name];
            }
            Commands.Add(command.Name, command);
            return command;
        }

        /// <summary>
        /// Allows easy retrieval of an Command by name.
        /// </summary>
        /// <returns>Command</returns>
        public static Command GetCommandByName(string name)
        {
            return (Command)Commands[name];
        }

        public static void AddCommandToDb(AccessLevel accesslevel, string commandname)
        {
            Lib.dbService.SystemCommands.AddCommandToDb((int)accesslevel, commandname);
        }


        /// <summary>
        /// Allows easy retrieval of an Command by word.
        /// </summary>
        /// <returns>Command</returns>
        public static Command GetCommandByWord(int accesslevel, string word)
        {
            string commandname = (string)SystemCommandWords[word];
            if (commandname != null)
                return (Command)Commands[commandname];
            commandname = (string)PlayerCommandWords[word];
            if (commandname != null)
                return (Command)Commands[commandname];
            if (accesslevel >= (int)AccessLevel.Builder)
            {
                commandname = (string)BuilderCommandWords[word];
                if (commandname != null)
                    return (Command)Commands[commandname];
            }
            if (accesslevel >= (int)AccessLevel.Admin)
            {
                commandname = (string)AdminCommandWords[word];
                if (commandname != null)
                    return (Command)Commands[commandname];
            }
            if (accesslevel >= (int)AccessLevel.UberAdmin)
            {
                commandname = (string)UberAdminCommandWords[word];
                if (commandname != null)
                    return (Command)Commands[commandname];
            }

            return null;
        }

        /// <summary>
        /// Gets a list of available system commands.
        /// </summary>
        /// <returns>The commands in a string array</returns>
        public static string[] GetCommandList()
        {
            string[] temp = new string[Commands.Count];
            Commands.Keys.CopyTo(temp, 0);
            return temp;
        }

        /// <summary>
        /// Gets a list of system command words.
        /// </summary>
        /// <returns>The words in a string array</returns>
        public static string[] GetCommandWordList(int accesslevel)
        {
            ArrayList commandwordlist = new ArrayList();

            IEnumerator enumerator = SystemCommandWords.Values.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Command command = GetCommandByName((string)enumerator.Current);
                foreach (string word in command.Words)
                {
                    if (!commandwordlist.Contains(word))
                    {
                        commandwordlist.Add(word);
                    }
                }
            }
            enumerator = PlayerCommandWords.Values.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Command command = GetCommandByName((string)enumerator.Current);
                foreach (string word in command.Words)
                {
                    if (!commandwordlist.Contains(word))
                    {
                        commandwordlist.Add(word);
                    }
                }
            }
            if (accesslevel >= (int)AccessLevel.Builder)
            {
                enumerator = BuilderCommandWords.Values.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Command command = GetCommandByName((string)enumerator.Current);
                    foreach (string word in command.Words)
                    {
                        if (!commandwordlist.Contains(word))
                        {
                            commandwordlist.Add(word);
                        }
                    }
                }
            }
            if (accesslevel >= (int)AccessLevel.Admin)
            {
                enumerator = AdminCommandWords.Values.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Command command = GetCommandByName((string)enumerator.Current);
                    foreach (string word in command.Words)
                    {
                        if (!commandwordlist.Contains(word))
                        {
                            commandwordlist.Add(word);
                        }
                    }
                }
            }
            if (accesslevel >= (int)AccessLevel.UberAdmin)
            {
                enumerator = UberAdminCommandWords.Values.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Command command = GetCommandByName((string)enumerator.Current);
                    foreach (string word in command.Words)
                    {
                        if (!commandwordlist.Contains(word))
                        {
                            commandwordlist.Add(word);
                        }
                    }
                }
            }
            commandwordlist.Sort(new HelpComparer());
            return (string[])commandwordlist.ToArray(typeof(string));
        }

        /// <summary>
        /// Adds an action to the global action list.
        /// </summary>
        /// <returns>Nothing</returns>
        public static void AddAction(IAction action)
        {
            if (Actions[action.Name] != null)
            {
                //Lib.PrintLine(action["name"] + " already exists.  Not adding.");
                return;
            }
            Actions.Add(action.Name, action);
            //Console.WriteLine("Loaded action: " + action["name"]);

        }

        /// <summary>
        /// Gets an IAction by name.
        /// </summary>
        /// <returns>An IAction</returns>
        public static IAction GetActionByName(string name)
        {
            return (IAction)Actions[name];
        }

        /// <summary>
        /// Splits a command from its arguments.
        /// </summary>
        /// <returns>The command and arguments in a string array</returns>
        public static string[] SplitCommand(string playercommand)
        {
            // If it starts with a letter, get the letters up to the first space for the
            // command and after the first space for the arguments
            string[] splitcommand = new string[2] { "", "" };

            if ((playercommand[0] >= 'a' && playercommand[0] <= 'z') || (playercommand[0] >= 'A' && playercommand[0] <= 'Z'))
            {
                int i = playercommand.IndexOf(" ");
                if ((i > 0) && (playercommand.Length > i + 1))
                {
                    splitcommand[0] = playercommand.Substring(0, i);
                    splitcommand[1] = playercommand.Substring(splitcommand[0].Length + 1);
                }
                else
                {
                    splitcommand[0] = playercommand;
                }
            }
            else // Otherwise, get the first character as command, everything else as arguments
            {
                splitcommand[0] += playercommand[0]; // Can't assign char to string, but value is "" so just add it
                if (playercommand.Length > 1)
                    splitcommand[1] = playercommand.Substring(1);
            }

            return splitcommand;
        }



        /// <summary>
        /// Takes a duration and finds the future ticks when the time is up
        /// </summary>
        /// <returns>The future tick count as a long</returns>
        public static float GetExpirationTicks(float duration)
        {
            float expiration = duration + Lib.GetTime();
            return expiration;
        }

        /// <summary>
        /// Sends a Mail Message on SMTP.
        /// </summary>
        /// <param name="mailMessage">The message to send.</param>
        public static void SendEmail(MailMessage mailMessage)
        {
            SmtpMail.SmtpServer = Lib.Serverinfo.SmtpServer;
            SmtpMail.Send(mailMessage);
        }


        /// <summary>
        /// Determines if a given server state variable already exists in the database.
        /// </summary>
        /// <returns>A boolean with the result</returns>
        public static bool ServerStateExistsInDatabase(string name)
        {
            return Lib.dbService.Library.ServerStateExists(name);
        }

        /// <summary>
        /// Loads a server state variable from the mudstate database table.
        /// </summary>
        /// <returns>Nothing</returns>
        public static void LoadServerState()
        {
            DataTable dt = Lib.dbService.Library.GetAllServerStates();
            foreach (DataRow datarow in dt.Rows)
            {
                string name = (string)datarow["name"];
                string value = (string)datarow["setting"];
                Lib.ServerState[name] = value;
            }
        }

        /// <summary>
        /// Gets the current server time measured in ticks.
        /// </summary>
        /// <returns>A long containing the number of server ticks</returns>
        public static long GetTime()
        {
            return Convert.ToInt64(Lib.ServerState["serverticks"].ToString());
        }

        /// <summary>
        /// Converts a string into its equivalent boolean.
        /// </summary>
        /// <returns>The new boolean</returns>
        public static bool ConvertToBoolean(object value)
        {
            if (value == null)
            {
                return false;
            }
            string newvalue = "";
            try
            {
                newvalue = Convert.ToString(value);
            }
            catch
            {
                return false;
            }
            return ConvertToBoolean(newvalue);


        }

        /// <summary>
        /// Converts a string into its equivalent boolean.
        /// </summary>
        /// <returns>The new boolean</returns>
        public static bool ConvertToBoolean(string value)
        {
            switch (value.ToLower())
            {
                case "0":
                    return false;
                case "no":
                    return false;
                case "false":
                    return false;
                case "1":
                    return true;
                case "yes":
                    return true;
                case "true":
                    return true;
                default:
                    return false;
            }
        }


        public static string PaddedLineItem(string item, string item2, char padchar, int width)
        {
            int pad = width - (item.Length + item2.Length);
            if (pad < 0)
            {
                pad = 0;
            }
            string line = item + new string(padchar, pad) + item2;
            return line;
        }

        public static string PaddedLineItem(string item, string item2, string item3, char padchar, int width)
        {
            double pad = width - (item.Length + item2.Length + item3.Length);
            pad = pad / 2;
            double pad2 = pad;
            if (pad < 0)
            {
                pad = 0;
            }
            if (pad2 < 0)
            {
                pad2 = 0;
            }
            // If the pad is odd
            if (((pad * 2) % 2) != 0)
            {
                pad -= .5;
                pad2 += .5;
            }
            string line = item + new string(padchar, (int)pad) + item2 + new string(padchar, (int)pad2) + item3;
            return line;
        }
    }



    /// <summary>
    /// Contains several useful data conversion methods.
    /// </summary>
    public sealed class Conversions
    {
        /// <summary>
        /// Converts a string to upper case and retains the numeric data in the string.
        /// Note that the standard ToUpper() won't.
        /// </summary>
        /// <returns>The new string</returns>
        public static string ToUpperKeepingNumbers(string stringIn)
        {
            System.Text.UnicodeEncoding UE_ByteConvertor = new UnicodeEncoding();

            byte[] data = UE_ByteConvertor.GetBytes(stringIn);

            for (int count = 0; count < data.Length; count++)
            {
                if (data[count] >= 97 && data[count] <= 122)
                    data[count] = (byte)(Convert.ToInt32(data[count]) - 32);
                else
                    data[count] = data[count];
            }

            return UE_ByteConvertor.GetString(data);
        }

        /// <summary>
        /// Converts a string to lower case and retains the numeric data in the string.
        /// Note that the standard ToLower() won't.
        /// </summary>
        /// <returns>The new string</returns>
        public static string ToLowerKeepingNumbers(string stringIn)
        {
            System.Text.UnicodeEncoding UE_ByteConvertor = new UnicodeEncoding();

            byte[] data = UE_ByteConvertor.GetBytes(stringIn);

            for (int count = 0; count < data.Length; count++)
            {
                if (data[count] >= 65 && data[count] <= 90)
                    data[count] = (byte)(Convert.ToInt32(data[count]) + 32);
            }

            return UE_ByteConvertor.GetString(data);
        }


        /// <summary>
        /// Converts a char number to its literal number.
        /// </summary>
        /// <returns>The new number as a byte</returns>
        public static byte CharToNum(char CharIn)
        {
            if (Char.IsNumber(CharIn))
                return (byte)(Convert.ToByte(CharIn) - 48);
            else
                throw new Exception("Not a number!");
        }

        /// <summary>
        /// Strips all non-numeric characters from a string.
        /// </summary>
        /// <returns>The new string</returns>
        public static string ToNumber(string StringIn)
        {
            if (StringIn.Length > 0)
            {
                if (StringIn.Substring(0, 1) == "-")
                    return "-" + Regex.Replace(StringIn, @"[\D]", "");
                else
                    return Regex.Replace(StringIn, @"[\D]", "");
            }
            else
                return "";
        }

        /// <summary>
        /// Strips all non-alphabetic characters from a string.
        /// </summary>

        /// <returns>The new string</returns>
        public static string ToCharacter(string StringIn)
        {
            return Regex.Replace(StringIn, @"[\W\d]", "");
        }

        /// <summary>
        /// Strips all non-alphanumeric characters from a string
        /// </summary>

        /// <returns>The new string</returns>
        public static string ToAlphanumeric(string StringIn)
        {
            return Regex.Replace(StringIn, @"[\W]", "");
        }

        /// <summary>
        /// Strips all non-numeric characters from a string while leaving any spaces.
        /// </summary>

        /// <returns>The new string</returns>
        public static string ToNumberLeavingSpaces(string StringIn)
        {
            return Regex.Replace(StringIn, @"[^0-9\x20]", "");
        }

        /// <summary>
        /// Strips all non-alphbetic characters from a string while leaving any spaces.
        /// </summary>

        /// <returns>The new string</returns>
        public static string ToCharacterLeavingSpaces(string StringIn)
        {
            return Regex.Replace(StringIn, @"[^a-zA-Z\x20]", "");
        }

        /// <summary>
        /// Strips all non-alphanumeric characters from a string while leaving any spaces.
        /// </summary>

        /// <returns>The new string</returns>
        public static string ToAlphanumericLeavingSpaces(string StringIn)
        {
            return Regex.Replace(StringIn, @"[^a-zA-Z0-9\x20]", "");
        }

        /// <summary>
        /// Converts a decimal into a monetary decimal. For example: 199.999 is turned into 200.00
        /// </summary>
        /// <returns>The new string</returns>
        public static string ToDecimalNumberComponent(string StringIn)
        {
            return Convert.ToString(Regex.Replace(StringIn, @"[^\d\-\.]", ""));
        }

        public static string ToDecimalNumber(string StringIn)
        {
            StringIn = Regex.Replace(StringIn, @"[^\-\d\.]", "");
            StringIn = Regex.Replace(StringIn, @"\-{2,}", "-");
            StringIn = Regex.Replace(StringIn, @"\.{2,}", ".");
            StringIn = Regex.Replace(StringIn, @"\s", "");

            StringIn = Regex.Match(StringIn, @"[-]{0,1}[\d]{1,}[\.]?[\d]{0,}").ToString();

            return StringIn;

        }
    }

    public sealed class Validation
    {
        /// <summary>
        /// Checks if a string is a number
        /// </summary>
        /// <returns>A boolean</returns>
        public static bool IsDecimalNumberComponant(string StringIn)
        {
            if (Convert.ToString(Regex.Replace(StringIn, @"[^\d\-\.]", "")).Length == StringIn.Length)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Checks if a string is numeric
        /// </summary>
        /// <returns>A boolean</returns>
        public static bool IsNumber(string StringIn)
        {
            if (Convert.ToString(Regex.Replace(StringIn, @"[\D]", "")).Length == StringIn.Length)
                return true;
            else
                return false;

        }

        public static bool IsDecimalNumber(string StringIn)
        {
            string StringSum = "";
            StringSum = Regex.Replace(StringIn, @"[^\-\d\.]", "");
            StringSum = Regex.Replace(StringSum, @"-{2,}", "-");
            StringSum = Regex.Replace(StringSum, @"\.{2,}", ".");
            StringSum = Regex.Replace(StringSum, @"\s", "");
            StringSum = Regex.Match(StringSum, @"[-]{0,1}[\d]{1,}[\.]?[\d]{0,}").ToString();
            if (StringSum == StringIn)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Checks if a string is a letter
        /// </summary>
        /// <returns>A boolean</returns>
        public static bool IsCharacter(string StringIn)
        {
            if (Convert.ToString(Regex.Replace(StringIn, @"[\W\d]", "")).Length == StringIn.Length)
                return true;
            else
                return false;

        }

        /// <summary>
        /// Checks if a string is alphanumeric
        /// </summary>
        /// <returns>A boolean</returns>
        public static bool IsAlphanumeric(string StringIn)
        {
            if (Convert.ToString(Regex.Replace(StringIn, @"[\W]", "")).Length == StringIn.Length)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Checks if a string is all numbers and spaces
        /// </summary>
        /// <returns>A boolean</returns>
        public static bool IsNumberWithSpaces(string StringIn)
        {
            if (Convert.ToString(Regex.Replace(StringIn, @"[^0-9\x20]", "")).Length == StringIn.Length)
                return true;
            else
                return false;

        }

        /// <summary>
        /// Checks if a string is a letter
        /// </summary>
        /// <returns>A boolean</returns>
        public static bool IsCharacterWithSpaces(string StringIn)
        {
            if (Convert.ToString(Regex.Replace(StringIn, @"[^a-zA-Z\x20]", "")).Length == StringIn.Length)
                return true;
            else
                return false;

        }

        /// <summary>
        /// Checks if a string is alphanumeric with spaces
        /// </summary>
        /// <returns>A boolean</returns>
        public static bool IsAlphanumericWithSpaces(string StringIn)
        {
            if (Convert.ToString(Regex.Replace(StringIn, @"[^a-zA-Z0-9\x20]", "")).Length == StringIn.Length)
                return true;
            else
                return false;
        }

        public static bool IsSpaces(string StringIn)
        {
            if (Convert.ToString(Regex.Replace(StringIn, @"\x20", "")).Length == 0)
                return true;
            else
                return false;

        }
    }
}