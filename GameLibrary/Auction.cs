using System;
using System.Collections;
using System.Text;

namespace GameLibrary
{
    /// <summary>
    /// Represents an item listed for sale at an auction.
    /// </summary>
    public struct Auction
    {
        private Item item;

        public Item Item
        {
            get { return item; }
            set { item = value; }
        }
       
        private int bidminimum;

        public int Bidminimum
        {
            get { return bidminimum; }
            set { bidminimum = value; }
        }

        private int bidcurrent;

        public int Bidcurrent
        {
            get { return bidcurrent; }
            set { bidcurrent = value; }
        }

        private int bidbuyout;

        public int Bidbuyout
        {
            get { return bidbuyout; }
            set { bidbuyout = value; }
        }

        private TimeSpan duration;

        public TimeSpan Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        private DateTime begin;

        public DateTime Begin
        {
            get { return begin; }
            set { begin = value; }
        }

        private DateTime end;

        public DateTime End
        {
            get { return end; }
            set { end = value; }
        }

        private PlayerCharacter highbidder;

        public PlayerCharacter Highbidder
        {
            get { return highbidder; }
            set { highbidder = value; }
        }

        
    }
}
