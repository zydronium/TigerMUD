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
using System.Security.Cryptography;
using TigerMUD.CommsLib;

namespace TigerMUD
{
    /// <summary>
    /// Summary description for EmailPassword.
    /// </summary>
    public class EmailPassword
    {
        #region Properties
        private IUserSocket userSocket = null;
        private string userName = "";
        private string email = "";
        //private DataTable userInfo = null;
        private MailMessage mailMessage = new MailMessage();
        private StringBuilder mailBody = new StringBuilder();
        private int[] randomPassword = new int[5];
        private string generatedPassword = "";
        #endregion

        #region Property Accessors
        public IUserSocket UserSocket
        {
            get
            {
                return userSocket;
            }
            set
            {
                userSocket = value;
            }
        }
        public string Username
        {
            get
            {
                return this.userName;
            }
            set
            {
                this.userName = value;
            }
        }
        #endregion

        #region Constructors
        public EmailPassword()
        {

        }

        public EmailPassword(IUserSocket userSocket, string Username)
        {
            this.userSocket = userSocket;
            this.userName = Username;
        }
        #endregion

        #region Events
        #endregion

        #region Methods
        // method called to begin the process
        public void CheckEmailPassword()
        {
            Actor actor = Lib.GetByName(this.userName);
            //Actor actor = GetEmailInfo(); //get the email info for the user
            if (actor != null)
            {
                if (actor["email"].ToString() == "")
                {
                    userSocket.SendLine("cannot send a new password to you in email because your account does not have an email address associated with it.");
                    userSocket.SendLine("You'll have to contact the MUD admin for assistance at " + Lib.AdminEmail + ".\r\n");
                    return;
                }
                else
                {
                    actor["email"] = RandomPasswordGenerator();
                    actor.Save();
                    this.MakeMailMessage();
                    this.SendEmail();
                    userSocket.SendLine("A new password was sent to your email address on file. Login with the new password to continue.");
                    return;
                }
            }
            else
            {
                // No valid user account to reset the password for.
                // To prevent hackers fishing for valid accounts, spoof this.
                Lib.log.Add(this.userSocket,"trying username: " + this.userName,"SECURITY: Attempt to reset password on an invalid account.");
                userSocket.SendLine("A new password was sent to your email address on file. Login with the new password to continue.");
                //userSocket.SendLine("No user account found please try again.");
                return;
            }
        }

        //Creates random password for user to relogin to the system with...
        public string RandomPasswordGenerator()
        {
            Random random = new Random();
            for (int i = 0; i <= 4; i++)
            {
                randomPassword[i] = random.Next(97, 122);
            }
            for (int i = 0; i <= 4; i++)
            {
                this.generatedPassword = this.generatedPassword + (((Char)randomPassword[i]).ToString());
            }
            return this.generatedPassword;
        }

      
        public string HashText(string userPassword)
        {
            string textToHash = userPassword;
            byte[] byteRepresentation = UnicodeEncoding.UTF8.GetBytes(textToHash);
            byte[] hashedTextInBytes = null;
            MD5CryptoServiceProvider myMD5 = new MD5CryptoServiceProvider();
            hashedTextInBytes = myMD5.ComputeHash(byteRepresentation);
            string hashedText = Convert.ToBase64String(hashedTextInBytes);
            return hashedText;
            // will display X03MO1qnZdYdgyfeuILPmQ== MessageBox.Show(hashedText); 
        }

        private bool SendEmail()
        {
            try
            {
                System.Web.Mail.SmtpMail.SmtpServer = Lib.Serverinfo.SmtpServer;
                System.Web.Mail.SmtpMail.Send(this.mailMessage);

                return true;
            }
            catch (Exception ex)
            {
                //if it doesnt work tell the user and reset the database
                //Lib.dbService.EmailPassword.UpdateUserEmailAddress(userName, "");
                userSocket.SendLine("Mail sending failed. Please contact the server administrator.");

                //check the InnerException
                while (ex.InnerException != null)
                {
                    Lib.log.Add(userSocket, userName, "Exception while sending email: " + ex.Message + ex.StackTrace);
                    ex = ex.InnerException;
                }
                return false;
            }
        }

        //creates the email message
        private void MakeMailMessage()
        {
            //make the body of the email using a string builder
            this.mailBody.Append("You have requested your password to be sent to you.\n");
            this.mailBody.Append("In order to accomplish this, we had to reset your password.\n");
            this.mailBody.Append("Your password is:");
            this.mailBody.Append(this.generatedPassword + "\n");
            this.mailBody.Append("Please log back onto the TigerMUD server.");

            //set the basic values for the mail message
            this.mailMessage.Body = this.mailBody.ToString();
            this.mailMessage.From = "admin@tigermud.com";
            this.mailMessage.Subject = "TigerMUD Password Request Accepted";
            this.mailMessage.To = this.email;
        }
        #endregion
    }
}
