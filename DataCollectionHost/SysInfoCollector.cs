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
using System.Net;
using System.ServiceModel.Discovery;

namespace DataCollectionHost
{
    class SysInfoCollector
    {
        IDataCollectionService collectorService = null;
        SystemInfoObject sysInfo = null;


        DataTable collectedData = new DataTable();

        ConfigReader confReader;
        SqlConnector sqlConn;

        public SysInfoCollector(SqlConnector sqlConn, ConfigReader confReader)
        {
            this.confReader = confReader;
            this.sqlConn = sqlConn;
        }


        public void ThreadWrapper()
        {
            DataTable clients = null;
            Dictionary<string, bool> types = null;

            //Loop to collect systeminfo from the hosts.
            while (true)
            {
                clients = GetClientsFromDB();
                if (clients != null)
                {
                    types = GetTypes();
                    if (types != null)
                    {
                        foreach (DataRow row in clients.Rows)
                        {
                            if (types[row["ClientTypeID"].ToString()] == false) //Checks whether the IsSNMPDevice flag in the tbl_clients table is set
                            {
                                try { CollectFromClient(row["IPAddress"].ToString(), confReader.GetPort(),row["ClientID"].ToString()); }
                                catch { setIsNotResponding(row["ClientID"].ToString(),row["IPAddress"].ToString()); }
                            }
                            else if (types[row["ClientTypeID"].ToString()] == true)
                            {
                                try { CollectFromSNMP(row["IPAddress"].ToString(), row["ClientID"].ToString()); }
                                catch { setIsNotResponding(row["ClientID"].ToString(), row["IPAddress"].ToString()); }
                            }
                        }
                    }
                }
                Thread.Sleep(10000); 
            }
        }

        #region Collect from Client
        //Collect data from the client and inserts it into the DB in a parameterized fashion to avoid SQL-injections
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
                {"@0",clientId},{"@1",DateTime.Now.ToString()},{"@2",sysInfo.OsVersion},{"@3",diskSpace},{"@4",sysInfo.Uptime},{"@5",sysInfo.Ram},{"@6",sysInfo.Cpu},{"@7",sysInfo.PingToServer},{"@8","TRUE"}
            };

