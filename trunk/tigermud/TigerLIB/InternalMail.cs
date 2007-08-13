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
using System.Collections;


namespace TigerMUD
{
	#region UserMail Classes

	#region UserMail Class
	public class UserMail
	{
		#region Enumerations
		public enum MailDisplayTypes
		{
			All		= 0,
			Unread	= 1,
			Read	= 2,
			Sent	= 3
		}
		#endregion

		#region Properties
		private ArrayList		al_Messages = new ArrayList();
		private Actor			o_User;
		private UserMailMessage	o_Message;
		private DataTable			dt_Messages=new DataTable();
		private TigerMUD.Menu	o_Menu;
		#endregion

		#region Property Accessors
		public ArrayList Messages
		{
			get
			{
				return this.al_Messages;
			}
		}
		public Actor User
		{
			get
			{
				return this.o_User;
			}
			set
			{
				this.o_User = value;
			}
		}
		#endregion

		#region Constructors
		public UserMail()
		{}
		public UserMail(Actor User)
		{
			this.o_User	= User;
		}
		#endregion

		#region Methods
		private void GetMail(MailDisplayTypes MailDisplayType)
		{
			if(MailDisplayType == MailDisplayTypes.All)
			{
                this.dt_Messages = Lib.GetAllMail(User["shortname"].ToString());
			}
			else if(MailDisplayType == MailDisplayTypes.Unread)
			{
                this.dt_Messages = Lib.GetUnreadMail(User["shortname"].ToString());
			}
			else if(MailDisplayType == MailDisplayTypes.Sent)
			{
                this.dt_Messages = Lib.GetSentMail(User["shortname"].ToString());
			}

			if(this.dt_Messages.Rows.Count >= 1)
			{
				foreach(DataRow dr_Tmp in this.dt_Messages.Rows)
				{
					this.o_Message = new UserMailMessage(Convert.ToInt32(dr_Tmp["M_ID"].ToString()),Convert.ToDateTime(dr_Tmp["M_Created"].ToString()),dr_Tmp["M_Sender"].ToString(),dr_Tmp["M_Receiver"].ToString(),dr_Tmp["M_Subject"].ToString(),dr_Tmp["M_Body"].ToString(),Lib.ConvertToBoolean(dr_Tmp["M_Read"]));
					this.al_Messages.Add(this.o_Message);
				}
			}
		}

		public void Mail()
		{
            this.dt_Messages = Lib.GetUnreadMail(o_User["shortname"].ToString());
			this.o_Menu				= new Menu();
			this.o_Menu.User		= this.o_User;
			this.o_Menu.Header		= "-- Internal Postal Service -- " + "\r\n" + "Please make a selection from below:";
			this.o_Menu.MenuItems	= new ArrayList ();
			this.o_Menu.MenuItems.Add("View New Mail");
			this.o_Menu.MenuItems.Add("View Inbox - All Mail");
			this.o_Menu.MenuItems.Add("View Sent Mail");
			this.o_Menu.MenuItems.Add("Send Mail");
			this.o_Menu.Footer		= "You have " + o_User["coloralertgood"] + this.dt_Messages.Rows.Count.ToString() + o_User["colormessages"] + " new messages";
			this.o_Menu.Prompt		= "Selection: ";
			this.o_Menu.MenuType	= Menu.MenuTypes.SingleChoice;
			this.dt_Messages		= null;
			ArrayList	al_Tmp	= new ArrayList();
			int result=0;

			al_Tmp = this.o_Menu.ShowMenu(ref result);
			// Always catch nulls coming back from menus.
			// That usually means the client disconnected at the menu.
			if (al_Tmp==null)
			{
				return;
			}
			
			if (result>=1)
			{
				//User chose back or exit
				return;
			}
			if(al_Tmp[0].ToString() == "1")
			{
				switch (this.DisplayMail(MailDisplayTypes.Unread))
				{
					case 1:
						return;
					case 2:
						Mail();
						return;
					default:
						Mail();
						return;
				}
			}
			else if(al_Tmp[0].ToString() == "2")
			{
				switch (this.DisplayMail(MailDisplayTypes.All))
				{
					case 1:
						return;
					case 2:
						Mail();
						return;
					default:
						Mail();
						return;
				}
			}
			else if(al_Tmp[0].ToString() == "3")
			{
				switch (this.DisplayMail(MailDisplayTypes.Sent))
				{
					case 1:
						return;
					case 2:
						Mail();
						return;
					default:
						Mail();
						return;
				}

			}
			else if(al_Tmp[0].ToString() == "4")
			{
				SendMail();
			}
			al_Tmp = null;
		}

		public int DisplayMail(MailDisplayTypes MailDisplayType)
		{
			this.al_Messages.Clear();
            
			this.GetMail(MailDisplayType);
			this.o_Menu				= new Menu();
			this.o_Menu.User		= this.o_User;

			if(MailDisplayType == MailDisplayTypes.All)
				this.o_Menu.Header		= "-- Inbox - All --";
			else if(MailDisplayType == MailDisplayTypes.Unread)
				this.o_Menu.Header		= "--Inbox - New--";
			else if(MailDisplayType == MailDisplayTypes.Sent)
				this.o_Menu.Header		= "--Sent--";

			this.o_Menu.MenuItems	= new ArrayList ();
			foreach(UserMailMessage o_Tmp in this.al_Messages)
			{
				this.o_Menu.MenuItems.Add("Subject: " + o_Tmp.Subject + "    From: " + o_Tmp.Sender + "    To: " + o_Tmp.Recipient);
			}
			this.o_Menu.Footer		= "";
			this.o_Menu.Prompt		= "Please make a selection: ";
			this.o_Menu.MenuType	= Menu.MenuTypes.SingleChoiceBack;
			int result=0;
			
			ArrayList	al_Tmp = new ArrayList();

			while (result<1)
			{
				// al_Tmp contains the integer number of the message to view
				al_Tmp=o_Menu.ShowMenu(ref result);
				// Always catch nulls coming back from menus.
				// That usually means the client disconnected at the menu.
				if (al_Tmp==null)
				{
					return 0;
				}
				if (result>=1)
				{
					return result;
				}
				if(al_Tmp.Count == 0)
				{
					this.Mail();
					return 1;
				}
				else
				{
					for (int i=al_Tmp.Count-1;i>=0;i--)
					{
						this.ViewMessage((UserMailMessage)al_Messages[((int)al_Tmp[0])-1]);
					}
				}
			}
			return 0;
		}

