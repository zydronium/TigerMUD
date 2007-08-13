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
using System.Threading;
using TigerMUD.CommsLib;
using XihSolutions.DotMSN;

namespace TigerMUD.CommsLib.MsnComms
{
	/// <summary>
	/// Listens out for MUD connections over the MSN 
	/// Messenger network.
	/// </summary>
	public class MsnConnectionListener : IConnectionListener
	{
    private Messenger messenger;
    private Thread listenerThread;
    private bool active;
    private bool acceptNewConnections;
    private bool enabled;
    private ArrayList connectedClients;
    private ReaderWriterLock clientsLock;

		public MsnConnectionListener(string account,
      string password)
		{
      messenger = new Messenger();
      // Set up MSN credentials (we are emulating a genuine MSN client)
      messenger.Credentials.ClientID = "msmsgs@msnmsgr.com";
      messenger.Credentials.ClientCode = "Q1P7W2E4J9R8U3S5";	
      messenger.Credentials.Account = "tigermud@gmail.com";
      messenger.Credentials.Password = "mudmud123";

      // For storing the list of connected clients
      clientsLock = new ReaderWriterLock();
      connectedClients = new ArrayList();

      active = false;
      acceptNewConnections = false;
      enabled = false;
		}

		#region IConnectionListener Members
		public bool Active
		{
			get
			{
				return active;
			}
		}

		public int ClientCount
		{
			get
			{
        try
        {
          clientsLock.AcquireReaderLock(Timeout.Infinite);
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

		public void Start()
		{
      acceptNewConnections = true;
      active = true;
      enabled = true;

      listenerThread = new Thread(new ThreadStart(this.StartMessenger));
      listenerThread.Start();
		}

    private void StartMessenger()
    {
      try 
      {
        // Set up messenger events.
        messenger.NameserverProcessor.ConnectionEstablished += new EventHandler(NameserverProcessor_ConnectionEstablished);
        messenger.Nameserver.AuthenticationError += new XihSolutions.DotMSN.Core.HandlerExceptionEventHandler(Nameserver_AuthenticationError);
        messenger.Nameserver.ContactAdded += new ListMutatedAddedEventHandler(Nameserver_ContactAdded);
        messenger.ConversationCreated += new ConversationCreatedEventHandler(messenger_ConversationCreated);
        messenger.Nameserver.ExceptionOccurred += new XihSolutions.DotMSN.Core.HandlerExceptionEventHandler(Nameserver_ExceptionOccurred);
        messenger.Nameserver.SignedIn += new EventHandler(Nameserver_SignedIn);
        messenger.Nameserver.SignedOff += new SignedOffEventHandler(Nameserver_SignedOff);
        messenger.Nameserver.SynchronizationCompleted += new EventHandler(Nameserver_SynchronizationCompleted);

        // Start connecting to the MSN network
        messenger.Connect();

        while(enabled)
        {
          // Just sleep until this ends
          System.Threading.Thread.Sleep(250);
        }

        // Disconnect from the network
        messenger.Disconnect();
      }
      catch (Exception ex)
      {
        Lib.log.Add("MSNConnectionListener.StartMessenger", ex.Message + ex.StackTrace);
      }
    }

		public void Stop()
		{
      enabled = false;
		}

    #endregion

    #region Messenger Event Handlers
    private void NameserverProcessor_ConnectionEstablished(object sender, EventArgs e)
    {

    }

    private void Nameserver_AuthenticationError(object sender, ExceptionEventArgs e)
    {

    }

    private void Nameserver_ContactAdded(object sender, ListMutateEventArgs e)
    {
      // Add this contact to our list.
      messenger.Nameserver.AddContactToList(e.Contact, MSNLists.AllowedList);
    }

    private void messenger_ConversationCreated(object sender, ConversationCreatedEventArgs e)
    {
      // Incoming connection.
      if(e.Initiator == null && enabled)
      {
        Lib.PrintLine("Starting new conversation");
        BeginClientConversation(e.Conversation);
      }
    }

    private void Nameserver_ExceptionOccurred(object sender, ExceptionEventArgs e)
    {
    }

    private void Nameserver_SignedIn(object sender, EventArgs e)
    {
      Lib.log.Add("MsnConnectionListener", "Signed in to MSN service.");

      // Syncrhonise the contact list.
      messenger.Nameserver.SynchronizeContactList();
    }

    private void Nameserver_SignedOff(object sender, SignedOffEventArgs e)
    {
      Lib.log.Add("MsnConnectionListener", "Signed out of MSN service.");
    }

    private void Nameserver_SynchronizationCompleted(object sender, EventArgs e)
    {
      messenger.Nameserver.SetPresenceStatus(PresenceStatus.Online);
    }
    #endregion

    #region Client Conversations
    private void BeginClientConversation(Conversation conversation)
    {
      // Create a new MsnClientSocket
      MsnClientSocket socket = null;

      try 
      {
        // Create a new MsnClientSocket
        socket = new MsnClientSocket(conversation,
          MsnClientSocket.DEFAULT_TIMEOUT);

        // Farm this out to a new listener
        ClientListener listener = new ClientListener(socket,
          new ClientDisconnectedEventHandler(ClientDisconnected));

        clientsLock.AcquireWriterLock(Timeout.Infinite);
        try 
        {
          connectedClients.Add(listener);
        }
        finally
        {
          clientsLock.ReleaseLock();
        }

        // Start the listener.
        listener.StartListening();
      } 
      catch (Exception ex)
      {
        Lib.log.Add(socket, 
          "unknown",
          "Error while trying to start a client listener for this socket: " + ex.Message + ex.StackTrace);
      }
    }

    private void ClientDisconnected(ClientListener sender)
    {
      
    }
    #endregion

  }
}
