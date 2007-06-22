using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.IO;
using System.Drawing;

namespace TigerMUD
{
    public class Server : IDisposable
    {

        List<GameLibrary.PlayerCharacter> connectedclients = new List<GameLibrary.PlayerCharacter>();
        GameLibrary.ThreadInitializationData threadstartup;
        EndPoint endpoint;
        GameLibrary.ClientHandler clienthandler;
        private object lockobj;
        GameLibrary.GameContext gamecontext;

        public Server()
        {
            lockobj = new object();
            Running = false;
            threadstartup = new GameLibrary.ThreadInitializationData();

            gamecontext = GameLibrary.GameContext.GetInstance();

            threadstartup.GameContext = GameLibrary.GameContext.GetInstance();
            threadstartup.GameContext.Database = new GameLibrary.Database();
            threadstartup.GameContext.Compiler = new GameLibrary.GameCompiler();
        }


        public bool QueueWorkItem(GameLibrary.PlayerCharacter pcin)
        {
            // Wrap stateinfo and feed it to the thread pool
            GameLibrary.StateInfo stateinfo = new GameLibrary.StateInfo();
            stateinfo.Pc = pcin;
            stateinfo.GameContext = gamecontext;

            // Process any pending commands for the client
            if (pcin.IsCommandQueued)
            {

                GameLibrary.PlayerCharacter.PlayerDelegate nextdelegate = pcin.GetNextCommand();
                return ThreadPool.QueueUserWorkItem(new WaitCallback(nextdelegate), stateinfo);
            }
            else return false;


        }


        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free other state (managed objects).
                //mu.Close();
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.
        }

