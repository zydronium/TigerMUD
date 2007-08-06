using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections.Specialized;

namespace GameLibrary
{
    /// <summary>
    /// Base class from which all game objects are derived. 
    /// </summary>
    public class GameObject
    {
        Dictionary<string, Item> inventory = new Dictionary<string, Item>();
        
        public bool AddInventory(Item item) 
        {
            try
            {
                inventory.Add(item.NameShort, item);

            }
            catch
            {
                return true;
            }


            return false;

        }

        public bool RemoveInventory(Item item)
        {
            try
            {
                inventory.Remove(item.NameShort);

            }
            catch
            {
                return true;
            }
            return false;

        }

        private bool changed;

        public bool Changed
        {
            get { return changed; }
            set { changed = value; }
        }

        private string id;

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        private string nameshort;

        public string NameShort
        {
            get { return nameshort; }
            set { nameshort = value.ToLower(); }
        }

        private string namedisplay;

        public string NameDisplay
        {
            get { return namedisplay; }
            set { namedisplay = value; }
        }

       
        

	

    }
}
