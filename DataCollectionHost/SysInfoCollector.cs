using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataCollectionClient;
using System.ServiceModel;
using System.Diagnostics;
using System.Threading;
using SnmpSharpNet;
using System.Data;

namespace DataCollectionHost
{
    class SysInfoCollector
    {
        IDataCollectionService collectorService = null;
        SystemInfoObject sysInfo = null;


        DataTable collectedData = new DataTable();

        ConfigReader confReader;
        SqlConnector sqlConn;

        public SysInfoCollector(SqlConnector sqlConn)
        {
            //confReader = new ConfigReader("path");
            this.sqlConn = sqlConn;
        }

        public void ThreadWrapper()
        {
            DataTable clients = null;
            Dictionary<string, string> types = null;

            while (true)
            {
                //Do all the stuff!ghj
                //Extract from DB - clients
                //Extract from DB - types
                clients = GetClients();
                if (clients != null)
                {
                    types = GetTypes();
                    if (types != null)
                    {
                        foreach (DataRow row in clients.Rows)
                        {
                            if (types[row["ClientTypeID"].ToString()] == "Client")
                            {
                                try { CollectFromClient(row["IPAddress"].ToString(), "2200",row["ClientID"].ToString()); }
                                catch { setIsNotResponding(row["ClientID"].ToString(),row["IPAddress"].ToString()); }
                            }
                            else if (types[row["ClientTypeID"].ToString()] == "SNMPdevice") //Change to check whether the IsSnmpDevice flag in the DB is set instead.
                            {
                                try { CollectFromSNMP(row["IPAddress"].ToString()); }
                                catch { /*Set as not responding*/ }
                            }
                        }
                    }
                }
                Thread.Sleep(60000);
            }
        }

        //Connect to DB to get list of clients
        private DataTable GetClients()
        {
            sqlConn.openConnection();

            try
            {
                sqlConn.executeGetQuery("SELECT * FROM tbl_Clients");
                return sqlConn.getData();
            }
            catch{ return null; }
            finally{ sqlConn.closeConnection(); }
            
        }

        //Connect to DB to get list of clienttypes
        private Dictionary<string, string> GetTypes()
        {
            DataTable temp;
            Dictionary<string, string> dictionary = new Dictionary<string,string>();

            sqlConn.openConnection();

            try
            {
                sqlConn.executeGetQuery("SELECT * FROM tbl_ClientTypes");
                temp = sqlConn.getData();

                foreach (DataRow row in temp.Rows)
                {
                    dictionary.Add(row["ClientTypeID"].ToString(), row["TypeName"].ToString());
                }
                return dictionary;
            }
            catch{ return null; }
            finally{ sqlConn.closeConnection(); }
        }

        //Handles insertion to DB, when clients are not responding
        private void setIsNotResponding(string clientId, string ipAdr)
        {

            Dictionary<string, string> dictionaryForInsert = new Dictionary<string, string>()
            {
                {"@0",clientId},{"@1",DateTime.Now.ToString()},{"@2","0"}
            };

            //Store in DB
            sqlConn.openConnection();
            sqlConn.executeInsertQuery("INSERT INTO tbl_ClientInfo(ClientID, TimeStamp, IsResponding) VALUES(@0,@1,@2);", dictionaryForInsert);
            sqlConn.closeConnection();
        }

        //Collects data from the client and inserts it into the DB in a parameterized fashion to avoid SQL-injections
        private void CollectFromClient(string ipAdr, string port, string clientId)
        {
            //Setup the end-point to the client to collect system data based on the IDataCollectionService
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://" + ipAdr + ":" + port + "/DataCollectionService"));
            NetTcpBinding binding = new NetTcpBinding();

            ChannelFactory<IDataCollectionService> factory =
            new ChannelFactory<IDataCollectionService>(binding, address);

            collectorService = factory.CreateChannel();

            sysInfo = new SystemInfoObject();
            sysInfo = collectorService.GetSystemInfo(sysInfo, ipAdr);


            //Convert Diskspace to a format fitting the DB
            string diskSpace = "";
            foreach (string str in sysInfo.DiskSpace)
            {
                diskSpace += str + ";";
            }

            //Creates the dictionary used by the executeInsertQuery method - which ensures parameterized insertion to the DB
            Dictionary<string, string> dictionaryForInsert = new Dictionary<string, string>()
            {
                {"@0",clientId},{"@1",DateTime.Now.ToString()},{"@2",sysInfo.OsVersion},{"@3",diskSpace},{"@4",sysInfo.Uptime},{"@5",sysInfo.Ram},{"@6",sysInfo.Cpu},{"@7",sysInfo.PingToServer},{"@8","1"}
            };

            //Store in DB
            sqlConn.openConnection();
            sqlConn.executeInsertQuery("INSERT INTO tbl_ClientInfo(ClientID, TimeStamp, OSVersion, DiskSpace, UpTime, RAM, CPU, Ping, IsResponding) VALUES(@0,@1,@2,@3,@4,@5,@6,@7,@8);", dictionaryForInsert);
            sqlConn.closeConnection();
        }

        private void CollectFromSNMP(string ipAdr)
        {

            //request snmp trap from client
            //listen for response
            //process data
        }
    }
}
