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
using System.Security;
using System.Collections.Generic;

namespace TigerMUD.DatabaseLib
{
	/// <summary>
	/// Takes care of "cleaning" up any input data to 
	/// remove possible security attacks.
	/// </summary>
	public class DataCleaning
	{
		public const string MESSAGE_SQL_INJECTION = "SQL injection attack detected.";

		/// <summary>
		/// Cleans text and removes harmful characters.
		/// </summary>
		/// <remarks>
		/// This overload ignores any SQL instructions found in the text.
		/// </remarks>
		/// <param name="inputText">The text to sanitize.</param>
		/// <returns>A sanitized string.</returns>
		public static void Sanitize(ref string inputText)
		{
			Sanitize(ref inputText, false, false);
		}

		public static string Sanitize(string inputText)
		{
			return Sanitize(inputText, false, false);
		}

		/// <summary>
		/// Cleans text and removes harmful characters. Also looks for 
		/// any SQL instructions. Throws a SecurityException if any
		/// are found.
		/// </summary>
		/// <param name="checkKeywords">Check for SQL keywords</param>
		/// <param name="inputText">The text to sanitize</param>
		/// <returns>A sanitized string.</returns>
		public static void Sanitize(ref string inputText,
			bool checkKeywords)
		{
			Sanitize(ref inputText, checkKeywords, false);
		}

		/// <summary>
		/// Cleans text and removes harmful characters. Also looks for 
		/// any SQL instructions. Throws a SecurityException if any
		/// are found.
		/// </summary>
		/// <param name="checkKeywords">Check for SQL keywords</param>
		/// <param name="inputText">The text to sanitize</param>
		/// <param name="ignoreBrackets">True if semicolons must be avoided (for ansi characters), false otherwise.</param>
		/// <returns>A sanitized string.</returns>
		public static void Sanitize(ref string inputText, bool checkKeywords, bool ignoreBrackets)
		{

            inputText = Sanitize(inputText, checkKeywords, ignoreBrackets);
		}

		/// <summary>
		/// Cleans text and removes harmful characters. Also looks for 
		/// any SQL instructions. Throws a SecurityException if any
		/// are found.
		/// </summary>
		/// <param name="checkKeywords">Check for SQL keywords</param>
		/// <param name="inputText">The text to sanitize</param>
		/// <param name="ignoreBrackets">True if semicolons must be avoided (for ansi characters), false otherwise.</param>
		/// <returns>A sanitized string.</returns>
		public static string Sanitize(string inputText, bool checkKeywords, bool ignoreBrackets)
		{
			if (string.IsNullOrEmpty(inputText))
			{
				return "";
			}

			inputText = inputText.Replace("'", "''");
			inputText = inputText.Replace("\"", "\"\"");

            List<string> badstrings = new List<string>();
            badstrings.Add("delete");
            badstrings.Add("update");
            badstrings.Add("select");
            badstrings.Add("delete");

			// Look for SQL instructions
            foreach (string badstring in badstrings)
            {
                int badstringposition = inputText.IndexOf(badstring);
                if (badstringposition != -1)
                {
                    // Found a bad string
                    inputText = inputText.Remove(badstringposition, badstring.Length);
                }
            }
            
			return inputText;
			
		}
	}
}