        // Use C# destructor syntax for finalization code.
        ~Server()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }


        static void Main()
        {
        }


        public void Start()
        {
            lock (lockobj)
            {
                Pending = true;
                // Already started
                if (Running)
                {
                    return;
                }


                threadstartup.GameContext.ReadConfiguration();
                threadstartup.TcpListener = new TcpListener(System.Net.IPAddress.Any, threadstartup.GameContext.ServerPort);

                // Echo server configuration to the server console
                Console.WriteLine(threadstartup.GameContext.ConfigurationString);

                CompileCommands();

                LoadPlanets();
                LoadRooms();
                

                StartGameClock();

                // Run various scheduled commands
                StartScheduledCommands();

                Running = true;
                Pending = false;

                StartClientListener(threadstartup);


            }


        }

        private void CompileCommands()
        {
            // TODO Compile commands and load assemblies
            string errors = threadstartup.GameContext.Compiler.InitCommands(threadstartup.GameContext, null);
            Console.WriteLine("Compile ERRORs: " + errors);
        }

        private void StartGameClock()
        {
            GameLibrary.Clock clock = new GameLibrary.Clock();
            threadstartup.GameContext.Clock = clock;
            Thread clockthread = new Thread(new ThreadStart(threadstartup.GameContext.Clock.AdvanceGameClock));
            clockthread.Start();
            Console.WriteLine("Game clock started. Time is {0}", clock.GameTime);
        }

        private void LoadPlanets()
        {
            Console.WriteLine("Loading Planets...");
            GameLibrary.Planet planet = new GameLibrary.Planet();
            ArrayList planetpaths = threadstartup.GameContext.GetFilesRecursive(threadstartup.GameContext.PlanetsFolder, "*.planet");

            foreach (string planetpath in planetpaths)
            {
                planet = GameLibrary.Planet.LoadPlanetFromFile(planetpath);

                threadstartup.GameContext.AddPlanet(planet);
                Console.WriteLine("Done.");

            }
            Console.WriteLine("Loaded " + planetpaths.Count + " planets.");
        }

        public void LoadRooms()
        {
            Console.WriteLine("Loading Rooms...");
            ArrayList roomids = new ArrayList();
            GameLibrary.Room room = new GameLibrary.Room();
            roomids = threadstartup.GameContext.Database.GetRooms();
            foreach (string roomid in roomids)
            {
                room = threadstartup.GameContext.Database.LoadRoom(roomid);
                threadstartup.GameContext.AddRoom(room);
            }
            Console.WriteLine("Loaded " + roomids.Count + " rooms.");
        }


        public void StartScheduledCommands()
        {
            GameLibrary.Scheduler scheduler = new GameLibrary.Scheduler();


            GameLibrary.Command heartbeat = threadstartup.GameContext.GetCommand("tick");
            heartbeat.Count = 1;
            heartbeat.End = DateTime.Now.AddSeconds(6);
            scheduler.AddToSchedule(heartbeat);


            Thread schedulerthread = new Thread(new ParameterizedThreadStart(scheduler.RunScheduledCommands));
            schedulerthread.Start(threadstartup);
            threadstartup.TcpListener.Start();
            Console.WriteLine("Listener started.");

        }




        /// <summary>
        /// Thread that handles all input from clients and sends any commands to the thread pool.
        /// </summary>
        /// <param name="threadstartupin"></param>
        public void StartClientListener(GameLibrary.ThreadInitializationData threadstartupin)
        {
            bool status = false;
            // Start the listener thread that handles all input from connected clients
            // and sends commands to the thread queue for processing



            // Main server loop (communications and new connections)
            while (Running)
            {
                // Handle any new inbound clients.
                if (threadstartupin.TcpListener.Pending())
                {
                    threadstartupin = AddNewConnection(threadstartupin);

                }
                Thread.Sleep(25);

                for (int i = gamecontext.Players.Count - 1; i >= 0; i--)
                {
                    GameLibrary.PlayerCharacter pc = gamecontext.Players[i];

                    // Process keystrokes
                    GameLibrary.CommunicationResult result = pc.HandleKeystrokes();
                    
                    // Skip clients that have no message for the server
                    if (result == GameLibrary.CommunicationResult.NoMessageReceived)
                    {
                        if (pc.ConnectionState == GameLibrary.ConnectionState.WaitingforClientResponse || pc.ConnectionState == GameLibrary.ConnectionState.WaitingAtMenuPrompt)
                        {
                            continue;
                        }
                    }

                    // Disconnect on connection errors
                    if (result == GameLibrary.CommunicationResult.Error)
                    {
                        clienthandler.Disconnect(pc);
                        continue;
                    }
                   

                    // Make newly connected clients login
                    if (pc.ConnectionState == GameLibrary.ConnectionState.NewConnection)
                    {
                        LoginClient(pc);

                    }

                    // Handle responses to menu prompts
                    if (result == GameLibrary.CommunicationResult.MessageReceived && pc.ConnectionState == GameLibrary.ConnectionState.WaitingAtMenuPrompt )
                    {
                            pc.Menu.HandleResponse(pc.Message);
                            continue;
                    }

                    // Ensure command prompt is displayed when in doubt
                    if (pc.ConnectionState == GameLibrary.ConnectionState.NotWaitingForClientResponse && !pc.IsCommandQueued)
                    {
                        pc.SetNextCommand(new GameLibrary.PlayerCharacter.PlayerDelegate(clienthandler.CommandPrompt));
                    }

                    // Run whatever command is queued for this client
                    status = QueueWorkItem(pc);

                    // Handle disconnecting clients during command execution
                    if (pc.ConnectionState == GameLibrary.ConnectionState.Disconnected)
                    {
                        clienthandler.Disconnect(pc);
                        continue;
                    }

                }
            }
            //Stop();

        }

        private void LoginClient(GameLibrary.PlayerCharacter pc)
        {
            // set stateinfo required for all playerdelegates
            GameLibrary.StateInfo stateinfo = new GameLibrary.StateInfo();
            stateinfo.Pc = pc;
            stateinfo.GameContext = gamecontext;

            // get welcome instance and queue it
            GameLibrary.Command command = gamecontext.GetCommand("welcome");
            pc.SetNextCommand(new GameLibrary.PlayerCharacter.PlayerDelegate(command.DoCommand));

            // Set next command to be the usernameprompt after welcome
            pc.SetNextCommand(new GameLibrary.PlayerCharacter.PlayerDelegate(clienthandler.UsernamePrompt));
            pc.ConnectionState = GameLibrary.ConnectionState.WaitingforClientResponse;
        }

        private GameLibrary.ThreadInitializationData AddNewConnection(GameLibrary.ThreadInitializationData threadstartupin)
        {
            threadstartupin.PlayerCharacter = new GameLibrary.PlayerCharacter();
            threadstartupin.GameContext = gamecontext;
            threadstartupin.PlayerCharacter.Connected = true;
            threadstartupin.TcpClient = threadstartupin.TcpListener.AcceptTcpClient();
            threadstartupin.PlayerCharacter.NetworkStream = threadstartupin.TcpClient.GetStream();
            threadstartupin.PlayerCharacter.Socket = threadstartupin.TcpClient.Client;

            endpoint = threadstartupin.PlayerCharacter.Socket.RemoteEndPoint;
            threadstartupin.PlayerCharacter.IPAddress = ((IPEndPoint)endpoint).Address.ToString();

            clienthandler = new GameLibrary.ClientHandler();

            threadstartupin.PlayerCharacter.ConnectionState = GameLibrary.ConnectionState.NewConnection;
            gamecontext.AddPlayerCharacter(threadstartupin.PlayerCharacter);
            return threadstartupin;
        }

        public void Stop()
        {

            lock (lockobj)
            {
                //mu.WaitOne();
                Pending = true;
                // Already stopped
                if (!Running)
                {
                    return;
                }

                Console.WriteLine("Stopping server...");
                threadstartup.TcpListener.Stop();
                //threadstartup.TcpClient.Close();

                Running = false;
                Pending = false;

                // Clean up steps
                threadstartup.GameContext.ClearPlanets();
                Console.WriteLine("Planets cleared.");

                Console.WriteLine("SERVER STOPPED.");

            }


        }

        private bool running;

        public bool Running
        {
            get { return running; }
            set { running = value; }
        }

        private bool pending;

        public bool Pending
        {
            get { return pending; }
            set { pending = value; }
        }

        private bool stopsignal;

        public bool StopSignal
        {
            get { return stopsignal; }
            set { stopsignal = value; }
        }



    }
}
