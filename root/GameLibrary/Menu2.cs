using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace GameLibrary
{
    // New menu class to support thread pooling

    // override the HandleResponse for your own logic
    public class Menu2
    {
        protected PlayerCharacter pc = new PlayerCharacter();
        protected ArrayList menuitems = new ArrayList();
        protected ArrayList menuchoices = new ArrayList();
        protected ArrayList keys = new ArrayList();
        protected string menutext = string.Empty;
        protected GameContext gamecontext;

        protected Dictionary<string, Menu2> childmenus = new Dictionary<string, Menu2>();

        public Menu2()
        {
            gamecontext = GameContext.GetInstance();
        }


        public Menu2(PlayerCharacter pcin)
        {
            gamecontext = GameContext.GetInstance();
            pc = pcin;
            // todo pc.Menu = this;
            pc.ConnectionState = GameLibrary.ConnectionState.WaitingAtMenuPrompt;

        }

        public Menu2(PlayerCharacter pcin, string title, string description, string footer, string prompt, string exitkey)
        {
            gamecontext = GameContext.GetInstance();
            pc = pcin;
            this.Title = title;
            this.Description = description;
            this.Footer = footer;
            this.Prompt = prompt;
            this.ExitKey = exitkey;

            // todo pc.Menu = this;
            pc.ConnectionState = GameLibrary.ConnectionState.WaitingAtMenuPrompt;
        }

        protected string lastkey=String.Empty;

        public string LastKey
        {
            get { return lastkey; }
            set { lastkey = value; }
        }

        private ArrayList choices;

        public ArrayList Choices
        {
            get { return choices; }
            set { choices = value; }
        }
	
	
        public virtual bool Show()
        {
            try
            {
                pc.Send(GetMenuText());
                // todo pc.Menu = this;
                pc.ConnectionState = GameLibrary.ConnectionState.WaitingAtMenuPrompt;
            }
            catch
            {
                return true;
            }
            return false;


        }

        /// <summary>
        /// Returns objects associated with the menu items that the user chose.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual void HandleResponse(string key)
        {
            lastkey = key;
            bool found = false;
            if (String.IsNullOrEmpty(key)) return;
            if (key == this.ExitKey)
            {
                return;
            }
            if (key == this.BackKey)
            {
                if (Parent != null)
                {
                    // todo pc.Menu = Parent;
                    Parent.Show();
                }
                return;
            }
            // Find the menu item that the user picked
            foreach (MenuItem mi in menuitems)
            {
                if (mi.Key == key)
                {
                    // found it now flag it
                    if (!mi.Flagged)
                    {
                        mi.Flagged = true;
                        keys.Add(key);
                        found = true;
                    }
                    else
                    {
                        // already flagged so unflag it
                        mi.Flagged = false;
                        keys.Remove(key);
                        found = true;
                    }

                    // if single choice menu, return on successful choice
                    if (this.Type == MenuType.SingleChoice)
                    {
                        CheckForChildMenus(GetChoices(keys));
                        return;
                    }

                    break;
                }
            }
            if (!found)
            {
                pc.SendLine("&RERROR");
            }
            this.Show();
            return;
        }

        public virtual void CheckForChildMenus(ArrayList keys)
        {
            // Show child menu if they exist and choice matches
            if (childmenus.Count > 0)
            {

                foreach (string key in keys)
                {
                    if (childmenus.ContainsKey(key))
                    {
                        childmenus[key].Parent = this;
                        childmenus[key].Show();
                        return;

                    }

                }
            }
            // If no children then just process responses
            ProcessChoices(keys);
            return;
        }

        public virtual bool ProcessChoices(ArrayList keys)
        {
            return false;
        }

       


        private Menu2 parent;

        public Menu2 Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        public virtual bool AddChildMenu(string menuoption, Menu2 newmenu)
        {
            if (!childmenus.ContainsKey(menuoption))
            {
                childmenus.Add(menuoption, newmenu);
                return true;
            }
            else
                return false;


        }

        public virtual bool RemoveChildMenu(string menuoption)
        {
            if (childmenus.ContainsKey(menuoption))
            {
                childmenus.Remove(menuoption);
                return true;
            }
            else
                return false;

        }


        public virtual string GetMenuText()
        {
            string menutext = string.Empty;

            menutext = string.Empty;
            menutext += "\r\n" + this.Title + "\r\n";
            menutext += this.Description + "\r\n";

            foreach (MenuItem mi in menuitems)
            {
                if (mi.Flagged) menutext += "**";
                menutext += "(" + mi.Key + ") - " + mi.Label;
                if (!String.IsNullOrEmpty(mi.Description))
                {
                    menutext += "\t" + mi.Description + "\r\n";
                }

            }
            menutext += "(" + this.ExitKey + ") - Exit\r\n";
            menutext += this.Footer + "\r\n";
            menutext += this.Prompt;

            return menutext;
        }

        public virtual ArrayList GetChoices(ArrayList keys)
        {
            if (keys == null) throw new ArgumentNullException("keys");

            menuchoices.Clear();
            foreach (string key in keys)
            {
                foreach (MenuItem mi in menuitems)
                {
                    if (key == mi.Key)
                    {
                        // If menuitems are not game objects (for example, just strings)
                        if (mi.GameObject == null) menuchoices.Add(mi.Key);
                        // Is a GameObject
                        else menuchoices.Add(mi.GameObject);

                        break;
                    }
                }
            }
            keys.Clear();
            return menuchoices;
        }

        public virtual bool AddMenuItem(MenuItem mi)
        {
            menuitems.Add(mi);
            return true;
        }

        public virtual bool RemoveMenuItem(MenuItem mi)
        {
            menuitems.Remove(mi);
            return true;
        }


        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free other state (managed objects).
                pc.Dispose();
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.
        }

        // Use C# destructor syntax for finalization code.
        ~Menu2()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }

        // set default key
        private string exitkey = "x";

        public string ExitKey
        {
            get
            {
                return exitkey;
            }
            set { exitkey = value; }
        }

        // set default key
        private string backkey;

        public string BackKey
        {
            get
            {
                return backkey;
            }
            set { backkey = value; }
        }


        private MenuType type = MenuType.SingleChoice;

        public MenuType Type
        {
            get { return type; }
            set { type = value; }
        }

        public Object GetMenuObject(string key)
        {
            foreach (MenuItem mi in menuitems)
            {
                if (key == mi.Key)
                    return mi.GameObject;
            }
            return null;
        }

        private string title;

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        private string description;

        public string Description
        {
            get { return description; }
            set { description = value; }
        }


        private string prompt = "Choice: ";

        public string Prompt
        {
            get { return prompt; }
            set { prompt = value; }
        }

        private string footer = string.Empty;

        public string Footer
        {
            get { return footer; }
            set { footer = value; }
        }


    }
}
