using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataCollectionClient;
using System.ServiceModel;
using System.Diagnostics;
using System.Threading;

namespace DataCollectionHost
{
    class SysInfoCollector
    {
        IDataCollectionService collectorService = null;
        SystemInfoObject sysInfo = null;
        string ipAdr = "127.0.0.1";

        public SysInfoCollector(string ipAdr, string port)
        {
            this.ipAdr = ipAdr;

            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://"+ipAdr+":"+port+"/DataCollectionService"));
            NetTcpBinding binding = new NetTcpBinding();

            ChannelFactory<IDataCollectionService> factory =
            new ChannelFactory<IDataCollectionService>(binding, address);

            collectorService = factory.CreateChannel();
        }

        public void collect()
        {
            while (true)
            {
                sysInfo = new SystemInfoObject();

                sysInfo = collectorService.GetSystemInfo(sysInfo, this.ipAdr);
                //Store stuff in Database (Entity Framework?)

                EventLog.WriteEntry("CollectorThread", sysInfo.Time.ToString() + " DEBUG");
                Thread.Sleep(60000);
            }
        }
    }
}
