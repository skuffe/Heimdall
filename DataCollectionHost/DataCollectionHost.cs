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
        SqlConnector sqlConn = new SqlConnector();

        public DataCollectionHost()
        {
            InitializeComponent();

            this.ServiceName = "DataCollectionHost";
            this.EventLog.Log = "Application";

            this.CanPauseAndContinue = false;
            this.CanStop = true;
            this.CanShutdown = true;

            sqlConn.setConnectionProperties("SQL01\\ODIN", "heimdall", "heimdall", "Silver44");
        }

        protected override void OnStart(string[] args)
        {
            //Creates the SystemInfo collector thread
            SysInfoCollector infoCollector = new SysInfoCollector(sqlConn);
            Thread thread = new Thread(infoCollector.ThreadWrapper);

            thread.IsBackground = true;
            thread.Start();
        }

        protected override void OnStop()
        {
            base.OnStop();
        }
    }
}
