using System;
using System.Collections;
using System.Text;
using System.Threading;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;

namespace GameLibrary
{
    /// <summary>
    /// A threadsafe singleton that holds game server context data used all over the game and shared between threads.
    /// </summary>
    public sealed class GameContext : IDisposable
    {

        public static object lockobj = new object();
        List<PlayerCharacter> connectedplayers;
        Hashtable commands;
        Dictionary<string, Planet> planets;
        Dictionary<string, Room> rooms;
        private Regex emailregex;
        string commandword;
        string[] words;
        static GameContext instance;

        private GameContext()
        {
            connectedplayers = new List<PlayerCharacter>();
            commands = new Hashtable();
            planets = new Dictionary<string, Planet>();
            rooms = new Dictionary<string, Room>();
            emailregex = new Regex("(?<user>[^@]+)@(?<host>.+)");
            

        }


        public static GameContext GetInstance()
        {
            lock (lockobj)
            {
                if (instance == null)
                {
                    instance = new GameContext();
                    
                }
                return instance;
            }
        }

        public Regex Emailregex
        {
            get
            {
                return emailregex;
            }

        }

   

        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free other state (managed objects).
                database.Dispose();
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.
        }

        // Use C# destructor syntax for finalization code.
        ~GameContext()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }

        private GameCompiler compiler;

        public GameCompiler Compiler
        {
            get { return compiler; }
            set { compiler = value; }
        }

        private Database database;

        public Database Database
        {
            get { return database; }
            set { database = value; }
        }

        public Planet GetPlanet(string id)
        {
            Planet planet;
            lock (lockobj)
            {
                planet = planets[id];
            }
            return planet;
        }

        public Room GetRoom(string id)
        {
            Room room;
            lock (lockobj)
            {
                room = rooms[id];

            }
            return room;

        }


        public Planet GetPlanetByName(string name)
        {
            lock (lockobj)
            {
                List<Planet> tmpplanets = new List<Planet>();
                Dictionary<string, Planet>.Enumerator enumerator = planets.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Value.NameDisplay == name)
                    {
                        return enumerator.Current.Value;
                    }
                }

            }
            return null;
        }


        public List<Planet> Planets
        {

            get
            {
                List<Planet> tmpplanets;
                lock (lockobj)
                {

                    tmpplanets = new List<Planet>();
                    Dictionary<string, Planet>.Enumerator enumerator = planets.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        tmpplanets.Add(enumerator.Current.Value);
                    }
                }
                return tmpplanets;
            }

            set
            {
                lock (lockobj)
                {
                    foreach (Planet planet in value)
                    {
                        planets.Add(planet.Id, planet);
                    }
                }
            }

        }

        public bool AddPlanet(Planet planet)
        {
            lock (lockobj)
            {
                planets.Add(planet.Id, planet);
            }
            return false;

        }

        public bool AddRoom(Room room)
        {
            lock (lockobj)
            {
                rooms.Add(room.Id, room);
            }
            return false;

        }

        public bool ClearPlanets()
        {
            lock (lockobj)
            {
                planets.Clear();
            }
            return false;

        }

        public bool ClearRooms()
        {
            lock (lockobj)
            {
                rooms.Clear();
            }
            return false;

        }



        /// <summary>
        /// Returns filenames of every file in and below the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ArrayList GetFilesRecursive(string path, string mask)
        {
            ArrayList list = new ArrayList();
            DirectoryInfo di = new DirectoryInfo(path);
            foreach (string f in Directory.GetFiles(di.FullName, mask))
            {
                FileInfo fi = new FileInfo(f);
                list.Add(fi.FullName);
            }
            foreach (string d in Directory.GetDirectories(path))
            {
                Console.WriteLine(d);
                list.AddRange(GetFilesRecursive(d, mask));
            }
            return list;
        }

        public string GetCommandAndParams(string message, ref string arguments)
        {
            if (message == null) throw new ArgumentNullException("message");

            // Pre-processing
            message = message.Trim();
            // Get words separated by spaces
            words = message.Split(' ');
            // First word is always the command
            commandword = words[0];
            // The rest are arguments
            arguments = message.TrimStart(commandword.ToCharArray());
            arguments = arguments.Trim();

            return commandword;
        }

        public void AddPlayerCharacter(PlayerCharacter pc)
        {
            if (pc == null) throw new ArgumentNullException("pc");

            lock (lockobj)
            {
                connectedplayers.Add(pc);
            }
            Console.WriteLine("Connection from {0} {1} total {2}", pc.Id, pc.IPAddress, connectedplayers.Count);
        }

        public void AddLoaderConsoleConnection(PlayerCharacter pc)
        {
            if (pc == null) throw new ArgumentNullException("pc");

            Console.WriteLine("LOADER CONSOLE connection from {0} {1} total {2}", pc.Id, pc.IPAddress, connectedplayers.Count);
        }

        public void RemovePlayerCharacter(PlayerCharacter pc)
        {
            if (pc == null) throw new ArgumentNullException("pc");

            lock (lockobj)
            {
                connectedplayers.Remove(pc);
            }
            Console.WriteLine("Disconnect from {0} {1} total {2}", pc.Id, pc.IPAddress, connectedplayers.Count);
        }

        public List<PlayerCharacter> Players
        {
            get
            {
                return connectedplayers;
            }
        }

        public void AddCommand(Command c)
        {
            if (c == null) throw new ArgumentNullException("c");

            Console.WriteLine("Adding command name: {0}", c.Name);

            lock (lockobj)
            {
                foreach (string word in c.Words)
                {
                    Console.WriteLine("Adding {0}", word);
                    if (commands.ContainsKey(word))
                    {
                        commands.Remove(word);
                    }
                    commands.Add(word, c);
                }
            }
        }

        public void RemoveCommand(Command c)
        {
            if (c == null) throw new ArgumentNullException("c");

            lock (lockobj)
            {
                foreach (string word in c.Words)
                    commands.Remove(word);
            }
        }

        public Command GetCommand(string word)
        {
            if (word == null) return null;

            return (Command)commands[word];
        }


        public void ReadConfiguration()
        {
            // Get the appSettings.
            NameValueCollection appSettings = System.Configuration.ConfigurationManager.AppSettings;
            if (!appSettings.HasKeys())
            {
                Console.WriteLine("ERROR: Missing app settings from config file");
                return;
            }

            this.DataSource = Convert.ToString(appSettings["Data Source"]);
            this.DatabaseName = Convert.ToString(appSettings["Database"]);
            this.Security = Convert.ToString(appSettings["Integrated Security"]);
            this.CommandsFolder = Convert.ToString(appSettings["Commands Folder"]);
            this.PlanetsFolder = Convert.ToString(appSettings["Planets Folder"]);
            this.MaxLoginAttempts = Convert.ToInt32(appSettings["Max Login Attempts"]);
            this.SmtpServer = Convert.ToString(appSettings["SMTP Server"]);
            this.SmtpServerUsername = Convert.ToString(appSettings["SMTP Server Username"]);
            this.SmtpServerPassword = Convert.ToString(appSettings["SMTP Server Password"]);
            this.WelcomeEmailFrom = Convert.ToString(appSettings["Welcome Email From"]);
            this.WelcomeEmailSubject = Convert.ToString(appSettings["Welcome Email Subject"]);
            this.WelcomeEmailBody = Convert.ToString(appSettings["Welcome Email Body"]);
            this.WelcomeEmailCC = Convert.ToString(appSettings["Welcome Email CC"]);
            this.WelcomeEmailBcc = Convert.ToString(appSettings["Welcome Email Bcc"]);

            this.CommandTimer = Convert.ToBoolean(appSettings["Command Timer"]);
            this.LoaderPort = Convert.ToInt32(appSettings["Loader Port"]);
            this.ServerPort = Convert.ToInt32(appSettings["Server Port"]);
            this.BypassAuthentication = Convert.ToBoolean(appSettings["Bypass Authentication"]);
            this.AutoStartServer = Convert.ToBoolean(appSettings["AutoStart Server"]);

        }

        private bool autostart;

        public bool AutoStartServer
        {
            get { return autostart; }
            set { autostart = value; }
        }
	
        private int loaderport;

        public int LoaderPort
        {
            get { return loaderport; }
            set { loaderport = value; }
        }

        private int serverport;

        public int ServerPort
        {
            get { return serverport; }
            set { serverport = value; }
        }

        private bool bypassauthentication;

        public bool BypassAuthentication
        {
            get { return bypassauthentication; }
            set { bypassauthentication = value; }
        }
	

        public string ConfigurationString
        {

            get
            {
                string config = string.Empty;
                // Get the AppSettings collection.
                NameValueCollection appSettings =
                   System.Configuration.ConfigurationManager.AppSettings;

                string[] keys = appSettings.AllKeys;

                // Loop to get key/value pairs.
                for (int i = 0; i < appSettings.Count; i++)
                {
                    config += keys[i] + "=" + appSettings[i] + "\r\n";
                }
                return config;
            }
        }


        public ArrayList Commands
        {
            get
            {
                ArrayList list = new ArrayList();
                IDictionaryEnumerator c = commands.GetEnumerator();
                while (c.MoveNext())
                {
                    list.Add(((DictionaryEntry)c.Current).Key);
                }

                return list;
            }

        }


        public void HandleError(Exception ex)
        {
            if (ex == null) throw new ArgumentNullException("ex");

            Console.WriteLine(ex.Message + ex.StackTrace);
        }

        private string commandsfolder;

        public string CommandsFolder
        {
            get { return commandsfolder; }
            set { commandsfolder = value; }
        }

        private string planetsfolder;

        public string PlanetsFolder
        {
            get { return planetsfolder; }
            set { planetsfolder = value; }
        }


        private int maxloginattempts;

        public int MaxLoginAttempts
        {
            get { return maxloginattempts; }
            set { maxloginattempts = value; }
        }

        private string datasource;

        public string DataSource
        {
            get { return datasource; }
            set { datasource = value; }
        }

        private string databasename;

        public string DatabaseName
        {
            get { return databasename; }
            set { databasename = value; }
        }

        private string security;

        public string Security
        {
            get { return security; }
            set { security = value; }
        }

        private string smtpserver;

        public string SmtpServer
        {
            get { return smtpserver; }
            set { smtpserver = value; }
        }

        private string welcomeemailsubject;

        public string WelcomeEmailSubject
        {
            get { return welcomeemailsubject; }
            set { welcomeemailsubject = value; }
        }

        private string welcomeemailbody;

        public string WelcomeEmailBody
        {
            get { return welcomeemailbody; }
            set { welcomeemailbody = value; }
        }

        private string welcomeemailfrom;

        public string WelcomeEmailFrom
        {
            get { return welcomeemailfrom; }
            set { welcomeemailfrom = value; }
        }

        private string welcomeemailcc;

        public string WelcomeEmailCC
        {
            get { return welcomeemailcc; }
            set { welcomeemailcc = value; }
        }


        private string welcomeemailbcc;

        public string WelcomeEmailBcc
        {
            get { return welcomeemailbcc; }
            set { welcomeemailbcc = value; }
        }


        private string smtpserverusername;

        public string SmtpServerUsername
        {
            get { return smtpserverusername; }
            set { smtpserverusername = value; }
        }

        private string smtpserverpassword;

        public string SmtpServerPassword
        {
            get { return smtpserverpassword; }
            set { smtpserverpassword = value; }
        }

        private bool commandtimer;

        public bool CommandTimer
        {
            get { return commandtimer; }
            set { commandtimer = value; }
        }



        public System.Drawing.Bitmap ConvertByteArraytoBmp(byte[] bArray)
        {
            //create memory stream using byte array
            MemoryStream stream = new MemoryStream(bArray);
            //create new bitmap using memory stream and return
            return new System.Drawing.Bitmap(stream);
        }

        private Clock clock;

        public Clock Clock
        {
            get { return clock; }
            set { clock = value; }
        }



















    }
}
