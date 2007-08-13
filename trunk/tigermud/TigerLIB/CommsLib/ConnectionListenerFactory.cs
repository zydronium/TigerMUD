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
using TigerMUD.CommsLib.TcpComms;
using TigerMUD.CommsLib.MsnComms;

namespace TigerMUD.CommsLib
{
	/// <summary>
	/// Creates connection listeners on request.
	/// </summary>
	public class ConnectionListenerFactory
	{
    /// <summary>
    /// Creates a new TcpConnectionListener on the specified port.
    /// </summary>
    /// <param name="port">The port to listen for new connections on.</param>
    /// <returns>An implementation of IConnectionListener.</returns>
    public static IConnectionListener CreateTcpConnectionListener(int port)
    {
      TcpConnectionListener listener = new TcpConnectionListener(port);
      return listener;
    }

    /// <summary>
    /// Creates a new MsnConnectionListener.
    /// </summary>
    /// <param name="account">The MSN account email address.</param>
    /// <param name="password">The MSN account password.</param>
    /// <returns>An implementation of IConnectionListener.</returns>
    public static IConnectionListener CreateMsnConnectionListener(string account,
      string password)
    {
      MsnConnectionListener listener = new MsnConnectionListener(account,
        password);
      return listener;
    }
	}
}
