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
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
using System.Collections;

namespace TigerMUD
{
	[Serializable]
	public class PluginLoader : MarshalByRefObject, TigerMUD.IPluginLoader
	{
		public PluginLoader()
		{
		}
		
		/// <summary>
		/// Returns all the Objects from a file.
		/// </summary>
		/// <param name="Filename">Name of the input file.</param>
		/// <returns>Objects in an array.</returns>
		public Object[] GetObjectsFromFile(String Filename, out string errors)
		{
            errors = "";
            try
            {
                return GetObjectsFromAssembly(GetAssemblyFromFile(Filename, out errors), out errors);
            }
            catch (Exception ex)
            {
                errors += ex.Message + ex.StackTrace + "\r\n";
                return null;
            }
		}

		/// <summary>
		/// Returns an Assembly created from a file.
		/// </summary>
		/// <param name="Filename">Name of the input file.</param>
		/// <returns>Assembly</returns>
		public System.Reflection.Assembly GetAssemblyFromFile(String Filename, out string errors)
		{
            errors = "";
            try
            {
                return System.Reflection.Assembly.LoadFrom(Filename);
            }
            catch (Exception ex)
            {
                errors += ex.Message + ex.StackTrace + "\r\n";
                return null;
            }

		} 
		
		/// <summary>
		/// Returns all the Objects from an array of files.
		/// </summary>
		/// <param name="Filenames">Array of filesnames to get an IDemo from.</param>
		/// <returns></returns>
		public Object[] GetObjectsFromFiles(String[] Filenames, out string errors)
		{
            errors = "";
            System.Reflection.Assembly PluginAssembly = null;
            System.Collections.ArrayList Plugins = new System.Collections.ArrayList();
            try
            {
                foreach (string Filename in Filenames)
                {
                    PluginAssembly = System.Reflection.Assembly.LoadFrom(Filename);
                    Plugins.Add(GetObjectsFromAssembly(PluginAssembly, out errors));
                }
            }
            catch (Exception ex)
            {
                errors += ex.Message + ex.StackTrace + "\r\n";
            }
			return (Object[])Plugins.ToArray(typeof(Object));
		}
		
		/// <summary>
		/// Returns all the Command and Action objects from an Assembly.
		/// </summary>
		/// <param name="Code">String containing the source code.</param>
		/// <returns>Objects in an array.</returns>
		private Object[] GetObjectsFromAssembly(System.Reflection.Assembly ScriptAssembly, out string errors)
		{
            errors = "";
			System.Collections.ArrayList ScriptArray = new System.Collections.ArrayList();
			
			try
			{
				foreach (Type type in ScriptAssembly.GetTypes())
				{
					try
					{
						if (type.GetInterface("ICommand") != null)
						{
							Command Command = (Command)Activator.CreateInstance(type);
							ScriptArray.Add(Command);
						}
						if (type.GetInterface("IAction") != null)
						{
							Action Action = (Action)Activator.CreateInstance(type);
							ScriptArray.Add(Action);
						}
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine("Failed to get Objects from Assembly.  Exception: " + ex.Message + ex.StackTrace);
					}
				}
			}
			catch (ReflectionTypeLoadException e)
			{
				foreach (Exception ex in e.LoaderExceptions)
				{
					Lib.PrintLine("EXCEPTION in PluginLoader (GetObjectsFromAssembly): " + ex.Message + ex.StackTrace);
				}
			}
			
			return (Object[])ScriptArray.ToArray(typeof(Object));
		}
		
	}
}
