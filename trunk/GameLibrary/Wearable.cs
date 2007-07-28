using System;
using System.Collections;
using System.Text;

namespace GameLibrary
{
    /// <summary>
    /// Represents a game object that can be worn on a player/npc's body wearslots.
    /// </summary>
    public class Wearable : GameLibrary.GameObject
    {
       

        private Object owner;

        public Object Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        private GameLibrary.Wearslot wearslot;

        public GameLibrary.Wearslot Wearslot
        {
            get { return wearslot; }
            set { wearslot = value; }
        }

        private int monetaryvalue;

        public int MonetaryValue
        {
            get { return monetaryvalue; }
            set { monetaryvalue = value; }
        }

    }
}
