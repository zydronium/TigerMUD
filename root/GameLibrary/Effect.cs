using System;
using System.Collections;
using System.Text;

namespace GameLibrary
{
    /// <summary>
    /// Represents a spell/skill effect that may be on a player or other game object.
    /// </summary>
    public struct Effect
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
        private Guid creator;

        public Guid Creator
        {
            get { return creator; }
            set { creator = value; }
        }
        private ArrayList statModifiers;

        public ArrayList StatModifiers
        {
            get { return statModifiers; }
            set { statModifiers = value; }
        }
    }
}
