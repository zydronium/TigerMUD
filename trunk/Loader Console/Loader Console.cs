using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace RemoteConsole
{
    class Loader : IDisposable
    {
        TigerMUD.Server program;
        GameLibrary.ThreadInitializationData threadstartup;
        //PlayerCharacter pc;
        GameLibrary.GameContext gamecontext;
        bool disconnect;
        GameLibrary.ClientHandler clienthandler;
        Thread serverthread;

        public Loader()
        {
            disconnect = false;
            program = new TigerMUD.Server();
            gamecontext = GameLibrary.GameContext.GetInstance();

            gamecontext.Database = new GameLibrary.Database();
            // failed to connect?
            if (gamecontext.Database == null) return;
                        
            gamecontext.Compiler = new GameLibrary.GameCompiler();
            threadstartup = new GameLibrary.ThreadInitializationData();
            gamecontext.ReadConfiguration();

            clienthandler = new GameLibrary.ClientHandler();

            // Echo config to the loader screen
            Console.WriteLine(gamecontext.ConfigurationString);
            threadstartup.TcpClient = new TcpClient();
            threadstartup.TcpListener = new TcpListener(System.Net.IPAddress.Any, gamecontext.LoaderPort);
            threadstartup.TcpListener.Start();
            Console.WriteLine("Loader running on port " + gamecontext.LoaderPort);
            HandleClientConnections(threadstartup, gamecontext);

            

        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "RemoteConsole.Loader")]
        static void Main()
        {
            Loader loader = new Loader();
            
            loader.Dispose();

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
                gamecontext.Dispose();
                program.Dispose();

            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.
        }

        // Use C# destructor syntax for finalization code.
        ~Loader()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }


        public void HandleClientConnections(GameLibrary.ThreadInitializationData threadstartupin, GameLibrary.GameContext gamecontext)
        {
            if (gamecontext.AutoStartServer)
            {
                StartServer(new GameLibrary.PlayerCharacter());
            }

            for (; ; )
            {
                // New connection inbound
                if (threadstartupin.TcpListener.Pending())
                {
                    threadstartupin = InitializeThreadData(threadstartupin, gamecontext);
                    Thread thread = new Thread(new ParameterizedThreadStart(StartClientCommunication));
                    thread.Start(threadstartupin);
                }
                Thread.Sleep(50);
            }
        }

        public GameLibrary.ThreadInitializationData InitializeThreadData(GameLibrary.ThreadInitializationData threadstartupin, GameLibrary.GameContext gamecontext)
        {
            threadstartupin.PlayerCharacter = new GameLibrary.PlayerCharacter();
            threadstartupin.GameContext = gamecontext;

            threadstartupin.PlayerCharacter.Connected = true;

            threadstartupin.TcpClient = threadstartupin.TcpListener.AcceptTcpClient();
            threadstartupin.PlayerCharacter.NetworkStream = threadstartupin.TcpClient.GetStream();
            threadstartupin.PlayerCharacter.Socket = threadstartupin.TcpClient.Client;
            EndPoint endpoint = threadstartupin.PlayerCharacter.Socket.RemoteEndPoint;
            threadstartupin.PlayerCharacter.IPAddress = ((IPEndPoint)endpoint).Address.ToString();
            clienthandler = new GameLibrary.ClientHandler();
            return threadstartupin;
        }


        /// <summary>
        /// Handles loader client communication
        /// </summary>
        /// <param name="threadstartupin"></param>
        public void StartClientCommunication(Object threadstartupin)
        {
            threadstartup = (GameLibrary.ThreadInitializationData)threadstartupin;
            GameLibrary.PlayerCharacter pc = threadstartup.PlayerCharacter;
            gamecontext = threadstartup.GameContext;
            
            
            gamecontext.AddLoaderConsoleConnection(pc);

            // Do Authentication prompts
            GameLibrary.StateInfo stateinfo = new GameLibrary.StateInfo();
            stateinfo.Pc = pc;
            stateinfo.GameContext = gamecontext;
                        
            clienthandler.Authenticate(stateinfo);
            if (pc.ConnectionState==GameLibrary.ConnectionState.Disconnected) return;

           

            // Main command processing loop
            CommandLoop(pc);
            // Client disconnected, end thread
        }

        public void CommandLoop(GameLibrary.PlayerCharacter pc)
        {
            ArrayList choice;
            GameLibrary.Menu menu = new GameLibrary.Menu(pc);
            menu.Title = "Server Control Panel";
            menu.Description = "Select a server control option.";
            menu.Prompt = "Choice: ";

            menu.Add(new GameLibrary.MenuItem("Start Server\r\n", "1"));
            menu.Add(new GameLibrary.MenuItem("Stop Server\r\n", "2"));
            menu.Add(new GameLibrary.MenuItem("Restart Server\r\n", "3"));

            for (; ; )
            {
                choice = menu.Show();
                if (choice == null ? true : choice.Count < 1)
                {
                    clienthandler.Disconnect(pc);
                    menu.Dispose();
                    return;
                }
                switch (choice[0].ToString())
                {
                    case "1":
                        StartServer(pc);
                        break;
                    case "2":
                        StopServer(pc);
                        break;
                    case "3":
                        StopServer(pc);
                        StartServer(pc);
                        break;
                }
            }

        }


        public void StartServer(GameLibrary.PlayerCharacter pc)
        {
            if (program.Pending)
            {
                pc.SendLine("ERROR: Server operation pending.");
                return;
            }
            else if (program.Running)
            {
                pc.SendLine("ERROR: Server already running.");
                return;
            }
            else
                pc.SendLine("Starting server...");
            serverthread = new Thread(new ThreadStart(program.Start));
            serverthread.Start();
        }


        public void StopServer(GameLibrary.PlayerCharacter pc)
        {
            if (program.Pending)
            {
                pc.SendLine("ERROR: Server operation pending.");
                return;
            }
            else if (!program.Running)
            {
                pc.SendLine("ERROR: Server already stopped.");
                return;
            }
            else
                pc.SendLine("Stopping server...");
            //serverthread = new Thread(new ThreadStart(program.Stop));
            //serverthread.Start();
            program.Stop();

        }

    }
}
