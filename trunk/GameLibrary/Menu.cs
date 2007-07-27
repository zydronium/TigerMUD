using System;
using System.Collections;
using System.Text;

namespace GameLibrary
{
    public class Menu : IDisposable
    {
        PlayerCharacter pc = new PlayerCharacter();
        ArrayList menuitems = new ArrayList();
        ArrayList menuchoices = new ArrayList();
        ArrayList keys = new ArrayList();
        string key = string.Empty;
        string menutext = string.Empty;


        public Menu(PlayerCharacter player)
        {
            pc = player;
        }

        public Menu(PlayerCharacter player, string title, string description, string footer, string prompt, string exitkey)
        {
            pc = player;
            this.Title = title;
            this.Description = description;
            this.Footer = footer;
            this.Prompt = prompt;
            this.ExitKey = exitkey;
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
        ~Menu()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }


        public ArrayList Show()
        {
            bool exit = false;
            bool found = false;
            while (!exit)
            {
                found = false;
                pc.Send(GetMenuText());
                key = pc.GetResponse();
                // disco
                if (key == null) return null;
                if (key == this.ExitKey) exit = true;
                if (key == this.BackKey)
                {
                    if (Parent != null)
                    {
                        Parent.Show();
                    }
                    return null;
                }
                if (key != this.ExitKey)
                {
                    foreach (MenuItem mi in menuitems)
                    {
                        if (mi.Key == key)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        pc.SendLine("&RERROR");
                        continue;
                    }

                    keys.Add(key);
                    if (this.Type == MenuType.SingleChoice)
                    {
                        break;
                    }
                    // locate item and flag it
                    foreach (MenuItem mi in menuitems)
                    {
                        if (key == mi.Key)
                        {
                            mi.Flagged = !mi.Flagged;
                        }
                    }
                }
            }
            return GetChoices(keys);
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

        public Object GetObject(string key)
        {
            foreach (MenuItem mi in menuitems)
            {
                if (key == mi.Key)
                    return mi.GameObject;
            }
            return null;
        }

        public ArrayList GetChoices(ArrayList keys)
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

        string GetMenuText()
        {
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

        public bool Add(MenuItem mi)
        {
            menuitems.Add(mi);
            return true;
        }

        public bool Remove(MenuItem mi)
        {
            menuitems.Remove(mi);
            return true;
        }

        private Menu parent;

        public Menu Parent
        {
            get { return parent; }
            set { parent = value; }
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
