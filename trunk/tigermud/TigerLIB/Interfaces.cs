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
using System.Reflection;
using System.Collections;

namespace TigerMUD
{
	#region HelpInfo
	public struct HelpInfo
	{
		public string Command;
		public string Summary;
		public string Syntax;
		public string[] Examples;
		public string Description;
	}
	#endregion
	
	#region ICommand
	public interface ICommand
	{
		string Name { get; }
		string[] Words { get; }
		HelpInfo Help { get; }
		bool DoCommand(Actor actor, string command, string arguments);
	}
	#endregion
	
	#region IAction
	public interface IAction
	{
		string Name { get; }
		string[] Words { get; }
		bool DoAction(Actor actor, Actor target, string action, string arguments);
	}
	#endregion
	
	#region IActionResponse
	public interface IActionResponse
	{
		bool ProcessAction(Actor actor, string action, string arguments);
	}
	#endregion	

	#region IScriptCompiler
	public interface IScriptCompiler
	{
        Object[] GetObjectsFromFile(String Filename, out string errors);
		Object[] GetObjectsFromFiles(String[] Filenames, out string errors);
        Object[] GetObjectsFromCode(String Code, out string errors);
        Assembly CompileFiles(String[] Filenames, out string errors);
        Assembly CompileCode(String Code, out string errors);
	}
	#endregion

	#region IPluginLoader
	public interface IPluginLoader
	{
        Object[] GetObjectsFromFile(String Filename, out string errors);
        Assembly GetAssemblyFromFile(String Filename, out string errors);
        Object[] GetObjectsFromFiles(String[] Filenames, out string errors);		
	}
	#endregion
}