using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography; // Needed for a descent random number generator.

namespace GameLibrary
{

    // This uses the crypto service provider to pump better random seeds into System.Random to avoid 
    // repeating number series.
    public class Randomizer
    {
        byte[] randbyte;
        RNGCryptoServiceProvider rng;
        Random rand;

        public Randomizer()
        {
            randbyte = new byte[1];
            rng = new RNGCryptoServiceProvider();
            
        }
        
        public int GetRandomNumber(int min, int max)
        {
            // bump max 1 since it is not included in the range from Random.Next, but min is.
            max++;
            rng.GetBytes(randbyte);
            rand = new Random(Convert.ToInt32(randbyte[0]));
            return rand.Next(min, max);
        }
    }

}
