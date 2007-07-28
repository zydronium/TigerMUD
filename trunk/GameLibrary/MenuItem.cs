using System;
using System.Collections.Generic;
using System.Text;

namespace GameLibrary
{
    public class MenuItem
    {

        public MenuItem(Object obj, string label, string description, string key)
        {
            this.GameObject = obj;
            this.Label = label;
            this.Description = description;
            this.Key = key;
        }

        /// <summary>
        /// For question menu types
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        public MenuItem(Object obj, string key)
        {
            this.GameObject = obj;
            this.Key = key;
        }

        public MenuItem(string label, string description, string key)
        {
            this.GameObject = null;
            this.Label = label;
            this.Description = description;
            this.Key = key;
        }

        public MenuItem(string label, string key)
        {
            this.GameObject = null;
            this.Label = label;
            this.Description = String.Empty;
            this.Key = key;
        }


        private Object go;

        public Object GameObject
        {
            get { return go; }
            set { go = value; }
        }

        private string label;

        public string Label
        {
            get { return label; }
            set { label = value; }
        }

        private string description;

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        private string key;

        public string Key
        {
            get { return key; }
            set { key = value; }
        }

        private bool flagged;

        public bool Flagged
        {
            get { return flagged; }
            set { flagged = value; }
        }
    }
}
