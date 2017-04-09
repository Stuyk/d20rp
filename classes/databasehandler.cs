using GTANetworkServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Data;

namespace d20rp.classes
{
    public class databasehandler : Script
    {
        // These will match our opening query.
        // They're what we will use to pull player information down from the database.
        public enum PlayerInfo
        {
            Id,
            Username,
            Password,
            Name,
            Strength,
            Dexterity,
            Charisma,
            Health,
            Armor
        }

        // Path To Our Database / Connection String
        static string path = "resources/d20rp/database/database.db";
        static string connString = string.Format("Data Source={0};Version=3", path);

        // Make sure this matches the PlayerInfo enum.
        static string openingQuery = @"CREATE TABLE IF NOT EXISTS
                            [Players] (
                            [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            [Username] NVARCHAR(2048) NULL,
                            [Password] NVARCHAR(2048) NULL,
                            [Name] NVARCHAR(2048) NULL,
                            [X] FLOAT,
                            [Y] FLOAT,
                            [Z] FLOAT,
                            [Strength] INTEGER,
                            [Dexterity] INTEGER,
                            [Charisma] INTEGER,
                            [Health] INTEGER,
                            [Armor] INTEGER)";

        // What happens when we start databasehandler resource.
        public databasehandler()
        {
            API.onResourceStart += API_onResourceStart;
            API.onResourceStop += API_onResourceStop;
        }

        private void API_onResourceStart()
        {
            API.consoleOutput("[SQLITE] Starting...");

            bool exists = File.Exists(path);
            if (!exists)
            {
                SQLiteConnection.CreateFile(path);
                API.consoleOutput("[SQLITE] Created Database File.");
            }
            else
            {
                API.consoleOutput("[SQLITE] Found existing database.");
            }

            // Create a connection.
            using (SQLiteConnection conn = new SQLiteConnection(connString))
            {
                // Try to open, and check if we can connect.
                try
                {
                    API.consoleOutput("[SQLITE] Attempting connection...");
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                        API.consoleOutput("[SQLITE] Connected Successfully");
                }
                catch (Exception ex)
                {
                    API.consoleOutput(string.Format("[SQLITE] [ERROR] {0}", ex));
                }
            }

            executeQuery(openingQuery);

            DataTable table = executeQueryWithResult("SELECT * FROM Players");
        }

        private void API_onResourceStop()
        {
            API.consoleOutput("[SQLITE] Connection closed.");
        }

        // Execute a single query for the database.
        public void executeQuery(string query)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connString))
            {
                try
                {
                    SQLiteCommand cmd = new SQLiteCommand(query, conn);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    API.consoleOutput(string.Format("[SQLITE] [ERROR] {0}", ex));
                }
            }
        }

        // Execute a single query for the database, but return a DataTable.
        public DataTable executeQueryWithResult(string query)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connString))
            {
                try
                {
                    SQLiteCommand cmd = new SQLiteCommand(query, conn);
                    conn.Open();
                    SQLiteDataReader reader = cmd.ExecuteReader();
                    DataTable results = new DataTable();
                    results.Load(reader);
                    reader.Close();
                    conn.Close();
                    return results;
                }
                catch (Exception ex)
                {
                    API.consoleOutput(string.Format("[SQLITE] [ERROR] {0}", ex));
                    return null;
                }
            }
        }

        // Execute a single prepared query for the database.
        public void executePreparedQuery(string query, Dictionary<string, string> parameters)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connString))
            {
                try
                {
                    SQLiteCommand cmd = new SQLiteCommand(query, conn);
                    conn.Open();
                    // Execute a foreach statement. Loops through each entry of the dictionary and add to our command.
                    foreach (KeyValuePair<string, string> entry in parameters)
                    {
                        cmd.Parameters.AddWithValue(entry.Key, entry.Value);
                    }
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    API.consoleOutput(string.Format("[SQLITE] [ERROR] {0}", ex));
                }
            }
        }

        public DataTable executePreparedQueryWithResult(string query, Dictionary<string, string> parameters)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connString))
            {
                try
                {
                    SQLiteCommand cmd = new SQLiteCommand(query, conn);
                    conn.Open();

                    foreach (KeyValuePair<string, string> entry in parameters)
                    {
                        cmd.Parameters.AddWithValue(entry.Key, entry.Value);
                    }

                    SQLiteDataReader rdr = cmd.ExecuteReader();
                    DataTable results = new DataTable();
                    results.Load(rdr);
                    rdr.Close();
                    return results;
                }
                catch (Exception ex)
                {
                    API.consoleOutput(string.Format("[SQLITE] [ERROR] {0}", ex));
                    return null;
                }
            }
        }

        /* Mr Booleans Magical Query Makers
        // EXAMPLE:
        // int playerID = 0;
        // string before = "UPDATE PlayerClothing SET";
        // string[] varNames = { "Username", "Password" };
        // string after = string.Format("WHERE Id='{0}'", playerID);
        // object[] args = { "Stuyk", "password" };
        // compileQuery(before, after, varNames, args);
        */
        public void compileQuery(string before, string after, string[] vars, object[] data)
        {
            int i = 0;
            string query;
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            query = string.Format("{0}", before);

            foreach (string label in vars)
            {
                if (i == vars.Length - 1)
                {
                    query = string.Format("{0} {1}=@{1}", query, label);
                }
                else
                {
                    query = string.Format("{0} {1}=@{1},", query, label);
                }

                parameters.Add(string.Format("@{0}", label), data[i].ToString());
                ++i;
            }

            //Add anything after the data formatting
            query = string.Format("{0} {1}", query, after);

            //Execute it
            executePreparedQuery(query, parameters);
        }

        /* EXAMPLE:
         * string[] varNamesTwo = { "Email", "Username", "Password", "SocialClub", "IP", "Health", "Armor", "RegisterDate" };
         * string tableName = "Players";
         * string[] dataTwo = { email, username, hash, player.socialClubName, player.address, "100", "0", date.ToString("yyyy-MM-dd HH:mm:ss") };
         * compileInsertQuery(tableName, varNamesTwo, dataTwo);
        */
        public void compileInsertQuery(string tableName, string[] vars, object[] data)
        {
            int i = 0;
            string query;
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            //Add the beginning of our query
            query = string.Format("INSERT INTO {0} (", tableName);

            //format and add our params
            foreach (string label in vars)
            {
                if (i == vars.Length - 1)
                {
                    query = string.Format("{0} {1}) VALUES (", query, label);
                }
                else
                {
                    query = string.Format("{0} {1},", query, label);
                }

                parameters.Add(string.Format("@{0}", label), data[i].ToString());
                ++i;
            }

            i = 0;
            foreach (string label in vars)
            {
                if (i == vars.Length - 1)
                {
                    query = string.Format("{0} @{1})", query, label);
                }
                else
                {
                    query = string.Format("{0} @{1},", query, label);
                }
                ++i;
            }

            executePreparedQuery(query, parameters);
        }

        // Compile Select Query
        public DataTable compileSelectQuery(string before, string[] vars, object[] data, string after = "")
        {
            int i = 0;
            string query;
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            //Add the beginning of our query
            query = string.Format("{0}", before);

            //format and add our params
            foreach (string label in vars)
            {
                if (i == vars.Length - 1)
                {
                    query = string.Format("{0} {1}=@{1}", query, label);
                }
                else
                {
                    query = string.Format("{0} {1}=@{1} AND", query, label);
                }

                parameters.Add(string.Format("@{0}", label), data[i].ToString());
                ++i;
            }

            //Add anything after the data formatting
            query = string.Format("{0} {1}", query, after);

            //Execute it
            return executePreparedQueryWithResult(query, parameters);
        }

    }
}
