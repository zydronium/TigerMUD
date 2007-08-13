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
using System.Threading;

namespace TigerMUD.CommsLib
{
	/// <summary>
	/// Listens out for client input and processes commands.
	/// </summary>
	public class ClientListener
	{
		private IUserSocket userSocket;
		private ClientDisconnectedEventHandler clientDisconnectedEventHandler;
		private Thread listenerThread;
		private bool active;
		private bool enabled;

		public ClientListener(IUserSocket userSocket,
			ClientDisconnectedEventHandler clientDisconnectedEventHandler)
		{
			this.userSocket = userSocket;
			this.clientDisconnectedEventHandler = clientDisconnectedEventHandler;
			this.listenerThread = new Thread(new ThreadStart(Listen));
			active = false;
			enabled = false;
		}

		#region Public Properties
		public IUserSocket UserSocket
		{
			get
			{
				return userSocket;
			}
		}

		public Thread ListenerThread
		{
			get
			{
				return listenerThread;
			}
		}

		public bool Active
		{
			get
			{
				return active;
			}
		}
		#endregion

		public void StartListening()
		{
			enabled = true;
			listenerThread.Start();
		}

		public void Listen()
		{
			string username;
			string recv;
			bool fireCallback = true;
			active = true;
			try 
			{
				Interlocked.Increment(ref Lib.connections);
				string clientip = userSocket.ClientEndpointId;
                Lib.log.Add(userSocket, null, "CONNECT (" + userSocket.ClientEndpointId + ") Online:" + Lib.connections);
				Commandprocessor cp = new Commandprocessor();

				// Send the welcome screen to the user.
				userSocket.Send(Lib.Welcomescreen);
				userSocket.Send(Lib.Ansifboldyellow + "Developers: " + Lib.Ansifboldwhite + Lib.Creditsdev + "\r\n");
				userSocket.Send(Lib.Ansifboldyellow + "Special Thanks: " + Lib.Ansifboldwhite + Lib.Creditsother + "\r\n\r\n" + Lib.Ansifwhite);

				// Authenticate this user.
				username = Lib.Authenticate(userSocket, cp); 
				if( username == null) 
				{
					// detected some socket error
					return; 
				}

				if( username == null )
				{
					try
					{
						// If the user is already disconnected, this might fail so catch the exception
						userSocket.SendLine("Too many bad login attempts. Byte!");
						Lib.Disco(userSocket, "too many bad logon attempts");
					}
					catch
					{
						return;
					}
					return;
				}

				// Log this user as connected
				Actor theUser = Lib.GetByName(username);
                //theUser["connected"] = true;
				// Set the user's client socket
				theUser.UserSocket = this.userSocket;

                //theUser.Save();

				while (enabled)
				{
					try
					{
						// if user disco, then socket is gone and we need to catch this exception
						recv = userSocket.GetResponse();
					}
					catch 
					{
						//Lib.PrintLine(DateTime.Now + " EXCEPTION in ClientListener (Waiting for client response): " + ex.Message + ex.StackTrace);
						// We should rather log these exceptions to the console
						// or to a log file
						return;
					}
					if( recv == null ) 
					{ 
						return;
					}
					cp.processcommand(userSocket, username, recv, false);
				}
				Lib.Disco(userSocket, "client listener switched off with the stoplistening method");
			}
			catch (ThreadAbortException ex)
			{
				// Signal that we should just quit and not fire the 
				// callback
				fireCallback = false;
				//Lib.PrintLine(DateTime.Now + " EXCEPTION in ClientListener (Thread aborted): " + ex.Message + ex.StackTrace);
			}
			finally
			{
				if(fireCallback)
				{
					// Fire the callback event
					IAsyncResult ar = clientDisconnectedEventHandler.BeginInvoke(this,
						null, 
						null);
				}

				// We are no longer active
				active = false;
			}
		}

		public void StopListening()
		{
			enabled = false;
		}
	}
}
