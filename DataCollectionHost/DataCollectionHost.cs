using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.ServiceModel;
using System.Threading;

using DataCollectionClient;
using SnmpSharpNet;

namespace DataCollectionHost
{
    public partial class DataCollectionHost : ServiceBase
    {
        IDataCollectionService collectorService = null;

        public DataCollectionHost()
        {
            InitializeComponent();

            this.ServiceName = "DataCollectionHost";
            this.EventLog.Log = "Application";

            this.CanPauseAndContinue = false;
            this.CanStop = true;
            this.CanShutdown = true;
        }

        protected override void OnStart(string[] args)
        {
            //Start the SNMP listener in a separate thread
            SNMPInterface snmpInterface = new SNMPInterface();
            Thread snmpThread = new Thread(snmpInterface.threadWrapper);

            snmpThread.IsBackground = true;
            snmpThread.Start();

            //Create an array of all the addresses and make endpoints/channels
            //Create a thread for each host
            SysInfoCollector infoCollector = new SysInfoCollector("127.0.0.1", "2200");
            Thread thread = new Thread(infoCollector.collect);

            thread.IsBackground = true;
            thread.Start();
        }

        protected override void OnStop()
        {
            base.OnStop();
        }
    }
}
