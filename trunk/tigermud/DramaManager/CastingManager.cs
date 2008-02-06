using System;
using System.Collections;
using System.Text;

namespace TigerMUD
{
    public enum Role
    {
        HERO = 0,
        VILLAIN,
        FALSE_HERO,
        MEDIATOR,
        DONOR,
        HELPER,
        PRINCESS,
        KING,
        FAMILY
    }

    public enum RelationshipType
    { 
        HOSTILE = -1,
        NEUTRAL = 0,
        FRIEND
    }

    public class RoleAssignment
    {
        string StoryID;
        Actor actor;
        Role assignment;
    }

    public class Relationship
    { 
        public Actor actor1;
        public Actor actor2;
        public RelationshipType relation;

        public Relationship(Actor a1, Actor a2, RelationshipType r)
        {
            actor1 = a1;
            actor2 = a2;
            relation = r;
        }
    }

    class CastingManager
    {
        ArrayList roles;

        RelationshipType RetrieveRelationship(string char1, string char2)
        {
            return RelationshipType.NEUTRAL;
        }

        public Actor ScoutForTalents(Role role, Actor actor)
        { 
            //search for an actor using some criteria for finding appropriate actors

            //iterate through all acquaintances
            foreach (Relationship r in Lib.RelationshipList)
            {
                // prioritize all acquaintances of this object
                if(((string)r.actor1["name"]).Equals(actor["name"].ToString()))
                    if (r.relation < RelationshipType.NEUTRAL)
                    { 
                        //cast this
                        return r.actor2;
                    }
            }


            return null;
        }
    }

}
