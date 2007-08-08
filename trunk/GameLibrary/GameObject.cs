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
        public Dictionary<string, Item> items = new Dictionary<string, Item>();

        
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


        private string description;

        public string Description
        {
            get { return description; }
            set { description = value; }
        }
	

       
        

	

    }
}
