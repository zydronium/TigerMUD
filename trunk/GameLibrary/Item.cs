using System;
using System.Collections;
using System.Text;

namespace GameLibrary
{
    /// <summary>
    /// Represents a game object that is not a player or NPC.
    /// </summary>
    public class Item : GameLibrary.GameObject
    {
        private bool movable;

        public bool Movable
        {
            get { return movable; }
            set { movable = value; }
        }
        
        private bool lockable;

        public bool Lockable
        {
            get { return lockable; }
            set { lockable = value; }
        }

        private bool locked;

        public bool Locked
        {
            get { return locked; }
            set { locked = value; }
        }

        private int monetaryvalue;

        public int MonetaryValue
        {
            get { return monetaryvalue; }
            set { monetaryvalue = value; }
        }

        private Object owner;

        public Object Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        private int quantity;

        public int Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }

        private bool stackable;

        public bool Stackable
        {
            get { return stackable; }
            set { stackable = value; }
        }

        ArrayList effects;

     
        private string room;

        public string Room
        {
            get { return room; }
            set { room = value; }
        }

        private int x;

        public int X
        {
            get { return x; }
            set { x = value; }
        }

        private int y;

        public int Y
        {
            get { return y; }
            set { y = value; }
        }

	
    }
}
