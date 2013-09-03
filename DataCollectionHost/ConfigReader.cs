using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DataCollectionHost
{
    class ConfigReader
    {
        string dbAdr;
        string port;
        string dbName;
        string user;
        string pass;
        string comName;
        string collectTime;

        public ConfigReader(string path)
        {
            dbAdr = "";
            port = "";
            dbName = "";
            user = "";
            pass = "";
            comName = "";
            collectTime = "";
            readConfig(path);
        }

        #region Private methods

        private bool readConfig(string path)
        {
            StreamReader stream = new StreamReader(path);

            try
            {
                while (!stream.EndOfStream)
                {
                    string currLine = stream.ReadLine();

                    if (currLine.ToLower().Contains("dbadr="))
                        this.dbAdr = currLine.Split('=')[1];
                    else if (currLine.ToLower().Contains("port="))
                        this.port = currLine.Split('=')[1];
                    else if (currLine.ToLower().Contains("dbname="))
                        this.dbName = currLine.Split('=')[1];
                    else if (currLine.ToLower().Contains("user="))
                        this.user = currLine.Split('=')[1];
                    else if (currLine.ToLower().Contains("pass="))
                        this.pass = currLine.Split('=')[1];
                    else if (currLine.ToLower().Contains("comname="))
                        this.comName = currLine.Split('=')[1];
                    else if (currLine.ToLower().Contains("collecttime="))
                        this.collectTime = currLine.Split('=')[1];
                }
                return true;
            }
            catch { return false; }
        }

        #endregion

        #region Get-statements

        public string GetDBaddress()
        {
            return this.dbAdr;
        }

        public string GetPort()
        {
            return this.port;
        }

        public string GetDBname()
        {
            return this.dbName;
        }

        public string GetUser()
        {
            return this.user;
        }

        public string getPass()
        {
            return this.pass;
        }

        public string getComName()
        {
            return this.comName;
        }

        public string getCollectTime()
        {
            return this.collectTime;
        }
        #endregion
    }
}
