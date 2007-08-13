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
using TigerMUD.CommsLib;

namespace TigerMUD
{
	/// <summary>
	/// Checks for naughty words in player text.
	/// </summary>
	public class ProfanityFilter
	{
    private static string[] BAD_WORDS = null; // empty list of profanities.
    private static string BAD_WORD_REPLACEMENT = "@#$@&!";
    private static char[] SPLITTER = new char[] {' '};

    // Runtime filter settings
    private static bool enableFilter = false;
    private static bool sendMessageToPlayer = true;
    private static string playerMessage = "Now now, that's no way to talk.";

    /// <summary>
    /// Filters a message and hides profanities behind asterisk characters. 
    /// Supports words that have been separated by special characters.
    /// </summary>
    /// <param name="message">The message to filter.</param>
    /// <param name="userSocket">The player's connection socket.</param>
    /// <returns>True if the message contained bad words, false otherwise.</returns>
    public static bool FilterMessage(ref string message, IUserSocket userSocket)
    {
      bool hasBadWords = false;

      if(enableFilter)
      {
        // Tokenise the sentence into words
        string[] words = message.Split(SPLITTER);

        // Clean up the message
        RemoveSpecialCharacters(words);
        ReplaceDigitsWithLetters(words);

        // Check each word against our profanity list
        hasBadWords = ReplaceBadWords(words);

        if(hasBadWords)
        {
          HandleBadWords(userSocket);

          // Reconstruct the message
          message = ReconstructMessage(message, words);
        }
      }

      return hasBadWords;
    }

    #region Profanity List
    /// <summary>
    /// Refresh the list of profanities.
    /// </summary>
    public static void RefreshProfanityList()
    {
      // Set our list to null so that
      // the next time we get the list it
      // is retrieved from the database.
      BAD_WORDS = null;
    }

    private static string[] GetProfanityList()
    {
      if(BAD_WORDS == null)
      {
        BAD_WORDS = Lib.dbService.ProfanityFilter.GetProfanityList();
      }

      return BAD_WORDS;
    }
    #endregion

    #region Message Filtering
    /// <summary>
    /// Remove special characters like underscores etc from the
    /// words.
    /// </summary>
    /// <param name="words">The words to check.</param>
    private static void RemoveSpecialCharacters(string[] words)
    {
      for(int counter = 0; counter < words.Length; counter++)
      {
        // Get the current word.
        string word = words[counter];

        word = word.Replace("-", "");
        word = word.Replace("*", "");
        word = word.Replace("_", "");
        word = word.Replace("\"", "");
        word = word.Replace("'", "");

        // Put the modified word back into the array.
        words[counter] = word;
      }
    }

    /// <summary>
    /// Replace digits with letters to prevent things like 
    /// "sh!t" and so on.
    /// </summary>
    /// <param name="words">The words to check.</param>
    private static void ReplaceDigitsWithLetters(string[] words)
    {
      for(int counter = 0; counter < words.Length; counter++)
      {
        // Get the current word.
        string word = words[counter];

        word = word.Replace("3", "e");
        word = word.Replace("1", "i");
        word = word.Replace("!", "i");
        word = word.Replace("$", "s");

        // Put the modified word back into the array.
        words[counter] = word;
      }      
    }

    /// <summary>
    /// Replace bad words with a masking string of uniform length.
    /// </summary>
    /// <param name="words">The words to check.</param>
    /// <returns>True if there were profanities, false otherwise.</returns>
    private static bool ReplaceBadWords(string[] words)
    {
      bool hasBadWords = false;
      for(int counter = 0; counter < words.Length; counter++)
      {
        bool foundBadWord = false;

        // Get the latest profanity list.
        string[] badWords = GetProfanityList();

        foreach(string badWord in BAD_WORDS)
        {
          if(words[counter].ToLower() == badWord)
          {
            foundBadWord = true;
            break;
          }
        }

        if(foundBadWord)
        {
          // Replace the profanity with asterisks.
          words[counter] = BAD_WORD_REPLACEMENT;
          hasBadWords = true;
        }
        else
        {
          // Mark the word as null, since the array holds
          // a list of replacement words.
          words[counter] = null;
        }
      }

      return hasBadWords;
    }

    /// <summary>
    /// Reconstruct the message back to the player, and replace those words found
    /// to have profanities.
    /// </summary>
    /// <param name="message">The original player message.</param>
    /// <param name="filteredWords">The collection of filtered words.</param>
    /// <returns>A reconstructed player string.</returns>
    private static string ReconstructMessage(string message, string[] filteredWords)
    {
      // Get the original set of words
      string[] words = message.Split(SPLITTER);
      StringBuilder builder = new StringBuilder();

      for(int counter = 0; counter < (words.Length); counter++)
      {
        if(filteredWords[counter] != null)
        {
          builder.Append(filteredWords[counter]);
        }
        else
        {
          builder.Append(words[counter]);
        }
        builder.Append(" ");
      }
  
      // Remove the last space
      builder.Remove(builder.Length - 1, 1);

      return builder.ToString();
    }
    #endregion

    #region Player Behavoiur Management
    /// <summary>
    /// React to the player speaking profanities.
    /// </summary>
    /// <param name="userSocket">The player's connection socket.</param>
    private static void HandleBadWords(IUserSocket userSocket)
    {
      // Do we send the player a message?
      if(sendMessageToPlayer)
      {
        userSocket.SendLine(playerMessage);
      }
    }
    #endregion

    #region Properties
    /// <summary>
    /// Set to true if you want to filter out profanities.
    /// </summary>
    public static bool EnableFilter
    {
      get
      {
        return enableFilter;
      }
      set
      {
        enableFilter = value;
      }
    }

    /// <summary>
    /// Set to true if you want to send a message to the player
    /// when a profanity is detected.
    /// </summary>
    public static bool SendMessageToPlayer
    {
      get
      {
        return sendMessageToPlayer;
      }
      set
      {
        sendMessageToPlayer = value;
      }
    }

    /// <summary>
    /// The message to send to the player if he swears.
    /// </summary>
    public static string PlayerMessage
    {
      get
      {
        return playerMessage;
      }
      set
      {
        playerMessage = value;
      }
    }
    #endregion
	}
}

