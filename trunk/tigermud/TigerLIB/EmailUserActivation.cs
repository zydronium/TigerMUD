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
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;
using System.Web.Mail;
using TigerMUD.CommsLib;

namespace TigerMUD
{
  /// <summary>
  /// Summary description for EmailUserActivation.
  /// </summary>
  public class EmailUserActivation
  {
    #region Properties
    private IUserSocket			userSocket			= null;	//the users socket
    private string			s_Username			= "";						
    private string			s_Email				= "";
    private string s_TmpEmail="";
    private string			s_ActivationCode	= "";
    private string s_TmpActivationCode="";
    private bool			b_Activated			= false;
    //private DataTable 	dt_ActivationInfo	= null;
    private MailMessage		o_MailMessage		= new MailMessage();
    private StringBuilder	sb_MailBody			= new StringBuilder();
    #endregion

    #region Property Accessors
    public string Username
    {
      get
      {
        return this.s_Username;
      }
      set
      {
        this.s_Username	= value;
      }
    }
    #endregion

    #region Constructors
    public EmailUserActivation()
    {
    }

    public EmailUserActivation(IUserSocket userSocket, string Username)
    {
      this.userSocket = userSocket;
      this.s_Username	= Username;
    }
    #endregion

    #region Events
    #endregion

    #region Methods
    // method called to begin the process
    public bool CheckEmailActivation()
    {
      this.GetActivationInfo(); //get the info from the db

      if(this.b_Activated == false)
      {
        //if user has no email or activation code assosiated with them
        if(this.s_Email == "" || this.s_ActivationCode == "") 
        {
          this.EnterEmail();
          return false;
        }
          //if not then they just need to enter the activation code
        else
        {
          if(!this.Activate())
            return false;
        }
      }

      return true;
    }

    //gets the info for the user from the db
    private void GetActivationInfo()
    {
      ////grab info from mudactors table
      //this.dt_ActivationInfo = Lib.dbService.EmailUserActivation.GetUserInfo(s_Username);

      //if (dt_ActivationInfo.Rows.Count > 0)
      //{
      //  this.s_Email = this.dt_ActivationInfo.Rows[0]["email"].ToString();
      //  this.b_Activated = Lib.ConvertToBoolean(this.dt_ActivationInfo.Rows[0]["activated"].ToString());
      //  this.s_ActivationCode	= this.dt_ActivationInfo.Rows[0]["activationcode"].ToString();
      //}

      Actor actor = Lib.GetByName(s_Username);
      if (actor != null)
      {
          this.s_Email = actor["email"].ToString();
          this.b_Activated = Lib.ConvertToBoolean(actor["activated"].ToString());
          this.s_ActivationCode = actor["activationcode"].ToString();
          actor.Save();
      }

    }

    //gets the user to input thier email and checks it then sends the activation email
    private void EnterEmail()
    {
      int		count	= 1;
      bool	Exit	= false;

      //tell the user what we're doing and what to do
      userSocket.SendLine("Your account does not current have an email address associated with it.");
      userSocket.SendLine("Once you have entered your email address you will be disconnected, please reconnect when you have recived your confirmation email with your activation code.");

      while(!Exit)
      {
        userSocket.SendLine("Please enter you email address now:");

        //wait for the email to be entered
        try
        {
          // if user disco, then socket is gone and we need to catch this exception
          s_TmpEmail = userSocket.GetResponse();
        }
        catch
        {
          return;
        }

        //check if it really is an email
        if(Lib.Emailregex.IsMatch(s_TmpEmail))
        {
          //if it is then store it
          this.s_Email			= s_TmpEmail;
          //make a new activation code
          this.s_ActivationCode	= this.CreateCode();
          //make a new email
          this.MakeMailMessage(this.s_ActivationCode);
          //send the message
          if(this.SendEmail())
          {
              Actor actor = Lib.GetByName(s_Username);
              if (actor != null)
              {
                  actor["activationcode"] = s_ActivationCode;
                  actor["email"] = s_Email;
                  actor.Save();
                  userSocket.SendLine("Email valid ... we will now send you an email with your activation code and dissconnect you.");
              }

            //Lib.dbService.EmailUserActivation.UpdateActivationCode(s_Username, s_ActivationCode);
            //Lib.dbService.EmailUserActivation.UpdateUserEmailAddress(s_Username, s_Email);
            //userSocket.SendLine("Email valid ... we will now send you an email with your activation code and dissconnect you.");
          }
          Exit = true;
        }
        else
        {
          //if not then find out how many bad emails have been entered
          if(count == 3)
          {
            //if its 3 then wave goodbye
            userSocket.SendLine("Too many attepmts at entering an email address. Bye!");
            Exit = true;
          }
          else
          {
            //if not just tell um they've been bad
            userSocket.SendLine("That email address is not valid.");
          }
        }
        count++;
      }
    }

