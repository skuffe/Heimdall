using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FrontEnd.Models
{
    public class ClientDetailsViewModel
    {
        public tbl_Clients tbl_clients { get; set; }
        public DotNet.Highcharts.Highcharts RAMChart { get; set; }
        public DotNet.Highcharts.Highcharts CPUChart { get; set; }
        public DotNet.Highcharts.Highcharts DiskSpaceChart { get; set; }
        public DotNet.Highcharts.Highcharts UptimeChart { get; set; }
        public DotNet.Highcharts.Highcharts PingChart { get; set; }
    }
}