using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FrontEnd.Models
{
    public class InterfaceDetailsViewModel
    {
        public tbl_Interfaces tbl_interfaces { get; set; }
        public DotNet.Highcharts.Highcharts InterfaceChart { get; set; }
    }
}