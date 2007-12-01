using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace TigerMUD
{
    public class ItemReader : TigerXMLReader
    {
        public ItemReader() : base() { }
        public ItemReader(string Filename) : base(Filename) { }

        public override Boolean Load(string filename)
        {
            Boolean endItem = false;
            Actor newItem = new Actor();

            mReader = new XmlTextReader(filename);
            
            mReader.Read();
            while (!mReader.EOF)
            {

                if (mReader.NodeType == XmlNodeType.Element)
                {
                    if (mReader.Name.ToUpper().Equals("ITEM"))
                    {
                        mReader.Read();
                        while (!endItem)
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
                                            newItem["container"] = a["id"];
                                            newItem["containertype"] = a["type"];
                                        }
                                        break;
                                    }

                                    String stateName = mReader.Name;
                                    mReader.Read(); //read text inside tags
                                    newItem[stateName] = mReader.Value;
                                    break;
                                case XmlNodeType.EndElement:

                                    if (mReader.Name.ToUpper().Equals("ITEM"))
                                        endItem = true;
                                    mReader.Read(); // read elements
                                    break;

                            }
                            mReader.Read();


                        }

                        //TODO: Verify minimum contents
                        // Add default fields if not existing
                        newItem["type"] = "item";
                        //newRoom["equippable"] = false;
                        //newRoom["shortnameupper"] = "";
                        //newRoom["combatactive"] = false;
                        newItem["respawntimer"] = 0;
                        newItem["health"] = 1;
                        newItem["healthmax"] = 1;

                        lock (Lib.spells.SyncRoot)
                        {
                            Lib.actors.Add(newItem);
                        }

                        //prepare for next room
                        endItem = false;
                        newItem = new Actor();
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
