using System;
using System.Collections;
using System.Text;

namespace GameLibrary 
{
    /// <summary>
    /// Represents a particular market for items where they can be auctioned.
    /// </summary>
    public class Market : GameLibrary.GameObject
    {
        private int fee;

        public int Fee
        {
            get { return fee; }
            set { fee = value; }
        }

        private TimeSpan maxauctionlength;

        public TimeSpan MaxAuctionLength
        {
            get { return maxauctionlength; }
            set { maxauctionlength = value; }
        }

        private int bidminimum;

        public int BidMinimum
        {
            get { return bidminimum; }
            set { bidminimum = value; }
        }

        private ArrayList auctions;

        
	
	
	
    }
}
