//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FrontEnd.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbl_ClientInfo
    {
        public int ClientInfoID { get; set; }
        public int ClientID { get; set; }
        public System.DateTime TimeStamp { get; set; }
        public string OSVersion { get; set; }
        public string DiskSpace { get; set; }
        public string UpTime { get; set; }
        public string RAM { get; set; }
        public string CPU { get; set; }
        public string Ping { get; set; }
        public bool IsResponding { get; set; }
    
        public virtual tbl_Clients tbl_Clients { get; set; }
    }
}
