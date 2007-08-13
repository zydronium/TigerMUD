using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using TigerMUD;
using System.ComponentModel;


namespace RemoteConsole
{

    public class Server
    {
        private static ArrayList connections = new ArrayList();
        private static bool exit = false;
        private static int listenerport = 8001;
        private static Socket connection = null;
        private static TigerMUD.Server server = null;
        private static Thread serverThread;

        public static void Main()
        {
            Thread listener = new Thread(ListenerThread);
            listener.Start();
            return;
        }

        public static void ListenerThread()
        {
            TcpListener listener = new TcpListener(System.Net.IPAddress.Any, listenerport);
            Console.WriteLine("Remote console running on port " + listenerport);

            for (; ; )
            {
                listener.Start();
                exit = false;
                connection = listener.AcceptSocket();
                try
                {
                    if (connection.Handle.ToString() != "-1")
                    {
                        connection.Send(Encoding.ASCII.GetBytes("Remote Server Console" + "\r\n\r\n"));
                        while (!exit)
                        {
                            ShowMenu();

                            string receivedchar = "";
                            string commands = "";
                            byte[] commandsData = new byte[1];

                            do
                            {
                                // if this socket isnt Connected anymore, then just return
                                if (connection.Handle.ToString() == "-1")
                                {
                                    return;
                                }
                                try
                                {
                                    // reset the one byte buffer for the next character
                                    commandsData[0] = 0;
                                    // get next character, a zero means there was a disconnect from the client
                                    connection.Receive(commandsData);
                                }
                                catch
                                {
                                    return;
                                }
                                receivedchar = Encoding.Default.GetString(commandsData, 0, commandsData.Length);
                                // Handle backspace or empty characters
                                if ((receivedchar == "\x08" && commands.Length > 0))
                                {
                                    commands = commands.Substring(0, commands.Length - 1);
                                }

                                // Keep building the command string if we havent hit a CR or LF yet
                                if ((receivedchar != "\r" && receivedchar != "\n" && receivedchar != "\x08"))
                                {
                                    commands = commands + receivedchar;
                                }
                                // Client disconnect is signalled by a zero
                                if (receivedchar.Substring(0, 1).ToCharArray()[0] == 0)
                                {
                                    connection.Close();
                                }
                                // Bail and return the command if we hit a newline character
                            } while (receivedchar != "\n");
                            ProcessCommand(commands);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + ex.StackTrace);
                    Send(ex.Message + ex.StackTrace + "\r\n");
                    return;
                }
                connection.Close();
            }
        }

        public static void ShowMenu()
        {
            string menu = "1. Start TigerMUD\r\n2. Stop TigerMUD\r\n3. Restart TigerMUD\r\n4. Exit Console\r\nChoice: ";
            Send(menu);
            return;
        }

        public static void ProcessCommand(string command)
        {
            int choice = 0;
            if (command.Length > 10 || command.Length < 1)
            {
                Send("***** Error\r\n\r\n");
                ShowMenu();
                return;
            }

            try
            {
                choice = Convert.ToInt32(command);
            }
            catch
            {
                Send("***** Error\r\n\r\n");
                ShowMenu();
                return;
            }

            switch (choice)
            {
                case 1:
                    StartMUD();
                    break;
                case 2:
                    StopMUD();
                    break;
                case 3:
                    StopMUD();
                    StartMUD();
                    break;
                case 4:
                    exit = true;
                    return;
                default:
                    Send("***** Error\r\n\r\n");
                    break;
            }
        }

        public static void Send(string message)
        {
            connection.Send(Encoding.ASCII.GetBytes(message));
            return;
        }

        public static void StartMUD()
        {
#if DEBUG
            Lib.PathtoDebugorRelease = @"\debug";
#else
            Lib.PathtoDebugorRelease = @"\release";
#endif

            if (server != null)
            {
                if (server.Started)
                {
                    Send("\r\nServer already running!\r\n\r\n");
                    return;
                }
                else
                {
                    serverThread.Abort();
                }
            }

            //appDomainSetup = new AppDomainSetup();
            //appDomainSetup.ApplicationBase = Lib.PathtoStartup;
            //appDomainSetup.ApplicationName = @"Main";
            //newappdomain = AppDomain.CreateDomain(@"Main", null, appDomainSetup);
            //server = (TigerMUD.Server)newappdomain.CreateInstanceFromAndUnwrap(Path.GetFullPath(Path.Combine(Lib.PathtoStartup, "TigerMUD.exe")), "TigerMUD.Server");
            server = new TigerMUD.Server();
            ThreadStart ts = new ThreadStart(server.Start);
            serverThread = new Thread(ts);
            Send("\r\nServer starting...\r\n");
            serverThread.Start();
            //server.Start();
            while (!server.Started)
            {
                Thread.Sleep(100);
            }
            //server.Start();
            Send("Server started.\r\n\r\n");
        }

        public static void StopMUD()
        {
            if (server == null || server.Started == false)
            {
                Send("\r\nServer not running!\r\n\r\n");
                return;
            }

            Send("\r\nServer stopping...\r\n");
            server.Stop();
            while (server.Started)
            {
                Thread.Sleep(100);
            }
            serverThread.Abort();
            Send("Server stopped.\r\n\r\n");

        }

    }
}