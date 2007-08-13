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
using System.Net.Sockets;
using System.Text;

namespace TigerMUD.CommsLib.TcpComms
{
	/// <summary>
	/// A utility for socket communication
	/// </summary>
	public class TcpSocketUtility
	{
    /// <summary>
    /// Function that sends a message to a given socket.
    /// This barebones send function is only used when we don't have the user object instance, which
    /// occurs at login before user authentication.
    /// </summary>
    /// <returns>A boolean to indicate success(true) or error(false).</returns>
    public static bool SendSimple(Socket ns, string message)
    {
      try 
      {
        byte[] senddata = Encoding.ASCII.GetBytes(message);
        if(ns.Handle.ToString() != "-1" ) 
        {
          ns.Send(senddata);
          return true;
        }
      } 
      catch (Exception ex ) 
      {
        Lib.log.Add("send function", "EXCEPTION " + ex.Message + ex.StackTrace);
        return false;
      }
      return false;
    }
	}
}
