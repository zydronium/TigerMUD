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

namespace TigerMUD.DatabaseLib
{
	/// <summary>
	/// This exposes the interfaces needed by various modules
	/// to talk to the database.
	/// </summary>s
	public class DbService
	{
		private string connectionString;
		private IEmailUserActivation emailUserActivation;
		private IEmailPassword emailpassword;
		private IMudLog mudLog;
		private IActor item;
		private ILibrary library;
		private IActor room;
		private ISpell spell;
		private IActor plant;
		private IActor user;
		private IActor actor;
		private ISystemCommands systemCommands;
		private IInternalMail internalMail;
		private IActor mob;
		private IProfanityFilter profanityFilter;
		private IThreadManager threadManager;
		private IMudBugs mudBugs;

		private bool logStatements;

		#region Properties
		/// <summary>
		/// Enable this to see the SQL query strings being dumped
		/// to the console window.
		/// </summary>
		public bool LogStatements
		{
			get
			{
				return logStatements;
			}
			set
			{
				logStatements = value;

				if(logStatements)
				{
					EnableLogStatements();
				}
				else
				{
					DisableLogStatements();
				}
			}
		}
		#endregion

		public DbService(string connectionString)
		{
			logStatements = false;
			this.connectionString = connectionString;
			CreateFunctionalAreas();
		}

		/// <summary>
		/// Instantiate each of the db functional areas to be offered
		/// by the db service.
		/// 
		/// Here's where we would do library switching based on db type.
		/// At the moment there is only one db type, ODBC, which has it's own
		/// library TigerLIB.DatabaseLib.Odbc.
		/// 
		/// This could be extended to native MySQL for example by adding an
		/// implementation library: Tiger.DatabaseLib.MySql.
		/// </summary>
		private void CreateFunctionalAreas()
		{
			emailUserActivation = new Odbc.EmailUserActivation(connectionString);
			emailpassword = new Odbc.EmailPassword(connectionString);
			mudLog = new Odbc.MudLog(connectionString);
			item = new Odbc.Actor(connectionString);
			library = new Odbc.Library(connectionString);
			spell = new Odbc.Spell(connectionString);
			plant = new Odbc.Actor(connectionString);
			user = new Odbc.Actor(connectionString);
			room = new Odbc.Actor(connectionString);
			actor = new Odbc.Actor(connectionString);
			systemCommands = new Odbc.SystemCommands(connectionString);
			internalMail = new Odbc.InternalMail(connectionString);
			mob = new Odbc.Actor(connectionString);
			profanityFilter = new Odbc.ProfanityFilter(connectionString);
			threadManager = new Odbc.ThreadManager(connectionString);
			mudBugs = new Odbc.MudBugs(connectionString);
		}

		#region Database Functional Areas
		/// <summary>
		/// Email User Activation Data.
		/// </summary>
		public IEmailUserActivation EmailUserActivation
		{
			get
			{
				return emailUserActivation;
			}
		}

		/// <summary>
		/// Email Data
		/// </summary>
		public IEmailPassword EmailPassword
		{
			get
			{
				return emailpassword;
			}
		}

		/// <summary>
		/// Interacts with the MUD log data.
		/// </summary>
		public IMudLog MudLog
		{
			get
			{
				return mudLog;
			}
		}

		/// <summary>
		/// Handles all Item-related data.
		/// </summary>
		public IActor Item
		{
			get
			{
				return item;
			}
		}

		/// <summary>
		/// Handles all Library-related data.
		/// </summary>
		public ILibrary Library
		{
			get
			{
				return library;
			}
		}

		/// <summary>
		/// Handles all Spell-based data.
		/// </summary>
		public ISpell Spell
		{
			get 
			{
				return spell;
			}
		}

	

		/// <summary>
		/// Handles all Actor-based data.
		/// </summary>
		public IActor Actor 
		{
			get
			{
				return actor;
			}
		}

		/// <summary>
		/// Handles all System Commands.
		/// </summary>
		public ISystemCommands SystemCommands
		{
			get
			{
				return systemCommands;
			}
		}

		/// <summary>
		/// Handles all Internal Mail-based data.
		/// </summary>
		public IInternalMail InternalMail
		{
			get
			{
				return internalMail;
			}
		}

		/// <summary>
		/// Handles all Mob-based data.
		/// </summary>
		public IActor Mob
		{
			get
			{
				return mob;
			}
		}

		/// <summary>
		/// Handles all data for the Profanity Filter.
		/// </summary>
		public IProfanityFilter ProfanityFilter
		{
			get
			{
				return profanityFilter;
			}
		}

		/// <summary>
		/// Handles all threadmanager-related data.
		/// </summary>
		public IThreadManager ThreadManager
		{
			get
			{
				return threadManager;
			}
		}

		/// <summary>
		/// Handles all data around the mud bug tracker.
		/// </summary>
		public IMudBugs MudBugs
		{
			get
			{
				return mudBugs;
			}
		}
		#endregion    

		#region SQL Statement Logging
		private void EnableLogStatements()
		{
			// The base library of whichever database implementation 
			// we are using must implement ILogStatements so that
			// we can turn statement logging on and off in the 
			// application.

			((ILogStatements)emailUserActivation).LogStatements = true;
			((ILogStatements)mudLog).LogStatements = true;
			((ILogStatements)emailpassword).LogStatements = true;
			((ILogStatements)item).LogStatements = true;
			((ILogStatements)library).LogStatements = true;
			((ILogStatements)spell).LogStatements = true;
			((ILogStatements)plant).LogStatements = true;
			((ILogStatements)user).LogStatements = true;
			((ILogStatements)room).LogStatements = true;
			((ILogStatements)actor).LogStatements = true;
			((ILogStatements)systemCommands).LogStatements = true;
			((ILogStatements)internalMail).LogStatements = true;
			((ILogStatements)mob).LogStatements = true;
			((ILogStatements)profanityFilter).LogStatements = true;
			((ILogStatements)threadManager).LogStatements = true;
			((ILogStatements)mudBugs).LogStatements = true;
		}

		private void DisableLogStatements()
		{
			((ILogStatements)emailUserActivation).LogStatements = false;
			((ILogStatements)mudLog).LogStatements = false;
			((ILogStatements)emailpassword).LogStatements = false;
			((ILogStatements)item).LogStatements = false;
			((ILogStatements)library).LogStatements = false;
			((ILogStatements)spell).LogStatements = false;
			((ILogStatements)plant).LogStatements = false;
			((ILogStatements)user).LogStatements = false;
			((ILogStatements)room).LogStatements = false;
			((ILogStatements)actor).LogStatements = false;
			((ILogStatements)systemCommands).LogStatements = false;
			((ILogStatements)internalMail).LogStatements = false;
			((ILogStatements)mob).LogStatements = false;
			((ILogStatements)profanityFilter).LogStatements = false;
			((ILogStatements)threadManager).LogStatements = false;
			((ILogStatements)mudBugs).LogStatements = false;
		}
		#endregion
	}
}
