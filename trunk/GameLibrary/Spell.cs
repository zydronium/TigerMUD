using System;
using System.Collections;
using System.Text;

namespace GameLibrary
{
    /// <summary>
    /// Represents a group of effects that one player can put on another called a spell.
    /// </summary>
    public struct Spell
    {
        private string nameShort;

        public string NameShort
        {
            get { return nameShort; }
            set { nameShort = value; }
        }
        private string nameDisplay;

        public string NameDisplay
        {
            get { return nameDisplay; }
            set { nameDisplay = value; }
        }
        private ArrayList effects;

        public ArrayList Effects
        {
            get { return effects; }
            set { effects = value; }
        }
        
    }
}
