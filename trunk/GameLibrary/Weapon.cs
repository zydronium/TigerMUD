using System;
using System.Collections;
using System.Text;

namespace GameLibrary
{
    /// <summary>
    /// Represents an object that can damage/modify the statstics of game objects by using a combat skill.
    /// </summary>
    public class Weapon : GameLibrary.GameObject
    {

        

        private Object owner;

        public Object Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        private GameLibrary.Weaponslot weaponslot;

        public GameLibrary.Weaponslot Weaponslot
        {
            get { return weaponslot; }
            set { weaponslot = value; }
        }

        private int monetaryvalue;

        public int MonetaryValue
        {
            get { return monetaryvalue; }
            set { monetaryvalue = value; }
        }

        private int damagemin;

        public int DamageMin
        {
            get { return damagemin; }
            set { damagemin = value; }
        }

        private int damagemax;

        public int DamageMax
        {
            get { return damagemax; }
            set { damagemax = value; }
        }

        private int damagecritical;

        public int DamageCritical
        {
            get { return damagecritical; }
            set { damagecritical = value; }
        }

        private int actioncost;

        public int ActionCost
        {
            get { return actioncost; }
            set { actioncost = value; }
        }

        private int mentalcost;

        public int MentalCost
        {
            get { return mentalcost; }
            set { mentalcost = value; }
        }
	

	
	

    }
}
