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
using System.Data;


namespace TigerMUD
{
	/// <summary>
	/// Defines an instance of a marketplace. 
	/// You can have more than one market, each with unique IDs.
	/// </summary>
	public class Market
	{
		private string id="";
		private string name="";
		ArrayList marketitems=new ArrayList();

		public Market(string id, string name)
		{
			this.Id=id;
			this.Name=name;
			//
			//
		}

		public string Name
		{
			get {return name;}
			set {name=value;}
		}


		/// <summary>
		/// Market identifier
		/// </summary>
		public string Id
		{
			get {return id;}
			set {id=value;}
		}


		/// <summary>
		/// Loads the market with items.
		/// Only run this method *after* all game items are loaded into memory.
		/// </summary>
		/// <returns>boolean</returns>
		public bool LoadMarket()
		{
			for (int i=Lib.actors.Count-1;i>=0;i--)
			{
				Actor item=(Actor)Lib.actors[i];
				if (item!=null)
				{
					// Only add item to this market if its a market item and for *this* market

					if (item["containertype"].ToString()=="market" && item["container"].ToString()==this.Id)
					{
						this.marketitems.Add(item);
					}
				}
			}
			return true;
		}


		public bool ShowWelcomeScreen(Actor user)
		{
			int result=0;
			Menu menu=new Menu(user);
			menu.MenuType=Menu.MenuTypes.SingleChoice;
			menu.Header="Welcome to " + this.Name;
			menu.Prompt="What would you like to do? ";
			menu.AddChoice("Buy an item");
			menu.AddChoice("Sell an item");
			ArrayList choices=menu.ShowMenu(ref result);
			// Always catch nulls coming back from menus.
			// That usually means the client disconnected at the menu.
			if (choices==null)
			{
				return false;
			}
			if (result==1)
			{
				return false;
			}

			int choice;
			try
			{
				choice=Convert.ToInt32(choices[0]);
			}
			catch
			{
				return false;
			}

            switch (choice)
			{
				case 1:
					Lib.market.BuyItem(user);
					break;
				case 2:
					Lib.market.SellItem(user);
					break;
				case 3:
					return false;
				default:
					return false;
			}
			return true;
		}

		
		public bool ViewItems(Actor user)
		{
			user.Send("Items for sale on the market:\r\n");
			// Get item from market arraylist
			for (int i=marketitems.Count-1;i>=0;i--)
			{
				Actor tmpitem=(Actor)marketitems[i];

				if (tmpitem==null)
				{
					user.SendError("Unable to view item.\r\n");
					return false;
				}
				user.Send(tmpitem["name"] + " - Current bid: " + tmpitem["bidprice"] + " Time remaining: " + (Convert.ToInt64(tmpitem["saleexpiration"])-Convert.ToInt64(Lib.ServerState["serverticks"])) + "\r\n");
			}

			return true;
		
		}

		public bool ViewItem(Actor user,string itemid)
		{
			// Get Item by id
			Actor item=Lib.GetByID(itemid);
			if (item==null)
			{
				user.SendError("Invalid market item.\r\n");
				return false;
			}

			// Get item from market arraylist
			for (int i=marketitems.Count-1;i>=0;i--)
			{
				Actor tmpitem=(Actor)marketitems[i];

				if (tmpitem==null)
				{
					user.SendError("Unable to view item.\r\n");
					return false;
				}

				if (tmpitem["id"]==item["id"])
				{
					user.Send(tmpitem["name"] + " - Current bid: " + tmpitem["bidprice"] + " Time remaining: " + (Convert.ToInt64(tmpitem["saleexpiration"])-Convert.ToInt64(Lib.ServerState["serverticks"])) + "\r\n");
				}

			}
			return true;
		}

		public bool BuyItem(Actor user)
		{
			MenuForItems menu=new MenuForItems(user);
			menu.MenuType=MenuForItems.MenuTypes.SingleChoiceBack;
			menu.Header="Buy an Item";
			menu.Prompt="Which item do you want to buy? ";
			// Load the available market items into the menu
			for (int i=Lib.market.marketitems.Count-1;i>=0;i--)
			{
				Actor item=(Actor)Lib.market.marketitems[i];
				if (item!=null)
				{
					menu.AddChoice(item);
				}
			}
			// show the menu
			bool exit=false;
			ArrayList choices=menu.ShowMenu(ref exit);
			// Always catch nulls coming back from menus.
			// That usually means the client disconnected at the menu.
			if (choices==null)
			{
				return false;
			}
			if (exit)
			{
				return false;
			}

			Actor choice=null;
			try
			{
				// Make sure user chooses a real item
				choice=(Actor)choices[0];
			}
			catch 
			{
				// Otherwise user chose to go back
				this.ShowWelcomeScreen(user);
			}

			if (choice==null)
			{
				return false;
			}

			if (!Lib.YesNo(user,"Buy this item - " + choice["name"] + " for " + choice["askprice"] + "?"))
			{
				return false;
			}

			if (Convert.ToInt32(user["cash"])<Convert.ToInt32(choice["askprice"]))
			{
				user.SendError("You don't have enough money to buy this item.\r\n");
				return false;
			}

			Lib.market.marketitems.Remove(choice);
			choice["container"]=user["id"];
			choice["containertype"]="user";
			user.Additem(choice);
			choice.Save();
			user.Save();
			user.Send(user["coloralertgood"] + "You bought " + choice["nameprefix"] + " " + choice["name"] + ".\r\n");

            // TODO Logic to buy item here		
			
			return true;
		}

		public bool CycleMarket()
		{
			return true;
		}

		public bool ListItem(Actor user,string itemid, int askprice, float saleexpiration)
		{
			// Get Item by id
			Actor item=Lib.GetByID(itemid);
			if (item==null)
			{
				user.SendError("Invalid item to list for sale on the market.\r\n");
				return false;
			}

			return true;
		}

		public bool SellItem(Actor user)
		{
			return true;
		}

		public static bool CheckForMarketer(Actor user)
		{
            Actor room = Lib.GetByID(user["container"].ToString());

			if (room==null)
			{
				user.SendError("Unable to get the user's current room.\r\n");
				return false;
			}
			ArrayList mobs=room.GetMobs();

			for (int i=mobs.Count-1;i>=0;i--)
			{
				Actor mob=(Actor)mobs[i];
				if (mob==null)
				{
					user.SendError("Unable to retrieve mob list for this room.\r\n");
					return false;
				}

                if (mob["subtype"].ToString() == "marketer")
				{
					return true;
				}

			}
			return false;
		
		}


	}
}