		public void ViewMessage(UserMailMessage mail)
		{
			o_User.Send(o_User["colorpeople"]);
			o_User.Send("Sent: " + o_User["colorpeople"] + mail.Date.ToString() + "\r\n");
			//o_User.Send("To: " + mail.Recipient + "\r\n");
			o_User.Send("From: " + o_User["colorpeople"] + mail.Sender + "\r\n");
			o_User.Send("Subject: " + o_User["colorpeople"] + mail.Subject + "\r\n\r\n");
			o_User.Send("Message: " + o_User["colorpeople"] + mail.Body + "\r\n");
			mail.Read = true;
            Lib.MarkAsRead(mail.ID);
			o_User.Send(o_User["colormessages"] + "\r\nHit Enter to continue...\r\n");
			o_User.UserSocket.GetResponse();
		}

		public void SendMail()
		{
			o_User.Send(o_User["colorpeople"]);
			o_User.Send("-- New Mail Message --" + "\r\n");
			o_User.Send(o_User["colormessages"]);
			o_User.Send("Recipient name: ");
            string recipientname = Lib.GetResponse(o_User);

            Actor recipient = Lib.GetByName(recipientname);
			// Ensure that recipient exists before going any further
			if (recipient==null)
			{
				o_User.SendError("\r\nNo such user exists.\r\n");
				return;
			}
			o_User.Send("Subject: ");
            string subject = Lib.GetResponse(o_User);
			o_User.Send("Type your mail message and input terminates when you hit Enter:\r\n");
            string message = Lib.GetResponse(o_User);
			o_User.Send("\r\n\r\nYou are about the send the following message:\r\n");
			o_User.Send(o_User["colormessages"] + "To: " + o_User["colorpeople"] + recipient["name"] + o_User["colormessages"] + "\r\nSubject: " + o_User["colorpeople"] + subject + o_User["colormessages"] + "\r\nMessage: " + o_User["colorpeople"] + message + "\r\n\r\n");
			o_User.Send(o_User["colorexits"]);
            if (Lib.YesNo(o_User, "\r\nSend this message?"))
			{
                if (Lib.SendMessage(o_User.Sanitize(o_User["shortname"].ToString()), o_User.Sanitize(recipient["shortname"].ToString()), o_User.Sanitize(subject), o_User.Sanitize(message)))
				{
					o_User.Send(o_User["coloralertgood"] + "Message sent!\r\n");
				}
				else
				{
					o_User.SendError("Send failed. There was an error trying to send your message.\r\n");
				}
			}
			else
			{
				o_User.SendError("Message aborted.\r\n");
			}
			o_User.Send(o_User["colormessages"] + "\r\nHit Enter to continue...\r\n" + o_User["colormessages"]);
			o_User.UserSocket.GetResponse();
			return;
		}


		#endregion
	}
	#endregion

	#region UserMailMessage class
	public class UserMailMessage
	{
		#region Properties
		private int		i_ID		= 0;
		private string	s_Sender	= "";
		private string	s_Recipient	= "";
		private string	s_Subject	= "";
		private string	s_Body		= "";
		private bool	b_Read		= false;
		// Date cannot be null so populate a value here
		private DateTime date=new DateTime();
		#endregion

		#region Property Accessors
		public int ID
		{
			get
			{
				return this.i_ID;
			}
			set
			{
				this.i_ID = value;
			}
		}
		public string Sender
		{
			get
			{
				return this.s_Sender;
			}
			set
			{
				this.s_Sender = value;
			}
		}
		public string Recipient
		{
			get
			{
				return this.s_Recipient;
			}
			set
			{
				this.s_Recipient = value;
			}
		}
		public string Subject
		{
			get
			{
				return this.s_Subject;
			}
			set
			{
				this.s_Subject = value;
			}
		}
		public string Body
		{
			get
			{
				return this.s_Body;
			}
			set
			{
				this.s_Body = value;
			}
		}
		public bool Read
		{
			get
			{
				return this.b_Read;
			}
			set
			{
				this.b_Read = value;
			}
		}
		public DateTime Date
		{
			get {return date;}
			set {date=value;}
		}

		#endregion

		#region Constructors
		public UserMailMessage()
		{}
		public UserMailMessage(int ID, DateTime Date, string Sender, string Recipient, string Subject, string Body, bool Read)
		{
			this.i_ID			= ID;
			this.Date=Date;
			this.s_Sender		= Sender;
			this.s_Recipient	= Recipient;
			this.s_Subject		= Subject;
			this.s_Body			= Body;
			this.b_Read			= Read;
		}
		#endregion
	}
	#endregion

	#endregion
}
