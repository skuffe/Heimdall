using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.ServiceModel;

namespace DataCollectionClient
{
    partial class DataCollectionServicehost : ServiceBase
    {
        ServiceHost serviceHost = null;

        public DataCollectionServicehost()
        {
            InitializeComponent();
            this.ServiceName = "DataCollectionService";
            this.EventLog.Log = "Application";

            this.CanPauseAndContinue = false;
            this.CanStop = true;
            this.CanShutdown = true;
        }

        protected override void OnStart(string[] args)
        {
            string host = "localhost";
            string port = "2200";

            Uri baseAddress = new Uri("net.tcp://" + host + ":" + port + "/DataCollectionService");
            NetTcpBinding binding = new NetTcpBinding();

            serviceHost = new ServiceHost(typeof(DataCollectionService), baseAddress);
            serviceHost.AddServiceEndpoint(typeof(IDataCollectionService), binding, baseAddress);
            
            serviceHost.Open();
        }

        protected override void OnStop()
        {
            serviceHost.Close();
            base.OnStop();
        }

        protected override void OnShutdown()
        {
            serviceHost.Close();
            base.OnShutdown();
        }
    }
}
