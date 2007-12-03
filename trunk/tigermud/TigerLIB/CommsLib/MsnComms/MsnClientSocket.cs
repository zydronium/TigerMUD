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
using System.Threading;

namespace TigerMUD.CommsLib.MsnComms
{
	/// <summary>
	/// Handles client communication via the MSN network.
	/// </summary>
  //  public class MsnClientSocket : IUserSocket
  //  {
  //  public const int DEFAULT_TIMEOUT = 10000; // default timeout is 10 seconds

  //  private string uniqueId;
  //  private Conversation conversation;
  //  private Contact userContact;
  //  private string lastMessage;
  //  private AutoResetEvent userInputWaitHandle;
  //  private int responseTimeout;

  //  /// <summary>
  //  /// Creates a new MSN client socket.
  //  /// </summary>
  //  /// <param name="conversation">The conversation started by the remote client.</param>
  //  /// <param name="responseTimeout">Time in milliseconds to wait for a client response.</param>
  //      public MsnClientSocket(Conversation conversation,
  //    int responseTimeout)
  //      {
  //    uniqueId = System.Guid.NewGuid().ToString();
  //    this.conversation = conversation;

  //    // Get the userContact
  //    foreach(Contact contact in this.conversation.Switchboard.Contacts.Values)
  //    {
  //      this.userContact = contact;
  //      break;
  //    }
      
  //    this.responseTimeout = responseTimeout;

  //    conversation.Switchboard.TextMessageReceived += new TextMessageReceivedEventHandler(Switchboard_TextMessageReceived);
  //    conversation.Switchboard.ServerErrorReceived += new XihSolutions.DotMSN.Core.ErrorReceivedEventHandler(Switchboard_ServerErrorReceived);
  //    conversation.Switchboard.SessionClosed += new SBChangedEventHandler(Switchboard_SessionClosed);
  //    conversation.Switchboard.UserTyping += new UserTypingEventHandler(Switchboard_UserTyping);

  //    userInputWaitHandle = new AutoResetEvent(false);
  //  }

  //  #region IUserSocket Members

  //  public string GetNewLineCharacter()
  //  {
  //    return null;
  //  }

  //  public void Send(string text)
  //  {
  //    // if there is no switchboard available, request a new switchboard session
  //    if(conversation.SwitchboardProcessor.Connected == false)
  //    {
  //      this.conversation.Messenger.Nameserver.RequestSwitchboard(conversation.Switchboard, this);
  //    }

  //    // conversation.Switchboard.SendTypingMessage();
  //    TextMessage message = new TextMessage(text);
  //    conversation.Switchboard.SendTextMessage(message);
  //  }

  //  public void SendLine(string text)
  //  {
  //    Send(text);
  //  }

  //  public string GetResponse()
  //  {
  //    if(userInputWaitHandle.WaitOne(responseTimeout, false))
  //    {
  //      lock(lastMessage)
  //      {
  //        return lastMessage;
  //      }
  //    }
  //    else
  //    {
  //      return null;
  //    }
  //  }

  //  public void Close()
  //  {
  //    conversation.Switchboard.Close();
  //  }

  //  public bool Connected
  //  {
  //    get
  //    {
  //      return conversation.SwitchboardProcessor.Connected;
  //    }
  //  }

  //  public string ClientEndpointId
  //  {
  //    get
  //    {
  //      /*
  //      if(userContact == null)
  //      {
  //        return "not known yet";
  //      }
  //      else
  //      {
  //        return userContact.Mail;
  //      }
  //      */
  //      return "not known";
  //    }
  //  }

  //  public string UniqueId
  //  {
  //    get
  //    {
  //      return uniqueId;
  //    }
  //  }
  //  #endregion

  //  #region Switchboard Events
  //  private void Switchboard_TextMessageReceived(object sender, TextMessageEventArgs e)
  //  {
  //    lock(lastMessage)
  //    {
  //      lastMessage = e.Message.Text;

  //      // Flag the caller that there is input
  //      userInputWaitHandle.Set();
  //    }
  //  }

  //  private void Switchboard_ServerErrorReceived(object sender, MSNErrorEventArgs e)
  //  {
  //    Lib.log.Add(this,
  //      "unknown",
  //      "MSN Server error: " + e.MSNError.ToString());
  //  }

  //  private void Switchboard_SessionClosed(object sender, EventArgs e)
  //  {
  //    // conversation.SwitchboardProcessor.Disconnect();
  //    Lib.PrintLine("Connection closed");
  //  }

  //  private void Switchboard_UserTyping(object sender, ContactEventArgs e)
  //  {
  //    // Might be useful when doing a GetResponse() call.
  //  }
  //  #endregion
  //}
}
