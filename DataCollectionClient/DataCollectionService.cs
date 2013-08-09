using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Management;

namespace DataCollectionClient
{
    public class DataCollectionService : IDataCollectionService
    {
        public SystemInfoObject GetSystemInfo(SystemInfoObject sysInfo, string ipAdr)
        {
            if (sysInfo == null)
                throw new ArgumentNullException("SystemInfo is null");

            sysInfo.Time = DateTime.Now;
            sysInfo.PcName = getMachineName();
            sysInfo.Ram = getAvailableRAM();
            sysInfo.OsVersion = getOSFriendlyName();
            sysInfo.Uptime = getUptime();
            sysInfo.Cpu = getCpuUsage();
            sysInfo.DiskSpace = getDiskSpace();
            sysInfo.PingToServer = getPingToServer(ipAdr, 4);

            return sysInfo;
        }
        
        public DriveInfo[] getDiskInfo()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();

            return drives;
        }

        public string[] getDiskSpace()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            string[] space = new string[drives.Length];

            int i = 0;
            foreach (DriveInfo drive in drives)
            {
                if (drive.IsReady == true)
                    space[i] = drive.Name + ": " + drive.TotalFreeSpace + " / " + drive.TotalSize;
                else
                    space[i] = "Unable to calculate; device not ready";
                i++;
            }

            return space;
        }

        public bool checkIfServiceIsRunning(string serviceName)
        {
            ServiceController sc = new ServiceController(serviceName);
            sc.Refresh();

            if (sc.Status == ServiceControllerStatus.Running)
                return true;

            return false;
        }

        public string getCpuUsage()
        {
            PerformanceCounter cpu = new PerformanceCounter("Processor", "% Processor Time");
            cpu.InstanceName = "_Total";

            cpu.NextValue();
            Thread.Sleep(1000);

            return cpu.NextValue() + "%";
        }

        public string getAvailableRAM()
        {
            PerformanceCounter ram = new PerformanceCounter("Memory", "Available MBytes");

            return ram.NextValue() + "MB";
        }

        public string getMachineName()
        {
            return System.Environment.MachineName;
        }

        public string getOSFriendlyName()
        {
            string result = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            foreach (ManagementObject os in searcher.Get())
            {
                result = os["Caption"].ToString();
                break;
            }
            return result;
        }

        public string getUptime()
        {
            using (var uptime = new PerformanceCounter("System", "System Up Time"))
            {
                uptime.NextValue();       //Call this an extra time before reading its value
                TimeSpan span = TimeSpan.FromSeconds(uptime.NextValue());

                return span.Days + " day(s) " + span.Hours.ToString().PadLeft(2, '0') + ":" +
                    span.Minutes.ToString().PadLeft(2, '0') + ":" + span.Seconds.ToString().PadLeft(2, '0');
            }
        }

        public string getPingToServer(string ipAdr, int timesToPing)
        {
            IPAddress address = IPAddress.Parse(ipAdr);
            PingOptions options = new PingOptions(128, true);
            Ping ping = new Ping();

            byte[] buffer = new byte[32];
            long[] times = new long[timesToPing];
            long successfulPings = 0;

            for (int i = 0; i < times.Length; i++)
            {

                PingReply reply = ping.Send(address, 1000, buffer, options);
                switch (reply.Status)
                {
                    case IPStatus.Success:
                        times[i] = reply.RoundtripTime;
                        successfulPings++;
                        break;
                    case IPStatus.TimedOut:
                        times[i] = -1;
                        break;
                    default:
                        break;
                }
            }
            if (successfulPings > 0)
                return (times.Sum() / successfulPings).ToString() + "ms";
            else
                return "Could not reach host";
        }
    }
}