            //Store in DB
            sqlConn.openConnection();
            sqlConn.executeInsertQuery("INSERT INTO tbl_ClientInfo(ClientID, TimeStamp, OSVersion, DiskSpace, UpTime, RAM, CPU, Ping, IsResponding) VALUES(@0,@1,@2,@3,@4,@5,@6,@7,@8);", dictionaryForInsert);
            sqlConn.closeConnection();
        }
        #endregion


        #region Collect from SNMP
        private DataTable GetInterfacesFromDB(string clientID)
        {
            DataTable temp = new DataTable();

            try
            {
                sqlConn.openConnection();
                sqlConn.executeGetQuery("SELECT * FROM tbl_Interfaces WHERE ClientID ='" + clientID +"';");
                temp = sqlConn.getData();
                return temp;
            }
            catch { return null; }
            finally { sqlConn.closeConnection(); }
        }


        private void CollectFromSNMP(string ipAdr, string clientID)
        {
            DataTable data = GetInterfacesFromDB(clientID);
            List<string> interfaceList = new List<string>();
            List<string> interfaceIDs = new List<string>();

            foreach (DataRow row in data.Rows)
            {
                interfaceList.Add(row["InterfaceName"].ToString());
                interfaceIDs.Add(row["InterfaceID"].ToString());
            }

            List<string> oidList = SendSNMPRequest("1.3.6.1.2.1.2.2.1.2", ipAdr, interfaceList);   //ifDescr - get the OIDs of the interfaces we want to monitor
            List<string> ifSpeed = SendSNMPRequest("1.3.6.1.2.1.2.2.1.5", ipAdr, oidList);       //ifSpeed - get the speed of the interfaces listed for monitoring
            List<string> ifOutOctets = SendSNMPRequest("1.3.6.1.2.1.2.2.1.10", ipAdr, oidList);   //ifInOctets - get the inOctets value for the interfaces listed
            List<string> ifInOctets = SendSNMPRequest("1.3.6.1.2.1.2.2.1.16", ipAdr, oidList);  //ifOutOctets - get the outOctets value for the interfaces listed
            List<string> ifOperStatus = SendSNMPRequest("1.3.6.1.2.1.2.2.1.8", ipAdr, oidList);  //ifOperStatus - get the operational status of the interface listed
            List<string> ifAlias = SendSNMPRequest("1.3.6.1.2.1.31.1.1.1.18", ipAdr, oidList);   //ifAlias - get the alias/description of the interface listed

            for (int i = 0; i < interfaceList.Count; i++)
            {
                Dictionary<string, string> dictionaryForInsert = new Dictionary<string, string>()
                {
                    {"@0",interfaceIDs[i]},{"@1",DateTime.Now.ToString()},{"@2",ifInOctets[i]},{"@3",ifOutOctets[i]},{"@4",ifSpeed[i]},{"@5",ifOperStatus[i]},{"@6",ifAlias[i]}
                };

                //Store in DB
                sqlConn.openConnection();
                sqlConn.executeInsertQuery("INSERT INTO tbl_InterfaceInfo(InterfaceID, TimeStamp, IfInOctets, IfOutOctets, IfSpeed, IsUp, IfAlias) VALUES(@0,@1,@2,@3,@4,@5,@6);", dictionaryForInsert);
                sqlConn.closeConnection();
            }
        }


        private List<string> SendSNMPRequest(string mib, string ipAdr, List<string> interfaces)
        {
            List<string> values = new List<string>();

            // SNMP community name
            OctetString community = new OctetString("heimdall");

            // Define agent parameters class
            AgentParameters param = new AgentParameters(community);
            // Set SNMP version to 2 (GET-BULK only works with SNMP ver 2 and 3)
            param.Version = SnmpVersion.Ver2;
            // Construct the agent address object
            // IpAddress class is easy to use here because
            //  it will try to resolve constructor parameter if it doesn't
            //  parse to an IP address
            IpAddress agent = new IpAddress(ipAdr);

            // Construct target
            UdpTarget target = new UdpTarget((IPAddress)agent, 161, 2000, 1);

            // Define Oid that is the root of the MIB
            //  tree you wish to retrieve;
            //Oid rootOid = new Oid("1.3.6.1.2.1.2.2.1.10");  //ifInOctets - Total amount of octets
            //Oid rootOid = new Oid("1.3.6.1.2.1.2.2.1.16");  //ifOutOctets - total amount of octets
            //Oid rootOid = new Oid("1.3.6.1.2.1.2.2.1.5");   //ifSpeed - total speed of the interface
            //Oid rootOid = new Oid("1.3.6.1.2.1.2.2.1.2");   //ifDescr - Name of the interface
            Oid rootOid = new Oid(mib);

            // This Oid represents last Oid returned by
            //  the SNMP agent
            Oid lastOid = (Oid)rootOid.Clone();

            // Pdu class used for all requests
            Pdu pdu = new Pdu(PduType.GetBulk);

            // In this example, set NonRepeaters value to 0
            pdu.NonRepeaters = 0;
            // MaxRepetitions tells the agent how many Oid/Value pairs to return
            // in the response.
            pdu.MaxRepetitions = 5;

            // Loop through results
            while (lastOid != null)
            {
                // When Pdu class is first constructed, RequestId is set to 0
                // and during encoding id will be set to the random value
                // for subsequent requests, id will be set to a value that
                // needs to be incremented to have unique request ids for each
                // packet
                if (pdu.RequestId != 0)
                {
                    pdu.RequestId += 1;
                }
                // Clear Oids from the Pdu class.
                pdu.VbList.Clear();
                // Initialize request PDU with the last retrieved Oid
                pdu.VbList.Add(lastOid);
                // Make SNMP request
                SnmpV2Packet result = (SnmpV2Packet)target.Request(pdu, param);
                // You should catch exceptions in the Request if using in real application.

                // If result is null then agent didn't reply or we couldn't parse the reply.
                if (result != null)
                {
                    // ErrorStatus other then 0 is an error returned by 
                    // the Agent - see SnmpConstants for error definitions
                    if (result.Pdu.ErrorStatus != 0)
                    {
                        // agent reported an error with the request
                        Console.WriteLine("Error in SNMP reply. Error {0} index {1}",
                            result.Pdu.ErrorStatus,
                            result.Pdu.ErrorIndex);
                        lastOid = null;
                        break;
                    }
                    else
                    {
                        // Walk through returned variable bindings
                        foreach (Vb v in result.Pdu.VbList)
                        {
                            if (interfaces.Contains(v.Value.ToString()))
                                values.Add(v.Oid.ToString().Split('.').Last());
                            else if (interfaces.Contains(v.Oid.ToString().Split('.').Last()))
                                values.Add(v.Value.ToString());
                            Console.WriteLine(v.Oid.ToString() + "  " + v.Value.ToString());
                            // Check that retrieved Oid is "child" of the root OID
                            if (rootOid.IsRootOf(v.Oid))
                            {
                                if (v.Value.Type == SnmpConstants.SMI_ENDOFMIBVIEW)
                                    lastOid = null;
                                else
                                    lastOid = v.Oid;
                            }
                            else
                            {
                                // we have reached the end of the requested
                                // MIB tree. Set lastOid to null and exit loop
                                lastOid = null;
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No response received from SNMP agent.");
                }
            }
            target.Close();

            return values;
        }
        #endregion


        #region Shared
        //Connect to DB to get list of clients
        private DataTable GetClientsFromDB()
        {
            sqlConn.openConnection();

            try
            {
                sqlConn.executeGetQuery("SELECT * FROM tbl_Clients");
                return sqlConn.getData();
            }
            catch { return null; }
            finally { sqlConn.closeConnection(); }
        }


        //Connect to DB to get list of clienttypes
        private Dictionary<string, bool> GetTypes()
        {
            DataTable temp;
            Dictionary<string, bool> dictionary = new Dictionary<string, bool>();

            try
            {
                sqlConn.openConnection();
                sqlConn.executeGetQuery("SELECT * FROM tbl_ClientTypes");
                temp = sqlConn.getData();

                foreach (DataRow row in temp.Rows)
                {
                    dictionary.Add(row["ClientTypeID"].ToString(), Boolean.Parse(row["IsSNMPDevice"].ToString()));
                }
                return dictionary;
            }
            catch { return null; }
            finally { sqlConn.closeConnection(); }
        }


        //Handle insertion to DB, when clients are not responding
        private void setIsNotResponding(string clientId, string ipAdr)
        {
            Dictionary<string, string> dictionaryForInsert = new Dictionary<string, string>()
            {
                {"@0",clientId},{"@1",DateTime.Now.ToString()},{"@2","FALSE"}
            };

            //Store in DB
            sqlConn.openConnection();
            sqlConn.executeInsertQuery("INSERT INTO tbl_ClientInfo(ClientID, TimeStamp, IsResponding) VALUES(@0,@1,@2);", dictionaryForInsert);
            sqlConn.closeConnection();
        }
        #endregion
    }
}
