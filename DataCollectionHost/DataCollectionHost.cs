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
        ConfigReader confReader = new ConfigReader("D:\\config.txt");

        public DataCollectionHost()
        {
            InitializeComponent();

            this.ServiceName = "DataCollectionHost";
            this.EventLog.Log = "Application";

            this.CanPauseAndContinue = false;
            this.CanStop = true;
            this.CanShutdown = true;

            sqlConn.setConnectionProperties(confReader.GetDBaddress(), confReader.GetDBname(), confReader.GetUser(), confReader.getPass());
        }

        protected override void OnStart(string[] args)
        {
            //Creates the SystemInfo collector thread
            SysInfoCollector infoCollector = new SysInfoCollector(sqlConn, confReader);
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
