using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace GameLibrary
{
    class XMLTemplate
    {
        /// <summary>
        /// Read an XML template by name
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public string ReadTemplate(string filename, string templatename)
        {
            //Load the reader with the XML file.
            XmlTextReader reader = new XmlTextReader(filename);
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);
            reader.Close();

            // Database Information
            XmlNode xmlNode = doc.SelectSingleNode(@"/" + templatename);
            foreach (XmlNode currentnode in xmlNode.ChildNodes)
            {
                switch (xmlNode.Name)
                {
                    case "Type":
                        string DatabaseType = currentnode.InnerText;
                        break;
                    case "DBServer":
                        string DatabaseServer = currentnode.InnerText;
                        break;
                    case "UserID":
                        string DatabaseUserID = currentnode.InnerText;
                        break;
                    case "Password":
                        string DatabasePassword = currentnode.InnerText;
                        break;
                    case "DatabaseName":
                        string DatabaseName = currentnode.InnerText;
                        break;
                    case "ShowSQLStatements":
                        bool ShowSQLStatements = bool.Parse(currentnode.InnerText);
                        break;
                }
            }

            return "whatever";
        }
    }
}
