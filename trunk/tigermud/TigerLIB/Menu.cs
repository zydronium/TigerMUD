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
	#region Menu Class

	public class Menu
	{
		#region Enumerations
		public enum MenuTypes
		{
			//returns after single selection **DEFAULT**
			SingleChoice		=	0,
			//returns after multiselection
			MultiChoice			=	1,
			//returns after single selection with "Back" option
			SingleChoiceBack	=	2,
			//returns after multiselection with "Back" option
			MultiChoiceBack		=	3
		}
		#endregion

		#region Properties
		private MenuTypes	e_MenuType		= MenuTypes.SingleChoice;
		private string		s_Footer		= "";
		private string		s_Header		= "";
		private string s_Choice="";
		private string		s_Prompt		= "";
		private ArrayList	al_MenuItems = new ArrayList();
		private ArrayList	al_ChosenItems	= new ArrayList();
		
		private Actor		o_User;
		#endregion

		#region Property Accessors
		public MenuTypes MenuType
		{
			get
			{
				return this.e_MenuType;
			}
			set
			{
				this.e_MenuType = value;
			}
		}

		public bool AddChoice(string choice)
		{
			MenuItems.Add(choice);
			return true;
		}

		public string Footer
		{
			get
			{
				return this.s_Footer;
			}
			set
			{
				this.s_Footer = value;
			}
		}
		public string Header
		{
			get
			{
				return this.s_Header;
			}
			set
			{
				this.s_Header = value;
			}
		}
		public string Prompt
		{
			get
			{
				return this.s_Prompt;
			}
			set
			{
				this.s_Prompt = value;
			}
		}
		public ArrayList MenuItems
		{
			get
			{
				return this.al_MenuItems;
			}
			set
			{
				this.al_MenuItems = value;
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
				this.o_User		= value;
			}
		}
		#endregion

		#region Constructors/Deconstructors
		
		//blank constructor
		public Menu()
		{
		
		}

		//blank constructor
		public Menu(Actor user)
		{
		
			this.User=user;
		}
		//menu with items and type pre-setup selected
		public Menu(string Header, string Footer, string Prompt, ArrayList MenuItems, MenuTypes MenuType, Actor User)
		{
			this.e_MenuType		= MenuType;
			this.s_Footer		= Footer;
			this.s_Header		= Header;
			this.s_Prompt		= Prompt;
			this.al_MenuItems	= MenuItems;
			this.o_User			= User;
		}
		#endregion

		#region Events
		#endregion

		#region Methods
		//displays the menu items and numbers them
		private void DisplayItems()
		{
			int count = 1;

			if(this.s_Header.Length != 0)
				this.User.Send("\r\n" + this.s_Header + "\r\n\r\n");
			
			foreach(string s_Tmp in this.al_MenuItems)
			{
				this.User.Send(count.ToString() + ". ");
				
				// cycle through choices and if this one has already been picked,
				// flag it with asterisks.
				if (this.MenuType==MenuTypes.MultiChoice || this.MenuType==MenuTypes.MultiChoiceBack)
				{
					foreach (int choice in this.al_ChosenItems)
					{
						if (choice==count)
						{
							this.User.Send("***");
						}
					}
				}
				this.User.Send(s_Tmp + "\r\n");
				count++;
			}

			if(this.e_MenuType == MenuTypes.SingleChoiceBack || this.e_MenuType == MenuTypes.MultiChoiceBack)
				this.User.Send("B. Back\r\n");

			if (this.e_MenuType==MenuTypes.SingleChoice)
			{
				this.User.Send("X/Q. Exit\r\n\r\n");
			}
			else if (this.e_MenuType==MenuTypes.MultiChoice)
			{
				this.User.Send("A. Accept Choices and Exit\r\n\r\n");
			}
			else 
			{
				this.User.Send("X/Q. Exit\r\n\r\n");
			}

			if(this.s_Footer.Length != 0)
				this.User.Send(this.s_Footer + "\r\n");

			if(this.s_Prompt.Length != 0)
				this.User.Send(this.s_Prompt + this.User["colorcommandtext"]);
		}



		public ArrayList ShowMenu(ref int result)
		{
			
			// result=1 means user chose "Exit"
			// result=2 means user chose "Back"
			// result=3 means user chose "Accept Choices and Exit"
			int count = 0;
			bool Exit = false;
			bool chk=false;
			
			// clear chosen items for non-multichoice menus
			if (this.e_MenuType != MenuTypes.MultiChoice && this.e_MenuType != MenuTypes.MultiChoiceBack)
			{
				this.al_ChosenItems.Clear();
			}

			//loop until told to exit
			while(!Exit)
			{
				try
				{
					// If user disco during this prompt, the socket it gone and we need to catch this exception.
					//display the menu
					this.DisplayItems();
					//get a reply
					s_Choice = this.User.userSocket.GetResponse();
					if (s_Choice==null)
					{
						return null;
					}

				}
				catch
				{
					result=1;
					return null;
				}
				
				// Validate proper input
				if (!Validation.IsAlphanumeric(s_Choice))
				{
					this.User.SendError("Only alphanumeric characters are allowed for input.\r\n" + this.User["colormessages"]);
					count++;
				}

				//find out if the reply is a number
				if (!Validation.IsNumber(s_Choice))
				{
					if (s_Choice.ToLower()!="a" && s_Choice.ToLower()!="b" && s_Choice.ToLower()!="x" && s_Choice.ToLower()!="q")
					{
						this.User.SendError("Please enter a valid choice from the menu.\r\n" + this.User["colormessages"]);
						count++;
					}
			
					// User picks exit?
					if (s_Choice.ToLower()=="x" || s_Choice.ToLower()=="q")
					{
						result=1;
						Exit = true;
					}
				
					// parse input for menus that offer a back choice
					if(this.e_MenuType != MenuTypes.SingleChoice && this.e_MenuType != MenuTypes.MultiChoice)
					{
						// User picks back?
						if (s_Choice.ToLower()=="b")
						{
							result=2;
							Exit = true;
						}
					}
					if (this.e_MenuType==MenuTypes.MultiChoice)
					{
						// User picks accept and exit?
						if (s_Choice.ToLower()=="a")
						{
							result=3;
							Exit = true;
						}
					}
				}

				// process numeric menu choices (not "back" or "exit")
				if(Validation.IsNumber(s_Choice))
				{
					int i_Choice = new int();
					if(s_Choice != null && s_Choice!="")
						i_Choice = Convert.ToInt32(s_Choice);
					
					//make sure its within the right range
					if(i_Choice > 0 && i_Choice <= this.al_MenuItems.Count)
					{
						//if this menu only takes one choice then exit
						if(this.e_MenuType == MenuTypes.SingleChoice || this.e_MenuType == MenuTypes.SingleChoiceBack)
						{
							this.al_ChosenItems.Add(i_Choice);
							Exit = true;
						}
						else
						{
							// for multichoice menus
							// if user picks something already picked, then toggle it off and remove it
							chk=false;
							for (int i=this.al_ChosenItems.Count-1;i>=0;i--)
							{
								if ((int)this.al_ChosenItems[i]==i_Choice)
								{
									this.al_ChosenItems.Remove(i_Choice);
									chk=true;
								}
							}
							// wasn't already picked so add it
							if (!chk)
							{
								this.al_ChosenItems.Add(i_Choice);
							}
							
						}
					}
					else
					{
						this.User.SendError("Please enter a valid choice from the menu.\r\n" + this.User["colormessages"]);
					}
				}
				count++;
			}
			return this.al_ChosenItems;
		}
		#endregion
	}
	#endregion
}
