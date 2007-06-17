using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace GameLibrary
{
    // New menu class to support thread pooling

    public class Menu3
    {
        protected PlayerCharacter pc = new PlayerCharacter();
        protected ArrayList menuitems = new ArrayList();
        protected ArrayList menuchoices = new ArrayList();
        protected ArrayList keys = new ArrayList();
        protected string lastkey = string.Empty;
        protected string menutext = string.Empty;
        protected GameContext gamecontext;

        protected Dictionary<string, Menu3> childmenus = new Dictionary<string, Menu3>();


        public Menu3()
        {
            gamecontext = GameContext.GetInstance();
        }


        public Menu3(PlayerCharacter pcin)
        {
            gamecontext = GameContext.GetInstance();
            pc = pcin;

            pc.Menu = this;
            pc.ConnectionState = GameLibrary.ConnectionState.WaitingAtMenuPrompt;

        }

        public Menu3(PlayerCharacter pcin, string title, string description, string footer, string prompt, string exitkey)
        {
            gamecontext = GameContext.GetInstance();
            pc = pcin;
            this.Title = title;
            this.Description = description;
            this.Footer = footer;
            this.Prompt = prompt;
            this.ExitKey = exitkey;

            pc.Menu = this;
            pc.ConnectionState = GameLibrary.ConnectionState.WaitingAtMenuPrompt;
        }

        public virtual void Exit()
        {
            pc.ConnectionState = GameLibrary.ConnectionState.NotWaitingForClientResponse;
            // Insert your logic here
        }

        public static Menu3 CreateMenu(PlayerCharacter pcin, string title, string description, string footer, string prompt, string exitkey)
        {
            Menu3 menu = new Menu3();

            GameContext gamecontext = GameContext.GetInstance();
            menu.Title = title;
            menu.Description = description;
            menu.Footer = footer;
            menu.Prompt = prompt;

            pcin.Menu = menu;
            pcin.ConnectionState = GameLibrary.ConnectionState.WaitingAtMenuPrompt;
            return menu;

        }



        /// <summary>
        /// ArrayList of MenuItems that represent the choices that the user made
        /// </summary>
        public ArrayList MenuChoices
        {
            get { return menuchoices; }
            set { menuchoices = value; }
        }


        public virtual bool Show()
        {
            try
            {
                pc.Menu = this;
                pc.Send(GetMenuText());
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
            bool found = false;
            if (key==null) return;

            if (key == this.ExitKey)
            {
                Exit();
                return;
                
            }

            if (key == this.BackKey)
            {
                if (Parent != null)
                {
                    pc.Menu = Parent;
                    Parent.Show();
                }
                return;
            }

            if (this.Type == MenuType.MultiChoice && key == this.AcceptKey)
            {
                
                CheckForChildMenus(GetChoices(keys));
                pc.ConnectionState = GameLibrary.ConnectionState.NotWaitingForClientResponse; 
                //IsExiting = true;
                return;
            }


            // Find the item that the user picked
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

                    // if single choice menus, return on successful choice
                    if (this.Type == MenuType.SingleChoice || this.Type==MenuType.Question)
                    {
                        CheckForChildMenus(GetChoices(keys));
                        pc.ConnectionState = GameLibrary.ConnectionState.NotWaitingForClientResponse;
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
            StateInfo stateinfo = new StateInfo();
            stateinfo.MenuChoices = keys;
            ProcessChoices(stateinfo);
            return;
        }

        /// <summary>
        /// Determine what the choices were and store them in an object arraylist
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public virtual void ProcessChoices(object stateinfoin)
        {
            // Store choices in public for easier retrieval
            GameLibrary.StateInfo stateinfo = (GameLibrary.StateInfo)stateinfoin;
            stateinfo.MenuChoices = menuchoices;
            this.MenuHandler(stateinfo);


        }

        public delegate void MenuChoiceProcessor(object stateinfo);

        private MenuChoiceProcessor menuhandler;

        public MenuChoiceProcessor MenuHandler
        {
            get { return menuhandler; }
            set { menuhandler = value; }
        }





        //private bool isexiting = false;

        //public bool IsExiting
        //{
        //    get { return isexiting; }
        //    set { isexiting = value; }
        //}

        private Menu3 parent;

        public Menu3 Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        public virtual bool AddChildMenu(string menuoption, Menu3 newmenu)
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
            menutext += "\r\n(" + this.ExitKey + ") - Exit\r\n";
            
            if (this.Type == MenuType.MultiChoice)
            {
                menutext += "(";

                if (AcceptKey == "")
                    menutext += "Enter";
                else
                    menutext += this.AcceptKey;
                
                menutext += ") - Accept and Continue\r\n";
            }

            
            menutext += this.Footer + "\r\n";
            menutext += this.Prompt;

            return menutext;
        }

        protected virtual ArrayList GetChoices(ArrayList keys)
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
            //this.MenuChoices = menuchoices;
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
        ~Menu3()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }

        private string acceptkey = "";

        public string AcceptKey
        {
            get { return acceptkey; }
            set { acceptkey = value; }
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
