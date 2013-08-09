using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;

namespace DataCollectionClient
{
    [ServiceContract()]
    public interface IDataCollectionService
    {
        [OperationContract]
        SystemInfoObject GetSystemInfo(SystemInfoObject sysInfo, string ipAdr);

        [OperationContract]
        DriveInfo[] getDiskInfo();

        [OperationContract]
        string[] getDiskSpace();

        [OperationContract]
        bool checkIfServiceIsRunning(string serviceName);

        [OperationContract]
        string getCpuUsage();

        [OperationContract]
        string getAvailableRAM();

        [OperationContract]
        string getMachineName();

        [OperationContract]
        string getOSFriendlyName();

        [OperationContract]
        string getUptime();

        [OperationContract]
        string getPingToServer(string ipAdr, int timesToPing);
    }

    [DataContract]
    public class SystemInfoObject
    {
        DateTime time = DateTime.Now;

        string pcName;
        string osVersion;
        string[] diskSpace;
        string uptime;
        string ram;
        string cpu;
        string pingToServer;

        [DataMember]
        public DateTime Time
        {
            get { return time; }
            set { time = value; }
        }

        [DataMember]
        public string PcName
        {
            get { return pcName; }
            set { pcName = value; }
        }

        [DataMember]
        public string OsVersion
        {
            get { return osVersion; }
            set { osVersion = value; }
        }

        [DataMember]
        public string[] DiskSpace
        {
            get { return diskSpace; }
            set { diskSpace = value; }
        }

        [DataMember]
        public string Uptime
        {
            get { return uptime; }
            set { uptime = value; }
        }

        [DataMember]
        public string Ram
        {
            get { return ram; }
            set { ram = value; }
        }

        [DataMember]
        public string Cpu
        {
            get { return cpu; }
            set { cpu = value; }
        }

        [DataMember]
        public string PingToServer
        {
            get { return pingToServer; }
            set { pingToServer = value; }
        }
    }
}
