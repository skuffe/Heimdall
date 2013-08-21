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
                                try { CollectFromClient(row["IPAddress"].ToString(), "2200"); }
                                catch { /*Set as not responding*/}
                            }
                            else if (types[row["ClientTypeID"].ToString()] == "SNMPdevice")
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
                sqlConn.executeQuery("SELECT * FROM tbl_Clients");
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
                sqlConn.executeQuery("SELECT * FROM tbl_ClientTypes");
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

        private void CollectFromClient(string ipAdr, string port)
        {
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://" + ipAdr + ":" + port + "/DataCollectionService"));
            NetTcpBinding binding = new NetTcpBinding();

            ChannelFactory<IDataCollectionService> factory =
            new ChannelFactory<IDataCollectionService>(binding, address);

            collectorService = factory.CreateChannel();

            sysInfo = new SystemInfoObject();

            sysInfo = collectorService.GetSystemInfo(sysInfo, ipAdr);
                
            //Store in DB
            EventLog.WriteEntry("CollectorThread", sysInfo.Cpu.ToString() + " DEBUG");
        }

        private void CollectFromSNMP(string ipAdr)
        {

            //request snmp trap from client
            //listen for response
            //process data
        }
    }
}
