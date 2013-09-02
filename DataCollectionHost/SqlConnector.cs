using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.IO;

namespace DataCollectionHost
{
    class SqlConnector
    {
        #region Properties

        private string host;
        private string database;
        private string user;
        private string pass;
        private string query;
        private string[] queries;

        private SqlConnection db_conn;
        private SqlDataAdapter reader;
        private DataTable data;

        #endregion
        #region Set_Statements

        public void setHost(string host)
        {
            this.host = host;
        }

        public void setDatabase(string database)
        {
            this.database = database;
        }

        public void setUser(string user)
        {
            this.user = user;
        }

        public void setPass(string pass)
        {
            this.pass = pass;
        }

        public void setQuery(string query)
        {
            this.query = query;
        }

        public void setConnectionProperties(string host, string database, string user, string pass)
        {
            this.host = host;
            this.database = database;
            this.user = user;
            this.pass = pass;
        }

        public void setQueries(string[] queries)
        {
            if (queries != null)
            {
                this.queries = new string[queries.Length];
                this.queries = queries;
            }
        }
        #endregion
        #region Get_Statements

        public string getHost()
        {
            return this.host;
        }

        public string getDatabase()
        {
            return this.database;
        }

        public string getUser()
        {
            return this.user;
        }

        public DataTable getData()
        {
            return this.data;
        }

        public string[] getQueries()
        {
            return this.queries;
        }
        #endregion
        #region Methods

        public bool openConnection()
        {
            //Creates a connection string based on the associated properties - WinAuth is also supported.
            string conString;
            if (pass == "Integrated" || user == "Integrated")
                conString = "server=" + host + ";" + "database=" + database + ";" + "Integrated Security=SSPI;";
            else
                conString = "user id=" + user + ";" + "password=" + pass + ";" + "server=" + host + ";" + "database=" + database + ";";
            //Tries to open a connection, and stores it if one is successfully established.   
            try
            {
                this.db_conn = new SqlConnection(conString);
                this.db_conn.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool executeQuery()
        {
            //Executes a query against the associated connection.
            try
            {
                data = new DataTable();
                reader = new SqlDataAdapter(this.query, this.db_conn);
                reader.Fill(data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool executeGetQuery(string query)
        {
            //Executes a specified query against the associated connection.
            try
            {
                data = new DataTable();
                reader = new SqlDataAdapter(query, this.db_conn);
                reader.Fill(data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool executeInsertQuery(string query, Dictionary<string, string> dictionary)
        {
            //Executes a specified query against the associated connection. Uses Parameters to protect against SQL Injections.
            try
            {
                DateTime time;
                SqlCommand cmd = new SqlCommand();

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = db_conn;

                for (int i = 0; i < dictionary.Count; i++)
                {
                    if (DateTime.TryParse(dictionary["@" + i], out time))
                        cmd.Parameters.AddWithValue("@" + i, time);
                    else
                        cmd.Parameters.AddWithValue("@" + i, dictionary["@" + i]);
                }

                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public bool executeUpdateQuery(string query)
        {
            //Executes a specified query against the associated connection. Uses Parameters to protect against SQL Injections.
            try
            {
                DateTime time;
                SqlCommand cmd = new SqlCommand();

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = db_conn;
                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public bool closeConnection()
        {
            //Tries to close the associated connection.
            try
            {
                this.db_conn.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
