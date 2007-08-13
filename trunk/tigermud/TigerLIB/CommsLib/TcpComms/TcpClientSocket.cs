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
using System.Text;
using System.Runtime.Serialization;

namespace TigerMUD.CommsLib.TcpComms
{
	/// <summary>
	/// Summary description for ClientSocket.
	/// </summary>
	//[Serializable()]        
	public class TcpClientSocket : IUserSocket 
	{
		private Socket socket; 
		private string uniqueId;
		private string clientEndPointId;

		public TcpClientSocket(Socket socket)
		
		{
			this.socket = socket;
			this.uniqueId = System.Guid.NewGuid().ToString();
			this.clientEndPointId = IPAddress.Parse(((IPEndPoint)socket.RemoteEndPoint).Address.ToString()).ToString();
		}

		public string GetNewLineCharacter()
		{
			return "\r\n";
		}

		public void Send(string text)
		{
			byte[] senddata = Encoding.ASCII.GetBytes(text);
			if(socket.Handle.ToString() != "-1" ) 
			{
				socket.Send(senddata);
			}
		}

		public void SendLine(string text)
		{
			this.Send(text + "\r\n");
		}

		public string GetResponse()
		{
			string receivedstring = "";
			string receivedchar = "";
			int recv = 0;
			byte[] receivedata=new byte[1];
			ASCIIEncoding ascii = new ASCIIEncoding();
			do 
			{
				// if this socket isnt Connected anymore, then just return -1
				if (socket.Handle.ToString() == "-1") 
				{
					return null;
				}
				try 
				{
					// reset the one byte buffer for the next character
					receivedata[0]=0;
					// get next character, a zero means there was a disconnect from the client
					recv = socket.Receive(receivedata);
				}
				catch  
				{
					return null;
				}
				receivedchar = ascii.GetString(receivedata, 0, receivedata.Length);
				// Handle backspace or empty characters
				if ((receivedchar == "\x08" && receivedstring.Length > 0)) 
				{ 
					receivedstring = receivedstring.Substring(0, receivedstring.Length - 1);
				}
				// Keep building the command string if we havent hit a CR or LF yet
				if ((receivedchar != "\r" && receivedchar != "\n" && receivedchar != "\x08"))
				{
					receivedstring = receivedstring + receivedchar;
				}
				// Client disconnect is signalled by a zero
				if (receivedchar.Substring(0,1).ToCharArray()[0]==0)
				{
					Lib.Disco(this, "client disconnected");
					//break;
					return null;
				}
				// Bail and return the command if we hit a newline character
			} while (receivedchar != "\n");
			
			// Remove all leading and trailing spaces from the string
			receivedstring=receivedstring.Trim();
			
			// reset ansi colors when carriage return is detected coming from the user
			Send(Lib.Ansireset);

			// Filter the player text
			ProfanityFilter.FilterMessage(ref receivedstring,
				this);

			return receivedstring;
		}

		public void Close()
		{
			this.socket.Close();
		}

		#region Public Properties
		public bool Connected
		{
			get
			{
				return ((socket.Connected) &&
					(socket.Handle.ToInt32() != -1));
			}
		}

		public string ClientEndpointId
		{
			get
			{
				return clientEndPointId;
			}
		}

		public string UniqueId
		{
			get
			{
				return uniqueId;
			}
		}
		#endregion
	}
}
