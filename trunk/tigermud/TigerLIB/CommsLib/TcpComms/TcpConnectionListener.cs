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
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections;
using TigerMUD.CommsLib;

namespace TigerMUD.CommsLib.TcpComms
{
    /// <summary>
    /// Listens out for new connections from users.
    /// </summary>
    public class TcpConnectionListener : IConnectionListener
    {
        private TcpListener tcpListener;
        private bool enabled;
        private ArrayList connectedClients;
        private ReaderWriterLock clientsLock;
        private Thread listenerThread;
        private bool acceptNewConnections;

        public TcpConnectionListener(int port)
        {
            tcpListener = new TcpListener(IPAddress.Any, port);
            listenerThread = new Thread(new ThreadStart(ListenForConnections));
            enabled = false;
            connectedClients = new ArrayList();
            clientsLock = new ReaderWriterLock();
            acceptNewConnections = true;
        }

        #region Public Properties
        /// <summary>
        /// Gets the status of the client listener.
        /// </summary>
        public bool Active
        {
            get
            {
                return enabled;
            }
        }

        /// <summary>
        /// Returns the number of connected clients.
        /// </summary>
        public int ClientCount
        {
            get
            {
                clientsLock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    return connectedClients.Count;
                }
                finally
                {
                    clientsLock.ReleaseLock();
                }
            }
        }

        public bool AcceptNewConnections
        {
            get
            {
                return acceptNewConnections;
            }
            set
            {
                acceptNewConnections = value;
            }
        }
        #endregion

        public void Start()
        {
            enabled = true;
            listenerThread.Start();
        }

        private void ListenForConnections()
        {
            try
            {
                // Start the listener listening for requests.
                tcpListener.Start();
                while (enabled)
                {
                    while (!tcpListener.Pending() && enabled)
                    {
                        Thread.Sleep(500);
                    }

                    // Make sure that while we were sleeping
                    // the service wasn't enabled.
                    // Also make sure that we are go for accepting
                    // new connections.
                    if (enabled)
                    {
                        // Get the socket.
                        TcpClientSocket socket = new TcpClientSocket(tcpListener.AcceptSocket());

                        if (acceptNewConnections)
                        {
                            // Create a new Client Listener and pass through the callback
                            ClientListener tcp = new ClientListener(socket,
                              new ClientDisconnectedEventHandler(this.ClientDisconnectedCallback));

                            // Add the client listener to the collection
                            clientsLock.AcquireWriterLock(Timeout.Infinite);
                            try
                            {
                                connectedClients.Add(tcp);
                            }
                            finally
                            {
                                clientsLock.ReleaseLock();
                            }

                            // Start the client listener.
                            tcp.StartListening();
                        }
                        else
                        {
                            // We simply close connections if we 
                            // are not accepting connections.
                            socket.Close();
                        }

                        // Sleep the current thread for a bit.
                        Thread.Sleep(1);
                    }
                }
            }
            catch (Exception ex)
            {
                this.enabled = false;
                Lib.log.Add("TcpConnectionListener.ListenForConnections exception", ex.Message + ex.StackTrace);
            }
            finally
            {
                tcpListener.Stop();
            }
        }

        /// <summary>
        /// Stop the connection listener.
        /// </summary>
        public void Stop()
        {
            enabled = false;
            // Stop all the client listener threads.
            StopClientListeners();
        }

        /// <summary>
        /// Stops all the client listener threads.
        /// </summary>
        private void StopClientListeners()
        {
            // 1. First we try requesting them to stop on their own.
            clientsLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                foreach (ClientListener clientListener in connectedClients)
                {
                    clientListener.StopListening();
                }
            }
            finally
            {
                clientsLock.ReleaseLock();
            }

            // 2. Next we sleep for a second to give the listeners
            //    a chance to disconnect and remove themselves from
            //    the connectedClients collection.
            System.Threading.Thread.Sleep(1000);

            // 3. If there are still connected users, we Lib.Disco
            //    the users and abort their threads.
            clientsLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                foreach (ClientListener cl in connectedClients)
                {
                    // Try to save his data
                    Lib.Disco(cl.UserSocket, "server shutdown");
                    // Abort his user thread.
                    cl.ListenerThread.Abort();
                }
            }
            finally
            {
                clientsLock.ReleaseLock();
            }

            // Go through and see if there are any connected clients
            clientsLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                foreach (ClientListener cl in connectedClients)
                {
                    if (cl.Active)
                    {
                        // Abort the thread again
                        cl.ListenerThread.Abort();
                    }
                }
            }
            finally
            {
                clientsLock.ReleaseLock();
            }

            // There are no more connected clients.
        }

        /// <summary>
        /// Called when the ClientListener has quit.
        /// </summary>
        /// <param name="sender">The client listener that quit.</param>
        private void ClientDisconnectedCallback(ClientListener sender)
        {
            clientsLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                // Lib.Disco this connection just to be safe
                //Lib.Disco(sender.UserSocket);
                // Remove this client listener from the collection
                connectedClients.Remove(sender);
            }
            finally
            {
                clientsLock.ReleaseLock();
            }
        }
    }
}
