using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace FrontEnd.Tools
{
    public static class Tools
    {
        public static string ToReadableString(this TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? String.Empty : "s") : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? String.Empty : "s") : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? String.Empty : "s") : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? String.Empty : "s") : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

            return formatted;
        }

        public static List<string> GetFileList(FtpConfig ftpInfo)
        {
            var fileList = new List<string>();
            WebResponse response = null;
            StreamReader reader = null;
            try
            {
                var ftpRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpInfo.Server + ":" + ftpInfo.Port + "/" + ftpInfo.RemotePath));

                ftpRequest.UseBinary = true;
                ftpRequest.Credentials = new NetworkCredential(ftpInfo.Username, ftpInfo.Password);
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                ftpRequest.Proxy = null;
                ftpRequest.KeepAlive = false;
                ftpRequest.UsePassive = true;
                response = ftpRequest.GetResponse();

                reader = new StreamReader(response.GetResponseStream(), Encoding.Default);

                var line = reader.ReadLine();
                while (line != null)
                {
                    fileList.Add(line.Trim());
                    line = reader.ReadLine();
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (response != null)
                {
                    response.Close();
                }
            }

            return fileList;
        }

        public static string GetFile(FtpConfig ftpInfo, string fileName)
        {
            string content;
            WebResponse response = null;
            StreamReader reader = null;
            try
            {
                var ftpRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpInfo.Server + ":" + ftpInfo.Port + "/" + ftpInfo.RemotePath + "/" + fileName));

                ftpRequest.UseBinary = true;
                ftpRequest.Credentials = new NetworkCredential(ftpInfo.Username, ftpInfo.Password);
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                ftpRequest.Proxy = null;
                ftpRequest.KeepAlive = false;
                ftpRequest.UsePassive = true;

                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                response = ftpRequest.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.Default);

                using(reader)
                {
                    content = reader.ReadToEnd();
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (response != null)
                {
                    response.Close();
                }
            }

            return content;
        }
    }

    public class FtpConfig
    {
        public string Server { get; set; }
        public string Port { get; set; }
        public string RemotePath { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}