    private bool SendEmail()
    {
      try
      {
        System.Web.Mail.SmtpMail.SmtpServer = Lib.Serverinfo.SmtpServer;
        System.Web.Mail.SmtpMail.Send(this.o_MailMessage);

        return true;
      }
      catch (Exception)
      {
        //if it doesnt work tell the user and reset the database
          Actor actor = Lib.GetByName(s_Username);
          if (actor != null)
          {
              actor["activationcode"] = "";
              actor["email"] = "";
              userSocket.SendLine("Mail sending failed. Please contact the server administrator at " + Lib.AdminEmail);
          }
        //Lib.dbService.EmailUserActivation.UpdateActivationCode(s_Username, "");
        //Lib.dbService.EmailUserActivation.UpdateUserEmailAddress(s_Username, "");
        //userSocket.SendLine("Mail sending failed. Please contact the server administrator at admin@tigermud.com");

        return false;
      }
    }

    //creates a new activation code
    private string CreateCode()
    {
      Random random = new Random();
      int count = 0;
      byte[] code = new byte[7]; // this will store the seven character activation code
      byte tmp;
      while(count <= 6)
      {
        int UpperOrLower = random.Next(1, 100);
        if(UpperOrLower <= 50) //if the number is less than or equal to 50 then its a lowercase letter otherwise its upper
          tmp = (byte)random.Next(65, 90);
        else
          tmp = (byte)random.Next(97, 122);

        code[count] = tmp;// add the letter to the activation code
        count++;
      }
      return System.Text.ASCIIEncoding.ASCII.GetString(code);
    }

    //creates the email message
    private void MakeMailMessage(string code)
    {
      //make the body of the email using a string builder
      this.sb_MailBody.Append("Thank you for creating a new user on TigerMUD\n");
      this.sb_MailBody.Append("Your activation code is:\n");
      this.sb_MailBody.Append(this.s_ActivationCode + "\n");
      this.sb_MailBody.Append("Please log back onto the TigerMUD server and enter your activation code.");

      //set the basic values for the mail message
      this.o_MailMessage.Body		= this.sb_MailBody.ToString();
      this.o_MailMessage.From		= "admin@tigermud.com";
      this.o_MailMessage.Subject	= "TigerMUD Activation Code";
      this.o_MailMessage.To		= this.s_Email;
    }

    private bool Activate()
    {
      int count = 0;
      bool Exit = false;
      while(!Exit)
      {
        if(count < 3) // 3 tries to enter your code
        {
          userSocket.SendLine("Please enter your activation code:");
					
          try
          {
            // if user disco, then socket is gone and we need to catch this exception
            s_TmpActivationCode = userSocket.GetResponse();
          }
          catch
          {
            return false;
          }
					
          if(s_TmpActivationCode == this.s_ActivationCode)
          {
            Lib.dbService.EmailUserActivation.UpdateUserActivationStatus(s_Username, true);
            userSocket.SendLine("Thank you for activating your account.");
            return true;
          }
          else
          {
            userSocket.SendLine("Incorrect activation code.");
          }
        }
        else
        {
          userSocket.SendLine("Too many incorrect trys. Disconecting.");
          Exit = true;
        }
      }
      return false;
    }
    #endregion
  }
}
