using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Specialized;
using System.Net.Mail;

namespace GameLibrary
{
    /// <summary>
    /// Represents a set of database functions.
    /// </summary>
    public class Database : IDisposable
    {
        SqlConnection sqlcon;
        SqlCommand sqlcomm;
        SqlDataAdapter adapter;
        DataSet dataset;
        string connectionstring;
        string datasource;
        string database;
        string security;
        string timeout;

        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free other state (managed objects).
                sqlcon.Close();
                sqlcon.Dispose();
                sqlcomm.Dispose();
                adapter.Dispose();
                dataset.Dispose();
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.
        }

        // Use C# destructor syntax for finalization code.
        ~Database()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }


        // Send mail with cc list
        public bool SendMail(string server, string username, string password, string mailto, string mailfrom, string mailbcc, string mailsubject, string mailbody, params string[] mailcc)
        {
            MailMessage message = null;
            try
            {
                message = new MailMessage(mailfrom, mailto, mailsubject, mailbody);
            }
            catch
            {
                return false;
            }

            if (mailcc != null)
            {
                foreach (string cc in mailcc)
                    message.CC.Add(cc);
            }
            if (mailbcc != null) message.Bcc.Add(mailbcc);
            
            SmtpClient client = new SmtpClient(server);
            System.Net.NetworkCredential creds = new System.Net.NetworkCredential(username, password);
            client.Credentials = creds;
            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return false;
            }

            return true;

        }

        // Send mail without cc list
        public bool SendMail(string server, string username, string password, string mailto, string mailfrom, string mailbcc, string mailsubject, string mailbody)
        {
            return SendMail(server, username, password, mailto, mailfrom, mailbcc,mailsubject, mailbody, null);
        }

        public Database()
        {
            Console.Write("Attempting to open database...");
            try
            {
                sqlcon = new SqlConnection(ConnectionString);
                sqlcon.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("FAILED.\r\n" + ex.Message);
                Console.ReadLine();
                return;
            }
            Console.WriteLine("SUCCESS!");
        }

        public string ConnectionString
        {
            get
            {
                NameValueCollection appSettings = System.Configuration.ConfigurationManager.AppSettings;
                datasource = Convert.ToString(appSettings["Data Source"]);
                database = Convert.ToString(appSettings["Database"]);
                security = Convert.ToString(appSettings["Integrated Security"]);
                timeout = Convert.ToString(appSettings["Connection Timeout"]);
                connectionstring = "Data Source=" + datasource + ";Database=" + database + ";Integrated Security=" + security + ";Connection Timeout=" + timeout + ";";
                return connectionstring;
            }
        }

        /// <summary>
        /// Return accountid if credentials validate
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string ValidatePassword(string username, string password)
        {
            string accountid = string.Empty;
            sqlcomm = new SqlCommand("select id from dbo.accounts where nameshort=@givenusername and password=@givenpassword", sqlcon);
            sqlcomm.Parameters.Add(new SqlParameter("@givenusername", username));
            sqlcomm.Parameters.Add(new SqlParameter("@givenpassword", password));


            adapter = new SqlDataAdapter(sqlcomm);
            dataset = new DataSet();
            adapter.Fill(dataset);
            // Repeat for each table in the DataSet collection.
            foreach (DataTable table in dataset.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    accountid = Convert.ToString(row["id"]);
                }
            }
            return accountid;
        }


        public ArrayList GetCharacters(string accountid)
        {
            if (accountid == null) return null;
            ArrayList characters = new ArrayList();
            CharacterList cl = new CharacterList();
            sqlcomm = new SqlCommand("select id,namefirst,namelast,level,class from dbo.characters where accountid=@accountid", sqlcon);
            sqlcomm.Parameters.Add(new SqlParameter("@accountid", accountid));
            //sqlcon.Open();
            adapter = new SqlDataAdapter(sqlcomm);
            dataset = new DataSet();
            adapter.Fill(dataset);
            // Repeat for each table in the DataSet collection.
            foreach (DataTable table in dataset.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    cl.Id = Convert.ToString(row["id"]);
                    cl.NameLast = Convert.ToString(row["namelast"]);
                    cl.NameFirst = Convert.ToString(row["namefirst"]);
                    cl.Level = Convert.ToInt32(row["level"]);
                    cl.CharacterClass = Convert.ToString(row["class"]);
                    characters.Add(cl);
                }
            }
            return characters;

        }



        public bool CreateCharacter(GameObject pa, string namefirst, string namelast)
        {
            if (pa == null) throw new ArgumentNullException("pa");


            sqlcomm = new SqlCommand("insert into dbo.characters (id,accountid,namefirst,namelast,class,level,action,health,mental,agility,strength,intellect,spirit,stamina,reputationtogive,reputationreceived) values (@id,@accountid,@namefirst,@namelast,@class,@level,@action,@health,@mental,@agility,@strength,@intellect,@spirit,@stamina,@reputationtogive,@reputationreceived)", sqlcon);
            sqlcomm.Parameters.Add(new SqlParameter("@id", Guid.NewGuid().ToString()));
            sqlcomm.Parameters.Add(new SqlParameter("@accountid", pa.Id));
            sqlcomm.Parameters.Add(new SqlParameter("@namefirst", namefirst));
            sqlcomm.Parameters.Add(new SqlParameter("@namelast", namelast));
            sqlcomm.Parameters.Add(new SqlParameter("@class", "none"));
            sqlcomm.Parameters.Add(new SqlParameter("@level", 1));
            sqlcomm.Parameters.Add(new SqlParameter("@action", 1));
            sqlcomm.Parameters.Add(new SqlParameter("@health", 1));
            sqlcomm.Parameters.Add(new SqlParameter("@mental", 1));
            sqlcomm.Parameters.Add(new SqlParameter("@agility", 1));
            sqlcomm.Parameters.Add(new SqlParameter("@strength", 1));
            sqlcomm.Parameters.Add(new SqlParameter("@intellect", 1));
            sqlcomm.Parameters.Add(new SqlParameter("@spirit", 1));
            sqlcomm.Parameters.Add(new SqlParameter("@stamina", 1));
            sqlcomm.Parameters.Add(new SqlParameter("@reputationtogive", 1));
            sqlcomm.Parameters.Add(new SqlParameter("@reputationreceived", 1));
            sqlcomm.ExecuteNonQuery();
            return true;
        }

        public PlayerAccount CreateAccount(string accountname, string email, string password)
        {
            PlayerAccount pa = new PlayerAccount();
            pa.Id = Guid.NewGuid().ToString();
            pa.NameShort = accountname;
            pa.NameDisplay = accountname;
            pa.Email = email;
            pa.Password = password;

            sqlcomm = new SqlCommand("insert into dbo.accounts (changed,id,namedisplay,nameshort,banned,connected,datecreated,datelastlogin,email,password,validated) values (@changed,@id,@namedisplay,@nameshort,@banned,@connected,@datecreated,@datelastlogin,@email,@password,@validated)", sqlcon);
            sqlcomm.Parameters.Add(new SqlParameter("@changed", false));
            sqlcomm.Parameters.Add(new SqlParameter("@id", Guid.NewGuid().ToString()));
            sqlcomm.Parameters.Add(new SqlParameter("@namedisplay", pa.NameDisplay));
            sqlcomm.Parameters.Add(new SqlParameter("@nameshort", pa.NameShort));
            sqlcomm.Parameters.Add(new SqlParameter("@banned", false));
            sqlcomm.Parameters.Add(new SqlParameter("@connected", true));
            sqlcomm.Parameters.Add(new SqlParameter("@datecreated", DateTime.Now));
            sqlcomm.Parameters.Add(new SqlParameter("@datelastlogin", DateTime.Now));
            sqlcomm.Parameters.Add(new SqlParameter("@email", email));
            sqlcomm.Parameters.Add(new SqlParameter("@password", password));
            sqlcomm.Parameters.Add(new SqlParameter("@validated", true));
            sqlcomm.ExecuteNonQuery();
            return pa;
        }

     


        public PlayerCharacter LoadCharacter(PlayerCharacter pc, string characterid)
        {
            if (pc == null) throw new ArgumentNullException("pc");

            sqlcomm = new SqlCommand("select * from dbo.characters where id=@characterid", sqlcon);
            sqlcomm.Parameters.Add(new SqlParameter("@characterid", characterid));
            //sqlcon.Open();
            adapter = new SqlDataAdapter(sqlcomm);
            dataset = new DataSet();
            adapter.Fill(dataset);
            // Repeat for each table in the DataSet collection.
            foreach (DataTable table in dataset.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    pc.Id = row["id"].ToString();
                    pc.Action = Convert.ToInt32(row["action"]);
                    pc.Health = Convert.ToInt32(row["health"]);
                    pc.Mental = Convert.ToInt32(row["mental"]);
                    pc.Agility = Convert.ToInt32(row["agility"]);
                    pc.Strength = Convert.ToInt32(row["strength"]);
                    pc.Intellect = Convert.ToInt32(row["intellect"]);
                    pc.Spirit = Convert.ToInt32(row["spirit"]);
                    pc.Stamina = Convert.ToInt32(row["stamina"]);
                    pc.ReputationToGive = Convert.ToInt32(row["reputationtogive"]);
                    pc.ReputationReceived = Convert.ToInt32(row["reputationreceived"]);
                    pc.NameLast = Convert.ToString(row["namelast"]);
                    pc.NameFirst = Convert.ToString(row["namefirst"]);
                    pc.Level = Convert.ToInt32(row["level"]);
                    pc.CharacterClass = Convert.ToString(row["class"]);
                    pc.AccountId = Convert.ToString(row["accountid"]);
                    pc.PlanetID = Convert.ToString(row["planetid"]);
                    pc.InWilderness = Convert.ToBoolean(row["inwilderness"]);

                }
            }
            return pc;

        }

        public ArrayList GetRoomIds()
        {
            ArrayList roomids = new ArrayList();
            sqlcomm = new SqlCommand("select id from dbo.rooms", sqlcon);
            adapter = new SqlDataAdapter(sqlcomm);
            dataset = new DataSet();
            adapter.Fill(dataset);
            // Repeat for each table in the DataSet collection.
            foreach (DataTable table in dataset.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    roomids.Add(Convert.ToString(row["id"]));
                }
            }
            return roomids;

        }

        public ArrayList GetItemIds()
        {
            ArrayList itemids = new ArrayList();
            sqlcomm = new SqlCommand("select id from dbo.items", sqlcon);
            adapter = new SqlDataAdapter(sqlcomm);
            dataset = new DataSet();
            adapter.Fill(dataset);
            // Repeat for each table in the DataSet collection.
            foreach (DataTable table in dataset.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    itemids.Add(Convert.ToString(row["id"]));
                }
            }
            return itemids;

        }

        public ArrayList GetExitIds()
        {
            ArrayList exitids = new ArrayList();
            sqlcomm = new SqlCommand("select id from dbo.exits", sqlcon);
            adapter = new SqlDataAdapter(sqlcomm);
            dataset = new DataSet();
            adapter.Fill(dataset);
            // Repeat for each table in the DataSet collection.
            foreach (DataTable table in dataset.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    exitids.Add(Convert.ToString(row["id"]));
                }
            }
            return exitids;

        }

        public ArrayList GetKeyIds()
        {
            ArrayList keyids = new ArrayList();
            sqlcomm = new SqlCommand("select id from dbo.keys", sqlcon);
            adapter = new SqlDataAdapter(sqlcomm);
            dataset = new DataSet();
            adapter.Fill(dataset);
            // Repeat for each table in the DataSet collection.
            foreach (DataTable table in dataset.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    keyids.Add(Convert.ToString(row["id"]));
                }
            }
            return keyids;

        }

        public Room LoadRoom(string roomid)
        {
            Room room = new Room();
            sqlcomm = new SqlCommand("select * from dbo.rooms where id=@roomid", sqlcon);
            sqlcomm.Parameters.Add(new SqlParameter("@roomid", roomid));
            adapter = new SqlDataAdapter(sqlcomm);
            dataset = new DataSet();
            adapter.Fill(dataset);

             // Repeat for each table in the DataSet collection.
            foreach (DataTable table in dataset.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    room.Id = Convert.ToString(row["id"]);
                    room.Description = Convert.ToString(row["description"]);
                    room.NameDisplay = Convert.ToString(row["namedisplay"]);
                    
                    //room.Planet = Planet.GetFromID(Convert.ToString(row["planetid"]));
                    //room.RoomType = (RoomType)Enum.ToObject(typeof(RoomType),Convert.ToInt32(row["roomtype"]));
                    room.Indoor = Convert.ToBoolean(row["indoor"]);
                    room.SkyVisible = Convert.ToBoolean(row["skyvisible"]);
                    
                }

            }
            return room;
        }


        public Item LoadItem(string itemid)
        {
            Item item = new Item();
            sqlcomm = new SqlCommand("select * from dbo.items where id=@itemid", sqlcon);
            sqlcomm.Parameters.Add(new SqlParameter("@itemid", itemid));
            adapter = new SqlDataAdapter(sqlcomm);
            dataset = new DataSet();
            adapter.Fill(dataset);

            // Repeat for each table in the DataSet collection.
            foreach (DataTable table in dataset.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    item.Id = Convert.ToString(row["id"]);
                    item.NameShort = Convert.ToString(row["nameshort"]);
                    item.NameDisplay = Convert.ToString(row["namedisplay"]);
                    item.Description = Convert.ToString(row["description"]);
                    
                    item.Quantity = Convert.ToInt32(row["quantity"]);
                    item.Stackable = Convert.ToBoolean(row["stackable"]);
                    item.X = Convert.ToInt32(row["x"]);
                    item.Y = Convert.ToInt32(row["y"]);
                    item.Locked = Convert.ToBoolean(row["locked"]);
                    item.Movable = Convert.ToBoolean(row["movable"]);
                    item.MonetaryValue = Convert.ToInt32(row["monetaryvalue"]);

                }

            }
            return item;
        }

        public Exit LoadExit(string exitid, GameContext gc)
        {
            Exit exit = new Exit();
            sqlcomm = new SqlCommand("select * from dbo.exits where id=@exitid", sqlcon);
            sqlcomm.Parameters.Add(new SqlParameter("@exitid", exitid));
            adapter = new SqlDataAdapter(sqlcomm);
            dataset = new DataSet();
            adapter.Fill(dataset);

            string destinationroomid = string.Empty;
            // Repeat for each table in the DataSet collection.
            foreach (DataTable table in dataset.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    exit.Id = Convert.ToString(row["id"]);
                    destinationroomid = Convert.ToString(row["destinationroomid"]);
                    exit.DestinationRoom = gc.GetRoom(destinationroomid);
                    exit.Hidden = Convert.ToBoolean(row["hidden"]);
                    exit.Magic = Convert.ToBoolean(row["magic"]);
                    exit.Locked = Convert.ToBoolean(row["locked"]);


                }

            }
            return exit;
        }

        public Key LoadKey(string keyid, GameContext gc)
        {
            Key key = new Key();
            sqlcomm = new SqlCommand("select * from dbo.keys where id=@keyid", sqlcon);
            sqlcomm.Parameters.Add(new SqlParameter("@keyid", keyid));
            adapter = new SqlDataAdapter(sqlcomm);
            dataset = new DataSet();
            adapter.Fill(dataset);

            // Repeat for each table in the DataSet collection.
            foreach (DataTable table in dataset.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    key.Id = Convert.ToString(row["id"]);

                    key.NameDisplay=Convert.ToString(row["namedisplay"]);
                    key.Description=Convert.ToString(row["description"]);
                }

            }
            return key;
        }

        public bool LoadRoomsExitsRelationships(Room room, GameContext gc)
        {
            sqlcomm = new SqlCommand("select * from dbo.roomsexits where roomid=@roomid", sqlcon);
            sqlcomm.Parameters.Add(new SqlParameter("@roomid", room.Id));
            adapter = new SqlDataAdapter(sqlcomm);
            dataset = new DataSet();
            adapter.Fill(dataset);

            // Repeat for each table in the DataSet collection.
            foreach (DataTable table in dataset.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    // Convert exit id to real exit
                    Exit exit=gc.GetExit(Convert.ToString(row["exitid"]));
                    room.AddExit(Convert.ToString(row["direction"]), exit);
                    room.AddExit(Convert.ToString(row["direction"]).Substring(0,1), exit);

                }

            }
            return false;
        }

        public bool LoadRoomsItemsRelationships(Room room, GameContext gc)
        {
            sqlcomm = new SqlCommand("select * from dbo.roomsitems where roomid=@roomid", sqlcon);
            sqlcomm.Parameters.Add(new SqlParameter("@roomid", room.Id));
            adapter = new SqlDataAdapter(sqlcomm);
            dataset = new DataSet();
            adapter.Fill(dataset);

            // Repeat for each table in the DataSet collection.
            foreach (DataTable table in dataset.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    // Convert exit id to real exit
                    Item item = gc.GetItem(Convert.ToString(row["itemid"]));
                    room.AddItem(item.NameShort, item);
                    room.AddItem(item.NameDisplay, item);

                }

            }
            return false;
        }

        public bool LoadKeysExitsRelationships(Key key, GameContext gc)
        {
            sqlcomm = new SqlCommand("select * from dbo.keysexits where keyid=@keyid", sqlcon);
            sqlcomm.Parameters.Add(new SqlParameter("@keyid", key.Id));
            adapter = new SqlDataAdapter(sqlcomm);
            dataset = new DataSet();
            adapter.Fill(dataset);

            // Repeat for each table in the DataSet collection.
            foreach (DataTable table in dataset.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    // Convert exit id to real exit
                    Exit exit = gc.GetExit(Convert.ToString(row["exitid"]));
                    exit.AddKey(gc.GetKey(Convert.ToString(row["keyid"])));
                }

            }
            return false;
        }



        public PlayerAccount LoadAccount(string accountid)
        {
            PlayerAccount pa = new PlayerAccount();
            sqlcomm = new SqlCommand("select * from dbo.accounts where id=@accountid", sqlcon);
            sqlcomm.Parameters.Add(new SqlParameter("@accountid", accountid));
            //sqlcon.Open();
            adapter = new SqlDataAdapter(sqlcomm);
            dataset = new DataSet();
            adapter.Fill(dataset);
            // Repeat for each table in the DataSet collection.
            foreach (DataTable table in dataset.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    pa.Banned = Convert.ToBoolean(row["banned"]);
                    pa.Connected = Convert.ToBoolean(row["connected"]);
                    //pa.Container = Convert.ToString(row["container"]);
                    pa.DateCreated = Convert.ToDateTime(row["datecreated"]);
                    pa.DateLastLogin = Convert.ToDateTime(row["datelastlogin"]);
                    pa.Email = Convert.ToString(row["email"]);
                    pa.Id = Convert.ToString(row["id"]);
                    pa.Locked = Convert.ToBoolean(row["locked"]);
                    pa.NameDisplay = Convert.ToString(row["namedisplay"]);
                    pa.NameShort = Convert.ToString(row["nameshort"]);
                    pa.Password = Convert.ToString(row["password"]);
                    
                }
            }
            return pa;
        }

        public bool WriteLogEntry(DateTime datetime,string ip, string charactername,string type,string text)
        {
            sqlcomm = new SqlCommand("insert into dbo.log (datetime,ip,charactername,type,text) values (@datetime,@ip,@namedisplay,@nameshort,@banned,@connected,@datecreated,@datelastlogin,@email,@password,@validated)", sqlcon);
            sqlcomm.Parameters.Add(new SqlParameter("@datetime", DateTime.Now));
            sqlcomm.Parameters.Add(new SqlParameter("@ip", ip));
            sqlcomm.Parameters.Add(new SqlParameter("@charactername", charactername));
            sqlcomm.Parameters.Add(new SqlParameter("@type", type));
            sqlcomm.Parameters.Add(new SqlParameter("@text", text));
            sqlcomm.ExecuteNonQuery();
            return false;
        }


    }
}
