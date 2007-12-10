using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace TigerMUD
{
    public class MobReader : TigerXMLReader
    {
        public MobReader() : base() { }
        public MobReader(string Filename) : base(Filename) { }

        public override Boolean Load(string filename)
        {
            Boolean endMob = false;
            Actor newMob = new Actor();

            mReader = new XmlTextReader(filename);

            mReader.Read();
            while (!mReader.EOF)
            {

                if (mReader.NodeType == XmlNodeType.Element)
                {
                    if (mReader.Name.ToUpper().Equals("MOB"))
                    {
                        mReader.Read();
                        while (!endMob)
                        {
                            //read room data
                            switch (mReader.NodeType)
                            {
                                case XmlNodeType.Element: // The node is an element.

                                    // Find the ID of the container.
                                    if (mReader.Name.ToUpper().Equals("CONTAINER"))
                                    {
                                        mReader.Read(); //read text inside tags
                                        Actor a = Lib.GetByName(mReader.Value);
                                        if (a != null)
                                        {
                                            newMob["container"] = a["id"];
                                            newMob["containertype"] = a["type"];
                                        }
                                        break;
                                    }

                                    String stateName = mReader.Name;
                                    mReader.Read(); //read text inside tags
                                    newMob[stateName] = mReader.Value;
                                    break;
                                case XmlNodeType.EndElement:

                                    if (mReader.Name.ToUpper().Equals("MOB"))
                                        endMob = true;
                                    mReader.Read(); // read elements
                                    break;

                            }
                            mReader.Read();


                        }

                        //TODO: Verify minimum contents
                        // Add default fields if not existing
                        newMob["type"] = "mob";
                        newMob["target"] = "";
                        newMob["targettype"] = "";
                        newMob["played"] = 0;
                        newMob["equipable"] = false;
                        newMob["containertype"] = "room";
                        newMob["spellpending"] = false;
                        //newRoom["equippable"] = false;
                        //newRoom["shortnameupper"] = "";
                        //newRoom["combatactive"] = false;
                        //newmob["respawntimer"] = 0;
                        //newmob["health"] = 1;
                        //newmob["healthmax"] = 1;

                        lock (Lib.spells.SyncRoot)
                        {
                            Lib.actors.Add(newMob);
                        }

                        //prepare for next room
                        endMob = false;
                        newMob = new Actor();
                    }
                    else
                        mReader.Read();
                }
                else
                    mReader.Read();
            }

            return true;
        }
    }
}
