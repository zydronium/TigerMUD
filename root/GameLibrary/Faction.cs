using System;
using System.Collections.Generic;
using System.Text;

namespace GameLibrary
{
    /// <summary>
    /// Represents the state of friendliness/agression that exists between a certain NPC group and a player.
    /// </summary>
    public struct Faction
    {
        private string id;

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        private string namedisplay;

        public string NameDisplay
        {
            get { return namedisplay; }
            set { namedisplay = value; }
        }

        private int amount;

        public int Amount
        {
            get { return amount; }
            set { amount = value; }
        }
	

    }
}
