using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GameLibrary
{
    public class Clock
    {
        DateTime gametime = new DateTime(1, 1, 1);

        public Clock()
        {
            
        }
        
        public void AdvanceGameClock()
        {
            for (; ; )
            {
                // sleep 1/10th of a second
                Thread.Sleep(100);
                // 24 hours in game is 1 hour realtime
                gametime=gametime.AddSeconds(2.4);
            }

        }

      

        private DateTime game;

        public DateTime GameTime
        {
            get { return gametime; }
            
        }
	


    }

}
