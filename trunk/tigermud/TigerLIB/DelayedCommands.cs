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

namespace TigerMUD
{
	public struct DelayedCommand
	{
		public Actor actor;
		public string command;
		public string arguments;
		public long time;
		public long delay; // in tenths of a second, so 10=1 second.
		public bool loop; // Does this command loop forever with 'delay' between each run?
	}
	
    //public class DelayedCommandComparer : IComparer
    //{
    //    int IComparer.Compare (Object x, Object y)
    //    {
    //        return (new CaseInsensitiveComparer()).Compare(((DelayedCommand)x).time+((DelayedCommand)x).delay, ((DelayedCommand)y).time+((DelayedCommand)y).delay);
    //    }
    //}

    public class DelayedCommandComparer : IComparer
    {
        CaseInsensitiveComparer cic = new CaseInsensitiveComparer();
        DelayedCommand newx;
        DelayedCommand newy;
        int result = 0;

        int IComparer.Compare(Object x, Object y)
        {
            newx = (DelayedCommand)x;
            newy = (DelayedCommand)y;
            result = cic.Compare(newx.time+newx.delay, newy.time+newy.delay);
            return result;
        }
    }
	
	/// <summary>
	/// Description of DelayedCommands.
	/// </summary>
	public class DelayedCommands
	{
		protected ArrayList commands = new ArrayList();
		
		public DelayedCommands()
		{
		}

        public virtual void Add(Actor actor, string commandname, string arguments, long time, long delay, bool loop)
		{
			lock (commands.SyncRoot)
			{
				DelayedCommand delayedcommand;
				delayedcommand.actor = actor;
				delayedcommand.command = commandname;
				delayedcommand.arguments = arguments;
				delayedcommand.time = time;
				delayedcommand.delay=delay;
				delayedcommand.loop=loop;
				// Some basic protection against deadlocks
				if (delayedcommand.delay<1)
				{
					delayedcommand.delay=1;
				}
				Add(delayedcommand);

				
			}
		}
		
		public virtual void Add(DelayedCommand delayedcommand)
		{
			// Some basic protection against deadlocks
			if (delayedcommand.delay<1)
			{
				delayedcommand.delay=1;
			}

			lock (commands.SyncRoot)
			{
				commands.Add(delayedcommand);
				commands.Sort(new DelayedCommandComparer());
				
			}
		}
		
		public void Remove(int index)
		{
			if (index < commands.Count)
			{
				commands.Remove(commands[index]);
			}
		}
		
		public void Remove(DelayedCommand delayedcommand)
		{
			lock (commands.SyncRoot)
			{
				commands.Remove(delayedcommand);
			}
		}
		
		public DelayedCommand Pop()
		{
			lock (commands.SyncRoot)
			{
				DelayedCommand delayedcommand = (DelayedCommand)commands[0];
				Remove(0);
				return delayedcommand;
			}
			
		}

		public DelayedCommand First {
			get {
				lock (commands.SyncRoot)
				{
					return (DelayedCommand)commands[0];
				}
			}
		}
		
		public int Count {
			get {
				
				return commands.Count;
			}
		}

		/// <summary>
		/// Method to display the delayed commands arraylist for debugging
		/// </summary>
		/// <returns></returns>
		public string Display()
		{
			string list="";

			lock (commands.SyncRoot)
			{
				foreach (DelayedCommand dc in commands)
				{
					list+=dc.command + "\r\n";
				}
			}
			return list;
		}
		
	}
}
